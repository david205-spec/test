using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace TestDesktop.Game;

public sealed class GameView : Control
{
    private static readonly IBrush Sky = Brush("#BFEAF1");
    private static readonly IBrush Ink = Brush("#203F4B");
    private static readonly IBrush Card = Brush("#F2203F4B");
    private static readonly IBrush Cream = Brush("#FFF8DF");
    private static readonly IBrush Yellow = Brush("#FFD568");
    private static readonly IBrush Orange = Brush("#EF9846");
    private static readonly IBrush Green = Brush("#70C49B");
    private static readonly IBrush DarkGreen = Brush("#3F9376");
    private static readonly IBrush Mint = Brush("#A5DBB0");
    private static readonly IBrush Cloud = Brush("#EAF8F7");
    private static readonly IBrush Muted = Brush("#AED1D2");
    private static readonly Pen Outline = new(Ink, 3);
    private readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(16) };
    private readonly Stopwatch clock = new();
    private double lastTime;

    public FlappyGame Game { get; } = new();

    public GameView()
    {
        Focusable = true;
        ClipToBounds = true;
        timer.Tick += (_, _) =>
        {
            var now = clock.Elapsed.TotalSeconds;
            Game.Update(now - lastTime);
            lastTime = now;
            InvalidateVisual();
        };
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        clock.Restart();
        lastTime = 0;
        timer.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        timer.Stop();
        clock.Stop();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        switch (e.Key)
        {
            case Key.Space:
            case Key.Up:
            case Key.W:
                Game.Flap();
                break;
            case Key.P:
            case Key.Escape:
                Game.TogglePause();
                break;
            case Key.R:
                Game.Restart();
                break;
            default:
                return;
        }
        e.Handled = true;
        InvalidateVisual();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            return;
        Focus();
        if (Game.State == GameState.Paused)
            Game.TogglePause();
        else
            Game.Flap();
        e.Handled = true;
        InvalidateVisual();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);
        context.FillRectangle(Ink, new Rect(Bounds.Size));
        var scale = Math.Min(Bounds.Width / FlappyGame.Width, Bounds.Height / FlappyGame.Height);
        if (scale <= 0)
            return;
        var left = (Bounds.Width - FlappyGame.Width * scale) / 2;
        var top = (Bounds.Height - FlappyGame.Height * scale) / 2;
        using var transform = context.PushTransform(
            Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(left, top));
        using var clip = context.PushClip(new Rect(0, 0, FlappyGame.Width, FlappyGame.Height));

        DrawLandscape(context);
        if (Game.State != GameState.Ready)
        {
            foreach (var pipe in Game.Pipes)
                DrawPipe(context, pipe);
            DrawBird(context, FlappyGame.BirdX, Game.BirdY,
                Math.Clamp(Game.BirdVelocity / 650, -0.45, 0.95));
            DrawHud(context);
        }

        context.FillRectangle(DarkGreen, new Rect(0, FlappyGame.GroundY, 480, 64));
        context.FillRectangle(Mint, new Rect(0, FlappyGame.GroundY, 480, 8));
        for (var x = -(Game.Distance % 24); x < 480; x += 24)
            context.DrawLine(new Pen(Green, 3), new Point(x, 593), new Point(x + 8, 585));
        Text(context, "SPACE / ↑  FLAP     P  PAUSE     R  RESTART", 240, 612, 12, Cream, center: true);

        switch (Game.State)
        {
            case GameState.Ready:
                DrawReady(context);
                break;
            case GameState.GameOver:
                DrawGameOver(context);
                break;
            case GameState.Paused:
                context.FillRectangle(Brush("#660B2634"), new Rect(0, 0, 480, FlappyGame.GroundY));
                context.DrawRectangle(Card, null, new Rect(44, 220, 392, 152), 24, 24);
                Text(context, "PAUSED", 240, 249, 32, Cream, FontWeight.Bold, true);
                Text(context, "Press P or click to continue", 240, 313, 16, Muted, center: true);
                break;
        }
    }

    private void DrawLandscape(DrawingContext context)
    {
        context.FillRectangle(Sky, new Rect(0, 0, 480, 640));
        var drift = Game.Distance * 0.12;
        for (var i = 0; i < 4; i++)
        {
            var x = ((i * 170 + 40 - drift) % 680 + 680) % 680 - 100;
            var y = 78 + i % 2 * 82;
            context.DrawEllipse(Cloud, null, new Point(x, y), 43, 15);
            context.DrawEllipse(Cloud, null, new Point(x - 16, y - 12), 22, 22);
            context.DrawEllipse(Cloud, null, new Point(x + 16, y - 7), 28, 22);
        }
        for (var i = 0; i < 7; i++)
        {
            var x = i * 90 - 30;
            context.DrawEllipse(Brush("#ACDCC6"), null, new Point(x, 565), 98, 65 + i % 3 * 12);
            context.DrawEllipse(Mint, null, new Point(x + 35, 590), 74, 55);
        }
    }

    private static void DrawPipe(DrawingContext context, Pipe pipe)
    {
        var top = pipe.GapCenter - FlappyGame.GapHeight / 2;
        var bottom = pipe.GapCenter + FlappyGame.GapHeight / 2;
        PipeSection(context, pipe.X, 0, top, top - 22);
        PipeSection(context, pipe.X, bottom, FlappyGame.GroundY - bottom, bottom);
    }

    private static void PipeSection(DrawingContext context, double x, double y, double height, double capY)
    {
        context.DrawRectangle(Green, Outline, new Rect(x, y, FlappyGame.PipeWidth, height));
        context.FillRectangle(Mint, new Rect(x + 8, y + 2, 9, Math.Max(0, height - 4)));
        context.FillRectangle(DarkGreen, new Rect(x + 55, y + 2, 14, Math.Max(0, height - 4)));
        context.DrawRectangle(Green, Outline, new Rect(x, capY, FlappyGame.PipeWidth, 22), 3, 3);
        context.DrawLine(new Pen(Mint, 3), new Point(x + 7, capY + 7), new Point(x + 62, capY + 7));
    }

    private static void DrawBird(DrawingContext context, double x, double y, double angle, double scale = 1)
    {
        using var transform = context.PushTransform(Matrix.CreateRotation(angle)
            * Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(x, y));
        context.DrawEllipse(Orange, Outline, new Point(-11, 5), 8, 5);
        context.DrawEllipse(Yellow, Outline, new Point(0, 0), 15, 13);
        context.DrawEllipse(Orange, new Pen(Ink, 2), new Point(-6, 5), 7, 4);
        var beak = new StreamGeometry();
        using (var geometry = beak.Open())
        {
            geometry.BeginFigure(new Point(11, 0), true);
            geometry.LineTo(new Point(23, 4));
            geometry.LineTo(new Point(11, 8));
            geometry.EndFigure(true);
        }
        context.DrawGeometry(Orange, new Pen(Ink, 2), beak);
        context.DrawEllipse(Cream, new Pen(Ink, 2), new Point(7, -5), 5, 5);
        context.DrawEllipse(Ink, null, new Point(9, -5), 2, 2);
    }

    private void DrawHud(DrawingContext context)
    {
        context.DrawRectangle(Card, null, new Rect(18, 18, 102, 62), 16, 16);
        Text(context, "SCORE", 69, 26, 10, Muted, FontWeight.Bold, true);
        Text(context, Game.Score.ToString(), 69, 41, 26, Cream, FontWeight.Bold, true);
        context.DrawRectangle(Card, null, new Rect(342, 18, 120, 62), 16, 16);
        Text(context, "SESSION BEST", 402, 26, 10, Muted, FontWeight.Bold, true);
        Text(context, Game.BestScore.ToString(), 402, 41, 26, Yellow, FontWeight.Bold, true);
    }

    private void DrawReady(DrawingContext context)
    {
        Text(context, "DESKTOP EDITION", 240, 34, 12, Ink, FontWeight.Bold, true);
        Text(context, "FLAPPY", 240, 91, 62, Ink, FontWeight.Bold, true);
        Text(context, "BIRD", 240, 151, 62, Ink, FontWeight.Bold, true);
        DrawBird(context, 236, 266 + Math.Sin(clock.Elapsed.TotalSeconds * 3) * 7, -0.12, 2.3);
        context.DrawRectangle(Card, null, new Rect(44, 340, 392, 184), 24, 24);
        Text(context, "READY TO FLY?", 240, 363, 25, Cream, FontWeight.Bold, true);
        Text(context, "Dodge the pipes. Keep your wings moving.", 240, 402, 14, Muted, center: true);
        context.DrawRectangle(Yellow, null, new Rect(72, 443, 336, 52), 16, 16);
        Text(context, "SPACE / CLICK TO START", 240, 458, 17, Ink, FontWeight.Bold, true);
        Text(context, "One point for every pair of pipes.", 240, 543, 13, Ink, center: true);
    }

    private void DrawGameOver(DrawingContext context)
    {
        context.FillRectangle(Brush("#660B2634"), new Rect(0, 0, 480, FlappyGame.GroundY));
        context.DrawRectangle(Card, null, new Rect(44, 190, 392, 280), 24, 24);
        Text(context, "GAME OVER", 240, 216, 32, Cream, FontWeight.Bold, true);
        Text(context, "YOUR SCORE", 240, 270, 11, Muted, FontWeight.Bold, true);
        Text(context, Game.Score.ToString(), 240, 289, 62, Yellow, FontWeight.Bold, true);
        Text(context, $"Session best: {Game.BestScore}", 240, 363, 14, Muted, center: true);
        context.DrawRectangle(Yellow, null, new Rect(72, 406, 336, 44), 14, 14);
        Text(context, "SPACE / CLICK TO RETRY", 240, 418, 17, Ink, FontWeight.Bold, true);
    }

    private static IBrush Brush(string hex) => new SolidColorBrush(Color.Parse(hex));

    private static void Text(DrawingContext context, string text, double x, double y,
        double size, IBrush brush, FontWeight? weight = null, bool center = false)
    {
        var formatted = new FormattedText(text, CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight, new Typeface(new FontFamily("fonts:Inter#Inter"), FontStyle.Normal,
                weight ?? FontWeight.Normal), size, brush);
        context.DrawText(formatted, new Point(center ? x - formatted.Width / 2 : x, y));
    }
}
