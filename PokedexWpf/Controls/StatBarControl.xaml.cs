using System.Windows;
using System.Windows.Controls;

namespace PokedexWpf.Controls;

public partial class StatBarControl : UserControl
{
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(
        nameof(Label), typeof(string), typeof(StatBarControl), new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value), typeof(int), typeof(StatBarControl), new PropertyMetadata(0));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public StatBarControl()
    {
        InitializeComponent();
    }
}
