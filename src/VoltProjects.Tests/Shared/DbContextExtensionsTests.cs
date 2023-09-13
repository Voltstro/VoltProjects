using NSubstitute;
using VoltProjects.Shared;
using VoltProjects.Shared.Models;

namespace VoltProjects.Tests.Shared;

public class DbContextExtensionsTests
{
    [Test]
    public void GenerateParamsTest()
    {
        ProjectPage? page = Substitute.For<ProjectPage>();
        page.PublishedDate = new DateTime(2004);
        page.Title = "Test";
        
        (object?[], string[]) values = DbContextExtensions.GenerateParams(new []{page}, p => new {p.PublishedDate, p.Title});
        Assert.That(values.Item1, Has.Length.EqualTo(2));
        Assert.That(values.Item2, Has.Length.EqualTo(1));

        object?[] objectValues = values.Item1;
        DateTime dateTime = (DateTime)objectValues[0]!;
        string title = (string)objectValues[1]!;

        Assert.That(dateTime, Is.EqualTo(new DateTime(2004)));
        StringAssert.AreEqualIgnoringCase("Test", title);
        StringAssert.AreEqualIgnoringCase("ROW(@p0,@p1)", values.Item2[0]);
    }
    
    [Test]
    public void GenerateParamsIndexTest()
    {
        ProjectPage? page = Substitute.For<ProjectPage>();
        page.PublishedDate = new DateTime(2004);
        page.Title = "Test";
        
        (object?[], string[]) values = DbContextExtensions.GenerateParams(new []{page}, p => new {p.PublishedDate, p.Title}, true, 1);
        Assert.That(values.Item1, Has.Length.EqualTo(3));
        Assert.That(values.Item2, Has.Length.EqualTo(1));

        object?[] objectValues = values.Item1;
        Assert.That(objectValues[0], Is.Null);
        DateTime dateTime = (DateTime)objectValues[1]!;
        string title = (string)objectValues[2]!;

        Assert.That(dateTime, Is.EqualTo(new DateTime(2004)));
        StringAssert.AreEqualIgnoringCase("Test", title);
        StringAssert.AreEqualIgnoringCase("ROW(@p1,@p2)", values.Item2[0]);
    }
    
    [Test]
    public void GenerateParamsMultipleTest()
    {
        //TODO: We can test these pages better lol
        ProjectPage? page1 = Substitute.For<ProjectPage>();
        page1.PublishedDate = new DateTime(2004);
        page1.Title = "Test";
        
        ProjectPage? page2 = Substitute.For<ProjectPage>();
        page2.PublishedDate = new DateTime(2003);
        page2.Title = "Test2";
        
        (object?[], string[]) values = DbContextExtensions.GenerateParams(new []{page1, page2}, p => new {p.PublishedDate, p.Title});
        Assert.That(values.Item1, Has.Length.EqualTo(4));
        Assert.That(values.Item2, Has.Length.EqualTo(2));

        //Page 1
        object?[] objectValues = values.Item1;
        DateTime dateTime1 = (DateTime)objectValues[0]!;
        string title1 = (string)objectValues[1]!;

        Assert.That(dateTime1, Is.EqualTo(new DateTime(2004)));
        StringAssert.AreEqualIgnoringCase("Test", title1);
        
        //Page 2
        DateTime dateTime2 = (DateTime)objectValues[2]!;
        string title2 = (string)objectValues[3]!;

        Assert.That(dateTime2, Is.EqualTo(new DateTime(2003)));
        StringAssert.AreEqualIgnoringCase("Test2", title2);
        
        StringAssert.AreEqualIgnoringCase("ROW(@p0,@p1)", values.Item2[0]);
        StringAssert.AreEqualIgnoringCase("ROW(@p2,@p3)", values.Item2[1]);
    }
}