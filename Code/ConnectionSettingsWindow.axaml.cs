using System;
using System.Net;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ProtankiProxy.Settings;

namespace ProtankiProxy
{
	public partial class ConnectionSettingsWindow : Window
	{
		public IPEndPoint LocalEndPoint { get; private set; } = null!;
		public IPEndPoint ServerEndPoint { get; private set; } = null!;

		public ConnectionSettingsWindow()
			: this(new ConnectionSettings()) { }

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
				var localIpText =
					LocalIpTextBox.Text
					?? throw new InvalidOperationException("Local IP is required.");
				var localPortText =
					LocalPortTextBox.Text
					?? throw new InvalidOperationException("Local port is required.");
				var serverIpText =
					ServerIpTextBox.Text
					?? throw new InvalidOperationException("Server IP is required.");
				var serverPortText =
					ServerPortTextBox.Text
					?? throw new InvalidOperationException("Server port is required.");

				var localIp = IPAddress.Parse(localIpText);
				var localPort = int.Parse(localPortText);
				var serverIp = IPAddress.Parse(serverIpText);
				var serverPort = int.Parse(serverPortText);

				LocalEndPoint = new IPEndPoint(localIp, localPort);
				ServerEndPoint = new IPEndPoint(serverIp, serverPort);

				Close();
			}
			catch (Exception ex)
			{
				// TODO: Show error message to user
				Console.WriteLine($"Invalid settings: {ex.Message}");
				throw;
			}
		}
	}
}
