using System.Net;

namespace VoltProjects.Server.Models;

public class ErrorPageModel
{
    public HttpStatusCode ErrorCode { get; init; }
}