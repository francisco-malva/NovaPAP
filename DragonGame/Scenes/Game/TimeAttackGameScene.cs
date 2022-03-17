using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Common;
using DuckDuckJump.Engine;
using DuckDuckJump.Engine.Scenes;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Game;
using DuckDuckJump.Game.Gameplay.Players.AI;
using DuckDuckJump.Game.Gameplay.Resources;
using DuckDuckJump.Game.Input.InputHandlers;
using DuckDuckJump.Scenes.MainMenu;
using SDL2;

namespace DuckDuckJump.Scenes.Game;

internal class TimeAttackGameScene : Scene
{
    private const int LevelCount = 1;

    private readonly PhysicalInputHandler _inputHandler = new(
        new InputProfile(SDL.SDL_Scancode.SDL_SCANCODE_A, SDL.SDL_Scancode.SDL_SCANCODE_D,
            SDL.SDL_Scancode.SDL_SCANCODE_S, SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE),
        new InputProfile(SDL.SDL_Scancode.SDL_SCANCODE_J, SDL.SDL_Scancode.SDL_SCANCODE_L,
            SDL.SDL_Scancode.SDL_SCANCODE_L, SDL.SDL_Scancode.SDL_SCANCODE_P));

    private readonly Renderer _renderer = GameContext.Instance!.Renderer;

    private readonly GameplayResources _resources;
    private int _currentLevel = -1;

    private GameMatch? _match;
    private long _timeTaken;

    public TimeAttackGameScene()
    {
        _resources = new GameplayResources(ResourceManager);
        AdvanceLevel();
    }

    private void AdvanceLevel()
    {
        ++_currentLevel;
        Restart();
    }

    private void Restart()
    {
        _match = new GameMatch(new GameInfo(5, 1, false, true, Environment.TickCount, AiDifficulty.Normal, true),
            _resources);
    }

    public override void OnTick()
    {
        if (_match == null)
            return;
        if (_match.HasMatchEnded)
        {
            if (_match.Winner == Winner.P1)
            {
                if (_currentLevel == LevelCount - 1)
                {
                    var client = new HttpClient();
                    client.BaseAddress = new Uri("http://localhost:8000/");

                    client.PostAsync("http://localhost:8000/time-attack",
                        JsonContent.Create(new TimeAttackScore("Test", _timeTaken),
                            new MediaTypeHeaderValue("application/json")));

                    GameContext.Instance?.SceneManager.Set(new MainMenuScene());
                    return;
                }

                AdvanceLevel();
            }
            else
            {
                Restart();
            }
        }
        else
        {
            _match.Update(_inputHandler);

            if (_match.MatchInCourse) ++_timeTaken;
        }

        _match.Draw();
        _renderer.Present();
    }

    protected override void OnUnload()
    {
        _resources.Dispose();
    }
}