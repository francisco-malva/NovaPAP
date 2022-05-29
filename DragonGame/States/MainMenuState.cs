#region

using System;
using System.Drawing;
using System.Numerics;
using DuckDuckJump.Engine.Assets;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Selector;
using DuckDuckJump.Engine.Subsystems.Auditory;
using DuckDuckJump.Engine.Subsystems.Flow;
using DuckDuckJump.Engine.Subsystems.Graphical;
using DuckDuckJump.Engine.Wrappers.SDL2.Graphics.Textures;
using DuckDuckJump.Game;
using DuckDuckJump.Game.Assets;
using DuckDuckJump.Game.Configuration;
using DuckDuckJump.Game.GameWork.Banner;
using DuckDuckJump.Game.Input;
using DuckDuckJump.States.GameModes;
using SDL2;

#endregion

namespace DuckDuckJump.States;

public class MainMenuSelector : TextSelector
{
    private static readonly string[] ActionNames =
    {
        "Left",
        "Right",
        "Special"
    };

    private static readonly TimerType[] TimerTypes =
    {
        new(10, 10),
        new(30, 50),
        new(60, 65),
        new(99, 100)
    };

    private readonly TextInputData _nicknameInput = new()
        { Text = string.Empty, MaxLength = Settings.Nickname.MaxLength };

    private int _currentBoundInput;
    private bool _items = false;
    private float _musicVolume;

    private byte _setCount = 1;

    private float _sfxVolume;
    private State _state = State.Title;

    public MainMenuSelector(Font font) : base(font)
    {
        _musicVolume = Settings.MyData.MusicVolume * 100.0f;
        _sfxVolume = Settings.MyData.SfxVolume * 100.0f;

        if (!Settings.MyData.NicknameDefined) _state = State.NicknameSetting;
    }

