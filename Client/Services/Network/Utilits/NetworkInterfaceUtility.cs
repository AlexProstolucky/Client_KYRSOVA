using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Client.Services.Network.Utilits
{
    internal class NetworkInterfaceUtility
    {
        public static IPAddress GetRadminVPNIPAddress()
        {
            NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();

            NetworkInterface radminVPNInterface = interfaces.FirstOrDefault(
                iface => iface.Name.Equals("Radmin VPN", StringComparison.OrdinalIgnoreCase));

            var ipAddresses = radminVPNInterface.GetIPProperties().UnicastAddresses;
            return (from ip in ipAddresses where ip.Address.AddressFamily == AddressFamily.InterNetwork select ip.Address).FirstOrDefault();
        }
    }
}
