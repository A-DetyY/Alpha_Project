using TMPro;
using UnityEditor;
using UnityEngine;

public static class ChatFontFixer
{
    [MenuItem("Tools/Fix Chat Font Atlas")]
    public static void FixAtlas()
    {
        const string fontPath = "Assets/Res/Fonts/AlibabaPuHuiTi-3-45-Light SDF.asset";
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
        if (font == null)
        {
            Debug.LogError("[ChatFontFixer] Font not found at: " + fontPath);
            return;
        }

        const int atlasSize = 1024;
        const int pointSize = 90;
        const int padding   = 5;

        // Step 1: write settings so ClearFontAssetData reads correct atlas size
        var so = new SerializedObject(font);
        so.Update();
        so.FindProperty("m_AtlasPopulationMode").intValue          = 1;
        so.FindProperty("m_AtlasWidth").intValue                   = atlasSize;
        so.FindProperty("m_AtlasHeight").intValue                  = atlasSize;
        so.FindProperty("m_AtlasPadding").intValue                 = padding;
        so.FindProperty("m_IsMultiAtlasTexturesEnabled").boolValue = true;
        var cs = so.FindProperty("m_CreationSettings");
        cs.FindPropertyRelative("pointSize").intValue   = pointSize;
        cs.FindPropertyRelative("atlasWidth").intValue  = atlasSize;
        cs.FindPropertyRelative("atlasHeight").intValue = atlasSize;
        cs.FindPropertyRelative("padding").intValue     = padding;
        so.FindProperty("m_FaceInfo")
          .FindPropertyRelative("m_PointSize").intValue = pointSize;
        so.ApplyModifiedProperties();

        // Step 2: ensure m_AtlasTextures[0] is a valid texture before calling
        // ClearFontAssetData — it crashes if the array is null or element is null.
        // TMP itself uses new Texture2D(0, 0, Alpha8, false) as the seed texture;
        // ClearAtlasTextures will Resize it to m_AtlasWidth x m_AtlasHeight.
        if (font.atlasTextures == null || font.atlasTextures.Length == 0 || font.atlasTextures[0] == null)
        {
            var seedTex = new Texture2D(0, 0, TextureFormat.Alpha8, false);
            seedTex.name = font.name + " Atlas";
            AssetDatabase.AddObjectToAsset(seedTex, fontPath);

            so = new SerializedObject(font);
            so.Update();
            var texArr = so.FindProperty("m_AtlasTextures");
            texArr.arraySize = 1;
            texArr.GetArrayElementAtIndex(0).objectReferenceValue = seedTex;
            so.ApplyModifiedProperties();

            AssetDatabase.SaveAssets();
        }

        // Step 3: let TMP's own code path clear and resize the atlas correctly
        font.ClearFontAssetData(setAtlasSizeToZero: false);

        // Step 4: bind atlas texture to the material's _MainTex
        // TMP Dynamic mode updates this at runtime, but the saved material has
        // fileID:0 — wire it now so the first frame renders correctly
        if (font.material != null && font.atlasTexture != null)
        {
            font.material.SetTexture(ShaderUtilities.ID_MainTex, font.atlasTexture);
            EditorUtility.SetDirty(font.material);
        }

        EditorUtility.SetDirty(font);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[ChatFontFixer] Done. Dynamic atlas={atlasSize} pointSize={pointSize}");
    }
}
