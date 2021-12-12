using System.Net;
using System.Net.Sockets;

namespace GameplayServer
{
    internal static class Program
    {
        private static TcpListener _listener;


        private static List<TcpClient> _clients = new List<TcpClient>();

        private static void Main()
        {
            _clients = new List<TcpClient>();
            _listener = new TcpListener(IPAddress.Any, 9999);

            var listeningThread = new Thread(ListenLoop);
            listeningThread.Start();

            while (true)
            {
                UpdateLoop();
            }
        }

        private static void ListenLoop()
        {
            while (true)
            {
                var client = _listener.AcceptTcpClient();

                lock (_clients)
                {
                    _clients.Add(client);
                }
            }
        }

        private static void UpdateLoop()
        {
            lock (_clients)
            {
                foreach (var client in _clients)
                {
                    client.
                }
            }
            
        }
    }
}
