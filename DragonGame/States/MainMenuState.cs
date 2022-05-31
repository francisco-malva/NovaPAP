#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Common.Parsers;
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
using DuckDuckJump.States.GameModes.NetworkMode;
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

    private static readonly string[] DifficultyCaptions =
    {
        "Human",
        "COM 1",
        "COM 2",
        "COM 3",
        "COM 4",
        "COM 5",
        "COM 6",
        "COM 7",
        "COM 8"
    };

    private readonly TextInputData _nicknameInput = new()
        {Text = string.Empty, MaxLength = Settings.Nickname.MaxLength};

    private int _currentBoundInput;

    private byte _difficulty;
    private bool _items;
    private float _musicVolume;
    private sbyte _rounds = 1;

    private List<Score> _scores;

    private float _sfxVolume;
    private State _state = State.Title;

    public MainMenuSelector(Font font) : base(font)
    {
        _musicVolume = Settings.MyData.MusicVolume * 100.0f;
        _sfxVolume = Settings.MyData.SfxVolume * 100.0f;

        if (!Settings.MyData.NicknameDefined) _state = State.NicknameSetup;

        Task.Run(GetScores);
    }

    private void GetScores()
    {
        Socket socket = null;
        try
        {
            socket = ScoringServer.ConnectToScoringServer();

            socket.Send(Encoding.UTF8.GetBytes("(\"GetScores\")"));

            Span<byte> buffer = stackalloc byte[4096];
            socket.Receive(buffer);

            _scores = new List<Score>();

            if (SExpressionParser.Parse(Encoding.UTF8.GetString(buffer)) is not List<object> list) return;
            foreach (List<object> score in list) _scores.Add(new Score(score[0] as string, (int) score[1]));
        }
        catch (Exception e)
        {
            // ignored
        }
        finally
        {
            socket?.Close();
        }
    }

    public override void Update()
    {
        base.Update();

        Begin();
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
            case State.NicknameSetup:
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
            case State.NetworkModes:
                UpdateNetworkModeMatches();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        End();
    }

    private void UpdateMatchSettings()
    {
        Break(30.0f);
        Label("MATCH SETTINGS", Color.Gold);
        Break(30.0f);

        if (Button(DifficultyCaptions[_difficulty])) _difficulty = (byte) ((_difficulty + 1) % 9);

        if (Button(_items ? "ITEMS" : "NO ITEMS")) _items = !_items;

        if (Button($"BEST OF {_rounds}"))
        {
            _rounds = (sbyte) (_rounds + 1 % 5);

            if (_rounds == 0) _rounds = 1;
        }

        Break(30.0f);

        if (Button("BEGIN"))
            GameFlow.Set(new VersusMode(new GameInfo(new ComInfo(0, _difficulty), 50, Environment.TickCount, _rounds,
                60 * 60, BannerWork.MessageIndex.NoBanner, _items ? GameInfo.Flags.None : GameInfo.Flags.NoItems)));
        if (Button("BACK")) _state = State.ModeSelect;
    }

    private void UpdateScoreboard()
    {
        Break(30.0f);
        Label("SCOREBOARD", Color.Gold);
        Break(30.0f);

        var place = 1;
        if (_scores != null)
            foreach (var score in _scores)
            {
                var span = TimeSpan.FromSeconds(score.Time / 60.0);
                Label($"{place++}. {score.Name} {span.Minutes.ToString("00")}:{span.Seconds.ToString("00")}");
            }

        if (Button("BACK"))
            _state = State.MainScreen;
    }

    private void UpdateInput()
    {
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
                        $"{ActionNames[j - offset]}: {SDL.SDL_GetScancodeName((SDL.SDL_Scancode) Settings.MyData.InputProfiles[j])}",
                        _currentBoundInput == j ? Color.White : Color.Gray);
                }
        }


        if (Keyboard.AnyDown(out var newBinding))
            unsafe
            {
                Settings.MyData.InputProfiles[_currentBoundInput] = (int) newBinding;
                ++_currentBoundInput;

                if (_currentBoundInput >= Settings.Data.InputProfileSize * Match.PlayerCount)
                {
                    Settings.Save();
                    _state = State.Settings;
                }
            }
    }


    private void UpdateTitle()
    {
        Break(30.0f);
        Label("DUCK DUCK JUMP", Color.Gold);
        Break(120.0f * 3.0f);

        if (Button("PRESS TO START"))
            _state = State.MainScreen;
    }

    private void UpdateAudioSettings()
    {
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
    }

    private void UpdateSettings()
    {
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
    }

    private void ModeSelection()
    {
        Break(30.0f);
        Label("MODE SELECTION", Color.Gold);
        Break(80.0f);

        if (Button("TIME ATTACK MODE")) GameFlow.Set(new TimeAttackMode());

        if (Button("NETWORK MODES")) _state = State.NetworkModes;

        if (Button("VERSUS MODE")) _state = State.MatchSettings;

        if (Button("WATCH MODE")) GameFlow.Set(new WatchMode());

        Break(20.0f);

        if (Button("BACK")) _state = State.MainScreen;
    }

    private void UpdateNetworkModeMatches()
    {
        Break(30.0f);
        Label("NETWORK MODES", Color.Gold);
        Break(80.0f);

        if (Button("HOST MATCH")) GameFlow.Set(new HostNetworkMode());

        if (Button("JOIN MATCH")) GameFlow.Set(new ClientNetworkMode());

        Break(80.0f);

        if (Button("BACK")) _state = State.ModeSelect;
    }

    private void UpdateNickname()
    {
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
                    Settings.MyData.Nickname.Length = (byte) _nicknameInput.Text.Length;
                }

            _nicknameInput.Text = string.Empty;
            Settings.Save();
            _state = State.Title;
        }
    }

    private void UpdateMainScreen()
    {
        Break(120.0f * 1.5f);
        if (Button("BEGIN")) _state = State.ModeSelect;
        if (Button("SCOREBOARD")) _state = State.Scoreboard;
        if (Button("SETTINGS")) _state = State.Settings;
        var quitEv = new SDL.SDL_Event
        {
            type = SDL.SDL_EventType.SDL_QUIT
        };

        if (Button("QUIT")) SDL.SDL_PushEvent(ref quitEv);
    }

    private enum State
    {
        Title,
        MainScreen,
        Settings,
        ModeSelect,
        NicknameSetup,
        AudioSettings,
        InputSettings,
        Scoreboard,
        MatchSettings,
        NetworkModes
    }
}

public class MainMenuState : IGameState
{
    private Font _font;

    private AudioClip _music;
    private MainMenuSelector _selector;

    public void Initialize()
    {
        _font = new Font("public-pixel-30");
        _selector = new MainMenuSelector(_font);
        _music = new AudioClip("menu", true);

        Audio.PlayMusic(_music);

        MatchAssets.Load();
        Match.Initialize(new GameInfo(new ComInfo(8, 8), 100, Environment.TickCount, 0, ushort.MaxValue,
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
        _selector?.Update();
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