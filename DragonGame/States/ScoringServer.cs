#region

using System.Net;
using System.Net.Sockets;

#endregion

namespace DuckDuckJump.States;

public static class ScoringServer
{
    public static Socket ConnectToScoringServer()
    {
        var localEndpoint = Dns.GetHostAddresses(Dns.GetHostName())[0];

        var remoteEndpoint = new IPEndPoint(Dns.GetHostAddresses("localhost")[0], 12168);
        var socket = new Socket(localEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        socket.Connect(remoteEndpoint);
        return socket;
    }
}