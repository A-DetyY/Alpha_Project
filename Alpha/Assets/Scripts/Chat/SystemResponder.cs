using UnityEngine;

public class SystemResponder
{
    private static readonly string[] Replies =
    {
        "已收到你的消息，正在处理中...",
        "系统运行正常，请继续输入指令。",
        "数据检索完毕，结果已记录。",
        "指令已执行，等待下一步操作。",
        "连接稳定，信号强度满格。",
        "扫描完成，未发现异常。",
        "任务已加入队列，预计稍后完成。",
        "权限验证通过，欢迎使用高级功能。",
        "日志已更新，继续监听中。",
        "模块加载成功，系统就绪。",
        "指令解析完毕，正在执行第二阶段。",
        "好的，我明白了。",
        "请求已转发至主控节点。",
        "能量储备充足，继续执行任务。"
    };

    public string GetReply(string userMessage)
    {
        return Replies[Random.Range(0, Replies.Length)];
    }
}
