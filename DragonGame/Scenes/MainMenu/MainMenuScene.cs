using System;
using System.Diagnostics;
using System.Drawing;
using System.Net.Http;
using System.Net.Http.Json;
using System.Numerics;
using Common;
using DuckDuckJump.Engine;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Scenes;
using DuckDuckJump.Engine.Selector;
using DuckDuckJump.Engine.Settings;
using DuckDuckJump.Engine.Sprites;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Scenes.Game;
using SDL2;

namespace DuckDuckJump.Scenes.MainMenu;

internal class MainMenuScene : Scene
{
    private const int DiamondCount = 32;
    private const int Alpha = 255 / DiamondCount;

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
        new("SETTINGS", Color.White, true),
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

    private static readonly Selection[] SettingsSelections =
    {
        Selection.Nothing,
        new("SETTINGS", Color.Yellow),
        Selection.Nothing,
        new("AUDIO SETTINGS", Color.White, true),
        Selection.Nothing,
        new("BACK", Color.White, true)
    };

    private readonly Selection[] _audioSettingSelections =
    {
        Selection.Nothing,
        new("SETTINGS", Color.Yellow),
        Selection.Nothing,
        new("", Color.White, true),
        new("", Color.White, true),
        Selection.Nothing,
        new("BACK", Color.White, true)
    };

    private readonly SpriteDrawer _drawer = new(GameContext.Instance!.Renderer);

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

    private float _rectangleRotation;

    private MainMenuState _state;
    private TimeAttackScore[]? _timeAttackScores;
    private int _viewableStart;

    public MainMenuScene()
    {
        var music = ResourceManager.Musics["MainMenu/music"];
        _resources = new MainMenuResources(ResourceManager);

        Debug.Assert(GameContext.Instance != null, "Engine.GameContext.Instance != null");
        _renderer = GameContext.Instance.Renderer;
        _selector = new TextSelector(MainScreenSelections, _renderer, _resources.TextDrawer);

        SetState(MainMenuState.MainScreen);

        music.Play();

        RequestTimeAttackScores();
    }


    private void UpdateSoundSettingsSelections()
    {
        _audioSettingSelections[3] =
            new Selection($"MUSIC VOLUME: {SettingsLoader.Current.MusicVolume}", Color.White, true);
        _audioSettingSelections[4] =
            new Selection($"SFX VOLUME: {SettingsLoader.Current.SfxVolume}", Color.White, true);
        _selector.RecalculateSizes();
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
        catch (Exception)
        {
            _timeAttackLeaderboards[6] = new Selection("Connection Failure.", Color.White);
        }
        finally
        {
            _selector.RecalculateSizes();
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

    private void OnAudioSettingsSelect(int selection)
    {
        switch (selection)
        {
            case 3:
                break;
            case 4:
                break;
            case 6:
                SetState(MainMenuState.SettingsScreen);
                break;
        }
    }

    private void OnSettingsSelect(int selection)
    {
        switch (selection)
        {
            case 3:
                SetState(MainMenuState.AudioSettingsScreen);
                break;
            case 5:
                SettingsLoader.Save();
                SetState(MainMenuState.MainScreen);
                break;
        }
    }

    private void SetState(MainMenuState newState)
    {
        _state = newState;
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
                _selector.Selections = SettingsSelections;
                _selector.OnSelect = OnSettingsSelect;
                _selector.SetSelectionImmediate(3);
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
            case MainMenuState.AudioSettingsScreen:
                UpdateSoundSettingsSelections();
                _selector.Selections = _audioSettingSelections;
                _selector.OnSelect = OnAudioSettingsSelect;
                _selector.SetSelectionImmediate(3);
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
                GameContext.Instance?.SceneManager.Set(new TimeAttackGameScene());
                break;
            case 5:
                SetState(MainMenuState.LeaderboardSelectionScreen);
                break;
            case 8:
                SetState(MainMenuState.SettingsScreen);
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
                if (_timeAttackScores == null)
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
                if (_timeAttackScores == null)
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
        _rectangleRotation += 0.025f;
        _selector.Update();

        if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_DOWN))
            _selector.IncreaseSelection();
        else if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_UP)) _selector.DecreaseSelection();

        if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_RETURN))
        {
            _selector.Select();
            return true;
        }

        switch (_state)
        {
            case MainMenuState.MainScreen:
                break;
            case MainMenuState.QuitScreen:
                break;
            case MainMenuState.SettingsScreen:
                break;
            case MainMenuState.ModeSelectionScreen:
                break;
            case MainMenuState.LeaderboardSelectionScreen:
                break;
            case MainMenuState.TimeAttackLeaderboardScreen:
                break;
            case MainMenuState.AudioSettingsScreen:
                switch (_selector.Selection)
                {
                    case 3:
                        if (Keyboard.KeyHeld(SDL.SDL_Scancode.SDL_SCANCODE_RIGHT))
                        {
                            ++SettingsLoader.Current.MusicVolume;
                            UpdateSoundSettingsSelections();
                        }
                        else if (Keyboard.KeyHeld(SDL.SDL_Scancode.SDL_SCANCODE_LEFT))
                        {
                            --SettingsLoader.Current.MusicVolume;
                            UpdateSoundSettingsSelections();
                        }

                        break;
                    case 4:
                        if (Keyboard.KeyHeld(SDL.SDL_Scancode.SDL_SCANCODE_RIGHT))
                        {
                            ++SettingsLoader.Current.SfxVolume;
                            UpdateSoundSettingsSelections();
                        }
                        else if (Keyboard.KeyHeld(SDL.SDL_Scancode.SDL_SCANCODE_LEFT))
                        {
                            --SettingsLoader.Current.SfxVolume;
                            UpdateSoundSettingsSelections();
                        }

                        break;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return false;
    }

    private void DrawRectangles()
    {
        for (var i = 0; i < DiamondCount; i++)
        {
            _drawer.PushMatrix();

            var rotation = _rectangleRotation + i / 4.0f;

            if (i + 1 % 2 == 0) rotation = -rotation;

            var matrix =
                Matrix3x2.CreateScale(2.0f + MathF.Cos(rotation), MathF.Sin(rotation), new Vector2(64.0f, 64.0f)) *
                Matrix3x2.CreateRotation(rotation, new Vector2(64.0f, 64.0f)) *
                Matrix3x2.CreateTranslation(640.0f / 2.0f - 64.0f, 480.0f / 2.0f - 64.0f);
            _drawer.MultiplyMatrix(matrix);

            _drawer.Draw(ResourceManager.Textures["Main Menu/Diamond"], Color.FromArgb(Alpha, 0, 0, 255), null);
            _drawer.PopMatrix();
        }
    }

    private void Draw()
    {
        _renderer.DrawColor = Color.Black;
        _renderer.Clear();

        DrawRectangles();

        if (!_disposed) _selector.Draw();

        _renderer.Present();
    }

    protected override void OnUnload()
    {
        _resources.Dispose();
        _disposed = true;
    }
}