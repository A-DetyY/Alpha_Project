using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

public class SystemResponderTests
{
    [Test]
    public void Constructor_DoesNotThrow()
    {
        // ApiConfig.asset 在 Edit Mode 测试中可能不存在，
        // ClaudeApiClient 内部已用 Debug.LogError 降级处理，不抛出异常。
        Assert.DoesNotThrow(() =>
        {
            var responder = new SystemResponder();
        });
    }

    [Test]
    public void GetReplyCoroutine_ReturnsNonNullEnumerator()
    {
        var responder = new SystemResponder();
        IEnumerator coroutine = responder.GetReplyCoroutine("测试消息", _ => { });
        Assert.IsNotNull(coroutine);
    }
}
