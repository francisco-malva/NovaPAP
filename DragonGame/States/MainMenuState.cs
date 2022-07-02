#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
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
using DuckDuckJump.Engine.Subsystems.Output;
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

    private readonly TextInputData _ipInput = new()
    {
        Text = string.Empty, MaxLength = 15
    };

    private readonly TextInputData _nicknameInput = new()
        { Text = string.Empty, MaxLength = Settings.Nickname.MaxLength };

    private byte _difficulty;
    private List<Height> _heights;
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
    }

    private void GetServerData()
    {
        Socket socket = null;
        try
        {
            socket = ScoringServer.ConnectToScoringServer();

            GetScores(socket);
            GetHeights(socket);
        }
        catch (Exception e)
        {
            Error.RaiseMessage(e.Message);
        }
        finally
        {
            socket?.Close();
        }
    }

    private void GetScores(Socket socket)
    {
        socket.Send(Encoding.UTF8.GetBytes("(\"GetScores\")"));

        Span<byte> buffer = stackalloc byte[4096];
        socket.Receive(buffer);

        _scores = new List<Score>();

        if (SExpressionParser.Parse(Encoding.UTF8.GetString(buffer)) is not List<object> list) return;
        foreach (List<object> score in list) _scores.Add(new Score(score[0] as string, (int)score[1]));
    }

    private void GetHeights(Socket socket)
    {
        socket.Send(Encoding.UTF8.GetBytes("(\"GetHeights\")"));

        Span<byte> buffer = stackalloc byte[4096];
        var recv = socket.Receive(buffer);

        _heights = new List<Height>();

        if (SExpressionParser.Parse(Encoding.UTF8.GetString(buffer)) is not List<object> list) return;
        foreach (List<object> height in list) _heights.Add(new Height(height[0] as string, (double)height[1]));
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
            case State.ScreenSettings:
                UpdateScreenSettings();
                break;
            case State.ScoreboardSelection:
                UpdateScoreboardSelection();
                break;
            case State.MatchSettings:
                UpdateMatchSettings();
                break;
            case State.NetworkModes:
                UpdateNetworkModeMatches();
                break;
            case State.IpConfig:
                UpdateIpConfiguration();
                break;
            case State.Extras:
                UpdateExtras();
                break;
            case State.TimeAttackScoreboard:
                UpdateTimeAttackScoreboard();
                break;
            case State.EndlessClimberScoreboard:
                UpdateEndlessClimberScoreboard();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        End();
    }

    private void UpdateScreenSettings()
    {
        Break(30.0f);
        Label("SCREEN SETTINGS", Color.Gold);
        Break(80.0f);

        if (Button("FULLSCREEN: " + (Settings.MyData.Fullscreen ? "YES" : "NO")))
        {
            Settings.MyData.Fullscreen = !Settings.MyData.Fullscreen;
            Graphics.SetFullscreen();
        }


        Break(20.0f);

        if (Button("BACK"))
        {
            Settings.Save();
            _state = State.Settings;
        }
    }

    private void UpdateScoreboardSelection()
    {
        Break(30.0f);
        Label("SCOREBOARDS", Color.Gold);
        Break(80.0f);

        if (Button("TIME ATTACK MODE")) _state = State.TimeAttackScoreboard;

        if (Button("ENDLESS CLIMBER MODE")) _state = State.EndlessClimberScoreboard;

        Break(20.0f);

        if (Button("BACK")) _state = State.MainScreen;
    }

    private void UpdateExtras()
    {
        Break(30.0f);
        Label("EXTRAS", Color.Gold);
        Break(80.0f);

        if (Button("WATCH MODE")) GameFlow.Set(new WatchMode());

        if (Button("ENDLESS CLIMBER MODE")) GameFlow.Set(new EndlessClimberMode());

        Break(20.0f);

        if (Button("BACK")) _state = State.MainScreen;
    }

    private void UpdateIpConfiguration()
    {
        Break(30.0f);
        Label("IP CONFIGURATION", Color.Gold);
        Break(30.0f);

        Text(_ipInput);

        if (Button("CONNECT") && IPAddress.TryParse(_ipInput.Text, out var ip)) GameFlow.Set(new ClientNetworkMode(ip));

        if (Button("BACK")) _state = State.NetworkModes;
    }

    private void UpdateMatchSettings()
    {
        Break(30.0f);
        Label("MATCH SETTINGS", Color.Gold);
        Break(30.0f);

        if (Button(DifficultyCaptions[_difficulty])) _difficulty = (byte)((_difficulty + 1) % 9);

        if (Button(_items ? "ITEMS" : "NO ITEMS")) _items = !_items;

        if (Button($"BEST OF {_rounds}"))
        {
            _rounds = (sbyte)(_rounds + 1 % 5);

            if (_rounds == 0) _rounds = 1;
        }

        Break(30.0f);

        if (Button("BEGIN"))
            GameFlow.Set(new VersusMode(new GameInfo(new ComInfo(0, _difficulty), 50, Environment.TickCount, _rounds,
                60 * 60, BannerWork.MessageIndex.NoBanner, _items ? GameInfo.Flags.None : GameInfo.Flags.NoItems)));
        if (Button("BACK")) _state = State.ModeSelect;
    }

    private void UpdateEndlessClimberScoreboard()
    {
        Break(30.0f);
        Label("ENDLESS CLIMBER", Color.Gold);
        Break(30.0f);

        var place = 1;
        if (_heights != null)
            foreach (var height in _heights)
                Label($"{place++}. {height.Name} {Math.Truncate(height.Amount)}m");

        if (Button("BACK"))
            _state = State.ScoreboardSelection;
    }

    private void UpdateTimeAttackScoreboard()
    {
        Break(30.0f);
        Label("TIME ATTACK", Color.Gold);
        Break(30.0f);

        var place = 1;
        if (_scores != null)
            foreach (var score in _scores)
            {
                var span = TimeSpan.FromSeconds(score.Time / 60.0);
                Label($"{place++}. {score.Name} {span.Minutes.ToString("00")}:{span.Seconds.ToString("00")}");
            }

        if (Button("BACK"))
            _state = State.ScoreboardSelection;
    }

    private int _input;
    
    private void UpdateInput()
    {
        unsafe
        {
            Break(30.0f);
            Label("INPUT BINDINGS", Color.Gold);
            Break(30.0f);

            var player = _input < 3 ? "P1" : "P2";
        
            Label($"KEY FOR {player} {ActionNames[_input % 3]}", Color.White);
            Label($"{SDL.SDL_GetScancodeName((SDL.SDL_Scancode)Settings.MyData.InputProfiles[_input])}",
                Color.White);

            Break(30.0f);
            if (Button("CONTINUE"))
            {
                ++_input;

                if (_input == Settings.Data.InputProfileSize * 2)
                {
                    Settings.Save();
                    _input = 0;
                    _state = State.Settings;
                }
            }

            if (Button("BACK"))
            {
                Settings.Save();
                _input = 0;
                _state = State.Settings;
            }


            if (Keyboard.AnyDown(out var newBinding))
                unsafe
                {
                    Settings.MyData.InputProfiles[_input] = (int)newBinding;
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
            _state = State.InputSettings;
        }

        if (Button("SCREEN"))
        {
            _state = State.ScreenSettings;
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

        if (Button("EXTRAS")) _state = State.Extras;

        Break(20.0f);

        if (Button("BACK")) _state = State.MainScreen;
    }

    private void UpdateNetworkModeMatches()
    {
        Break(30.0f);
        Label("NETWORK MODES", Color.Gold);
        Break(80.0f);

        if (Button("HOST MATCH")) GameFlow.Set(new HostNetworkMode());

        if (Button("JOIN MATCH")) _state = State.IpConfig;

        Break(80.0f);

        if (Button("BACK")) _state = State.ModeSelect;
    }

    private void UpdateNickname()
    {
        Break(30.0f);
        Label("NICKNAME SETUP", Color.Gold);
        Break(30.0f);

        Text(_nicknameInput);

        if (!Button("CONFIRM") || _nicknameInput.Text.Length <= 3) return;

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

    private void UpdateMainScreen()
    {
        Break(120.0f * 1.5f);
        if (Button("BEGIN")) _state = State.ModeSelect;
        if (Button("SCOREBOARDS"))
        {
            _state = State.ScoreboardSelection;
            Task.Run(GetServerData);
        }

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
        ScoreboardSelection,
        MatchSettings,
        NetworkModes,
        Extras,
        IpConfig,
        TimeAttackScoreboard,
        EndlessClimberScoreboard,
        ScreenSettings
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