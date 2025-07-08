using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using ProtankiProxy.Models;

namespace ProtankiProxy.ViewModels
{
    public class PacketListViewModel
    {
        public ObservableCollection<PacketListItem> Packets { get; } = new();
        public ObservableCollection<PacketListItem> FilteredPackets { get; } = new();
        public FlatTreeDataGridSource<PacketListItem> TreeDataGridSource { get; }
        public string SearchText { get; set; } = string.Empty;

        public PacketListViewModel()
        {
            TreeDataGridSource = new FlatTreeDataGridSource<PacketListItem>(FilteredPackets)
            {
                Columns =
                {
                    new TextColumn<PacketListItem, DateTime>("Time", x => x.Timestamp, width: new Avalonia.Controls.GridLength(130)),
                    new TextColumn<PacketListItem, string>("Source", x => x.Source),
                    new TextColumn<PacketListItem, string>("Destination", x => x.Destination),
                    new TextColumn<PacketListItem, string>("Type", x => x.TypeWithId),
                    new TextColumn<PacketListItem, int>("Size", x => x.Size, width: new Avalonia.Controls.GridLength(120)),
                },
            };
        }

        public void AddPacket(PacketListItem packet)
        {
            Packets.Add(packet);
            if (string.IsNullOrEmpty(SearchText) || PacketMatchesSearch(packet, SearchText))
            {
                FilteredPackets.Add(packet);
            }
        }

        public void ApplyFilter(string search)
        {
            SearchText = search;
            FilteredPackets.Clear();
            var filtered = string.IsNullOrEmpty(search)
                ? Packets
                : Packets.Where(p => PacketMatchesSearch(p, search));
            foreach (var item in filtered)
                FilteredPackets.Add(item);
        }

        private bool PacketMatchesSearch(PacketListItem p, string search)
        {
            return (p.Timestamp.ToString("HH:mm:ss.fff").Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                   (p.Source?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (p.Destination?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (p.TypeWithId?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                   (p.Size.ToString().Contains(search));
        }
    }
} 