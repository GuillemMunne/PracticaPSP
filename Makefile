CC = gcc
CFLAGS = -pthread

all: sorter

sorter: main.c
	$(CC) $(CFLAGS) -o sorter main.c

clean:
	rm -f sorter
