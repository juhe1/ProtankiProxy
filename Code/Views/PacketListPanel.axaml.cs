using System;
using System.Collections.Specialized;
using Avalonia.Controls;
using ProtankiProxy.Models;
using ProtankiProxy.ViewModels;
using Serilog;

namespace ProtankiProxy.Views
{
    public partial class PacketListPanel : UserControl
    {
        public PacketInfoPanel? TargetPacketInfoPanel { get; set; }
        public event Action<PacketListItem?>? PacketSelected;

        public TextBox GetPacketSearchBox() => this.FindControl<TextBox>("PacketSearchBox");

        public TreeDataGrid GetPacketList() => this.FindControl<TreeDataGrid>("PacketList");

        public CheckBox GetAutoScrollCheckBox() => this.FindControl<CheckBox>("AutoScrollCheckBox");

        public PacketListPanel()
        {
            InitializeComponent();

            this.AttachedToVisualTree += OnAttachedToVisualTree;
            this.GetPacketSearchBox().TextChanged += OnSearchBoxTextChanged;
        }

        /// <summary>
        /// Called manually outside the class.
        /// </summary>
        public void ManualInit(PacketListViewModel viewModel)
        {
            DataContext = viewModel;
            viewModel.FilteredPackets.CollectionChanged += OnFilteredPacketsChanged;
        }

        private void OnAttachedToVisualTree(
            object? sender,
            Avalonia.VisualTreeAttachmentEventArgs e
        )
        {
            var packetList = this.GetPacketList();
            if (packetList == null)
            {
                Console.WriteLine("ERROR: PacketList TreeDataGrid not found!");
                return;
            }
            if (DataContext is PacketListViewModel vm)
            {
                packetList.Source = vm.TreeDataGridSource;
            }
            if (packetList.RowSelection == null)
            {
                Console.WriteLine(
                    "ERROR: PacketList.RowSelection is still null after visual tree attachment!"
                );
                return;
            }
            packetList.RowSelection.SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object? sender, EventArgs e)
        {
            var selected = this.GetPacketList().RowSelection.SelectedItems;
            var selectedPacket = selected.Count > 0 ? selected[0] as PacketListItem : null;
            PacketSelected?.Invoke(selectedPacket);
            if (TargetPacketInfoPanel != null)
            {
                TargetPacketInfoPanel.DisplayPacket(selectedPacket);
            }
        }

        private void OnSearchBoxTextChanged(object? sender, EventArgs e)
        {
            if (DataContext is PacketListViewModel vm)
                vm.ApplyFilter(this.GetPacketSearchBox().Text?.Trim() ?? string.Empty);
        }

        private void OnFilteredPacketsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.GetAutoScrollCheckBox().IsChecked == true)
            {
                var grid = this.GetPacketList();
                Avalonia.Threading.Dispatcher.UIThread.Post(
                    () =>
                    {
                        grid.Scroll.Offset = grid.Scroll.Offset.WithY(grid.Scroll.Extent.Height);
                    },
                    Avalonia.Threading.DispatcherPriority.Background
                );
            }
        }
    }
}

