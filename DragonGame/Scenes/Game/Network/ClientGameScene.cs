using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DragonGame.Engine.Utilities;
using DragonGame.Scenes.Game.Input;
using DragonGame.Scenes.MainMenu;
using SDL2;

namespace DragonGame.Scenes.Game.Network
{
    internal sealed class ClientGameScene : OnlineGameScene
    {
        private readonly TcpClient _client;

        public ClientGameScene() : base(0, false)
        {
            _client = new TcpClient();
            _client.Connect(new IPEndPoint(IPAddress.Parse(File.ReadAllText("config.txt")), 3000));

             Stream = new StreamReaderWriter(_client.GetStream(), Encoding.Default, true);

            SetRoundsToWin(Stream.Reader.ReadByte());
            Random.Setup(Stream.Reader.ReadInt32());

            Console.Write("Client Setup successfully");

            ChangeState(GameState.GetReady);
        }

        protected override void OnGameEnd()
        {
            _client.Dispose();
            base.OnGameEnd();
        }

        private void IoLoop()
        {

        }
    }
}
