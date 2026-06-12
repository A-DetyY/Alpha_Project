using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class FileTools
{
    readonly string _workspace;

    public FileTools(string workspacePath)
    {
        _workspace = Path.GetFullPath(workspacePath);
        Directory.CreateDirectory(_workspace);
    }

    public string ReadFile(string path)
    {
        if (!IsSafePath(path, out string full))
            return "[error] 路径不合法：" + path;
        if (!File.Exists(full))
            return "[error] 文件不存在：" + path;
        try { return File.ReadAllText(full); }
        catch (Exception e) { return "[error] 读取失败：" + e.Message; }
    }

    public string WriteFile(string path, string content)
    {
        if (!IsSafePath(path, out string full))
            return "[error] 路径不合法：" + path;
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            File.WriteAllText(full, content);
            return "ok";
        }
        catch (Exception e) { return "[error] 写入失败：" + e.Message; }
    }

    public string ListDirectory(string path)
    {
        string dir;
        if (string.IsNullOrEmpty(path))
            dir = _workspace;
        else if (IsSafePath(path, out string full))
            dir = full;
        else
            return "[error] 路径不合法：" + path;

        if (!Directory.Exists(dir))
            return "[error] 目录不存在：" + path;

        var entries = new List<object>();
        foreach (var f in Directory.GetFiles(dir))
            entries.Add(new { name = Path.GetFileName(f), type = "file", size = new FileInfo(f).Length });
        foreach (var d in Directory.GetDirectories(dir))
            entries.Add(new { name = Path.GetFileName(d), type = "dir",  size = 0L });

        return JsonConvert.SerializeObject(entries);
    }

    bool IsSafePath(string relativePath, out string fullPath)
    {
        fullPath = null;
        if (string.IsNullOrEmpty(relativePath) || relativePath.Contains(".."))
            return false;
        fullPath = Path.GetFullPath(Path.Combine(_workspace, relativePath));
        return IsSafeFinal(fullPath);
    }

    bool IsSafeFinal(string fullPath)
    {
        string sep = Path.DirectorySeparatorChar.ToString();
        return fullPath.StartsWith(_workspace + sep, StringComparison.OrdinalIgnoreCase)
            || string.Equals(fullPath, _workspace, StringComparison.OrdinalIgnoreCase);
    }
}
