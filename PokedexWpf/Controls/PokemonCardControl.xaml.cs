using System.Windows;
using System.Windows.Controls;
using PokedexWpf.Models;

namespace PokedexWpf.Controls;

public partial class PokemonCardControl : UserControl
{
    public static readonly DependencyProperty PokemonProperty = DependencyProperty.Register(
        nameof(Pokemon), typeof(Pokemon), typeof(PokemonCardControl), new PropertyMetadata(null));

    public Pokemon? Pokemon
    {
        get => (Pokemon?)GetValue(PokemonProperty);
        set => SetValue(PokemonProperty, value);
    }

    public PokemonCardControl()
    {
        InitializeComponent();
    }
}
