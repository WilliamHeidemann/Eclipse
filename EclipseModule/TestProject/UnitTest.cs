using HelloWorld;

namespace TestProject;

public class Tests
{
    [Test]
    public void TestHelloWorld()
    {
        Assert.That(true, Is.EqualTo(true));
    }
}
