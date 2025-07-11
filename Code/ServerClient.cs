using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using ProtankiNetworking.Networking;
using ProtankiNetworking.Packets;
using ProtankiNetworking.Security;
using ProtankiNetworking.Packets.Network;
using ProtankiProxy.Events;
using Serilog;

namespace ProtankiProxy
{
    /// <summary>
    /// Represents a client connection to the Protanki game server.
    /// This class handles the communication between the proxy and the actual game server,
    /// managing packet encryption/decryption and forwarding packets between the client and server.
    /// </summary>
    public class ServerClient : TankiTcpClient
    {
        private readonly TankiTcpClientHandler _clientHandler;
        private readonly string _serverAddress;
        private readonly int _serverPort;
        private readonly ProxyServer _proxyServer;
        private readonly Protection _protection;

        public ServerClient(Protection protection, TankiTcpClientHandler clientHandler, string serverAddress, int serverPort, CancellationToken cancellationToken, ProxyServer proxyServer)
            : base(new IPEndPoint(IPAddress.Parse(serverAddress), serverPort), protection)
        {
            _clientHandler = clientHandler;
            _serverAddress = serverAddress;
            _serverPort = serverPort;
            _proxyServer = proxyServer;
            _protection = protection;
        }

        protected override Task OnConnectedAsync()
        {
            Log.Information("Connected to ProTanki server {Server}:{Port}", _serverAddress, _serverPort);
            return Task.CompletedTask;
        }

        protected override async Task OnRawPacketReceivedAsync(byte[] rawPacket)
        {
            // Forward raw packets from server to client
            await _clientHandler.SendRawPacketAsync(rawPacket);
        }

        protected override Task OnPacketReceivedAsync(AbstractPacket packet)
        {
            // Activate protection from client handler. We don't need to activate the protection from this class,
            // it is already activated in the base class.
            if (packet.Id == ActivateProtection.IdStatic)
            {
				var keys = (List<object>?)packet.GetObjectByAttributeName("keys");
                if (!(keys is null))
                {
                    var intKeys = keys.Select(k => (byte)k).ToArray();
                    _clientHandler.Protection.Activate(intKeys);
                }
            }
            var clientHandler = (ProxyClientHandler)_clientHandler;
            var args = new PacketEventArgs(packet, _serverAddress + ":" + _serverPort, clientHandler.GetClientEndPoint());
            _proxyServer.OnPacketReceived(args);
            return Task.CompletedTask;
        }

        protected override Task OnErrorAsync(Exception exception, string context)
        {
            Log.Error(exception, "Error in ProxyServerClient: {Context}", context);
            return Task.CompletedTask;
        }

        protected override Task OnDisconnectedAsync()
        {
            Log.Information("Disconnected from server {Server}:{Port}", _serverAddress, _serverPort);
            return Task.CompletedTask;
        }

        protected override Task OnPacketUnwrapFailureAsync(Type packetType, int packetId, Exception exception)
        {
            Log.Error(exception, "Failed to unwrap packet of type {PacketType} with ID {PacketId}", 
                packetType.Name, packetId);
            return Task.CompletedTask;
        }
    }
} 