using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using Common;
using DuckDuckJump.Engine;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Scenes;
using DuckDuckJump.Engine.Selector;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using SDL2;

namespace DuckDuckJump.Scenes.MainMenu;

internal class MainMenuScene : Scene
{
    private static readonly Selection[] MainScreenSelections =
    {
        Selection.Nothing,
        new("DUCK DUCK JUMP!", Color.Yellow),
        Selection.Nothing,
        Selection.Nothing,
        new("SINGLE PLAYER", Color.White, true),
        new("LEADERBOARDS", Color.White, true),
        Selection.Nothing,
        Selection.Nothing,
        new("OPTIONS", Color.White, true),
        new("QUIT", Color.White, true)
    };

    private static readonly Selection[] QuitScreenSelections =
    {
        Selection.Nothing,
        new("ARE YOU SURE?", Color.Red),
        Selection.Nothing,
        Selection.Nothing,
        Selection.Nothing,
        new("YES!", Color.White, true),
        new("NO...", Color.White, true)
    };

    private static readonly Selection[] LeaderboardSelections =
    {
        Selection.Nothing,
        new("LEADERBOARDS", Color.Yellow),
        Selection.Nothing,
        Selection.Nothing,
        Selection.Nothing,
        new("TIME ATTACK", Color.White, true),
        new("SURVIVAL", Color.White, true),
        Selection.Nothing,
        new("BACK", Color.White, true)
    };

    private readonly Renderer _renderer;
    private readonly MainMenuResources _resources;
    private readonly TextSelector _selector;

    private readonly Selection[] _timeAttackLeaderboards =
    {
        new("TIME ATTACK", Color.Yellow),
        Selection.Nothing,
        new("UP", Color.White, true),
        Selection.Nothing,
        Selection.Nothing,
        Selection.Nothing,
        new("FETCHING...", Color.White),
        Selection.Nothing,
        Selection.Nothing,
        Selection.Nothing,
        new("DOWN", Color.White, true),
        Selection.Nothing,
        new("BACK", Color.White, true)
    };

    private bool _disposed;

    private bool _hasScores;
    private TimeAttackScore[]? _timeAttackScores;
    private int _viewableStart;

    public MainMenuScene()
    {
        _resources = new MainMenuResources(ResourceManager);

        Debug.Assert(GameContext.Instance != null, "Engine.GameContext.Instance != null");
        _renderer = GameContext.Instance.Renderer;
        _selector = new TextSelector(MainScreenSelections, _renderer, _resources.TextDrawer);

        SetState(MainMenuState.MainScreen);

        RequestTimeAttackScores();
    }

    private async void RequestTimeAttackScores()
    {
        using var client = new HttpClient();
        try
        {
            var result = await client.GetAsync("http://localhost:8000/time-attack");
            var scores = await result.Content.ReadFromJsonAsync<TimeAttackScore[]>();
            _hasScores = true;
            _timeAttackScores = scores;

            UpdateScores();
        }
        catch (Exception e)
        {
            _timeAttackLeaderboards[6] = new Selection("Connection Failure.", Color.White);
        }
    }


    private void UpdateScores()
    {
        if (!_hasScores || _timeAttackScores == null)
            return;

        var displayIndex = 4;
        for (var i = _viewableStart; i < _viewableStart + 5; i++)
        {
            string display;

            if (i < 0 || i >= _timeAttackScores.Length)
            {
                display = "------";
            }
            else
            {
                var score = _timeAttackScores[i];
                display = $"{i + 1}. {score.Name} {score.TimeTaken}";
            }

            _timeAttackLeaderboards[displayIndex] = new Selection(display, Color.White);
            ++displayIndex;
        }

        _selector.RecalculateSizes();
    }

    private void SetState(MainMenuState newState)
    {
        switch (newState)
        {
            case MainMenuState.MainScreen:
                _selector.Selections = MainScreenSelections;
                _selector.OnSelect = OnMainSelect;
                _selector.SetSelectionImmediate(4);
                break;
            case MainMenuState.QuitScreen:
                _selector.Selections = QuitScreenSelections;
                _selector.OnSelect = OnQuitSelect;
                _selector.SetSelectionImmediate(5);
                break;
            case MainMenuState.SettingsScreen:
                break;
            case MainMenuState.LeaderboardSelectionScreen:
                _selector.Selections = LeaderboardSelections;
                _selector.OnSelect = OnLeaderboardSelect;
                _selector.SetSelectionImmediate(5);
                break;
            case MainMenuState.ModeSelectionScreen:
                break;
            case MainMenuState.TimeAttackLeaderboardScreen:
                _viewableStart = 0;
                UpdateScores();
                _selector.Selections = _timeAttackLeaderboards;
                _selector.OnSelect = OnTimeAttackLeaderboardSelect;
                _selector.SetSelectionImmediate(_timeAttackLeaderboards.Length - 1);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState));
        }
    }

    private void OnQuitSelect(int selection)
    {
        switch (selection)
        {
            case 5:
                if (GameContext.Instance != null) GameContext.Instance.Running = false;
                break;
            case 6:
                SetState(MainMenuState.MainScreen);
                break;
        }
    }

    private void OnMainSelect(int selection)
    {
        switch (selection)
        {
            case 4:
                SetState(MainMenuState.ModeSelectionScreen);
                break;
            case 5:
                SetState(MainMenuState.LeaderboardSelectionScreen);
                break;
            case 8:
                break;
            case 9:
                SetState(MainMenuState.QuitScreen);
                break;
        }
    }

    private void OnTimeAttackLeaderboardSelect(int selection)
    {
        switch (selection)
        {
            case 2:
            {
                if(_timeAttackScores == null)
                    return;
                var intendedView = _viewableStart - 5;

                if (intendedView >= 0)
                {
                    _viewableStart = intendedView;
                    UpdateScores();
                }
            }
                break;
            case 10:
            {
                if(_timeAttackScores == null)
                    return;
                var intendedView = _viewableStart + 5;

                if (intendedView < _timeAttackScores.Length)
                {
                    _viewableStart = intendedView;
                    UpdateScores();
                }
            }
                break;
            case 12:
                SetState(MainMenuState.LeaderboardSelectionScreen);
                break;
        }
    }

    private void OnLeaderboardSelect(int selection)
    {
        switch (selection)
        {
            case 5:
                SetState(MainMenuState.TimeAttackLeaderboardScreen);
                break;
            case 6:
                break;
            case 8:
                SetState(MainMenuState.MainScreen);
                break;
        }
    }

    public override void OnTick()
    {
        if (!Update()) Draw();
    }

    private bool Update()
    {
        _selector.Update();

        if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_DOWN))
            _selector.IncreaseSelection();
        else if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_UP)) _selector.DecreaseSelection();

        if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_RETURN))
        {
            _selector.Select();
            return true;
        }

        return false;
    }

    private void Draw()
    {
        _renderer.DrawColor = Color.Black;
        _renderer.Clear();

        if (!_disposed) _selector.Draw();

        _renderer.Present();
    }

    protected override void OnUnload()
    {
        _resources.Dispose();
        _disposed = true;
    }
}