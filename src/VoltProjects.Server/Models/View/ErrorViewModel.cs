using System.Net;
using VoltProjects.Shared.Models;

namespace VoltProjects.Server.Models.View;

/// <summary>
///     Model for an error page
/// </summary>
public class ErrorViewModel
{
    public HttpStatusCode? ErrorCode { get; init; }
    
    public string ErrorMessage { get; init; }
    
    public string ErrorMessageDetailed { get; init; }
    
    public ProjectNavModel? ProjectNavModel { get; init; }
}