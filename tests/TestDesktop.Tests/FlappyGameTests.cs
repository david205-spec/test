using TestDesktop.Game;
using Xunit;

namespace TestDesktop.Tests;

public class FlappyGameTests
{
    [Fact]
    public void ReadyGameDoesNotMoveUntilFirstFlap()
    {
        var game = new FlappyGame(42);
        var initialY = game.BirdY;
        game.Update(0.1);
        Assert.Equal(initialY, game.BirdY);
        game.Flap();
        game.Update(1.0 / 60);
        Assert.Equal(GameState.Playing, game.State);
        Assert.True(game.BirdY < initialY);
    }

    [Fact]
    public void FallingIntoTheGroundEndsTheRun()
    {
        var game = new FlappyGame(42);
        game.Flap();
        for (var i = 0; i < 120 && game.State == GameState.Playing; i++)
            game.Update(1.0 / 60);
        Assert.Equal(GameState.GameOver, game.State);
        Assert.Equal(FlappyGame.GroundY - FlappyGame.BirdRadius, game.BirdY);
    }

    [Fact]
    public void HittingTheCeilingEndsTheRun()
    {
        var game = new FlappyGame(42);
        game.Flap();
        game.BirdY = FlappyGame.BirdRadius + 1;
        game.Update(1.0 / 60);
        Assert.Equal(GameState.GameOver, game.State);
    }

    [Theory]
    [InlineData(180, GameState.GameOver)]
    [InlineData(300, GameState.Playing)]
    [InlineData(420, GameState.GameOver)]
    public void BirdMustPassThroughThePipeGap(double birdY, GameState expected)
    {
        var game = new FlappyGame(42);
        game.Flap();
        game.BirdY = birdY;
        game.BirdVelocity = 0;
        game.Pipes[0].X = FlappyGame.BirdX;
        game.Pipes[0].GapCenter = 300;
        game.Update(1.0 / 60);
        Assert.Equal(expected, game.State);
    }

    [Fact]
    public void PassingOnePipeScoresExactlyOnceAndBestSurvivesRestart()
    {
        var game = new FlappyGame(42);
        game.Flap();
        game.Pipes[0].X = FlappyGame.BirdX - FlappyGame.PipeWidth - FlappyGame.BirdRadius - 1;
        game.Update(1.0 / 60);
        Assert.Equal(1, game.Score);
        game.Update(1.0 / 60);
        Assert.Equal(1, game.Score);
        game.Restart();
        Assert.Equal(0, game.Score);
        Assert.Equal(1, game.BestScore);
        Assert.Equal(GameState.Ready, game.State);
        Assert.All(game.Pipes, pipe => Assert.False(pipe.Passed));
    }

    [Fact]
    public void PauseFreezesBirdAndPipesAndResumeContinues()
    {
        var game = new FlappyGame(42);
        game.Flap();
        game.TogglePause();
        var y = game.BirdY;
        var pipeX = game.Pipes[0].X;
        game.Update(0.1);
        game.Flap();
        Assert.Equal(y, game.BirdY);
        Assert.Equal(pipeX, game.Pipes[0].X);
        Assert.Equal(GameState.Paused, game.State);
        game.TogglePause();
        game.Update(1.0 / 60);
        Assert.True(game.Pipes[0].X < pipeX);
    }

    [Fact]
    public void FlapAfterGameOverStartsANewRun()
    {
        var game = new FlappyGame(42);
        game.Flap();
        game.BirdY = FlappyGame.GroundY;
        game.Update(1.0 / 60);
        Assert.Equal(GameState.GameOver, game.State);
        game.Flap();
        Assert.Equal(GameState.Playing, game.State);
        Assert.Equal(0, game.Score);
        Assert.True(game.BirdY < FlappyGame.GroundY / 2);
        Assert.True(game.BirdVelocity < 0);
    }

    [Fact]
    public void OffscreenPipesAreRecycledBeyondTheLastPipe()
    {
        var game = new FlappyGame(42);
        game.Flap();
        var lastX = game.Pipes.Max(pipe => pipe.X);
        game.Pipes[0].X = -FlappyGame.PipeWidth - 1;
        game.Update(1.0 / 60);
        Assert.True(game.Pipes[0].X > lastX);
        Assert.False(game.Pipes[0].Passed);
        Assert.InRange(game.Pipes[0].GapCenter, 164, 412);
    }

    [Fact]
    public void FixedTimeStepsGiveTheSamePhysicsAtDifferentFrameRates()
    {
        var fast = new FlappyGame(42);
        var slow = new FlappyGame(42);
        fast.Flap();
        slow.Flap();
        for (var i = 0; i < 30; i++) fast.Update(1.0 / 120);
        for (var i = 0; i < 15; i++) slow.Update(1.0 / 60);
        Assert.Equal(fast.BirdY, slow.BirdY, 6);
        Assert.Equal(fast.Pipes[0].X, slow.Pipes[0].X, 6);
    }
}
