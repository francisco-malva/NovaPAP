#region

using System;
using System.Diagnostics;
using System.IO;
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

    private static readonly object GameStateLock = new();

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
                if (_gameState == null) continue;
                lock (GameStateLock)
                {
                    _gameState?.OnEvent(ref sdlEvent);
                }
            }

            Keyboard.Update();

            var screenshot = Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_P);
            
            if (_gameState != null)
                lock (GameStateLock)
                {
                    _gameState?.Update();
                    Graphics.Begin();
                    _gameState?.Draw();
                    Graphics.End();

                    if (screenshot)
                    {
                        TakeScreenshot();
                    }
                }

            stopwatch.Stop();

            if (stopwatch.ElapsedMilliseconds < 16)
                Thread.Sleep((int) (16 - stopwatch.ElapsedMilliseconds));
        }

        FileSystem.Quit();
        Graphics.Quit();
        Audio.Quit();
    }

    private static unsafe void TakeScreenshot()
    {
        var surface =
            (SDL.SDL_Surface*) SDL.SDL_CreateRGBSurface(0, 640, 480, 32,
                0x00ff0000, 0x0000ff00, 0x000000ff, 0xff000000);

        var rect = new SDL.SDL_Rect() {x = 0, y = 0, w = 640, h = 480};
        if (SDL.SDL_RenderReadPixels(Graphics.Renderer, ref rect, SDL.SDL_PIXELFORMAT_ARGB8888, surface->pixels,
                surface->pitch) == 0)
        {
            if (!Directory.Exists("screenshots"))
            {
                Directory.CreateDirectory("screenshots");
            }

            SDL.SDL_SaveBMP((IntPtr) surface, $"screenshots/{Directory.GetFiles("screenshots").Length + 1}.bmp");
        }
        
        SDL.SDL_FreeSurface((IntPtr) surface);
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