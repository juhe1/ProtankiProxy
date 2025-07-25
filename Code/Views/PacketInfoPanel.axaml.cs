using Avalonia.Controls;
using ProtankiProxy.Models;
using ProtankiProxy.ViewModels;

namespace ProtankiProxy.Views
{
	public partial class PacketInfoPanel : UserControl
	{
		public TextBox GetPacketInfo() => this.FindControl<TextBox>("PacketInfo")!;

		public TextBox GetHexView() => this.FindControl<TextBox>("HexView")!;

		public TextBox GetDecryptedHexView() => this.FindControl<TextBox>("DecryptedHexView")!;

		public PacketInfoPanel()
		{
			InitializeComponent();
			if (DataContext == null)
				DataContext = new PacketInfoViewModel();

			var fontFamily = new Avalonia.Media.FontFamily(
				"Consolas, Menlo, Monaco, 'Courier New', monospace"
			);
			GetPacketInfo().FontFamily = fontFamily;
			GetHexView().FontFamily = fontFamily;
			GetDecryptedHexView().FontFamily = fontFamily;
			GetPacketInfo().TextWrapping = Avalonia.Media.TextWrapping.NoWrap;
			GetHexView().TextWrapping = Avalonia.Media.TextWrapping.NoWrap;
			GetDecryptedHexView().TextWrapping = Avalonia.Media.TextWrapping.NoWrap;
		}

		public void DisplayPacket(PacketListItem? selectedPacket)
		{
			if (DataContext is PacketInfoViewModel vm)
			{
				vm.SetPacket(selectedPacket);
				GetPacketInfo().Text = vm.PacketInfo;
				GetHexView().Text = vm.HexView;
				GetDecryptedHexView().Text = vm.DecryptedHexView;
			}
		}
	}
}
