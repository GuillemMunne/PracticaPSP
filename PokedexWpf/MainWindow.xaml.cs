using System.Windows;
using PokedexWpf.ViewModels;

namespace PokedexWpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }
}
