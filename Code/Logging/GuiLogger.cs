using System;
using Serilog;

namespace ProtankiProxy.Logging
{
	public static class GuiLogger
	{
		public static event Action<string>? LogMessage;

		public static void Initialize(Action<string> writeToConsole)
		{
			LogMessage += writeToConsole;
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Debug()
				.WriteTo.Console()
				.WriteTo.Sink(new GuiConsoleSink(message => LogMessage?.Invoke(message)))
				.CreateLogger();
			Log.Information("Application started");
		}

		public static void Write(string message)
		{
			LogMessage?.Invoke(message);
		}
	}
}
