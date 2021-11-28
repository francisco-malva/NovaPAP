using System;
using DragonGame.Engine.Assets.Audio;
using DragonGame.Engine.Assets.Textures;
using DragonGame.Engine.Events;
using DragonGame.Engine.Scenes;
using DragonGame.Engine.Wrappers.SDL2;
using DragonGame.Scenes.MainMenu;
using SDL2;

namespace DragonGame.Engine
{
    internal class Game : IDisposable
    {
        private bool _running;

        public AudioManager AudioManager;

        public EventPump EventPump;
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
            AudioManager?.Dispose();
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

            EventPump = new EventPump();
            EventPump.Subscribe(SDL.SDL_EventType.SDL_QUIT, _ => Exit());

            TextureManager = new TextureManager();

            AudioManager = new AudioManager();

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
}