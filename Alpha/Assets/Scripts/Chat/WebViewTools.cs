using System;
using System.Collections;
using System.IO;
using Newtonsoft.Json.Linq;

public class WebViewTools
{
    readonly WebViewManager _manager;
    readonly string         _workspace;

    public WebViewTools(WebViewManager manager, string workspace)
    {
        _manager   = manager;
        _workspace = workspace;
    }

    public IEnumerator Execute(string toolName, JObject input, Action<string> onResult)
    {
        switch (toolName)
        {
            case "open_webview":
            {
                string rel = input["path"]?.ToString() ?? "";
                if (string.IsNullOrEmpty(rel) || rel.Contains(".."))
                {
                    onResult("[error] 路径不合法：" + rel);
                    yield break;
                }
                string abs = Path.GetFullPath(Path.Combine(_workspace, rel));
                if (!abs.StartsWith(_workspace, StringComparison.OrdinalIgnoreCase))
                {
                    onResult("[error] 路径越界：" + rel);
                    yield break;
                }
                yield return _manager.Open(abs, onResult);
                break;
            }
            case "eval_js":
            {
                string script = input["script"]?.ToString() ?? "";
                yield return _manager.EvalJs(script, onResult);
                break;
            }
            default:
                onResult($"[error] 未知工具：{toolName}");
                break;
        }
    }
}
