using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace VoltProjects.Server.Services.Dev;

/// <summary>
///     Runs vite build watcher
/// </summary>
public sealed class ClientAppWatcher : BackgroundService
{
    private readonly string clientAppRootPath;
    private readonly ILogger<ClientAppWatcher> logger;

    private Process? backgroundProcess;
    
    public ClientAppWatcher(ILogger<ClientAppWatcher> appWatcherLogger, IWebHostEnvironment env)
    {
        logger = appWatcherLogger;
        clientAppRootPath = Path.GetFullPath($"{env.ContentRootPath}/ClientApp");

        if (!Directory.Exists(clientAppRootPath))
            throw new DirectoryNotFoundException("Fail to find client app directory!");
    }
    
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Starting background vite build watcher...");
        ProcessStartInfo startInfo = new ProcessStartInfo("yarn", "run watch")
        {
            WorkingDirectory = clientAppRootPath,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };
        
        try
        {
            backgroundProcess = Process.Start(startInfo);
            if (backgroundProcess == null)
            {
                logger.LogError("Process object failed to create!");
                return Task.CompletedTask;
            }
            
            backgroundProcess.ErrorDataReceived += ProcessOnErrorDataReceived;
            backgroundProcess.OutputDataReceived += BackgroundProcessOnOutputDataReceived;
        }
        catch (Exception ex)
        {
            logger.LogError("Failed to start background vite build watcher! {Ex}", ex);
            return Task.CompletedTask;
        }
        
        logger.LogInformation("Background vite build watcher is now running!");

        stoppingToken.Register(StopWatcher);
        return Task.CompletedTask;
    }

    private void BackgroundProcessOnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        logger.LogInformation("Vite build watcher: {Message}", e.Data);
    }

    private void ProcessOnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        logger.LogError("Error in background vite builder! {Message}", e.Data);
    }

    private void StopWatcher()
    {
        if(backgroundProcess == null)
            return;

        logger.LogInformation("Stopping background vite build watcher...");
        
        backgroundProcess.Kill();
        backgroundProcess.Dispose();
    }
}