using VoltProjects.DocsBuilder.DocFx;

namespace VoltProjects.Tests.DocsBuilder;

public class DocfxTests
{
    [Test]
    public void BuildDocfxTest()
    {
        DocFxDocxBuilder builder = new DocFxDocxBuilder();
        builder.Build("Docs/Docfx");
    }
}