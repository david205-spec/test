using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using TestDesktop;
using TestDesktop.Game;
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
    public void WindowRendersAndKeyboardStartsPausesAndRestartsGame()
    {
        var window = new MainWindow();
        window.Show();
        try
        {
            var view = window.FindControl<GameView>("GameSurface")!;
            Assert.Equal("Flappy Bird", window.Title);
            Assert.Equal(GameState.Ready, view.Game.State);
            SaveFrame(window, "flappy-bird-menu.png");

            Press(window, Key.Space);
            Assert.Equal(GameState.Playing, view.Game.State);
            Press(window, Key.P);
            Assert.Equal(GameState.Paused, view.Game.State);
            Press(window, Key.R);
            Assert.Equal(GameState.Ready, view.Game.State);
        }
        finally { window.Close(); }
    }

    [AvaloniaFact]
    public void MouseClickStartsGameAndResumesPause()
    {
        var window = new MainWindow();
        window.Show();
        try
        {
            var view = window.FindControl<GameView>("GameSurface")!;
            Click(window, new Point(240, 460));
            Assert.Equal(GameState.Playing, view.Game.State);
            view.Game.TogglePause();
            Click(window, new Point(240, 300));
            Assert.Equal(GameState.Playing, view.Game.State);
            SaveFrame(window, "flappy-bird-playing.png");
        }
        finally { window.Close(); }
    }

    private static void Press(MainWindow window, Key key)
    {
        var physical = key switch
        {
            Key.Space => PhysicalKey.Space,
            Key.P => PhysicalKey.P,
            Key.R => PhysicalKey.R,
            _ => PhysicalKey.None
        };
        window.KeyPress(key, RawInputModifiers.None, physical, null);
        window.KeyRelease(key, RawInputModifiers.None, physical, null);
    }

    private static void Click(MainWindow window, Point point)
    {
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
    }

    private static void SaveFrame(MainWindow window, string name)
    {
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        using var frame = window.CaptureRenderedFrame();
        Assert.NotNull(frame);
        Assert.Equal(new PixelSize(480, 640), frame.PixelSize);
        var screenshots = Environment.GetEnvironmentVariable("FLAPPY_SCREENSHOT_DIR");
        if (screenshots is not null)
        {
            Directory.CreateDirectory(screenshots);
            frame.Save(Path.Combine(screenshots, name), new PngBitmapEncoderOptions());
        }
    }
}
