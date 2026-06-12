using System;
using System.Collections;

public class SystemResponder
{
    readonly ClaudeApiClient _apiClient;

    public SystemResponder()
    {
        _apiClient = new ClaudeApiClient();
    }

    public void SetDispatcher(ToolDispatcher dispatcher)
    {
        _apiClient.SetDispatcher(dispatcher);
    }

    public IEnumerator GetReplyCoroutine(string userMessage, Action<string> onReply)
    {
        yield return _apiClient.SendCoroutine(userMessage, onReply);
    }
}
