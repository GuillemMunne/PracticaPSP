using System.Collections.ObjectModel;

namespace PokedexWpf.Models;

public class DamageModifier
{
    public string ModifierLabel { get; set; } = string.Empty;
    public ObservableCollection<PokemonType> Types { get; } = new();
}
