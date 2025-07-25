using System;
using ProtankiNetworking.Packets;

namespace ProtankiProxy.Models
{
    public class PacketListItem
    {
        public DateTime Timestamp { get; set; }
        public string Source { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;
        public int Size { get; set; }
        public string Type { get; set; } = string.Empty;
        public Packet Packet { get; set; } = default!;

        public string TypeWithId => Packet != null ? $"{Type} (ID: {Packet.Id})" : Type;

        public override string ToString()
        {
            return $"[{Timestamp:HH:mm:ss.fff}] {Source} -> {Destination} | {Type} ({Size} bytes)";
        }
    }
}

