using System;
using System.Net;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ProtankiProxy.Settings;

namespace ProtankiProxy
{
    public partial class ConnectionSettingsWindow : Window
    {
        public IPEndPoint LocalEndPoint { get; private set; }
        public IPEndPoint ServerEndPoint { get; private set; }

        public ConnectionSettingsWindow(ConnectionSettings currentSettings)
        {
            InitializeComponent();

            // Set current values
            LocalIpTextBox.Text = currentSettings.LocalIp;
            LocalPortTextBox.Text = currentSettings.LocalPort.ToString();
            ServerIpTextBox.Text = currentSettings.ServerIp;
            ServerPortTextBox.Text = currentSettings.ServerPort.ToString();
        }

        private void OnCancelClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnSaveClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var localIp = IPAddress.Parse(LocalIpTextBox.Text);
                var localPort = int.Parse(LocalPortTextBox.Text);
                var serverIp = IPAddress.Parse(ServerIpTextBox.Text);
                var serverPort = int.Parse(ServerPortTextBox.Text);

                LocalEndPoint = new IPEndPoint(localIp, localPort);
                ServerEndPoint = new IPEndPoint(serverIp, serverPort);

                Close();
            }
            catch (Exception ex)
            {
                // TODO: Show error message to user
                Console.WriteLine($"Invalid settings: {ex.Message}");
            }
        }
    }
} 