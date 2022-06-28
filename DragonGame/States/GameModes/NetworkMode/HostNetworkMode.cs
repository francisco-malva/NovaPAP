#region

using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Common.Web;
using DuckDuckJump.Engine.Subsystems.Output;
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

    protected override async Task EstablishConnection()
    {
        var localEndPoint = new IPEndPoint(IpUtilities.GetIpFromName(Dns.GetHostName()), 11000);

        lock (SocketLock)
        {
            MySocket = new Socket(localEndPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            Error.RaiseMessage($"Listening on {localEndPoint}");
            MySocket.Bind(localEndPoint);
            MySocket.Listen();

            OtherSocket = MySocket.Accept();

            var seed = Environment.TickCount;

            Match.Initialize(new GameInfo(new ComInfo(0, 0), 50, seed, 3, 60 * 60,
                BannerWork.MessageIndex.NoBanner, GameInfo.Flags.None));

            var buffer = new byte[sizeof(int)];
            BitConverter.TryWriteBytes(buffer, seed);

            OtherSocket.Send(buffer);
        }
    }
}