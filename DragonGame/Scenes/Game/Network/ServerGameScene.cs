using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using DragonGame.Engine.Utilities;

namespace DragonGame.Scenes.Game.Network
{
    internal sealed class ServerGameScene : OnlineGameScene
    {
        private readonly TcpListener _listener;
        private readonly TcpClient _client;

        public ServerGameScene(byte roundsToWin) : base(roundsToWin, true)
        {
            _listener = new TcpListener(IPAddress.Any, 3000);
            _listener.Start();
            _client = _listener.AcceptTcpClient();
            Stream = new StreamReaderWriter(_client.GetStream(), Encoding.Default, true);


            var tickCount = Environment.TickCount;
            Random.Setup(tickCount);

            Stream.Writer.Write(roundsToWin);
            Stream.Writer.Write(tickCount);

            Console.Write("Server Setup successfully");

            ChangeState(GameState.GetReady);
        }

        protected override void OnGameEnd()
        {
            _listener.Stop();
            _client.Dispose();
            base.OnGameEnd();
        }
    }
}
