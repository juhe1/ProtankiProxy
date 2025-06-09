using System;
using System.Threading.Tasks;
using Serilog.Core;
using Serilog.Events;

namespace ProtankiProxy.Logging
{
    public class GuiConsoleSink : ILogEventSink
    {
        private readonly Action<string> _writeAction;

        public GuiConsoleSink(Action<string> writeAction)
        {
            _writeAction = writeAction;
        }

        public void Emit(LogEvent logEvent)
        {
            var message = $"[{logEvent.Timestamp:HH:mm:ss}] [{logEvent.Level}] {logEvent.RenderMessage()}";
            if (logEvent.Exception != null)
            {
                message += $"\n{logEvent.Exception}";
            }
            _writeAction(message);
        }
    }
} 