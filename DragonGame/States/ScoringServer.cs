#region

using System.Net;
using System.Net.Sockets;

#endregion

namespace DuckDuckJump.States;

public static class ScoringServer
{
    public static Socket ConnectToScoringServer()
    {
        var remoteEndpoint = new IPEndPoint(IPAddress.Parse("5.230.67.59"), 12168);
        var socket = new Socket(remoteEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        socket.Connect(remoteEndpoint);
        return socket;
    }
}