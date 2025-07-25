using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Serilog;

namespace ProtankiProxy;

public partial class App : Application
{
	public override void Initialize()
	{
		try
		{
			AvaloniaXamlLoader.Load(this);
		}
		catch (System.Exception ex)
		{
			Log.Error(ex, "Failed to initialize application");
			throw;
		}
	}

	public override void OnFrameworkInitializationCompleted()
	{
		try
		{
			if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
			{
				desktop.MainWindow = new MainWindow();
			}

			base.OnFrameworkInitializationCompleted();
		}
		catch (System.Exception ex)
		{
			Log.Error(ex, "Failed to initialize framework");
			throw;
		}
	}
}
