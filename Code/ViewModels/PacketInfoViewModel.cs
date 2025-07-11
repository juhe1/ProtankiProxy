using System;
using System.Text;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using ProtankiProxy.Models;
using Newtonsoft.Json.Converters;

namespace ProtankiProxy.ViewModels
{
    public class PacketInfoViewModel
    {
        public string PacketInfo { get; private set; } = string.Empty;
        public string HexView { get; private set; } = string.Empty;
        public string DecryptedHexView { get; private set; } = string.Empty;
        private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Objects,
        };

        public PacketInfoViewModel()
        {
            // Add StringEnumConverter to the serializer settings
            jsonSerializerSettings.Converters.Add(new StringEnumConverter());
        }

        public void SetPacket(PacketListItem? selectedPacket)
        {
            if (selectedPacket == null || selectedPacket.Packet == null)
            {
                PacketInfo = string.Empty;
                HexView = string.Empty;
                DecryptedHexView = string.Empty;
                return;
            }

            var packet = selectedPacket.Packet;
            var infoSb = new StringBuilder();
            infoSb.AppendLine($"Packet Type: {packet.GetType().Name}");
            infoSb.AppendLine($"Packet ID: {packet.Id}");
            infoSb.AppendLine($"Description: {packet.GetType().GetProperty("Description", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.GetValue(null)}");
            infoSb.AppendLine();

            if (packet.ObjectByAttributeName.Count > 0)
            {
                foreach (var kvp in packet.ObjectByAttributeName)
                {
                    if (kvp.Value is JsonNode)
                    {
                        infoSb.AppendLine($"{kvp.Key}:");
                        var json = ((JsonNode)kvp.Value).ToJsonString(new System.Text.Json.JsonSerializerOptions
                        {
                            WriteIndented = true
                        });
                        infoSb.AppendLine(json);
                        infoSb.AppendLine();
                    }
                    else
                    {
                        infoSb.AppendLine($"{kvp.Key}:");
                        var json = JsonConvert.SerializeObject(kvp.Value, jsonSerializerSettings);
                        infoSb.AppendLine(json);
                        infoSb.AppendLine();
                    }
                }
            }
            else
            {
                infoSb.AppendLine("No attributes found in packet.");
            }

            PacketInfo = infoSb.ToString();
            HexView = CreateHexDump(packet.RawData);
            DecryptedHexView = CreateHexDump(packet.DecryptedData);
        }

        private string CreateHexDump(byte[] data)
        {
            if (data == null)
                return "No data available";

            var sb = new StringBuilder();
            for (int i = 0; i < data.Length; i += 16)
            {
                sb.Append($"{i:X8}: ");
                for (int j = 0; j < 16; j++)
                {
                    if (i + j < data.Length)
                    {
                        sb.Append($"{data[i + j]:X2} ");
                    }
                    else
                    {
                        sb.Append("   ");
                    }
                }
                sb.Append(" | ");
                for (int j = 0; j < 16; j++)
                {
                    if (i + j < data.Length)
                    {
                        byte b = data[i + j];
                        sb.Append(b >= 32 && b <= 126 ? (char)b : '.');
                    }
                }
                sb.AppendLine();
            }
            return sb.ToString();
        }
    }
} 
