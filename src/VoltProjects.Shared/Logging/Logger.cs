using Figgle;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace VoltProjects.Shared.Logging;

/// <summary>
///     Holder class for Serilog logger
/// </summary>
public sealed class Logger : IDisposable
{
    internal Logger(IConfiguration configuration)
    {
        const string logFormat = "{Timestamp:dd-MM hh:mm:ss tt} [{Level:u3}] [T: {ThreadId}] [{SourceContext}] {Message:lj}{NewLine}{Exception}";
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .WriteTo.Console(outputTemplate: logFormat)
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        //NOTE: We are adding a new line due to all the stuff at the start of each log message
        Log.Information($"\n{FiggleFonts.Graffiti.Render("VoltProjects")}");
        Log.Information("VoltProjects starting...");
    }
    
    public void Dispose()
    {
        Log.CloseAndFlush();
    }
}