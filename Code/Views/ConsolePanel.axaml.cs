using Avalonia.Controls;
using ProtankiProxy.Logging;
using Avalonia.Threading;

namespace ProtankiProxy.Views
{
    public partial class ConsolePanel : UserControl
    {
        public TextBox? GetConsoleOutput() => this.FindControl<TextBox>("ConsoleOutput");
        public ConsolePanel()
        {
            InitializeComponent();
            GuiLogger.Initialize(message =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    AppendLog(message);
                });
            });
        }

        public void AppendLog(string message)
        {
            var box = GetConsoleOutput();
            if (box != null)
            {
                box.Text += message + "\n";
                box.CaretIndex = box.Text.Length;
            }
        }
    }
} 