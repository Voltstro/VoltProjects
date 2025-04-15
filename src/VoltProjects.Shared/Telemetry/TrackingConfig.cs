namespace VoltProjects.Shared.Telemetry;

public class TrackingConfig
{
    public const string SectionName = "Tracking";
    
    public string? OtlpEndpoint { get; set; }
    public string? OtlpHeaders { get; set; }
}