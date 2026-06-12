using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class ChatMessage
{
    public string role;
    public string content;
}

[Serializable]
class MessagesWrapper
{
    public string model;
    public List<ChatMessage> messages;
}

[Serializable]
class ResponseChoice
{
    public ChatMessage message;
}

[Serializable]
class ApiResponse
{
    public ResponseChoice[] choices;
}

public class ClaudeApiClient
{
    const float Timeout = 30f;

    readonly string _url;
    readonly string _key;
    readonly string _model;
    readonly List<ChatMessage> _history = new List<ChatMessage>();

    public ClaudeApiClient()
    {
        var cfg = Resources.Load<ApiConfig>("ApiConfig");
        if (cfg == null)
        {
            Debug.LogError("[ClaudeApiClient] ApiConfig.asset not found in Resources/");
            _url = _key = _model = string.Empty;
            return;
        }
        _url   = cfg.apiUrl;
        _key   = cfg.apiKey;
        _model = cfg.model;
    }

    public IEnumerator SendCoroutine(string userMessage, Action<string> onReply)
    {
        _history.Add(new ChatMessage { role = "user", content = userMessage });

        var wrapper = new MessagesWrapper { model = _model, messages = _history };
        string json = JsonUtility.ToJson(wrapper);

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        using var req = new UnityWebRequest(_url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.timeout         = (int)Timeout;
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + _key);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.ConnectionError ||
            req.result == UnityWebRequest.Result.DataProcessingError)
        {
            Debug.LogError("[ClaudeApiClient] Network error: " + req.error);
            _history.RemoveAt(_history.Count - 1);
            onReply("[网络连接失败，请检查网络]");
            yield break;
        }

        if (req.responseCode >= 400)
        {
            Debug.LogError($"[ClaudeApiClient] HTTP {req.responseCode}: {req.downloadHandler.text}");
            _history.RemoveAt(_history.Count - 1);
            onReply($"[服务异常（HTTP {req.responseCode}），请稍后重试]");
            yield break;
        }

        ApiResponse response;
        try
        {
            response = JsonUtility.FromJson<ApiResponse>(req.downloadHandler.text);
        }
        catch (Exception e)
        {
            Debug.LogError("[ClaudeApiClient] JSON parse error: " + e.Message);
            _history.RemoveAt(_history.Count - 1);
            onReply("[响应解析失败，请稍后重试]");
            yield break;
        }

        if (response?.choices == null || response.choices.Length == 0)
        {
            Debug.LogError("[ClaudeApiClient] Empty choices in response: " + req.downloadHandler.text);
            _history.RemoveAt(_history.Count - 1);
            onReply("[响应解析失败，请稍后重试]");
            yield break;
        }

        string reply = response.choices[0].message.content;
        _history.Add(new ChatMessage { role = "assistant", content = reply });
        onReply(reply);
    }
}
