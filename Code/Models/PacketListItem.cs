using System;
using ProtankiNetworking.Packets;

namespace ProtankiProxy.Models
{
    public class PacketListItem
    {
        public DateTime Timestamp { get; set; }
        public string Source { get; set; }
        public string Destination { get; set; }
        public int Size { get; set; }
        public string Type { get; set; }
        public AbstractPacket Packet { get; set; }

        public string TypeWithId => Packet != null ? $"{Type} (ID: {Packet.Id})" : Type;

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss.fff}] {Source} -> {Destination} | {Type} ({Size} bytes)";
        }
    }
} 