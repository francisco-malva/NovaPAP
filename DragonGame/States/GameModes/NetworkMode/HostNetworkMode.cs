#region

using System;
using System.Net;
using System.Net.Sockets;
using DuckDuckJump.Game;
using DuckDuckJump.Game.GameWork.Banner;

#endregion

namespace DuckDuckJump.States.GameModes.NetworkMode;

public class HostNetworkMode : NetworkMode
{
    public HostNetworkMode()
    {
        MyInputIndex = 0;
        OtherInputIndex = 1;
    }

    protected override void EstablishConnection()
    {
        var ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
        var ipAddress = ipHostInfo.AddressList[0];
        var localEndPoint = new IPEndPoint(ipAddress, 11000);

        lock (SocketLock)
        {
            MySocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            MySocket.Bind(localEndPoint);
            MySocket.Listen();

            OtherSocket = MySocket.Accept();

            var seed = Environment.TickCount;

            Match.Initialize(new GameInfo(new ComInfo(0, 0), 50, seed, 3, 60 * 60,
                BannerWork.MessageIndex.NoBanner, GameInfo.Flags.None));

            Span<byte> buffer = stackalloc byte[sizeof(int)];
            BitConverter.TryWriteBytes(buffer, seed);

            OtherSocket.Send(buffer);
        }
    }
}