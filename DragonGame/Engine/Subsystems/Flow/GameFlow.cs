#region

using System.Diagnostics;
using System.Threading;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Subsystems.Auditory;
using DuckDuckJump.Engine.Subsystems.Files;
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

    public static void Run()
    {
        FileSystem.Initialize();
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
                _gameState?.OnEvent(ref sdlEvent);
            }

            Keyboard.Update();

            _gameState?.Update();
            Graphics.Begin();
            _gameState?.Draw();
            Graphics.End();

            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds < 16)
                Thread.Sleep((int) (16 - stopwatch.ElapsedMilliseconds));
        }

        FileSystem.Quit();
        Graphics.Quit();
        Audio.Quit();
    }

    public static void Set(IGameState state)
    {
        _gameState?.Exit();

        _gameState = state;

        _gameState?.Initialize();
    }
}