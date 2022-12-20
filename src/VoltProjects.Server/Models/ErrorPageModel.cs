using System.Net;

namespace VoltProjects.Server.Models;

/// <summary>
///     Model for an error page
/// </summary>
public class ErrorPageModel
{
    public HttpStatusCode ErrorCode { get; init; }
}