using System.Diagnostics;
using System.Drawing;
using DuckDuckJump.Engine;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Scenes;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Engine.Wrappers.SDL2.TTF;
using DuckDuckJump.Game;
using DuckDuckJump.Game.Gameplay.Resources;
using DuckDuckJump.Game.Input.InputHandlers;
using DuckDuckJump.Scenes.MainMenu;
using SDL2;

namespace DuckDuckJump.Scenes.Game;

internal class VersusMatchGameScene : Scene
{
    private readonly GameInfo _info;

    private readonly PhysicalInputHandler _inputHandler = new(
        new InputProfile(SDL.SDL_Scancode.SDL_SCANCODE_A, SDL.SDL_Scancode.SDL_SCANCODE_D,
            SDL.SDL_Scancode.SDL_SCANCODE_S, SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE),
        new InputProfile(SDL.SDL_Scancode.SDL_SCANCODE_J, SDL.SDL_Scancode.SDL_SCANCODE_L,
            SDL.SDL_Scancode.SDL_SCANCODE_L, SDL.SDL_Scancode.SDL_SCANCODE_P));

    private readonly Font _matchPointsFont;

    private readonly Renderer _renderer;
    private readonly GameplayResources _resources;

    private GameMatch _match;

    private bool _matchOver;

    private Texture? _matchPointsTexture;

    private byte _p1Score, _p2Score;

    public VersusMatchGameScene(GameInfo info)
    {
        Debug.Assert(GameContext.Instance != null, "Engine.Game.Instance != null");
        _renderer = GameContext.Instance.Renderer;
        _resources = new GameplayResources(ResourceManager);

        _info = info;
        _match = new GameMatch(_info, _resources);

        _matchPointsFont = ResourceManager.Fonts["VeraMono"];
        _matchPointsFont.Size = 64;
    }

    public override void OnTick()
    {
        if (!_matchOver)
        {
            if (_match.HasMatchEnded)
            {
                if (_match.Winner == Winner.P1)
                    ++_p1Score;
                else
                    ++_p2Score;

                using var surface = _matchPointsFont.RenderTextBlended($"{_p1Score} - {_p2Score}", Color.Azure);
                _matchPointsTexture?.Dispose();
                _matchPointsTexture = new Texture(_renderer, surface);
                _matchOver = true;
            }
            else
            {
                _match.Update(_inputHandler);
            }
        }
        else
        {
            if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_RETURN))
            {
                Restart();
            }
            else if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE))
            {
                GameContext.Instance?.SceneManager.Set(new MainMenuScene());
                return;
            }
        }

        _match.Draw();
        if (_matchOver)
        {
            _renderer.DrawColor = Color.FromArgb(128, 0, 0, 0);
            _renderer.FillRect(null);

            if (_matchPointsTexture != null)
            {
                var texInfo = _matchPointsTexture.QueryTexture();
                _renderer.Copy(_matchPointsTexture, null,
                    new Rectangle(640 / 2 - texInfo.Width / 2, 480 / 2 - texInfo.Height / 2, texInfo.Width,
                        texInfo.Height));
            }
        }

        _renderer.Present();
    }

    private void Restart()
    {
        _matchOver = false;
        _match = new GameMatch(_info, _resources);
    }

    protected override void OnUnload()
    {
        _matchPointsTexture?.Dispose();
    }
}