namespace PokedexWpf.Models;

public class PokemonType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public override string ToString() => Name;
}
