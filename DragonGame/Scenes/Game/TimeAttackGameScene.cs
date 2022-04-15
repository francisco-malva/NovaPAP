using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Common;
using DuckDuckJump.Engine;
using DuckDuckJump.Engine.Scenes;
using DuckDuckJump.Engine.Text;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Engine.Wrappers.SDL2.Mixer;
using DuckDuckJump.Game;
using DuckDuckJump.Game.Gameplay.Players.AI;
using DuckDuckJump.Game.Gameplay.Resources;
using DuckDuckJump.Game.Input.InputHandlers;
using DuckDuckJump.Scenes.MainMenu;
using SDL2;

namespace DuckDuckJump.Scenes.Game;

internal class TimeAttackGameScene : Scene
{
    private const int LevelCount = 8;

    private readonly PhysicalInputHandler _inputHandler = new(
        new InputProfile(SDL.SDL_Scancode.SDL_SCANCODE_A, SDL.SDL_Scancode.SDL_SCANCODE_D,
            SDL.SDL_Scancode.SDL_SCANCODE_S, SDL.SDL_Scancode.SDL_SCANCODE_ESCAPE),
        new InputProfile(SDL.SDL_Scancode.SDL_SCANCODE_J, SDL.SDL_Scancode.SDL_SCANCODE_L,
            SDL.SDL_Scancode.SDL_SCANCODE_L, SDL.SDL_Scancode.SDL_SCANCODE_P));

    private readonly Renderer _renderer = GameContext.Instance!.Renderer;

    private readonly GameplayResources _resources;

    private readonly TextDrawer _smallDrawer;
    private int _currentLevel = -1;

    private GameMatch? _match;

    private long _timeTaken;

    public TimeAttackGameScene()
    {
        Debug.Assert(GameContext.Instance != null, "GameContext.Instance != null");
        _smallDrawer = new TextDrawer(ResourceManager.Fonts["PublicPixel-0W6DP"], GameContext.Instance.Renderer, 10,
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ 0123456789'");
        _resources = new GameplayResources(ResourceManager);
        AdvanceLevel();

        var music = ResourceManager.Musics["Game/music"];
        music.Play();
    }

    private void AdvanceLevel()
    {
        ++_currentLevel;
        Restart();
    }

    private void Restart()
    {
        var normalizedLevels = _currentLevel / (double)LevelCount;
        var difficulty = Mathematics.Lerp((int)AiDifficulty.Easy, (int)AiDifficulty.Nightmare,
            (float)normalizedLevels);
        var platformCount = Mathematics.Lerp(50, 100,
            (float)normalizedLevels);
        _match = new GameMatch(
            new GameInfo((ushort)platformCount, 1, false, true, Environment.TickCount, (AiDifficulty)(int)difficulty,
                true),
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

        var stageText = _currentLevel + 1 == LevelCount ? "FINAL STAGE" : $"STAGE {_currentLevel + 1}";

        var seconds = _timeTaken / 60;
        var minutes = seconds / 60;
        var timeText = $"{minutes}'{seconds % 60}";

        var stageTextMeasurements = _smallDrawer.MeasureText(stageText);
        var timeTextMeasurements = _smallDrawer.MeasureText(timeText);

        _smallDrawer.DrawText(640 / 2 - stageTextMeasurements.Width / 2, 0, stageText, Color.White);
        _smallDrawer.DrawText(640 / 2 - timeTextMeasurements.Width / 2, 480 - timeTextMeasurements.Height, timeText,
            Color.White);
        _renderer.Present();
    }

    protected override void OnUnload()
    {
        Music.Halt();
        _smallDrawer.Dispose();
    }
}