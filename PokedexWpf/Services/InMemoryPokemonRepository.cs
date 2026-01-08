using PokedexWpf.Models;

namespace PokedexWpf.Services;

public class InMemoryPokemonRepository : IPokemonRepository
{
    private readonly List<Pokemon> _pokemons;
    private readonly List<PokemonType> _types;
    private readonly List<string> _abilities;

    public InMemoryPokemonRepository()
    {
        _types = new List<PokemonType>
        {
            new() { Id = 1, Name = "Fire" },
            new() { Id = 2, Name = "Water" },
            new() { Id = 3, Name = "Grass" },
            new() { Id = 4, Name = "Flying" },
            new() { Id = 5, Name = "Electric" },
            new() { Id = 6, Name = "Ice" },
        };

        _abilities = new List<string> { "Blaze", "Torrent", "Overgrow", "Intimidate", "Pressure" };

        var charmander = new Pokemon
        {
            Id = 4,
            Code = "004",
            Name = "Charmander",
            Description = "Pokemon de tipus foc amb cua encesa.",
            Height = 0.6,
            Weight = 8.5,
        };
        charmander.Types.Add(_types[0]);
        charmander.Abilities.Add("Blaze");
        charmander.Stats.Hp = 39;
        charmander.Stats.Attack = 52;
        charmander.Stats.Defense = 43;
        charmander.Stats.SpecialAttack = 60;
        charmander.Stats.SpecialDefense = 50;
        charmander.Stats.Speed = 65;

        var charizard = new Pokemon
        {
            Id = 6,
            Code = "006",
            Name = "Charizard",
            Description = "Pokemon de foc i vol que escup flames.",
            Height = 1.7,
            Weight = 90.5,
        };
        charizard.Types.Add(_types[0]);
        charizard.Types.Add(_types[3]);
        charizard.Abilities.Add("Blaze");
        charizard.Stats.Hp = 78;
        charizard.Stats.Attack = 84;
        charizard.Stats.Defense = 78;
        charizard.Stats.SpecialAttack = 109;
        charizard.Stats.SpecialDefense = 85;
        charizard.Stats.Speed = 100;

        charmander.Evolutions.Add(charmander);
        charmander.Evolutions.Add(charizard);

        charizard.Evolutions.Add(charmander);
        charizard.Evolutions.Add(charizard);

        _pokemons = new List<Pokemon> { charmander, charizard };
    }

    public IReadOnlyList<Pokemon> GetAll() => _pokemons;

    public IReadOnlyList<PokemonType> GetAllTypes() => _types;

    public IReadOnlyList<string> GetAllAbilities() => _abilities;

    public void Save(Pokemon pokemon)
    {
        // TODO: Reemplaçar amb persistència MySQL.
        if (!_pokemons.Contains(pokemon))
        {
            _pokemons.Add(pokemon);
        }
    }

    public void Delete(Pokemon pokemon)
    {
        // TODO: Reemplaçar amb persistència MySQL.
        _pokemons.Remove(pokemon);
    }
}
