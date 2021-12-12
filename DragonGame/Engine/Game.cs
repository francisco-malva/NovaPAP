using System;
using DuckDuckJump.Engine.Assets.Audio;
using DuckDuckJump.Engine.Assets.Textures;
using DuckDuckJump.Engine.Events;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Scenes;
using DuckDuckJump.Engine.Wrappers.SDL2;
using DuckDuckJump.Scenes.MainMenu;
using SDL2;

namespace DuckDuckJump.Engine;

internal class Game : IDisposable
{
    private bool _running;
    public ChunkManager ChunkManager;

    public EventPump EventPump;
    public MusicManager MusicManager;
    public Renderer Renderer;
    public SceneManager SceneManager;

    public TextureManager TextureManager;
    public Window Window;

    public Game()
    {
        CreateSingleton();
        Initialize();
    }

    public static Game Instance { get; private set; }

    public void Dispose()
    {
        SceneManager?.Clear();
        Window?.Dispose();
        Renderer?.Dispose();
        TextureManager?.Dispose();
        ChunkManager?.Dispose();
        MusicManager?.Dispose();
    }

    private void CreateSingleton()
    {
        if (Instance != null) throw new Exception("Multiple instances of the Game class are now allowed!");
        Instance = this;
    }

    private void Initialize()
    {
        Window = new Window("Duck Duck Jump", SDL.SDL_WINDOWPOS_CENTERED, SDL.SDL_WINDOWPOS_CENTERED,
            640, 480, 0);
        Renderer = new Renderer(Window, -1,
            SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL.SDL_RendererFlags.SDL_RENDERER_TARGETTEXTURE);

        Renderer.SetLogicalSize(new Point(640, 480));

        EventPump = new EventPump();
        EventPump.Subscribe(SDL.SDL_EventType.SDL_QUIT, _ => Exit());

        TextureManager = new TextureManager();

        SDL_mixer.Mix_OpenAudio(44100, SDL_mixer.MIX_DEFAULT_FORMAT, 2, 1024);

        ChunkManager = new ChunkManager();
        MusicManager = new MusicManager();

        SceneManager = new SceneManager();
        SceneManager.Set(new MainMenuScene());
    }

    public void Run(uint fps)
    {
        var ms = (uint)(1.0f / fps * 1000.0f);
        _running = true;

        while (_running)
        {
            var start = SDL.SDL_GetTicks();

            EventPump.Dispatch();
            Keyboard.Update();

            SceneManager.Tick();

            var end = SDL.SDL_GetTicks();

            var delta = end - start;

            if (delta < ms) SDL.SDL_Delay(ms - delta);
        }
    }

    public void Exit()
    {
        _running = false;
    }
}