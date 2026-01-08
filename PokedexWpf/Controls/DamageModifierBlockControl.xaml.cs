using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using PokedexWpf.Models;

namespace PokedexWpf.Controls;

public partial class DamageModifierBlockControl : UserControl
{
    public static readonly DependencyProperty ModifierLabelProperty = DependencyProperty.Register(
        nameof(ModifierLabel), typeof(string), typeof(DamageModifierBlockControl), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty TypesProperty = DependencyProperty.Register(
        nameof(Types), typeof(ObservableCollection<PokemonType>), typeof(DamageModifierBlockControl), new PropertyMetadata(new ObservableCollection<PokemonType>()));

    public string ModifierLabel
    {
        get => (string)GetValue(ModifierLabelProperty);
        set => SetValue(ModifierLabelProperty, value);
    }

    public ObservableCollection<PokemonType> Types
    {
        get => (ObservableCollection<PokemonType>)GetValue(TypesProperty);
        set => SetValue(TypesProperty, value);
    }

    public DamageModifierBlockControl()
    {
        InitializeComponent();
    }
}
