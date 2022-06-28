#region

using System.Diagnostics;
using System.Threading;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Subsystems.Auditory;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.States;
using SDL2;

#endregion

namespace DuckDuckJump.Engine.Subsystems.Flow;

public static class GameFlow
{
    private const int FrameRate = 60;
    public const float TimeStep = 1.0f / FrameRate;
    private static IGameState _gameState;

    private static readonly object GameStateLock = new();

    public static void Run()
    {
        Graphics.Initialize();
        Audio.Initialize();

        Set(new MainMenuState());

        var running = true;

        var stopwatch = new Stopwatch();

        while (running)
        {
            stopwatch.Restart();

            while (SDL.SDL_PollEvent(out var sdlEvent) > 0)
            {
                if (sdlEvent.type == SDL.SDL_EventType.SDL_QUIT) running = false;
                if (_gameState == null) continue;
                lock (GameStateLock)
                {
                    _gameState?.OnEvent(ref sdlEvent);
                }
            }

            Keyboard.Update();

            if (_gameState != null)
                lock (GameStateLock)
                {
                    _gameState?.Update();
                    Graphics.Begin();
                    _gameState?.Draw();
                    Graphics.End();
                }

            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds < 16)
                Thread.Sleep((int) (16 - stopwatch.ElapsedMilliseconds));
        }

        Graphics.Quit();
        Audio.Quit();
    }

    public static void Set(IGameState state)
    {
        lock (GameStateLock)
        {
            _gameState?.Exit();

            _gameState = state;

            _gameState?.Initialize();
        }
    }
}