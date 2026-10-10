namespace TestDesktop.Game;

public enum GameState { Ready, Playing, Paused, GameOver }

public sealed class Pipe
{
    public double X { get; internal set; }
    public double GapCenter { get; internal set; }
    public bool Passed { get; internal set; }
}

/// <summary>Game rules in a fixed-size world, independent of the window and rendering.</summary>
public sealed class FlappyGame
{
    public const double Width = 480;
    public const double Height = 640;
    public const double GroundY = 576;
    public const double BirdX = 136;
    public const double BirdRadius = 14;
    public const double PipeWidth = 72;
    public const double GapHeight = 184;
    private const double Step = 1.0 / 120;
    private const double PipeSpacing = 248;
    private readonly Random random;
    private readonly List<Pipe> pipes = [];
    private double accumulator;

    public FlappyGame(int? seed = null)
    {
        random = seed.HasValue ? new Random(seed.Value) : new Random();
        Restart();
    }

    public GameState State { get; private set; }
    public double BirdY { get; internal set; }
    public double BirdVelocity { get; internal set; }
    public int Score { get; private set; }
    public int BestScore { get; private set; }
    public double Distance { get; private set; }
    public IReadOnlyList<Pipe> Pipes => pipes;

    public void Restart()
    {
        State = GameState.Ready;
        BirdY = 270;
        BirdVelocity = 0;
        Score = 0;
        Distance = 0;
        accumulator = 0;
        pipes.Clear();
        for (var i = 0; i < 3; i++)
            pipes.Add(new Pipe { X = Width + 80 + i * PipeSpacing, GapCenter = NextGap() });
    }

    public void Flap()
    {
        if (State == GameState.Paused)
            return;
        if (State is GameState.Ready or GameState.GameOver)
        {
            Restart();
            State = GameState.Playing;
        }
        BirdVelocity = -330;
    }

    public void TogglePause()
    {
        if (State == GameState.Playing)
            State = GameState.Paused;
        else if (State == GameState.Paused)
            State = GameState.Playing;
        accumulator = 0;
    }

    public void Update(double seconds)
    {
        if (State != GameState.Playing || !double.IsFinite(seconds) || seconds <= 0)
            return;

        // Cap catch-up after a stalled window; small fixed steps prevent tunnelling.
        accumulator += Math.Min(seconds, 0.1);
        while (accumulator >= Step && State == GameState.Playing)
        {
            accumulator -= Step;
            Tick();
        }
    }

    private void Tick()
    {
        BirdVelocity += 950 * Step;
        BirdY += BirdVelocity * Step;
        var movement = 160 * Step;
        Distance += movement;
        foreach (var pipe in pipes)
            pipe.X -= movement;

        if (BirdY - BirdRadius <= 0 || BirdY + BirdRadius >= GroundY)
        {
            BirdY = Math.Clamp(BirdY, BirdRadius, GroundY - BirdRadius);
            EndGame();
            return;
        }

        foreach (var pipe in pipes)
        {
            var top = pipe.GapCenter - GapHeight / 2;
            var bottom = pipe.GapCenter + GapHeight / 2;
            if (HitsRectangle(pipe.X, 0, PipeWidth, top)
                || HitsRectangle(pipe.X, bottom, PipeWidth, GroundY - bottom))
            {
                EndGame();
                return;
            }
        }

        foreach (var pipe in pipes)
        {
            if (!pipe.Passed && pipe.X + PipeWidth < BirdX - BirdRadius)
            {
                pipe.Passed = true;
                Score++;
                BestScore = Math.Max(BestScore, Score);
            }
            if (pipe.X + PipeWidth < 0)
            {
                pipe.X = pipes.Max(p => p.X) + PipeSpacing;
                pipe.GapCenter = NextGap();
                pipe.Passed = false;
            }
        }
    }

    private bool HitsRectangle(double x, double y, double width, double height)
    {
        var dx = BirdX - Math.Clamp(BirdX, x, x + width);
        var dy = BirdY - Math.Clamp(BirdY, y, y + height);
        return dx * dx + dy * dy <= BirdRadius * BirdRadius;
    }

    private double NextGap() => random.Next(164, 413);

    private void EndGame()
    {
        State = GameState.GameOver;
        BirdVelocity = 0;
        accumulator = 0;
    }
}
