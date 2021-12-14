using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using DuckDuckJump.Engine.Input;
using DuckDuckJump.Engine.Utilities;
using DuckDuckJump.Engine.Wrappers.SDL2;
using DuckDuckJump.Scenes.Game.Gameplay;
using DuckDuckJump.Scenes.Game.Input;
using DuckDuckJump.Scenes.MainMenu;
using SDL2;

namespace DuckDuckJump.Scenes.Game.Network;

internal class OnlineGameScene : GameScene
{
    private readonly bool _onLeftSide;
    protected StreamReaderWriter Stream;

    private TcpClient _client;

    private bool _disconnected = false;

    public OnlineGameScene(GameInfo info, TcpClient client, bool onLeftSide) : base(info)
    {
        _client = client;
        Stream = new StreamReaderWriter(_client.GetStream(), Encoding.UTF8);
        _onLeftSide = onLeftSide;

        if (onLeftSide)
        {
            info.Save(Stream.Writer);
        }

        _client.ReceiveTimeout = 500;
        _client.SendTimeout = 500;
    }

    public override void OnTick()
    {
        try
        {
            if (_disconnected)
            {
                UI.DrawText(new Point(0, 0), Color.White, "Disconnected from player.\nPress SPACE to go back to the main menu.");
                return;
            }
            var gameInput = GameInput.None;

            ProcessInput(ref gameInput, SDL.SDL_Scancode.SDL_SCANCODE_A, SDL.SDL_Scancode.SDL_SCANCODE_D,
                SDL.SDL_Scancode.SDL_SCANCODE_S);

            Stream.Writer.Write((byte)gameInput);
            var foreignInput = (GameInput)Stream.Reader.ReadByte();

            SimulateAndDraw(_onLeftSide ? gameInput : foreignInput, _onLeftSide ? foreignInput : gameInput);
        }
        catch (Exception)
        {
            Engine.Game.Instance.SceneManager.Set(new MainMenuScene());
        }
    }

    protected override void OnGameEnd()
    {
        Stream.Dispose();
        _client.Dispose();
        base.OnGameEnd();
    }
}