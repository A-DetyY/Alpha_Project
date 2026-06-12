using System;
using System.Collections;
using Newtonsoft.Json.Linq;

public class ToolDispatcher
{
    readonly FileTools     _fileTools;
    readonly ConfirmDialog _confirmDialog;
    readonly WebViewTools  _webViewTools;

    static readonly System.Collections.Generic.HashSet<string> NeedsConfirm =
        new System.Collections.Generic.HashSet<string>
        { "write_file", "open_webview", "export_to_downloads" };

    public ToolDispatcher(FileTools fileTools, ConfirmDialog confirmDialog, WebViewTools webViewTools = null)
    {
        _fileTools     = fileTools;
        _confirmDialog = confirmDialog;
        _webViewTools  = webViewTools;
    }

    public IEnumerator Dispatch(string toolName, string inputJson, Action<string> onResult)
    {
        JObject input;
        try { input = JObject.Parse(inputJson); }
        catch { onResult("[error] 工具参数解析失败"); yield break; }

        if (NeedsConfirm.Contains(toolName))
        {
            string prompt = BuildConfirmPrompt(toolName, input);
            bool allowed = false;
            yield return _confirmDialog.Ask(prompt, r => allowed = r);
            if (!allowed) { onResult("用户已拒绝此操作"); yield break; }
        }

        if (toolName == "open_webview" || toolName == "eval_js")
        {
            if (_webViewTools != null)
                yield return _webViewTools.Execute(toolName, input, onResult);
            else
                onResult("[error] WebViewTools 未初始化");
            yield break;
        }

        onResult(ExecuteTool(toolName, input));
    }

    string BuildConfirmPrompt(string toolName, JObject input) => toolName switch
    {
        "write_file"          => $"AI 要创建/覆盖文件：{input["path"]}，是否允许？",
        "open_webview"        => $"AI 要在 WebView 中打开：{input["path"]}，是否允许？",
        "export_to_downloads" => $"AI 要将项目 {input["project_path"]} 导出到 Download 目录，是否允许？",
        _                     => $"AI 要执行操作：{toolName}，是否允许？"
    };

    string ExecuteTool(string toolName, JObject input) => toolName switch
    {
        "read_file"           => _fileTools.ReadFile(input["path"]?.ToString() ?? ""),
        "write_file"          => _fileTools.WriteFile(
                                     input["path"]?.ToString()    ?? "",
                                     input["content"]?.ToString() ?? ""),
        "list_directory"      => _fileTools.ListDirectory(input["path"]?.ToString() ?? ""),
        "export_to_downloads" => "[error] 导出功能在计划B中实现",
        _                     => $"[error] 未知工具：{toolName}"
    };
}
