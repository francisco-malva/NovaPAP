#region

using System.Net;
using System.Net.Sockets;
using Common.Web;

#endregion

namespace DuckDuckJump.States;

public static class ScoringServer
{
    public static Socket ConnectToScoringServer()
    {
        var serverAddress = IpUtilities.GetIpFromName(Dns.GetHostName());

        var remoteEndpoint = new IPEndPoint(serverAddress, 12168);
        var socket = new Socket(serverAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        socket.Connect(remoteEndpoint);
        return socket;
    }
}