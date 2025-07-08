using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using ProtankiNetworking.Networking;
using ProtankiNetworking.Packets;
using ProtankiNetworking.Security;
using ProtankiProxy.Events;
using Serilog;

namespace ProtankiProxy
{
    /// <summary>
    /// Handles a single client connection to the proxy server.
    /// This class manages the communication between a client and the game server,
    /// forwarding packets between them and handling packet encryption/decryption.
    /// </summary>
    public class ProxyClientHandler : TankiTcpClientHandler
    {
        private readonly IPEndPoint _serverEndPoint;
        private TankiTcpClient? _serverClient;
        private IPAddress _clientAddress;
        private readonly ProxyServer _proxyServer;
        private string _clientEndPoint = string.Empty;

        /// <summary>
        /// Creates a new instance of ProxyClientHandler.
        /// </summary>
        /// <param name="client">The TCP client connection</param>
        /// <param name="serverEndPoint">The endpoint of the game server to connect to</param>
        /// <param name="protection">The protection instance for packet encryption/decryption</param>
        /// <param name="cancellationToken">The cancellation token for the client handler</param>
        /// <param name="proxyServer">The proxy server instance</param>
        public ProxyClientHandler(TcpClient client, IPEndPoint serverEndPoint, Protection protection, CancellationToken cancellationToken, ProxyServer proxyServer)
            : base(client, protection, cancellationToken)
        {
            _serverEndPoint = serverEndPoint;
            var remoteEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            if (remoteEndPoint != null)
            {
                _clientAddress = remoteEndPoint.Address;
                _clientEndPoint = remoteEndPoint.ToString();
            }
            else
            {
                _clientAddress = IPAddress.None;
                _clientEndPoint = "Unknown";
            }
            _proxyServer = proxyServer;
        }

        /// <summary>
        /// Gets the client's endpoint as a string (IP address and port).
        /// </summary>
        /// <returns>The client's endpoint string</returns>
        public string GetClientEndPoint()
        {
            return _clientEndPoint;
        }

        /// <summary>
        /// Called when the client connects to the proxy server.
        /// Creates a connection to the game server.
        /// </summary>
        protected override async Task OnConnectedAsync()
        {
            Log.Information("Connecting to ProTanki server at {Address}:{Port}", _serverEndPoint.Address, _serverEndPoint.Port);
            _serverClient = new ServerClient(new Protection(), this, _serverEndPoint.Address.ToString(), _serverEndPoint.Port, _cancellationToken, _proxyServer);
            await _serverClient.ConnectAsync();
        }

        /// <summary>
        /// Called when a raw packet is received from the client.
        /// Forwards the packet to the game server.
        /// </summary>
        /// <param name="rawPacket">The complete raw packet data including headers</param>
        protected override async Task OnRawPacketReceivedAsync(byte[] rawPacket)
        {
            // Forward raw packets from client to server
            if (_serverClient != null)
            {
                await _serverClient.SendRawPacketAsync(rawPacket);
            }
        }

        /// <summary>
        /// Called when a packet is received from the client.
        /// Raises the PacketReceived event with the packet information.
        /// </summary>
        /// <param name="packet">The received packet</param>
        protected override Task OnPacketReceivedAsync(AbstractPacket packet)
        {
            var args = new PacketEventArgs(packet, _clientAddress.ToString(), _serverEndPoint.ToString());
            _proxyServer.OnPacketReceived(args);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when an error occurs during client communication.
        /// Logs the error and disconnects the client.
        /// </summary>
        /// <param name="exception">The exception that occurred</param>
        /// <param name="context">The context where the error occurred</param>
        protected override Task OnErrorAsync(Exception exception, string context)
        {
            Log.Error(exception, "Error in ProxyClientHandler: {Context}", context);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called when the client disconnects from the proxy server.
        /// Disconnects from the game server and cleans up resources.
        /// </summary>
        protected override async Task OnDisconnectedAsync()
        {
            Log.Information("Client disconnected from {ClientEndPoint}", _clientEndPoint);
            if (_serverClient != null)
            {
                await _serverClient.DisconnectAsync();
                _serverClient = null;
            }
        }

        /// <summary>
        /// Called when a packet fails to unwrap.
        /// Logs the error and continues processing.
        /// </summary>
        /// <param name="packetType">The type of packet that failed</param>
        /// <param name="packetId">The ID of the packet that failed</param>
        /// <param name="exception">The exception that occurred during unwrapping</param>
        protected override Task OnPacketUnwrapFailureAsync(Type packetType, int packetId, Exception exception)
        {
            Log.Error(exception, "Failed to unwrap packet of type {PacketType} with ID {PacketId}", 
                packetType.Name, packetId);
            return Task.CompletedTask;
        }
    }
} 