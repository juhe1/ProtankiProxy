using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ProtankiNetworking.Networking;
using ProtankiNetworking.Security;
using ProtankiProxy.Events;
using Serilog;

namespace ProtankiProxy
{
    /// <summary>
    /// Main proxy server that handles client connections and forwards packets between clients and the game server.
    /// This class manages the TCP listener and creates client handlers for each connected client.
    /// </summary>
    public class ProxyServer : TankiTcpListener
    {
        private readonly string _serverAddress;
        private readonly int _serverPort;

        /// <summary>
        /// Gets the address of the game server.
        /// </summary>
        public string ServerAddress => _serverAddress;

        /// <summary>
        /// Gets the port of the game server.
        /// </summary>
        public int ServerPort => _serverPort;

        /// <summary>
        /// Event that is raised when a packet is received from either a client or the server.
        /// </summary>
        public event EventHandler<PacketEventArgs> PacketReceived;

        /// <summary>
        /// Creates a new instance of ProxyServer.
        /// </summary>
        /// <param name="localEndPoint">The local endpoint on which the proxy server will listen for client connections</param>
        /// <param name="serverAddress">The address of the game server to connect to</param>
        /// <param name="serverPort">The port of the game server to connect to</param>
        /// <param name="cancellationToken">The cancellation token for the proxy server</param>
        public ProxyServer(IPEndPoint localEndPoint, string serverAddress, int serverPort, CancellationToken cancellationToken)
            : base(localEndPoint)
        {
            _serverAddress = serverAddress;
            _serverPort = serverPort;
        }

        protected override TankiTcpClientHandler CreateClientHandler(TcpClient client, CancellationToken cancellationToken)
        {
            return new ProxyClientHandler(client, new IPEndPoint(IPAddress.Parse(_serverAddress), _serverPort), new Protection(true), cancellationToken, this);
        }

        protected override async Task OnClientConnectedAsync(TcpClient client)
        {
            Log.Information("Client connected from {Address}", ((IPEndPoint)client.Client.RemoteEndPoint).Address);
        }

        protected override async Task OnClientDisconnectedAsync(TcpClient client)
        {
            Log.Information("Client disconnected from {Address}", ((IPEndPoint)client.Client.RemoteEndPoint).Address);
        }

        protected override async Task OnErrorAsync(Exception exception, string context)
        {
            Log.Error(exception, "Error in ProxyServer: {Context}", context);
        }

        internal void OnPacketReceived(PacketEventArgs e)
        {
            PacketReceived?.Invoke(this, e);
        }
    }
} 