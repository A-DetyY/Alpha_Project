using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class ClaudeApiClient
{
    const int   MaxToolCalls = 20;
    const float Timeout      = 30f;

    readonly string        _url;
    readonly string        _key;
    readonly string        _model;
    readonly List<JObject> _history  = new List<JObject>();
    readonly JArray        _toolsDef = JArray.Parse(ToolDefinitions.AllToolsJson);

    ToolDispatcher _dispatcher;

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

    public void SetDispatcher(ToolDispatcher dispatcher) => _dispatcher = dispatcher;

    public IEnumerator SendCoroutine(string userMessage, Action<string> onReply)
    {
        _history.Add(new JObject { ["role"] = "user", ["content"] = userMessage });

        int    toolCallCount   = 0;
        string accumulatedText = null;

        while (true)
        {
            var requestBody = new JObject
            {
                ["model"]      = _model,
                ["max_tokens"] = 4096,
                ["tools"]      = _toolsDef,
                ["messages"]   = new JArray(_history)
            };

            byte[] bodyRaw = Encoding.UTF8.GetBytes(requestBody.ToString(Formatting.None));
            var req = new UnityWebRequest(_url, "POST");
            req.uploadHandler   = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.timeout         = (int)Timeout;
            req.SetRequestHeader("Content-Type",  "application/json");
            req.SetRequestHeader("Authorization", "Bearer " + _key);

            yield return req.SendWebRequest();

            string responseText = req.downloadHandler.text;
            bool   netError     = req.result == UnityWebRequest.Result.ConnectionError ||
                                  req.result == UnityWebRequest.Result.DataProcessingError;
            long   statusCode   = req.responseCode;
            req.Dispose();

            if (netError)
            {
                Debug.LogError("[ClaudeApiClient] Network error");
                _history.RemoveAt(_history.Count - 1);
                onReply("[网络连接失败，请检查网络]");
                yield break;
            }

            if (statusCode >= 400)
            {
                Debug.LogError($"[ClaudeApiClient] HTTP {statusCode}: {responseText}");
                _history.RemoveAt(_history.Count - 1);
                onReply($"[服务异常（HTTP {statusCode}），请稍后重试]");
                yield break;
            }

            JObject response;
            try { response = JObject.Parse(responseText); }
            catch (Exception e)
            {
                Debug.LogError("[ClaudeApiClient] JSON parse error: " + e.Message);
                _history.RemoveAt(_history.Count - 1);
                onReply("[响应解析失败，请稍后重试]");
                yield break;
            }

            // OpenAI 格式：choices[0].message
            JObject message = response["choices"]?[0]?["message"] as JObject;
            if (message == null)
            {
                Debug.LogError("[ClaudeApiClient] Unexpected response: " + responseText);
                _history.RemoveAt(_history.Count - 1);
                onReply("[响应解析失败，请稍后重试]");
                yield break;
            }

            // 收集文本
            string textContent = message["content"]?.Type == JTokenType.String
                ? message["content"].ToString()
                : null;
            if (!string.IsNullOrEmpty(textContent))
                accumulatedText = accumulatedText == null ? textContent : accumulatedText + "\n" + textContent;

            // 检查 tool_calls（OpenAI 格式）
            JArray toolCalls = message["tool_calls"] as JArray;
            if (toolCalls == null || toolCalls.Count == 0)
            {
                _history.Add(new JObject { ["role"] = "assistant", ["content"] = textContent ?? "" });
                onReply(accumulatedText ?? "[无回复内容]");
                yield break;
            }

            // 将含 tool_calls 的助手消息存入历史
            _history.Add(new JObject
            {
                ["role"]       = "assistant",
                ["content"]    = textContent,
                ["tool_calls"] = toolCalls
            });

            // 逐个执行工具调用
            foreach (var call in toolCalls)
            {
                if (toolCallCount >= MaxToolCalls)
                {
                    onReply("任务已超出工具调用上限（20次），请重新描述需求。");
                    yield break;
                }
                toolCallCount++;

                string callId    = call["id"]?.ToString() ?? "";
                string toolName  = call["function"]?["name"]?.ToString() ?? "";
                string inputJson = call["function"]?["arguments"]?.ToString() ?? "{}";

                string toolResult = null;
                if (_dispatcher != null)
                    yield return _dispatcher.Dispatch(toolName, inputJson, r => toolResult = r);
                else
                    toolResult = "[error] ToolDispatcher 未初始化";

                _history.Add(new JObject
                {
                    ["role"]         = "tool",
                    ["tool_call_id"] = callId,
                    ["content"]      = toolResult
                });
            }
            // 继续循环，将工具结果带入下一轮请求
        }
    }
}
