using System;
using System.Diagnostics;
using System.Drawing;
using DuckDuckJump.Engine;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Scenes;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics;
using DuckDuckJump.Game;
using DuckDuckJump.Game.Gameplay.Players.AI;
using DuckDuckJump.Scenes.Game;
using SDL2;

namespace DuckDuckJump.Scenes.MainMenu;

internal class MainMenuScene : Scene
{
    private static readonly MenuOptionType[] MainScreenSelections =
    {
        MenuOptionType.Empty,
        MenuOptionType.Title,
        MenuOptionType.Empty,
        MenuOptionType.Empty,
        MenuOptionType.Empty,
        MenuOptionType.Start,
        MenuOptionType.Replays,
        MenuOptionType.Leaderboard,
        MenuOptionType.Statistics,
        MenuOptionType.Settings,
        MenuOptionType.Quit
    };

    private static readonly int[] MainScreenSelectionsIndex =
    {
        5,
        9,
        10
    };

    private static readonly MenuOptionType[] QuitScreenSelections =
    {
        MenuOptionType.Empty,
        MenuOptionType.AreYouSure,
        MenuOptionType.Empty,
        MenuOptionType.Empty,
        MenuOptionType.Empty,
        MenuOptionType.Yes,
        MenuOptionType.No
    };

    private static readonly int[] QuitScreenSelectionsIndex =
    {
        5,
        6
    };

    private static readonly MenuOptionType[] ModeSelectScreenSelections =
    {
        MenuOptionType.Empty,
        MenuOptionType.ModeSelect,
        MenuOptionType.Empty,
        MenuOptionType.Empty,
        MenuOptionType.Empty,
        MenuOptionType.TimeAttack,
        MenuOptionType.Survival,
        MenuOptionType.SingleMatch,
        MenuOptionType.WatchMode,
        MenuOptionType.Empty,
        MenuOptionType.Back
    };

    private static readonly int[] ModeSelectScreenSelectionsIndex =
    {
        5,
        6,
        7,
        8,
        10
    };

    private readonly Renderer _renderer;
    private readonly MainMenuResources _resources;

    private readonly int[] _savedSelections = new int[Enum.GetValues(typeof(MainMenuState)).Length];

    private int _actualSelection;

    private bool _disposed;
    private int _oldRenderedSelection;
    private MenuOptionType[]? _renderedOptions;
    private int[]? _renderedOptionsIndex;

    private int _renderedSelection;
    private float _renderedSelectionFadeProgress;
    private MainMenuState _state;

    public MainMenuScene()
    {
        _resources = new MainMenuResources(ResourceManager);

        Debug.Assert(GameContext.Instance != null, "Engine.GameContext.Instance != null");
        _renderer = GameContext.Instance.Renderer;

        SetState(MainMenuState.MainScreen);
    }

    private void SetRenderedSelection(int newSelection)
    {
        if (newSelection == _renderedSelection)
            return;
        _oldRenderedSelection = _renderedSelection;
        _renderedSelection = newSelection;
        _renderedSelectionFadeProgress = 0.0f;
    }

