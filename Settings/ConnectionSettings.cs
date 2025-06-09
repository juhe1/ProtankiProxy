using System;
using System.Net;

namespace ProtankiProxy.Settings
{
    public class ConnectionSettings
    {
        public string LocalIp { get; set; } = "127.0.0.1";
        public int LocalPort { get; set; } = 1212;
        public string ServerIp { get; set; } = "146.59.110.146";
        public int ServerPort { get; set; } = 25565;

        public IPEndPoint GetLocalEndPoint()
        {
            return new IPEndPoint(IPAddress.Parse(LocalIp), LocalPort);
        }

        public IPEndPoint GetServerEndPoint()
        {
            return new IPEndPoint(IPAddress.Parse(ServerIp), ServerPort);
        }

        public void UpdateFromEndPoints(IPEndPoint localEndPoint, IPEndPoint serverEndPoint)
        {
            LocalIp = localEndPoint.Address.ToString();
            LocalPort = localEndPoint.Port;
            ServerIp = serverEndPoint.Address.ToString();
            ServerPort = serverEndPoint.Port;
        }
    }
} 