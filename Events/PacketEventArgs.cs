using System;
using ProtankiNetworking.Packets;

namespace ProtankiProxy.Events
{
    public class PacketEventArgs : EventArgs
    {
        public DateTime Timestamp { get; }
        public string Source { get; }
        public string Destination { get; }
        public int Size { get; }
        public string Type { get; }
        public AbstractPacket Packet { get; }

        public PacketEventArgs(AbstractPacket packet, string source, string destination)
        {
            Timestamp = DateTime.Now;
            Source = source;
            Destination = destination;
            Size = packet.RawData.Length;
            Type = packet.GetType().Name;
            Packet = packet;
        }
    }
} 