    private void SetState(MainMenuState newState)
    {
        switch (newState)
        {
            case MainMenuState.MainScreen:
                _renderedOptions = MainScreenSelections;
                _renderedOptionsIndex = MainScreenSelectionsIndex;
                break;
            case MainMenuState.QuitScreen:
                _renderedOptions = QuitScreenSelections;
                _renderedOptionsIndex = QuitScreenSelectionsIndex;
                break;
            case MainMenuState.SettingsScreen:
                break;
            case MainMenuState.ModeSelectionScreen:
                _renderedOptions = ModeSelectScreenSelections;
                _renderedOptionsIndex = ModeSelectScreenSelectionsIndex;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        _savedSelections[(int) _state] = _actualSelection;
        _state = newState;
        _actualSelection = _savedSelections[(int) newState];

        _renderedSelectionFadeProgress = 1.0f;
        if (_renderedOptionsIndex == null) return;
        _oldRenderedSelection = _renderedOptionsIndex[_actualSelection];
        _renderedSelection = _renderedOptionsIndex[_actualSelection];
    }

    private int CalculateY(int menuOption)
    {
        var y = 0;

        if (_renderedOptions == null) return y;

        var option = MainMenuResources.GetOption(_renderedOptions[menuOption]);

        for (var i = 1; i <= menuOption; i++) y += _resources.TextDrawer.MeasureText(option.Caption).Height;

        return y;
    }

    private Rectangle GetDestination(int menuOption)
    {
        if (_renderedOptions == null) return Rectangle.Empty;

        var menu = _renderedOptions[menuOption];

        var size = _resources.TextDrawer.MeasureText(MainMenuResources.GetOption(menu).Caption);

        var destination = new Rectangle(640 / 2 - size.Width / 2, CalculateY(menuOption), size.Width,
            size.Height);
        return destination;
    }

    private void DrawRectangle()
    {
        var prev = GetDestination(_oldRenderedSelection);
        var current = GetDestination(_renderedSelection);

        _renderer.BlendMode = SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND;
        _renderer.DrawColor = Color.FromArgb(128, Color.Blue.R, Color.Blue.G, Color.Blue.B);
        _renderer.FillRect(new Rectangle(
            (int) Mathematics.Lerp(prev.X, current.X, _renderedSelectionFadeProgress),
            (int) Mathematics.Lerp(prev.Y, current.Y, _renderedSelectionFadeProgress),
            (int) Mathematics.Lerp(prev.Width, current.Width, _renderedSelectionFadeProgress),
            (int) Mathematics.Lerp(prev.Height, current.Height, _renderedSelectionFadeProgress)));
    }

    private void DrawOptions()
    {
        if (_renderedOptions == null)
            return;
        for (var i = 0; i < _renderedOptions.Length; i++)
        {
            var option = MainMenuResources.GetOption(_renderedOptions[i]);
            var y = CalculateY(i);

            var measuredSize = _resources.TextDrawer.MeasureText(option.Caption);

            _resources.TextDrawer.DrawText(640 / 2 - measuredSize.Width / 2, y, option.Caption, option.Color);
        }
    }

    private void DrawScreen()
    {
        DrawRectangle();
        DrawOptions();
    }

    public override void OnTick()
    {
        Update();
        Draw();
    }

    private void Update()
    {
        _renderedSelectionFadeProgress = MathF.Min(1.0f, _renderedSelectionFadeProgress + 0.25f);
        if (_renderedOptionsIndex == null)
            return;
        if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_DOWN))
        {
            ++_actualSelection;

            if (_actualSelection == _renderedOptionsIndex.Length)
                _actualSelection = 0;
        }
        else if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_UP))
        {
            --_actualSelection;
            if (_actualSelection < 0) _actualSelection = _renderedOptionsIndex.Length - 1;
        }

        if (Keyboard.KeyDown(SDL.SDL_Scancode.SDL_SCANCODE_RETURN)) OnSelect();

        SetRenderedSelection(_renderedOptionsIndex[_actualSelection]);
    }

    private void OnSelect()
    {
        switch (_state)
        {
            case MainMenuState.MainScreen:
                switch (_actualSelection)
                {
                    case 0:
                        SetState(MainMenuState.ModeSelectionScreen);
                        break;
                    case 1:
                        break;
                    case 2:
                        SetState(MainMenuState.QuitScreen);
                        break;
                }

                break;
            case MainMenuState.QuitScreen:
                switch (_actualSelection)
                {
                    case 0:
                        if (GameContext.Instance != null) GameContext.Instance.Running = false;
                        break;
                    case 1:
                        SetState(MainMenuState.MainScreen);
                        break;
                }

                break;
            case MainMenuState.SettingsScreen:
                break;
            case MainMenuState.ModeSelectionScreen:
                switch (_actualSelection)
                {
                    case 0:
                        GameContext.Instance?.SceneManager.Set(new TimeAttackGameScene());
                        break;
                    case 1:
                        break;
                    case 2:
                        GameContext.Instance?.SceneManager.Set(new VersusMatchGameScene(new GameInfo(50, 1, false,
                            false,
                            Environment.TickCount, AiDifficulty.Easy, true)));
                        break;
                    case 3:
                        break;
                    case 4:
                        SetState(MainMenuState.MainScreen);
                        break;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void Draw()
    {
        _renderer.DrawColor = Color.Black;
        _renderer.Clear();

        if (!_disposed) DrawScreen();

        _renderer.Present();
    }

    protected override void OnUnload()
    {
        _resources.Dispose();
        _disposed = true;
    }
}