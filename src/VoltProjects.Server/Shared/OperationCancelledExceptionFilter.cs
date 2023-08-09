using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace VoltProjects.Server.Shared;

public sealed class OperationCancelledExceptionFilter : ExceptionFilterAttribute
{
    private readonly ILogger<OperationCancelledExceptionFilter> logger;

    public OperationCancelledExceptionFilter(ILogger<OperationCancelledExceptionFilter> logger)
    {
        this.logger = logger;
    }
    
    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is not OperationCanceledException) 
            return;
        
        logger.LogInformation("Request was cancelled");
        context.ExceptionHandled = true;
        context.Result = new StatusCodeResult(400);
    }
}