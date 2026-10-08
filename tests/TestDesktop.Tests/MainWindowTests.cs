using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Interactivity;
using TestDesktop;
using Xunit;

[assembly: AvaloniaTestApplication(typeof(TestDesktop.Tests.TestAppBuilder))]

namespace TestDesktop.Tests;

public static class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UseSkia()
            .WithInterFont()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false });
}

public class MainWindowTests
{
    [AvaloniaFact]
    public void GreetingUsesTheEnteredName()
    {
        var window = new MainWindow();
        window.Show();
        try
        {
            window.FindControl<TextBox>("NameInput")!.Text = "  Alex  ";
            window.FindControl<Button>("GreetButton")!
                .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.Equal("Hello, Alex!", window.FindControl<TextBlock>("GreetingText")!.Text);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void BlankNameUsesTheDefaultGreeting()
    {
        var window = new MainWindow();
        window.Show();
        try
        {
            window.FindControl<TextBox>("NameInput")!.Text = "   ";
            window.FindControl<Button>("GreetButton")!
                .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

            Assert.Equal("Hello!", window.FindControl<TextBlock>("GreetingText")!.Text);
        }
        finally
        {
            window.Close();
        }
    }
}
