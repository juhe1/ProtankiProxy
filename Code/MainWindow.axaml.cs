using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Text.Json.Nodes;
using System.Threading;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Models.TreeDataGrid;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Newtonsoft.Json;
using ProtankiProxy.Events;
using ProtankiProxy.Logging;
using ProtankiProxy.Models;
using ProtankiProxy.Settings;
using Serilog;
using Newtonsoft.Json.Converters;

namespace ProtankiProxy;

public partial class MainWindow : Window
{
    private ProxyServer _proxyServer;
    private IPEndPoint _localEndPoint;
    private IPEndPoint _serverEndPoint;
    private ConnectionSettings _settings;
    private TextBox _consoleOutput;
    private TreeDataGrid _packetList;
    private TextBox _packetInfo;
    private TextBox _hexView;
    private TextBox _decryptedHexView;
    private CancellationTokenSource _cancellationTokenSource;
    private ObservableCollection<PacketListItem> _packets;
    private CheckBox _autoScrollCheckBox;
    private TextBox _packetSearchBox;
    private ObservableCollection<PacketListItem> _filteredPackets;
    private FlatTreeDataGridSource<PacketListItem> _treeDataGridSource;
    private static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings
	{
		Formatting = Formatting.Indented,
		TypeNameHandling = TypeNameHandling.Objects,
	};

    public MainWindow()
    {
        InitializeComponent();
        InitializeLogging();
        InitializePacketList();
        _cancellationTokenSource = new CancellationTokenSource();
        LoadSettings();
        InitializeSearch();

        // Add StringEnumConverter to the serializer settings
        jsonSerializerSettings.Converters.Add(new StringEnumConverter());
    }

    private void InitializeLogging()
    {
        // Find the console output TextBox
        _consoleOutput = this.FindControl<TextBox>("ConsoleOutput");
        _packetList = this.FindControl<TreeDataGrid>("PacketList");
        _packetInfo = this.FindControl<TextBox>("PacketInfo");
        _hexView = this.FindControl<TextBox>("HexView");
        _decryptedHexView = this.FindControl<TextBox>("DecryptedHexView");
        
        // Set monospace font for packet info and hex views
        if (_packetInfo != null && _hexView != null && _decryptedHexView != null)
        {
            var fontFamily = new Avalonia.Media.FontFamily("Consolas, Menlo, Monaco, 'Courier New', monospace");
            _packetInfo.FontFamily = fontFamily;
            _hexView.FontFamily = fontFamily;
            _decryptedHexView.FontFamily = fontFamily;
            _packetInfo.TextWrapping = Avalonia.Media.TextWrapping.NoWrap;
            _hexView.TextWrapping = Avalonia.Media.TextWrapping.NoWrap;
            _decryptedHexView.TextWrapping = Avalonia.Media.TextWrapping.NoWrap;
        }

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.Sink(new GuiConsoleSink(WriteToConsole))
            .CreateLogger();

        Log.Information("Application started");
    }

    private void InitializePacketList()
    {
        _packets = new ObservableCollection<PacketListItem>();
        _filteredPackets = new ObservableCollection<PacketListItem>();
        _packetList = this.FindControl<TreeDataGrid>("PacketList");
        
        // Create the TreeDataGrid source
        _treeDataGridSource = new FlatTreeDataGridSource<PacketListItem>(_filteredPackets)
        {
            Columns =
            {
                new TextColumn<PacketListItem, DateTime>("Time", x => x.Timestamp, width: new GridLength(130)),
                new TextColumn<PacketListItem, string>("Source", x => x.Source),
                new TextColumn<PacketListItem, string>("Destination", x => x.Destination),
                new TextColumn<PacketListItem, string>("Type", x => x.TypeWithId),
                new TextColumn<PacketListItem, int>("Size", x => x.Size, width: new GridLength(120)),
            },
        };

        if (_packetList != null)
        {
            _packetList.Source = _treeDataGridSource;
            _packetList.RowSelection.SelectionChanged += (s, e) =>
            {
                var selected = _packetList.RowSelection.SelectedItems;
                if (selected.Count > 0 && selected[0] is PacketListItem selectedPacket)
                {
                    OnPacketSelected(selectedPacket);
                }
            };
        }
        _autoScrollCheckBox = this.FindControl<CheckBox>("AutoScrollCheckBox");
    }

    private void InitializeSearch()
    {
        _packetSearchBox = this.FindControl<TextBox>("PacketSearchBox");
        if (_packetSearchBox != null)
        {
            _packetSearchBox.TextChanged += PacketSearchBox_TextChanged;
        }
    }

    private void PacketSearchBox_TextChanged(object sender, EventArgs e)
    {
        ApplyPacketFilter();
    }

    private void ApplyPacketFilter()
    {
        if (_packetSearchBox == null)
            return;
        string search = _packetSearchBox.Text?.Trim() ?? string.Empty;
        _filteredPackets.Clear();
        var filtered = string.IsNullOrEmpty(search)
            ? _packets
            : _packets.Where(p => PacketMatchesSearch(p, search));
        foreach (var item in filtered)
            _filteredPackets.Add(item);
    }

