#include <errno.h>
#include <pthread.h>
#include <semaphore.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>
#include <unistd.h>

// Numero de enteros que se leeran de golpe al fusionar ficheros.
#define TAM_BUFFER_LECTURA 4096

// Representa un bloque del fichero con su cantidad de elementos.
struct Bloque {
    uint32_t *datos;
    size_t cantidad;
};

// Informacion necesaria para que un hilo ordene un bloque.
struct TareaOrdenacion {
    struct Bloque *bloque;
    sem_t *finalizado;
};

// Lector con buffer para recorrer un fichero binario ordenado.
struct LectorStream {
    FILE *archivo;
    uint32_t buffer[TAM_BUFFER_LECTURA];
    size_t indice;
    size_t cargados;
};

static int comparar_uint32(const void *a, const void *b) {
    uint32_t izquierda = *(const uint32_t *)a;
    uint32_t derecha = *(const uint32_t *)b;
    if (izquierda < derecha) {
        return -1;
    }
    if (izquierda > derecha) {
        return 1;
    }
    return 0;
}

// Funcion de hilo: ordena el bloque usando qsort y avisa mediante semaforo.
static void *ordenar_bloque(void *arg) {
    struct TareaOrdenacion *tarea = (struct TareaOrdenacion *)arg;
    if (tarea->bloque->cantidad > 1) {
        qsort(tarea->bloque->datos, tarea->bloque->cantidad, sizeof(uint32_t), comparar_uint32);
    }
    sem_post(tarea->finalizado);
    return NULL;
}

static void lector_iniciar(struct LectorStream *lector, FILE *archivo) {
    lector->archivo = archivo;
    lector->indice = 0;
    lector->cargados = 0;
}

// Devuelve 1 si se ha podido leer un valor, 0 si se llego al final.
static int lector_siguiente(struct LectorStream *lector, uint32_t *valor) {
    if (lector->indice >= lector->cargados) {
        lector->cargados = fread(lector->buffer, sizeof(uint32_t), TAM_BUFFER_LECTURA, lector->archivo);
        lector->indice = 0;
        if (lector->cargados == 0) {
            return 0;
        }
    }
    *valor = lector->buffer[lector->indice++];
    return 1;
}

// Fusiona dos ficheros ordenados en uno nuevo (salida).
static int fusionar_ficheros(const char *ruta_izquierda, const char *ruta_derecha, const char *ruta_salida) {
    FILE *izquierda = fopen(ruta_izquierda, "rb");
    if (!izquierda) {
        fprintf(stderr, "Error al abrir %s: %s\n", ruta_izquierda, strerror(errno));
        return -1;
    }
    FILE *derecha = fopen(ruta_derecha, "rb");
    if (!derecha) {
        fprintf(stderr, "Error al abrir %s: %s\n", ruta_derecha, strerror(errno));
        fclose(izquierda);
        return -1;
    }
    FILE *salida = fopen(ruta_salida, "wb");
    if (!salida) {
        fprintf(stderr, "Error al abrir %s: %s\n", ruta_salida, strerror(errno));
        fclose(izquierda);
        fclose(derecha);
        return -1;
    }

    struct LectorStream lector_izquierda;
    struct LectorStream lector_derecha;
    lector_iniciar(&lector_izquierda, izquierda);
    lector_iniciar(&lector_derecha, derecha);

    uint32_t valor_izquierda = 0;
    uint32_t valor_derecha = 0;
    int hay_izquierda = lector_siguiente(&lector_izquierda, &valor_izquierda);
    int hay_derecha = lector_siguiente(&lector_derecha, &valor_derecha);

    // Se van escribiendo siempre los valores mas pequenos de ambos flujos.
    while (hay_izquierda && hay_derecha) {
        if (valor_izquierda <= valor_derecha) {
            fwrite(&valor_izquierda, sizeof(uint32_t), 1, salida);
            hay_izquierda = lector_siguiente(&lector_izquierda, &valor_izquierda);
        } else {
            fwrite(&valor_derecha, sizeof(uint32_t), 1, salida);
            hay_derecha = lector_siguiente(&lector_derecha, &valor_derecha);
        }
    }

    // Copiar el resto de valores del fichero que aun tenga datos.
    while (hay_izquierda) {
        fwrite(&valor_izquierda, sizeof(uint32_t), 1, salida);
        hay_izquierda = lector_siguiente(&lector_izquierda, &valor_izquierda);
    }

    while (hay_derecha) {
        fwrite(&valor_derecha, sizeof(uint32_t), 1, salida);
        hay_derecha = lector_siguiente(&lector_derecha, &valor_derecha);
    }

    fclose(izquierda);
    fclose(derecha);
    fclose(salida);
    return 0;
}

