using System;
using System.Windows;
using System.Windows.Threading;

namespace PokedexWpf;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var splash = new SplashScreenWindow();
        splash.Show();

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(2) };
        timer.Tick += (_, _) =>
        {
            timer.Stop();
            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();
            splash.Close();
        };
        timer.Start();
    }
}