    private bool PacketMatchesSearch(PacketListItem p, string search)
    {
        return (p.Timestamp.ToString("HH:mm:ss.fff").Contains(search, StringComparison.OrdinalIgnoreCase)) ||
               (p.Source?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
               (p.Destination?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
               (p.Type?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
               (p.Size.ToString().Contains(search));
    }

    private void WriteToConsole(string message)
    {
        // Ensure we're on the UI thread
        Dispatcher.UIThread.Post(() =>
        {
            _consoleOutput.Text += message + "\n";
            // Scroll to the bottom
            _consoleOutput.CaretIndex = _consoleOutput.Text.Length;
        });
    }

    private void OnPacketReceived(object sender, PacketEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            var packetItem = new PacketListItem
            {
                Timestamp = e.Timestamp,
                Source = e.Source,
                Destination = e.Destination,
                Size = e.Size,
                Type = e.Type,
                Packet = e.Packet
            };

            _packets.Add(packetItem);
            // Only add to filteredPackets if it matches the current search
            string search = _packetSearchBox?.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(search) || PacketMatchesSearch(packetItem, search))
            {
                _filteredPackets.Add(packetItem);
            }

            // Auto-scroll to bottom if enabled
            if (_autoScrollCheckBox?.IsChecked == true && _packetList != null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    _packetList.Scroll.Offset = _packetList.Scroll.Offset.WithY(_packetList.Scroll.Extent.Height);
                }, DispatcherPriority.Loaded);
            }
        });
    }

    /// <summary>
    /// Creates a hex dump string from byte array
    /// </summary>
    /// <param name="data">The byte array to create hex dump from</param>
    /// <returns>Formatted hex dump string</returns>
    private string CreateHexDump(byte[] data)
    {
        if (data == null)
            return "No data available";

        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < data.Length; i += 16)
        {
            // Add offset
            sb.Append($"{i:X8}: ");
            
            // Add hex values
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
            
            // Add ASCII representation
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

    private void OnPacketSelected(PacketListItem selectedPacket)
    {
        var packet = selectedPacket.Packet;
        var infoSb = new System.Text.StringBuilder();
        
        // Add packet type and ID
        infoSb.AppendLine($"Packet Type: {packet.GetType().Name}");
        infoSb.AppendLine($"Packet ID: {packet.Id}");
        infoSb.AppendLine($"Description: {packet.GetType().GetProperty("Description", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.GetValue(null)}");
        infoSb.AppendLine();

        // Add each attribute and its value
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
                } else
                {
                    infoSb.AppendLine($"{kvp.Key}:");
                    var json = JsonConvert.SerializeObject(kvp.Value, Formatting.Indented);
                    infoSb.AppendLine(json);
                    infoSb.AppendLine();
                }
            }
        }
        else
        {
            infoSb.AppendLine("No attributes found in packet.");
        }

        _packetInfo.Text = infoSb.ToString();
        _hexView.Text = CreateHexDump(packet.RawData);
        _decryptedHexView.Text = CreateHexDump(packet.DecryptedData);
    }

    private async void LoadSettings()
    {
        try
    {
        _settings = await SettingsManager.LoadSettingsAsync();
        _localEndPoint = _settings.GetLocalEndPoint();
        _serverEndPoint = _settings.GetServerEndPoint();
        StartProxyServerInternal();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to load settings");
        }
    }

    private void StartProxyServer(object sender, RoutedEventArgs e)
    {
        StartProxyServerInternal();
    }

    private void StartProxyServerInternal()
    {
        try
        {
            // Create and start proxy server
            _proxyServer = new ProxyServer(
                _localEndPoint,
                _serverEndPoint.Address.ToString(),
                _serverEndPoint.Port,
                _cancellationTokenSource.Token);
            _proxyServer.PacketReceived += OnPacketReceived;
            _proxyServer.Start();

            Log.Information("Proxy server started on {LocalEndPoint}", _localEndPoint);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start proxy server");
        }
    }

    private async void OnConnectionSettingsClick(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new ConnectionSettingsWindow(_settings);
        await settingsWindow.ShowDialog(this);

        if (settingsWindow.LocalEndPoint != null && settingsWindow.ServerEndPoint != null)
        {
            // Stop current server if running
            if (_proxyServer != null)
            {
                _proxyServer.PacketReceived -= OnPacketReceived;
                await _proxyServer.StopAsync();
            }

            // Update endpoints
            _localEndPoint = settingsWindow.LocalEndPoint;
            _serverEndPoint = settingsWindow.ServerEndPoint;

            // Update and save settings
            _settings.UpdateFromEndPoints(_localEndPoint, _serverEndPoint);
            await SettingsManager.SaveSettingsAsync(_settings);

            // Restart server with new settings
            StartProxyServerInternal();
        }
    }

    private async void StopProxyServer(object sender, RoutedEventArgs e)
    {
        if (_proxyServer != null)
        {
            _proxyServer.PacketReceived -= OnPacketReceived;
            await _proxyServer.StopAsync();
        }
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
    }
}