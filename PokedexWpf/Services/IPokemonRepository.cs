using PokedexWpf.Models;

namespace PokedexWpf.Services;

public interface IPokemonRepository
{
    IReadOnlyList<Pokemon> GetAll();
    IReadOnlyList<PokemonType> GetAllTypes();
    IReadOnlyList<string> GetAllAbilities();
    void Save(Pokemon pokemon);
    void Delete(Pokemon pokemon);
}