    public override void Update()
    {
        base.Update();

        switch (_state)
        {
            case State.Title:
                UpdateTitle();
                break;
            case State.MainScreen:
                UpdateMainScreen();
                break;
            case State.Settings:
                UpdateSettings();
                break;
            case State.AudioSettings:
                UpdateAudioSettings();
                break;
            case State.ModeSelect:
                ModeSelection();
                break;
            case State.NicknameSetting:
                UpdateNickname();
                break;
            case State.InputSettings:
                UpdateInput();
                break;
            case State.Scoreboard:
                UpdateScoreboard();
                break;
            case State.MatchSettings:
                UpdateMatchSettings();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateMatchSettings()
    {
        Begin();

        Break(30.0f);
        Label("MATCH SETTINGS");
        Break(30.0f);

        if (Button("BACK"))
        {
        }
    }

    private void UpdateScoreboard()
    {
        Begin();

        if (Button("BACK"))
            _state = State.MainScreen;

        End();
    }

    private void UpdateInput()
    {
        Begin();

        Break(30.0f);
        Label("INPUT BINDING", Color.Gold);
        Break(30.0f);

        for (byte i = 0; i < Match.PlayerCount; i++)
        {
            Label($"P{i + 1}", Color.Gold);
            Break(30.0f);

            var offset = Settings.MyData.GetInputStartingOffset(i);

            for (var j = offset; j < offset + Settings.Data.InputProfileSize; j++)
                unsafe
                {
                    Label(
                        $"{ActionNames[j - offset]}: {SDL.SDL_GetScancodeName((SDL.SDL_Scancode)Settings.MyData.InputProfiles[j])}",
                        _currentBoundInput == j ? Color.White : Color.Gray);
                }
        }


        if (Keyboard.AnyDown(out var newBinding))
            unsafe
            {
                Settings.MyData.InputProfiles[_currentBoundInput] = (int)newBinding;
                ++_currentBoundInput;

                if (_currentBoundInput >= Settings.Data.InputProfileSize * Match.PlayerCount)
                {
                    Settings.Save();
                    _state = State.Settings;
                }
            }

        End();
    }


    private void UpdateTitle()
    {
        Begin();

        Break(30.0f);
        Label("DUCK DUCK JUMP", Color.Gold);
        Break(120.0f * 3.0f);

        if (Button("PRESS TO START"))
            _state = State.MainScreen;

        End();
    }

    private void UpdateAudioSettings()
    {
        Begin();

        Break(30.0f);
        Label("AUDIO SETTINGS", Color.Gold);
        Break(30.0f);

        if (Button("+"))
        {
            _musicVolume = Math.Clamp(_musicVolume + 5.0f, 0.0f, 100.0f);
            Settings.MyData.MusicVolume = _musicVolume / 100.0f;
        }

        Break(10.0f);

        Label($"MUSIC: {_musicVolume}%");

        Break(10.0f);
        if (Button("-"))
        {
            _musicVolume = Math.Clamp(_musicVolume - 5.0f, 0.0f, 100.0f);
            Settings.MyData.MusicVolume = _musicVolume / 100.0f;
        }

        if (Button("+"))
        {
            _sfxVolume = Math.Clamp(_sfxVolume + 5.0f, 0.0f, 100.0f);
            Settings.MyData.SfxVolume = _sfxVolume / 100.0f;
        }

        Break(10.0f);

        Label($"SFX: {_sfxVolume}%");

        Break(10.0f);
        if (Button("-"))
        {
            _sfxVolume = Math.Clamp(_sfxVolume - 5.0f, 0.0f, 100.0f);
            Settings.MyData.SfxVolume = _sfxVolume / 100.0f;
        }

        Break(20.0f);

        if (Button("BACK"))
        {
            Settings.Save();
            _state = State.Settings;
        }

        End();
    }

    private void UpdateSettings()
    {
        Begin();

        Break(30.0f);
        Label("SETTINGS", Color.Gold);
        Break(30.0f);

        if (Button("AUDIO")) _state = State.AudioSettings;
        if (Button("INPUT"))
        {
            _currentBoundInput = 0;
            _state = State.InputSettings;
        }

        Break(10.0f);

        if (Button("BACK")) _state = State.MainScreen;
        End();
    }

    private void ModeSelection()
    {
        Begin();

        Break(30.0f);
        Label("MODE SELECTION", Color.Gold);
        Break(80.0f);

        if (Button("TIME ATTACK")) GameFlow.Set(new TimeAttackMode());


        if (Button("VS CPU")) GameFlow.Set(new VersusMode());

        if (Button("VS PLAYER"))
        {
        }

        if (Button("WATCH")) GameFlow.Set(new WatchMode());

        Break(20.0f);

        if (Button("BACK")) _state = State.MainScreen;
        End();
    }

    private void UpdateNickname()
    {
        Begin();
        Break(30.0f);
        Label("NICKNAME SETUP", Color.Gold);
        Break(30.0f);

        Text(_nicknameInput);

        if (Button("CONFIRM") && _nicknameInput.Text.Length > 3)
        {
            Settings.MyData.NicknameDefined = true;

            for (var i = 0; i < _nicknameInput.Text.Length; i++)
                unsafe
                {
                    var character = i > _nicknameInput.Text.Length - 1 ? char.MinValue : _nicknameInput.Text[i];
                    Settings.MyData.Nickname.Characters[i] = character;
                    Settings.MyData.Nickname.Length = (byte)_nicknameInput.Text.Length;
                }

            _nicknameInput.Text = string.Empty;
            Settings.Save();
            _state = State.Title;
        }

        End();
    }

    private void UpdateMainScreen()
    {
        Begin();
        Break(120.0f * 1.5f);
        if (Button("BEGIN")) _state = State.ModeSelect;
        if (Button("SCOREBOARD")) _state = State.Scoreboard;
        if (Button("SETTINGS")) _state = State.Settings;
        var quitEv = new SDL.SDL_Event
        {
            type = SDL.SDL_EventType.SDL_QUIT
        };

        if (Button("QUIT")) SDL.SDL_PushEvent(ref quitEv);
        End();
    }

    private struct TimerType
    {
        public sbyte Seconds;
        public short PlatformCount;

        public TimerType(sbyte seconds, short platformCount)
        {
            Seconds = seconds;
            PlatformCount = platformCount;
        }
    }

    private enum State
    {
        Title,
        MainScreen,
        Settings,
        ModeSelect,
        NicknameSetting,
        AudioSettings,
        InputSettings,
        Scoreboard,
        MatchSettings
    }
}

public class MainMenuState : IGameState
{
    private Font _font;

    private AudioClip _music;
    private MainMenuSelector _selector;

    public void Initialize()
    {
        _font = new Font("public-pixel-30", 30);
        _selector = new MainMenuSelector(_font);
        _music = new AudioClip("menu", true);

        Audio.PlayMusic(_music);

        MatchAssets.Load();
        Match.Initialize(new GameInfo(new ComLevels(8, 8), 100, Environment.TickCount, 0, ushort.MaxValue,
            BannerWork.MessageIndex.NoBanner, GameInfo.Flags.Exhibition | GameInfo.Flags.NoItems));
    }

    public void Exit()
    {
        _music.Dispose();
        _font.Dispose();
    }

    public void OnEvent(ref SDL.SDL_Event sdlEvent)
    {
        _selector.OnEvent(ref sdlEvent);
    }

    public void Update()
    {
        Span<GameInput> inputs = stackalloc GameInput[Match.PlayerCount];
        Match.Update(inputs);
        _selector.Update();
    }

    public void Draw()
    {
        Match.Draw();
        Graphics.Draw(Texture.White, null,
            Matrix3x2.CreateScale(Graphics.LogicalSize.Width, Graphics.LogicalSize.Height),
            Color.FromArgb(220, 0, 0, 0));
        _selector.Draw();
    }
}