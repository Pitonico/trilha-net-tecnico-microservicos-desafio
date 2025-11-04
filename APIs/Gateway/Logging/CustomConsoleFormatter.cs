using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;

namespace APIs.Gateway.Logging
{
    public class CustomConsoleFormatter : ConsoleFormatter
{
    public new const string Name = "minimal";
    public CustomConsoleFormatter() : base(Name) { }

    public override void Write<TState>(
        in LogEntry<TState> logEntry,
        IExternalScopeProvider? scopeProvider,
        TextWriter textWriter)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string level = logEntry.LogLevel switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "CRIT",
            _ => "UNK"
        };
        var message = logEntry.Formatter(logEntry.State, logEntry.Exception);
        
        string category = logEntry.Category;
        string prefix = category.Split('.').LastOrDefault() ?? "Unknown";

        var builder = new StringBuilder();
        builder.Append($"[{timestamp}] [{level}] [{prefix}] {message}");

        if (logEntry.Exception != null)
        {
            builder.AppendLine();
            builder.AppendLine(logEntry.Exception.ToString());
        }

        textWriter.WriteLine(builder.ToString());
    }
}
}