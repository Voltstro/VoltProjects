using System.Text.Json;
using VoltProjects.Shared.Models;

namespace VoltProjects.Builder.Core;

public struct TocItem
{
    public string TocRel { get; set; }
    
    public LinkItem TocLinkItem { get; set; }

    public string FormattedString()
    {
        string linkItemJson = JsonSerializer.Serialize(TocLinkItem);
        return $"ROW('{TocRel}','{linkItemJson}'::jsonb)";
    }

    public override string ToString()
    {
        string linkItemJson = JsonSerializer.Serialize(TocLinkItem);
        string formattedRow = $"ROW('{TocRel}','{linkItemJson}'::jsonb)";
        return formattedRow;
    }
}