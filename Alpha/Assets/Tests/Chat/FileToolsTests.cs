using System.IO;
using NUnit.Framework;
using UnityEngine;

public class FileToolsTests
{
    string _workspace;
    FileTools _ft;

    [SetUp]
    public void SetUp()
    {
        _workspace = Path.Combine(Application.temporaryCachePath, "ft_test_" + System.Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_workspace);
        _ft = new FileTools(_workspace);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_workspace))
            Directory.Delete(_workspace, true);
    }

    [Test]
    public void WriteFile_ThenReadFile_ReturnsContent()
    {
        string result = _ft.WriteFile("hello.txt", "world");
        Assert.AreEqual("ok", result);
        Assert.AreEqual("world", _ft.ReadFile("hello.txt"));
    }

    [Test]
    public void ReadFile_Missing_ReturnsError()
    {
        string result = _ft.ReadFile("nope.txt");
        StringAssert.StartsWith("[error]", result);
    }

    [Test]
    public void WriteFile_PathTraversal_ReturnsError()
    {
        string result = _ft.WriteFile("../../evil.txt", "bad");
        StringAssert.StartsWith("[error]", result);
    }

    [Test]
    public void ListDirectory_ReturnsJsonArray()
    {
        _ft.WriteFile("a.txt", "1");
        _ft.WriteFile("b.txt", "2");
        string json = _ft.ListDirectory("");
        StringAssert.Contains("a.txt", json);
        StringAssert.Contains("b.txt", json);
    }

    [Test]
    public void ListDirectory_PathTraversal_ReturnsError()
    {
        string result = _ft.ListDirectory("../outside");
        StringAssert.StartsWith("[error]", result);
    }
}