static void imprimir_uso(const char *programa) {
    fprintf(stderr, "Uso: %s <entrada.bin> <salida.bin> <M> <N>\n", programa);
    fprintf(stderr, "  M = numeros por bloque, N = numero de bloques/hilos\n");
}

int main(int argc, char **argv) {
    if (argc != 5) {
        imprimir_uso(argv[0]);
        return 1;
    }

    const char *ruta_entrada = argv[1];
    const char *ruta_salida = argv[2];
    long valor_m = strtol(argv[3], NULL, 10);
    long valor_n = strtol(argv[4], NULL, 10);

    if (valor_m <= 0 || valor_n <= 0) {
        fprintf(stderr, "M y N deben ser enteros positivos.\n");
        return 1;
    }

    size_t tam_bloque = (size_t)valor_m;
    size_t cantidad_bloques = (size_t)valor_n;

    FILE *entrada = fopen(ruta_entrada, "rb");
    if (!entrada) {
        fprintf(stderr, "Error al abrir %s: %s\n", ruta_entrada, strerror(errno));
        return 1;
    }

    char ruta_prefijo[512];
    char ruta_bloques[512];
    char ruta_fusion[512];
    snprintf(ruta_prefijo, sizeof(ruta_prefijo), "%s.prefijo", ruta_salida);
    snprintf(ruta_bloques, sizeof(ruta_bloques), "%s.bloques", ruta_salida);
    snprintf(ruta_fusion, sizeof(ruta_fusion), "%s.fusion", ruta_salida);

    struct Bloque *bloques = calloc(cantidad_bloques, sizeof(struct Bloque));
    if (!bloques) {
        fprintf(stderr, "No se pudieron reservar los bloques.\n");
        fclose(entrada);
        return 1;
    }

    for (size_t i = 0; i < cantidad_bloques; ++i) {
        bloques[i].datos = malloc(tam_bloque * sizeof(uint32_t));
        if (!bloques[i].datos) {
            fprintf(stderr, "No se pudo reservar el bloque %zu.\n", i);
            for (size_t j = 0; j < i; ++j) {
                free(bloques[j].datos);
            }
            free(bloques);
            fclose(entrada);
            return 1;
        }
    }

    sem_t ordenacion_finalizada;
    sem_init(&ordenacion_finalizada, 0, 0);

    pthread_t *hilos = calloc(cantidad_bloques, sizeof(pthread_t));
    struct TareaOrdenacion *tareas = calloc(cantidad_bloques, sizeof(struct TareaOrdenacion));
    if (!hilos || !tareas) {
        fprintf(stderr, "No se pudieron reservar los hilos.\n");
        fclose(entrada);
        return 1;
    }

    struct timespec inicio;
    struct timespec fin;
    clock_gettime(CLOCK_MONOTONIC, &inicio);

    size_t total_numeros = 0;
    int hay_prefijo = 0;

    // Bucle principal: leer N bloques, ordenarlos y fusionarlos con el prefijo.
    while (1) {
        size_t bloques_activos = 0;
        for (size_t i = 0; i < cantidad_bloques; ++i) {
            size_t leidos = fread(bloques[i].datos, sizeof(uint32_t), tam_bloque, entrada);
            bloques[i].cantidad = leidos;
            if (leidos > 0) {
                tareas[i].bloque = &bloques[i];
                tareas[i].finalizado = &ordenacion_finalizada;
                if (pthread_create(&hilos[i], NULL, ordenar_bloque, &tareas[i]) != 0) {
                    fprintf(stderr, "No se pudo crear el hilo %zu.\n", i);
                    fclose(entrada);
                    return 1;
                }
                bloques_activos++;
                total_numeros += leidos;
            }
        }

        if (bloques_activos == 0) {
            break;
        }

        // Esperar a que todos los hilos terminen su bloque.
        for (size_t i = 0; i < bloques_activos; ++i) {
            sem_wait(&ordenacion_finalizada);
        }

        for (size_t i = 0; i < cantidad_bloques; ++i) {
            if (bloques[i].cantidad > 0) {
                pthread_join(hilos[i], NULL);
            }
        }

        // Fusionar los bloques ordenados en un solo fichero temporal.
        FILE *archivo_bloques = fopen(ruta_bloques, "wb");
        if (!archivo_bloques) {
            fprintf(stderr, "Error al abrir %s: %s\n", ruta_bloques, strerror(errno));
            fclose(entrada);
            return 1;
        }

        size_t restantes = 0;
        for (size_t i = 0; i < cantidad_bloques; ++i) {
            restantes += bloques[i].cantidad;
        }

        size_t *indices = calloc(cantidad_bloques, sizeof(size_t));
        if (!indices) {
            fprintf(stderr, "No se pudieron reservar los indices de fusion.\n");
            fclose(archivo_bloques);
            fclose(entrada);
            return 1;
        }

        // Selecciona el minimo de cada bloque hasta consumirlos todos.
        while (restantes > 0) {
            size_t bloque_minimo = SIZE_MAX;
            uint32_t valor_minimo = 0;
            for (size_t i = 0; i < cantidad_bloques; ++i) {
                if (indices[i] >= bloques[i].cantidad) {
                    continue;
                }
                uint32_t valor = bloques[i].datos[indices[i]];
                if (bloque_minimo == SIZE_MAX || valor < valor_minimo) {
                    bloque_minimo = i;
                    valor_minimo = valor;
                }
            }
            if (bloque_minimo == SIZE_MAX) {
                break;
            }
            fwrite(&valor_minimo, sizeof(uint32_t), 1, archivo_bloques);
            indices[bloque_minimo]++;
            restantes--;
        }

        free(indices);
        fclose(archivo_bloques);

        // Primera iteracion: no hay prefijo, se renombra el fichero de bloques.
        if (!hay_prefijo) {
            if (rename(ruta_bloques, ruta_prefijo) != 0) {
                fprintf(stderr, "Error al renombrar %s a %s: %s\n", ruta_bloques, ruta_prefijo, strerror(errno));
                fclose(entrada);
                return 1;
            }
            hay_prefijo = 1;
        } else {
            // Fusionar prefijo ordenado con el nuevo bloque ordenado.
            if (fusionar_ficheros(ruta_prefijo, ruta_bloques, ruta_fusion) != 0) {
                fclose(entrada);
                return 1;
            }
            if (remove(ruta_prefijo) != 0) {
                fprintf(stderr, "Error al borrar %s: %s\n", ruta_prefijo, strerror(errno));
                fclose(entrada);
                return 1;
            }
            if (remove(ruta_bloques) != 0) {
                fprintf(stderr, "Error al borrar %s: %s\n", ruta_bloques, strerror(errno));
                fclose(entrada);
                return 1;
            }
            if (rename(ruta_fusion, ruta_prefijo) != 0) {
                fprintf(stderr, "Error al renombrar %s a %s: %s\n", ruta_fusion, ruta_prefijo, strerror(errno));
                fclose(entrada);
                return 1;
            }
        }
    }

    fclose(entrada);

    if (!hay_prefijo) {
        fprintf(stderr, "El fichero de entrada no contenia numeros.\n");
        return 1;
    }

    if (rename(ruta_prefijo, ruta_salida) != 0) {
        fprintf(stderr, "Error al renombrar %s a %s: %s\n", ruta_prefijo, ruta_salida, strerror(errno));
        return 1;
    }

    clock_gettime(CLOCK_MONOTONIC, &fin);

    double segundos = (double)(fin.tv_sec - inicio.tv_sec) +
                      (double)(fin.tv_nsec - inicio.tv_nsec) / 1000000000.0;

    printf("Ordenados %zu numeros en %.3f segundos.\n", total_numeros, segundos);
    printf("Salida escrita en %s\n", ruta_salida);

    for (size_t i = 0; i < cantidad_bloques; ++i) {
        free(bloques[i].datos);
    }
    free(bloques);
    free(hilos);
    free(tareas);
    sem_destroy(&ordenacion_finalizada);

    return 0;
}
