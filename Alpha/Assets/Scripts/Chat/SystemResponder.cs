using System;
using System.Collections;
using UnityEngine;

public class SystemResponder
{
    readonly ClaudeApiClient _apiClient;

    public SystemResponder()
    {
        _apiClient = new ClaudeApiClient();
    }

    public IEnumerator GetReplyCoroutine(string userMessage, Action<string> onReply)
    {
        yield return _apiClient.SendCoroutine(userMessage, onReply);
    }
}
