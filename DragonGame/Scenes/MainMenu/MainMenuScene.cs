using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DuckDuckJump.Engine.GUI;
using DuckDuckJump.Engine.Scenes;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Engine.Wrappers.SDL2;
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

    private readonly Selector _selector;

    private readonly TcpListener _tcpListener;

    private TcpClient _client;


    private Thread _networkTask;

    public MainMenuScene()
    {
        _selector = new Selector();

        _tcpListener = new TcpListener(IPAddress.Any, 9999);

        _mainSelections = new[]
        {
            new("DUCK DUCK JUMP", null, null, false),
            new Selection("VS CPU",
                () =>
                {
                    Engine.Game.Instance.SceneManager.Set(new OfflineGameScene(new GameInfo(50, 3, false, true,
                        Environment.TickCount, AiDifficulty.Nightmare)));
                }, null),
            new Selection("VS PLAYER",
                () =>
                {
                    Engine.Game.Instance.SceneManager.Set(new OfflineGameScene(new GameInfo(50, 3, false, false,
                        Environment.TickCount, AiDifficulty.Nightmare)));
                }, null),
            new Selection("WATCH MODE",
                () =>
                {
                    Engine.Game.Instance.SceneManager.Set(new OfflineGameScene(new GameInfo(50, 3, true, true,
                        Environment.TickCount, AiDifficulty.Nightmare)));
                }, null),
            new Selection("NETWORK", () => { _selector.Push(_networkGamesSelection); }, null),
            new Selection("QUIT", ShowQuitPrompt, null)
        };

        _networkGamesSelection = new[]
        {
            new("NETWORK GAME", null, null, false),
            new Selection("HOST", BeginHosting, null),
            new Selection("JOIN", BeginSearching, null),
            new Selection("RETURN", () => { _selector.Pop(); }, null)
        };

        _quitYouSureSelection = new[]
        {
            new("ARE YOU SURE YOU WANT TO QUIT?", null, null, false),
            new Selection("YES!", () => { Engine.Game.Instance.Exit(); }, null),
            new Selection("NO!", () => { _selector.Pop(); }, null)
        };

        _hostingSelection = new[]
        {
            new Selection("WAITING FOR PLAYER...", null, null, false)
        };

        _selector.Push(_mainSelections);
    }

    private void BeginHosting()
    {
        var tcpListener = new TcpListener(IPAddress.Any, 9999);
        tcpListener.Start();
        var client = tcpListener.AcceptTcpClient();

        Engine.Game.Instance.SceneManager.Set(new OnlineGameScene(
            new GameInfo(50, 3, false, false, Environment.TickCount, AiDifficulty.Easy),
            client, true));

        tcpListener.Stop();
    }

    private void BeginSearching()
    {
        var client = new TcpClient(File.ReadAllText("config.txt").Trim(), 9999);

        using var reader = new BinaryReader(client.GetStream(), Encoding.UTF8, true);
        Engine.Game.Instance.SceneManager.Set(new OnlineGameScene(new GameInfo(reader), client, false));
    }

    private void EndHosting()
    {
        _networkTask.Interrupt();
        _selector.Pop();
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

    private void ShowQuitPrompt()
    {
        _selector.Push(_quitYouSureSelection);
    }
}