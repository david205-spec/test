using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TestDesktop;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private void Greet(object? sender, RoutedEventArgs e)
    {
        var name = NameInput.Text?.Trim();
        GreetingText.Text = string.IsNullOrEmpty(name) ? "Hello!" : $"Hello, {name}!";
    }
}
