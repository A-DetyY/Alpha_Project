using NUnit.Framework;

public class SystemResponderTests
{
    [Test]
    public void GetReply_ReturnsNonEmptyString()
    {
        var responder = new SystemResponder();
        string reply = responder.GetReply("你好");
        Assert.IsNotNull(reply);
        Assert.IsNotEmpty(reply);
    }

    [Test]
    public void GetReply_NeverThrowsOnRepeatedCalls()
    {
        var responder = new SystemResponder();
        Assert.DoesNotThrow(() =>
        {
            for (int i = 0; i < 100; i++)
                responder.GetReply("test");
        });
    }
}
