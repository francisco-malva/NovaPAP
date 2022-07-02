using System.Net;
using System.Net.Sockets;

namespace Common.Web;

public static class IpUtilities
{
    public static IPAddress GetIpFromName(string name)
    {
        return IPAddress.TryParse(name, out var ipAddress)
            ? ipAddress
            : Dns.GetHostAddresses(name, AddressFamily.InterNetwork)[0];
    }
}