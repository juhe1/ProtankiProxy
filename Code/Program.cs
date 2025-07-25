using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace ProtankiProxy;

class Program
{
	// App entry point
	public static void Main(string[] args)
	{
		BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
	}

	public static AppBuilder BuildAvaloniaApp() =>
		AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace();
}
