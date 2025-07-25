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
        public Packet Packet { get; }

        public PacketEventArgs(Packet packet, string source, string destination)
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
