using System.Net;
using System.Net.Sockets;

namespace Client.Services.Network.Utilits
{
    internal class FreePort
    {
        public static int GetPort()
        {
            int startPort = 60000;
            int endPort = 63000;

            for (int i = startPort; i <= endPort; i++)
            {
                if (IsPortAvailable(i))
                {
                    return i;
                }
            }

            return -1;
        }

        static bool IsPortAvailable(int port)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    client.Connect(IPAddress.Loopback, port);
                    return false;
                }
            }
            catch (SocketException)
            {
                return true;
            }
        }
    }
}
