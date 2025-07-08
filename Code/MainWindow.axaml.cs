using System;
using System.Net;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ProtankiProxy.Events;
using ProtankiProxy.Models;
using ProtankiProxy.Settings;
using ProtankiProxy.ViewModels;
using ProtankiProxy.Views;
using Serilog;

namespace ProtankiProxy;

public partial class MainWindow : Window
{
    private ProxyServer _proxyServer;
    private IPEndPoint _localEndPoint;
    private IPEndPoint _serverEndPoint;
    private ConnectionSettings _settings;
    private PacketListPanel _packetListPanel;
    private PacketInfoPanel _packetInfoPanel;
    private CancellationTokenSource _cancellationTokenSource;
    private PacketListViewModel _packetListViewModel;

    public MainWindow()
    {
        InitializeComponent();

        // Find UserControls
        _packetListPanel = this.FindControl<PacketListPanel>("PacketListPanel");
        _packetInfoPanel = this.FindControl<PacketInfoPanel>("PacketInfoPanel");

        _packetListViewModel = new PacketListViewModel();
        _packetListPanel.ManualInit(_packetListViewModel);

        // Wire up PacketListPanel to update PacketInfoPanel directly
        _packetListPanel.TargetPacketInfoPanel = _packetInfoPanel;

        _cancellationTokenSource = new CancellationTokenSource();
        LoadSettings();
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
                Packet = e.Packet,
            };

            _packetListViewModel.AddPacket(packetItem);
        });
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
            throw;
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
                _cancellationTokenSource.Token
            );
            _proxyServer.PacketReceived += OnPacketReceived;
            _proxyServer.Start();

            Log.Information("Proxy server started on {LocalEndPoint}", _localEndPoint);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to start proxy server");
            throw;
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

