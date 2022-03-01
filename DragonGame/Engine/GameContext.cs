using System;
using System.Drawing;
using DuckDuckJump.Engine.Events;
using DuckDuckJump.Engine.Exceptions;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Scenes;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Scenes.MainMenu;
using SDL2;

namespace DuckDuckJump.Engine;

internal class GameContext : IDisposable
{
    public static GameContext? Instance;
    private readonly EventPump _eventPump;
    private readonly Window _window;
    public readonly Renderer Renderer;

    public readonly SceneManager SceneManager;
    private bool _running;

    public GameContext()
    {
        if (Instance != null) throw new Exception("Multiple instances of the Game class are now allowed!");
        Instance = this;

        if (SDL_ttf.TTF_Init() != 0)
            throw new Exception($"Could not initialize SDL TTF. SDL TTF error: {SDL_ttf.TTF_GetError()}");

        _window = new Window("Duck Duck Jump", SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED,
            640, 480, 0);
        Renderer = new Renderer(_window, -1,
            SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE);

        Renderer.SetLogicalSize(new Size(640, 480));

        _eventPump = new EventPump();
        _eventPump.Subscribe(SDL.SDL_EventType.SDL_QUIT, _ => Exit());

        if (SDL_mixer.Mix_OpenAudio(44100, SDL_mixer.MIX_DEFAULT_FORMAT, 2, 1024) != 0)
            throw new GameInitException($"Could not open audio. SDL Mixer Error: {SDL_mixer.Mix_GetError()}");

        SceneManager = new SceneManager();
        SceneManager.Set(new MainMenuScene());
    }

    public void Dispose()
    {
        SceneManager.Clear();
        _window.Dispose();
        Renderer.Dispose();

        SDL_ttf.TTF_Quit();
    }

    public void Run(uint fps)
    {
        var ms = (uint) (1.0 / fps * 1000.0);
        _running = true;

        while (_running)
        {
            var start = SDL.SDL_GetTicks();

            _eventPump.HandleEvents();
            Keyboard.Update();

            SceneManager.Tick();

            var end = SDL.SDL_GetTicks();

            var delta = end - start;

            if (delta < ms) SDL.SDL_Delay(ms - delta);
        }
    }

    private void Exit()
    {
        _running = false;
    }
}