using System.Collections.ObjectModel;

namespace PokedexWpf.Models;

public class Pokemon
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Height { get; set; }
    public double Weight { get; set; }
    public ObservableCollection<PokemonType> Types { get; } = new();
    public ObservableCollection<string> Abilities { get; } = new();
    public ObservableCollection<Pokemon> Evolutions { get; } = new();
    public PokemonStats Stats { get; } = new();

    public Pokemon Clone()
    {
        var clone = new Pokemon
        {
            Id = Id,
            Code = Code,
            Name = Name,
            Description = Description,
            Height = Height,
            Weight = Weight,
        };

        foreach (var type in Types)
        {
            clone.Types.Add(new PokemonType { Id = type.Id, Name = type.Name });
        }

        foreach (var ability in Abilities)
        {
            clone.Abilities.Add(ability);
        }

        foreach (var evolution in Evolutions)
        {
            clone.Evolutions.Add(evolution);
        }

        clone.Stats.Hp = Stats.Hp;
        clone.Stats.Attack = Stats.Attack;
        clone.Stats.Defense = Stats.Defense;
        clone.Stats.SpecialAttack = Stats.SpecialAttack;
        clone.Stats.SpecialDefense = Stats.SpecialDefense;
        clone.Stats.Speed = Stats.Speed;

        return clone;
    }
}
