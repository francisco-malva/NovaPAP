using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Input;
using SDL2;

namespace DragonGame.Scenes.Game.Network
{
    internal sealed class ServerGameScene : GameScene
    {
        private TcpListener _listener;
        private StreamManipulator _manipulator;
        private TcpClient _client;

        private readonly byte _roundsToWin;

        public ServerGameScene(byte roundsToWin) : base(roundsToWin)
        {
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 3000);
            _listener.Start();
            _client = _listener.AcceptTcpClient();
            _manipulator = new StreamManipulator(_client.GetStream(), Encoding.Default, true);


            var tickCount = Environment.TickCount;
            Random.Setup(tickCount);

            _manipulator.Writer.Write(_roundsToWin);
            _manipulator.Writer.Write(tickCount);

            Console.Write("Server Setup successfully");

            ChangeState(GameState.GetReady);
        }

        public override void OnTick()
        {
            var gameInput = GameInput.None;

            ProcessInput(ref gameInput, SDL.SDL_Scancode.SDL_SCANCODE_A, SDL.SDL_Scancode.SDL_SCANCODE_D,
                SDL.SDL_Scancode.SDL_SCANCODE_S);

            _manipulator.Writer.Write((byte)gameInput);
            var foreignInput = (GameInput)_manipulator.Reader.ReadByte();
            
            SimulateFrame(gameInput, foreignInput);
            Draw();
            base.OnTick();
        }

        protected override void OnGameEnd()
        {
            _listener.Stop();
            _manipulator.Dispose();
            _client.Dispose();
        }
    }
}
