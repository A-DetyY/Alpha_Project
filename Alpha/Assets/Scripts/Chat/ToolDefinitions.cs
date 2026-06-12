public static class ToolDefinitions
{
    public const string AllToolsJson = @"[
  {
    ""type"": ""function"",
    ""function"": {
      ""name"": ""read_file"",
      ""description"": ""读取工作区内指定路径的文件内容"",
      ""parameters"": {
        ""type"": ""object"",
        ""properties"": {
          ""path"": { ""type"": ""string"", ""description"": ""相对于工作区根目录的文件路径"" }
        },
        ""required"": [""path""]
      }
    }
  },
  {
    ""type"": ""function"",
    ""function"": {
      ""name"": ""write_file"",
      ""description"": ""在工作区内创建或覆盖文件"",
      ""parameters"": {
        ""type"": ""object"",
        ""properties"": {
          ""path"":    { ""type"": ""string"", ""description"": ""相对于工作区根目录的文件路径"" },
          ""content"": { ""type"": ""string"", ""description"": ""文件内容"" }
        },
        ""required"": [""path"", ""content""]
      }
    }
  },
  {
    ""type"": ""function"",
    ""function"": {
      ""name"": ""list_directory"",
      ""description"": ""列出工作区内指定目录的文件和子目录"",
      ""parameters"": {
        ""type"": ""object"",
        ""properties"": {
          ""path"": { ""type"": ""string"", ""description"": ""相对路径，空字符串表示工作区根目录"" }
        },
        ""required"": [""path""]
      }
    }
  },
  {
    ""type"": ""function"",
    ""function"": {
      ""name"": ""open_webview"",
      ""description"": ""在 WebView 中打开工作区内的 HTML 文件"",
      ""parameters"": {
        ""type"": ""object"",
        ""properties"": {
          ""path"": { ""type"": ""string"", ""description"": ""相对于工作区根目录的 HTML 文件路径"" }
        },
        ""required"": [""path""]
      }
    }
  },
  {
    ""type"": ""function"",
    ""function"": {
      ""name"": ""eval_js"",
      ""description"": ""在当前打开的 WebView 中执行 JavaScript 并返回结果"",
      ""parameters"": {
        ""type"": ""object"",
        ""properties"": {
          ""script"": { ""type"": ""string"", ""description"": ""要执行的 JavaScript 代码"" }
        },
        ""required"": [""script""]
      }
    }
  },
  {
    ""type"": ""function"",
    ""function"": {
      ""name"": ""export_to_downloads"",
      ""description"": ""将工作区内的项目目录导出到 Android Download 目录"",
      ""parameters"": {
        ""type"": ""object"",
        ""properties"": {
          ""project_path"": { ""type"": ""string"", ""description"": ""工作区内的项目目录名"" }
        },
        ""required"": [""project_path""]
      }
    }
  }
]";
}
