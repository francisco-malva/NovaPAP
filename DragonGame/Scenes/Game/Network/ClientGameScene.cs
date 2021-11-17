using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Input;
using SDL2;

namespace DragonGame.Scenes.Game.Network
{
    internal sealed class ClientGameScene : GameScene
    {
        private TcpClient _client;
        private StreamManipulator _manipulator;

        public ClientGameScene() : base(0)
        {
            _client = new TcpClient();
            _client.Connect(new IPEndPoint(IPAddress.Parse(File.ReadAllText("config.txt")), 3000));

            _manipulator = new StreamManipulator(_client.GetStream(), Encoding.Default, true);

            SetRoundsToWin(_manipulator.Reader.ReadByte());
            Random.Setup(_manipulator.Reader.ReadInt32());

            Console.Write("Client Setup successfully");

            ChangeState(GameState.GetReady);
        }

        public override void OnTick()
        {
            var gameInput = GameInput.None;

            ProcessInput(ref gameInput, SDL.SDL_Scancode.SDL_SCANCODE_A, SDL.SDL_Scancode.SDL_SCANCODE_D,
                SDL.SDL_Scancode.SDL_SCANCODE_S);

            _manipulator.Writer.Write((byte)gameInput);
            var foreignInput = (GameInput)_manipulator.Reader.ReadByte();

            SimulateFrame(foreignInput, gameInput);
            Draw();
        }

        protected override void OnGameEnd()
        {
            _client.Dispose();
            _manipulator.Dispose();
        }
    }
}
