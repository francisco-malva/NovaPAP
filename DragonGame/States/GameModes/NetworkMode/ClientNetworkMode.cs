#region

using System;
using System.Net;
using System.Net.Sockets;
using DuckDuckJump.Game;
using DuckDuckJump.Game.GameWork.Banner;

#endregion

namespace DuckDuckJump.States.GameModes.NetworkMode;

public class ClientNetworkMode : NetworkMode
{
    private readonly IPAddress _address;

    public ClientNetworkMode(IPAddress address)
    {
        MyInputIndex = 1;
        OtherInputIndex = 0;
        _address = address;
    }

    protected override void EstablishConnection()
    {
        var localEndPoint = new IPEndPoint(_address, 11000);

        OtherSocket = new Socket(_address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        lock (SocketLock)
        {
            MySocket = OtherSocket;
        }

        lock (SocketLock)
        {
            OtherSocket.Connect(localEndPoint);

            var buffer = new byte[sizeof(int)];

            OtherSocket.Receive(buffer);

            Match.Initialize(new GameInfo(new ComInfo(0, 0), 50, BitConverter.ToInt32(buffer), 3, 60 * 60,
                BannerWork.MessageIndex.NoBanner, GameInfo.Flags.None));

            buffer[0] = byte.MaxValue;
            for (var i = 1; i < buffer.Length; i++) buffer[i] = 0;

            OtherSocket.Send(buffer);
        }
    }
}