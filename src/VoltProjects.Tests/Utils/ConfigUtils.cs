using Microsoft.Extensions.Options;
using NSubstitute;
using VoltProjects.Server.Shared;

namespace VoltProjects.Tests.Utils;

public class ConfigUtils
{
    public static IOptions<VoltProjectsConfig> CreateServerConfig()
    {
        IOptions<VoltProjectsConfig> options = Substitute.For<IOptions<VoltProjectsConfig>>();
        
        options.Value.Returns(new VoltProjectsConfig
        {
            SiteUrl = "http://localhost:5000",
            PublicUrl = "http://localhost:10000/devstoreaccount1/voltprojects/",
            ProjectVersionsCacheTime = TimeSpan.FromHours(1),
        });

        return options;
    }
}