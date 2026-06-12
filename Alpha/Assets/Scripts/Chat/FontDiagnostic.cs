using TMPro;
using UnityEngine;

public class FontDiagnostic : MonoBehaviour
{
    void Start()
    {
        var fa = FindObjectOfType<TMP_Text>(true)?.font;
        if (fa == null) return;

        foreach (char c in "啊啥在线发送系统对话输入消息")
            fa.HasCharacter(c, true, true);
    }
}
