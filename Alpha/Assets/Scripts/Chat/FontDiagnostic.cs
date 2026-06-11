using TMPro;
using UnityEngine;

public class FontDiagnostic : MonoBehaviour
{
    void Start()
    {
        // Only check once — all TMP objects share the same font asset
        var fa = FindObjectOfType<TMP_Text>(true)?.font;
        if (fa == null) { Debug.LogError("[FontDiag] No TMP_Text found"); return; }

        var tex = fa.atlasTexture;
        Debug.Log($"[FontDiag] font={fa.name}  populationMode={fa.atlasPopulationMode}");
        Debug.Log($"[FontDiag] atlas tex={tex?.name ?? "NULL"}  size={tex?.width}x{tex?.height}  format={tex?.format}  readable={tex?.isReadable}");
        Debug.Log($"[FontDiag] glyphs={fa.glyphTable.Count}  chars={fa.characterTable.Count}");
        Debug.Log($"[FontDiag] atlasWidth={fa.atlasWidth}  atlasHeight={fa.atlasHeight}  atlasPopMode={fa.atlasPopulationMode}");

        // Force '啊' and '啥' into atlas and report
        foreach (char c in "啊啥在线发送系统对话输入消息")
        {
            bool ok = fa.HasCharacter(c, true, true);
            Debug.Log($"[FontDiag]   '{c}' (U+{(int)c:X4}) HasCharacter={ok}");
        }

        Debug.Log($"[FontDiag] after force: glyphs={fa.glyphTable.Count}  chars={fa.characterTable.Count}");
    }
}
