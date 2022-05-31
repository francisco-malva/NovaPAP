#region

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using DuckDuckJump.Game;
using DuckDuckJump.Game.GameWork.Banner;

#endregion

namespace DuckDuckJump.States.GameModes.NetworkMode;

public class ClientNetworkMode : NetworkMode
{
    public ClientNetworkMode()
    {
        MyInputIndex = 1;
        OtherInputIndex = 0;
    }

    protected override void EstablishConnection()
    {
        var ipHostInfo = Dns.GetHostEntry(File.ReadAllText("ip.txt"));
        var ipAddress = ipHostInfo.AddressList[0];
        var localEndPoint = new IPEndPoint(ipAddress, 11000);

        OtherSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        lock (SocketLock)
        {
            MySocket = OtherSocket;
        }

        lock (SocketLock)
        {
            OtherSocket.Connect(localEndPoint);

            Span<byte> buffer = stackalloc byte[sizeof(int)];

            OtherSocket.Receive(buffer);

            Match.Initialize(new GameInfo(new ComInfo(0, 0), 50, BitConverter.ToInt32(buffer), 3, 60 * 60,
                BannerWork.MessageIndex.NoBanner, GameInfo.Flags.None));
        }
    }
}