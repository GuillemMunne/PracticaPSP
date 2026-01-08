using System.Windows;
using System.Windows.Controls;
using PokedexWpf.Models;

namespace PokedexWpf.Controls;

public partial class EvolutionBubbleControl : UserControl
{
    public static readonly DependencyProperty PokemonProperty = DependencyProperty.Register(
        nameof(Pokemon), typeof(Pokemon), typeof(EvolutionBubbleControl), new PropertyMetadata(null));

    public Pokemon? Pokemon
    {
        get => (Pokemon?)GetValue(PokemonProperty);
        set => SetValue(PokemonProperty, value);
    }

    public EvolutionBubbleControl()
    {
        InitializeComponent();
    }
}
