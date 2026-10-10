using Avalonia.Controls;

namespace TestDesktop;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Opened += (_, _) => GameSurface.Focus();
        Deactivated += (_, _) =>
        {
            if (GameSurface.Game.State == Game.GameState.Playing)
            {
                GameSurface.Game.TogglePause();
                GameSurface.InvalidateVisual();
            }
        };
    }
}
