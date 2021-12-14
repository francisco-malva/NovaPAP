using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DuckDuckJump.Engine.GUI;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Scenes;
using DuckDuckJump.Engine.Wrappers.SDL2;
using DuckDuckJump.Scenes.Game;
using DuckDuckJump.Scenes.Game.Gameplay;
using DuckDuckJump.Scenes.Game.Gameplay.Players.AI;
using DuckDuckJump.Scenes.Game.Local;
using DuckDuckJump.Scenes.Game.Network;

namespace DuckDuckJump.Scenes.MainMenu;

internal class MainMenuScene : Scene
{
    private readonly Selection[] _mainSelections;

    private readonly Selection[] _networkGamesSelection;

    private readonly Selection[] _quitYouSureSelection;

    private readonly Selection[] _hostingSelection;

    private readonly Selection[] _matchSettingsSelection;

    private readonly Selector _selector;

    private readonly TcpListener _tcpListener;

    private GameMode _mode;


    private byte _matchPoints = 3;
    private bool _items = true;
    private AiDifficulty _difficulty;
    private ushort _platformCount;

    public MainMenuScene()
    {
        _selector = new Selector();

        _tcpListener = new TcpListener(IPAddress.Any, 9999);

        _mainSelections = new[]
        {
            new("DUCK DUCK JUMP", null, null, null, false),
            new Selection("VS CPU",
            null,
                (sel) =>
                {
                    _mode = GameMode.VsCpu;
                    _selector.Push(_matchSettingsSelection);
                }, null),
            new Selection("VS PLAYER",
            null,
                (sel) =>
                {
                    _mode = GameMode.VsPlayer;
                    _selector.Push(_matchSettingsSelection);
                }, null),
            new Selection("WATCH MODE",
            null,
                (sel) =>
                {
                     _mode = GameMode.Watch;
                    _selector.Push(_matchSettingsSelection);
                }, null),
            new Selection("NETWORK", null, (sel) => { _selector.Push(_networkGamesSelection); }, null),
            new Selection("QUIT", null, ShowQuitPrompt, null)
        };

        _networkGamesSelection = new[]
        {
            new("NETWORK GAME", null, null, null, false),
            new Selection("HOST", null, BeginHosting, null),
            new Selection("JOIN", null, BeginSearching, null),
            new Selection("RETURN", null, (sel) => { _selector.Pop(); }, null)
        };

        _quitYouSureSelection = new[]
        {
            new("ARE YOU SURE YOU WANT TO QUIT?", null, null, null, false),
            new Selection("YES!", null, (sel) => { Engine.Game.Instance.Exit(); }, null),
            new Selection("NO!", null, (sel) => { _selector.Pop(); }, null)
        };

        _hostingSelection = new[]
        {
            new Selection("WAITING FOR PLAYER...", null, null, null, false)
        };

        _matchSettingsSelection = new[]{
            new Selection("MATCH SETTINGS", null, null,null,false),
            new Selection(string.Empty, null, OnPointsPush, OnPointsHover, true),
            new Selection(string.Empty, OnDifficultyPush, null, OnDifficultyHover),
            new Selection(string.Empty,OnItemPush , null, OnItemHover),
            new Selection("BEGIN", null,BeginGame,null),
            new Selection("RETURN", null,(sel) => {_selector.Pop();},null)
        };

        _selector.Push(_mainSelections);
    }

    private void BeginGame(Selection selection)
    {
        GameScene scene = new OfflineGameScene(GetGameInfo());

        Engine.Game.Instance.SceneManager.Set(scene);
    }

    private void OnItemPush(Selection selection)
    {
        selection.Label = $"ITEMS: {(_items ? "ON" : "OFF")}";
    }

    private void OnItemHover(Selection selection)
    {
        if (Keyboard.KeyDown(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_RETURN))
        {
            _items = !_items;
            OnItemPush(selection);
        }
    }

    private string DifficultyToString(AiDifficulty difficulty)
    {
        switch (difficulty)
        {
            case AiDifficulty.Easy:
                return "EASY";
            case AiDifficulty.Normal:
                return "NORMAL";
            case AiDifficulty.Hard:
                return "HARD";
            case AiDifficulty.Nightmare:
                return "NIGHTMARE";
            default:
                return "INVALID";
        }
    }
    private void OnDifficultyPush(Selection selection) => selection.Label = $"AI DIFFICULTY: {DifficultyToString(_difficulty)}";

    private void OnDifficultyHover(Selection selection)
    {
        if (Keyboard.KeyDown(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_LEFT))
        {
            if (_difficulty == AiDifficulty.Easy)
            {
                _difficulty = AiDifficulty.Nightmare;
            }
            _difficulty = (AiDifficulty)((int)_difficulty - 1);
        }
        else if (Keyboard.KeyDown(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_RIGHT))
        {
            if (_difficulty == AiDifficulty.Nightmare)
            {
                _difficulty = AiDifficulty.Easy;
            }
            _difficulty = (AiDifficulty)((int)_difficulty + 1);
        }

        OnDifficultyPush(selection);
    }

    private void OnPointsPush(Selection selection)
    {
        selection.Label = $"MATCH POINTS: {_matchPoints}";
    }

    private void OnPointsHover(Selection selection)
    {
        if (Keyboard.KeyDown(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_LEFT))
        {
            if (_matchPoints == 1)
            {
                _matchPoints = 5;
            }
            else
            {
                _matchPoints--;
            }
        }
        else if (Keyboard.KeyDown(SDL2.SDL.SDL_Scancode.SDL_SCANCODE_RIGHT))
        {
            if (_matchPoints == 5)
            {
                _matchPoints = 1;
            }
            else
            {
                _matchPoints++;
            }
        }
        OnPointsPush(selection);
    }

    private void BeginHosting(Selection selection)
    {
        var tcpListener = new TcpListener(IPAddress.IPv6Loopback, 29170);
        tcpListener.Start();
        var client = tcpListener.AcceptTcpClient();

        Engine.Game.Instance.SceneManager.Set(new OnlineGameScene(
           GetGameInfo(), client, true));

        tcpListener.Stop();
    }


    private GameInfo GetGameInfo()
    {
        switch (_mode)
        {
            case GameMode.VsCpu:
                return new GameInfo(50, _matchPoints, false, true, Environment.TickCount, _difficulty, _items);
            case GameMode.VsPlayer:
                return new GameInfo(50, _matchPoints, false, false, Environment.TickCount, AiDifficulty.Easy, _items);
            case GameMode.Watch:
                return new GameInfo(50, _matchPoints, true, true, Environment.TickCount, AiDifficulty.Nightmare, _items);
        }
        return null;
    }
    private void BeginSearching(Selection selection)
    {
        var client = new TcpClient(File.ReadAllText("config.txt").Trim(), 29170);

        using var reader = new BinaryReader(client.GetStream(), Encoding.UTF8, true);
        Engine.Game.Instance.SceneManager.Set(new OnlineGameScene(new GameInfo(reader), client, false));
    }

    public override void OnTick()
    {
        _selector.Tick();

        Engine.Game.Instance.Renderer.SetDrawColor(Color.Black);
        Engine.Game.Instance.Renderer.Clear();
        _selector.Draw();
        Engine.Game.Instance.Renderer.Present();
    }

    protected override void OnUnload()
    {
    }

    private void ShowQuitPrompt(Selection selection)
    {
        _selector.Push(_quitYouSureSelection);
    }
}