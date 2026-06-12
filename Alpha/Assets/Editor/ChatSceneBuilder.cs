using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class ChatSceneBuilder
{
    // ── Colors ────────────────────────────────────────────────────────────────
    static readonly Color ColBg            = C(0x0d0a1e, 1.00f);
    static readonly Color ColHeader        = C(0x6a3fb5, 0.25f);
    static readonly Color ColDot           = C(0x00e5ff, 1.00f);
    static readonly Color ColTitle         = C(0xd4b8ff, 1.00f);
    static readonly Color ColOnline        = C(0x6a5f88, 1.00f);
    static readonly Color ColInputBar      = C(0x000000, 0.71f);
    static readonly Color ColSendBtn       = C(0x6a3fb5, 1.00f);
    static readonly Color ColSysBubble     = C(0x6a3fb5, 0.30f);
    static readonly Color ColSysText       = C(0xd4b8ff, 1.00f);
    static readonly Color ColSysAvatBg     = C(0x3d1f6e, 1.00f);
    static readonly Color ColSysAvatTx     = C(0x9d6fe8, 1.00f);
    static readonly Color ColUserBubble    = C(0x00b4dc, 0.30f);
    static readonly Color ColUserText      = C(0xa0f0ff, 1.00f);
    static readonly Color ColUserAvatBg    = C(0x003d5c, 1.00f);
    static readonly Color ColUserAvatTx    = C(0x00e5ff, 1.00f);
    static readonly Color ColInputText     = C(0xd4b8ff, 1.00f);
    static readonly Color ColInputPh       = new Color(100 / 255f, 90 / 255f, 120 / 255f, 180 / 255f);
    static readonly Color ColInputBg       = new Color(1, 1, 1, 20 / 255f);

    static Color C(int hex, float a) => new Color(
        (hex >> 16 & 0xFF) / 255f,
        (hex >>  8 & 0xFF) / 255f,
        (hex       & 0xFF) / 255f, a);

    // ── Entry point ───────────────────────────────────────────────────────────
    [MenuItem("Tools/Build Chat Scene")]
    public static void BuildChatScene()
    {
        EnsureFolder("Assets/Scenes");
        EnsureFolder("Assets/Prefabs");
        EnsureFolder("Assets/Prefabs/Chat");

        TMP_FontAsset font = FindFont();

        // Create empty scene first — prefab temp objects are built here,
        // then immediately saved and destroyed, so nothing leaks.
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject sysPrefab  = BuildMessagePrefab("MessageItem_System", false, font);
        GameObject userPrefab = BuildMessagePrefab("MessageItem_User",   true,  font);

        CreateCamera();
        CreateEventSystem();

        GameObject canvasGO = CreateCanvas();
        Transform  canvasT  = canvasGO.transform;

        CreateBackground(canvasT);
        CreateHeader(canvasT, font);

        TMP_InputField inputField;
        Button         sendButton;
        CreateInputBar(canvasT, font, out inputField, out sendButton);

        Transform  content;
        ScrollRect scrollRect = CreateScrollView(canvasT, out content);

        // ChatManager at scene root (not under Canvas — it has no visual role)
        GameObject chatManagerGO = new GameObject("ChatManager");
        ChatManager chatManager  = chatManagerGO.AddComponent<ChatManager>();

        // Wire private [SerializeField] fields via SerializedObject
        SerializedObject so = new SerializedObject(chatManager);
        SetProp(so, "scrollRect",          scrollRect);
        SetProp(so, "messageContainer",    content);
        SetProp(so, "inputField",          inputField);
        SetProp(so, "sendButton",          sendButton);
        SetProp(so, "systemMessagePrefab", sysPrefab);
        SetProp(so, "userMessagePrefab",   userPrefab);
        so.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene(),
            "Assets/Scenes/ChatScene.unity");

        Debug.Log("[ChatSceneBuilder] Done! Scene saved to Assets/Scenes/ChatScene.unity");
    }

    // ── Scene helpers ─────────────────────────────────────────────────────────

    static void CreateCamera()
    {
        GameObject go = new GameObject("Main Camera");
        go.tag = "MainCamera";
        Camera cam = go.AddComponent<Camera>();
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = new Color(0.05f, 0.04f, 0.12f, 1f);
        cam.depth            = -1;
        go.AddComponent<AudioListener>();
    }

    static void CreateEventSystem()
    {
        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
    }

    static GameObject CreateCanvas()
    {
        GameObject go     = new GameObject("Canvas");
        Canvas     canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight  = 0.5f;

        go.AddComponent<GraphicRaycaster>();
        return go;
    }

    static void CreateBackground(Transform parent)
    {
        GameObject go = UIObj("Background", parent);
        Stretch(go);
        go.AddComponent<Image>().color = ColBg;
    }

    static void CreateHeader(Transform parent, TMP_FontAsset font)
    {
        GameObject go = UIObj("Header", parent);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot     = new Vector2(0.5f, 1f);
        rt.offsetMin = new Vector2(0, -120);
        rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = ColHeader;

        // Status dot
        GameObject dot   = UIObj("StatusDot", go.transform);
        RectTransform drt = dot.GetComponent<RectTransform>();
        drt.anchorMin = drt.anchorMax = new Vector2(0, 0.5f);
        drt.pivot     = new Vector2(0, 0.5f);
        drt.anchoredPosition = new Vector2(30, 0);
        drt.sizeDelta        = new Vector2(20, 20);
        dot.AddComponent<Image>().color = ColDot;

        // Title
        GameObject title = UIObj("TitleText", go.transform);
        RectTransform trt = title.GetComponent<RectTransform>();
        trt.anchorMin = trt.anchorMax = new Vector2(0, 0.5f);
        trt.pivot     = new Vector2(0, 0.5f);
        trt.anchoredPosition = new Vector2(60, 0);
        trt.sizeDelta        = new Vector2(400, 60);
        TextMeshProUGUI titleTmp = title.AddComponent<TextMeshProUGUI>();
        titleTmp.text      = "系 统 对 话";
        titleTmp.fontSize  = 32;
        titleTmp.color     = ColTitle;
        titleTmp.alignment = TextAlignmentOptions.MidlineLeft;
        if (font != null) titleTmp.font = font;

        // Online label
        GameObject online = UIObj("OnlineText", go.transform);
        RectTransform ort = online.GetComponent<RectTransform>();
        ort.anchorMin = ort.anchorMax = new Vector2(1, 0.5f);
        ort.pivot     = new Vector2(1, 0.5f);
        ort.anchoredPosition = new Vector2(-20, 0);
        ort.sizeDelta        = new Vector2(80, 40);
        TextMeshProUGUI onlineTmp = online.AddComponent<TextMeshProUGUI>();
        onlineTmp.text      = "在线";
        onlineTmp.fontSize  = 24;
        onlineTmp.color     = ColOnline;
        onlineTmp.alignment = TextAlignmentOptions.MidlineRight;
        if (font != null) onlineTmp.font = font;
    }

    static void CreateInputBar(Transform parent, TMP_FontAsset font,
        out TMP_InputField inputField, out Button sendButton)
    {
        GameObject bar = UIObj("InputBar", parent);
        RectTransform brt = bar.GetComponent<RectTransform>();
        brt.anchorMin = new Vector2(0, 0);
        brt.anchorMax = new Vector2(1, 0);
        brt.pivot     = new Vector2(0.5f, 0f);
        brt.offsetMin = Vector2.zero;
        brt.offsetMax = new Vector2(0, 120);
        bar.AddComponent<Image>().color = ColInputBar;

        // ── InputField ───────────────────────────────────────────────────────
        GameObject ifGO = UIObj("InputField", bar.transform);
        RectTransform ifRt = ifGO.GetComponent<RectTransform>();
        ifRt.anchorMin = ifRt.anchorMax = new Vector2(0, 0.5f);
        ifRt.pivot     = new Vector2(0, 0.5f);
        ifRt.anchoredPosition = new Vector2(20, 0);
        ifRt.sizeDelta        = new Vector2(820, 72);
        ifGO.AddComponent<Image>().color = ColInputBg;

        // Text Area (viewport with clipping)
        GameObject area = UIObj("Text Area", ifGO.transform);
        RectTransform art = area.GetComponent<RectTransform>();
        art.anchorMin = Vector2.zero;
        art.anchorMax = Vector2.one;
        art.offsetMin = new Vector2(10, 6);
        art.offsetMax = new Vector2(-10, -6);
        area.AddComponent<RectMask2D>();

        // Placeholder
        GameObject ph = UIObj("Placeholder", area.transform);
        Stretch(ph);
        TextMeshProUGUI phTmp = ph.AddComponent<TextMeshProUGUI>();
        phTmp.text      = "输入消息...";
        phTmp.fontSize  = 28;
        phTmp.color     = ColInputPh;
        phTmp.alignment = TextAlignmentOptions.MidlineLeft;
        if (font != null) phTmp.font = font;

        // Input text
        GameObject txt = UIObj("Text", area.transform);
        Stretch(txt);
        TextMeshProUGUI txtTmp = txt.AddComponent<TextMeshProUGUI>();
        txtTmp.fontSize  = 28;
        txtTmp.color     = ColInputText;
        txtTmp.alignment = TextAlignmentOptions.MidlineLeft;
        if (font != null) txtTmp.font = font;

        // TMP_InputField component
        TMP_InputField tmpIF     = ifGO.AddComponent<TMP_InputField>();
        tmpIF.textViewport        = art;
        tmpIF.textComponent       = txtTmp;
        tmpIF.placeholder         = phTmp;
        tmpIF.contentType         = TMP_InputField.ContentType.Standard;
        tmpIF.lineType            = TMP_InputField.LineType.SingleLine;
        inputField = tmpIF;

        // ── Send button ──────────────────────────────────────────────────────
        GameObject btnGO = UIObj("SendButton", bar.transform);
        RectTransform brrt = btnGO.GetComponent<RectTransform>();
        brrt.anchorMin = brrt.anchorMax = new Vector2(1, 0.5f);
        brrt.pivot     = new Vector2(1, 0.5f);
        brrt.anchoredPosition = new Vector2(-20, 0);
        brrt.sizeDelta        = new Vector2(160, 72);
        btnGO.AddComponent<Image>().color = ColSendBtn;
        sendButton = btnGO.AddComponent<Button>();

        GameObject btnTxtGO = UIObj("Text", btnGO.transform);
        Stretch(btnTxtGO);
        TextMeshProUGUI btnTmp = btnTxtGO.AddComponent<TextMeshProUGUI>();
        btnTmp.text      = "发送";
        btnTmp.fontSize  = 28;
        btnTmp.color     = Color.white;
        btnTmp.alignment = TextAlignmentOptions.Center;
        if (font != null) btnTmp.font = font;
    }

    static ScrollRect CreateScrollView(Transform parent, out Transform content)
    {
        GameObject svGO = UIObj("ChatScrollView", parent);
        RectTransform svRt = svGO.GetComponent<RectTransform>();
        svRt.anchorMin = Vector2.zero;
        svRt.anchorMax = Vector2.one;
        svRt.offsetMin = new Vector2(0,  120);
        svRt.offsetMax = new Vector2(0, -120);

        svGO.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        ScrollRect sr = svGO.AddComponent<ScrollRect>();
        sr.horizontal        = false;
        sr.vertical          = true;
        sr.scrollSensitivity = 30;

        // Viewport
        GameObject vp = UIObj("Viewport", svGO.transform);
        Stretch(vp);
        vp.AddComponent<RectMask2D>();
        sr.viewport = vp.GetComponent<RectTransform>();

        // Content
        GameObject contentGO = UIObj("Content", vp.transform);
        RectTransform contentRt = contentGO.GetComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = new Vector2(1, 1);
        contentRt.pivot     = new Vector2(0.5f, 1f);
        contentRt.offsetMin = Vector2.zero;
        contentRt.offsetMax = Vector2.zero;

        VerticalLayoutGroup vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing               = 16;
        vlg.padding               = new RectOffset(16, 16, 16, 16);
        vlg.childAlignment        = TextAnchor.UpperLeft;
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth      = true;
        vlg.childControlHeight     = false;

        ContentSizeFitter csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        csf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

        sr.content = contentRt;
        content    = contentRt;
        return sr;
    }

    // ── Prefab builder ────────────────────────────────────────────────────────

    static GameObject BuildMessagePrefab(string prefabName, bool isUser, TMP_FontAsset font)
    {
        GameObject root = UIObj(prefabName, null);

        HorizontalLayoutGroup hlg = root.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing               = 12;
        hlg.padding               = new RectOffset(0, 0, 4, 4);
        hlg.childAlignment        = isUser ? TextAnchor.LowerRight : TextAnchor.LowerLeft;
        hlg.childForceExpandWidth  = false;
        hlg.childForceExpandHeight = false;
        hlg.childControlWidth      = true;
        hlg.childControlHeight     = false;

        ContentSizeFitter rootCsf = root.AddComponent<ContentSizeFitter>();
        rootCsf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        rootCsf.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

        // Avatar
        GameObject avatar = UIObj("Avatar", root.transform);
        avatar.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
        avatar.AddComponent<Image>().color = isUser ? ColUserAvatBg : ColSysAvatBg;

        LayoutElement avatarLE = avatar.AddComponent<LayoutElement>();
        avatarLE.minWidth = avatarLE.preferredWidth = 40;
        avatarLE.minHeight = avatarLE.preferredHeight = 40;
        avatarLE.flexibleWidth = 0;

        GameObject avatarTxtGO = UIObj("AvatarText", avatar.transform);
        Stretch(avatarTxtGO);
        TextMeshProUGUI avatarTmp = avatarTxtGO.AddComponent<TextMeshProUGUI>();
        avatarTmp.text      = isUser ? "我" : "系";
        avatarTmp.fontSize  = 22;
        avatarTmp.color     = isUser ? ColUserAvatTx : ColSysAvatTx;
        avatarTmp.alignment = TextAlignmentOptions.Center;
        if (font != null) avatarTmp.font = font;

        // Bubble
        GameObject bubble = UIObj("Bubble", root.transform);
        bubble.AddComponent<Image>().color = isUser ? ColUserBubble : ColSysBubble;

        LayoutElement bubbleLE = bubble.AddComponent<LayoutElement>();
        bubbleLE.preferredWidth = 650;
        bubbleLE.flexibleWidth  = 0;

        VerticalLayoutGroup bubbleVLG = bubble.AddComponent<VerticalLayoutGroup>();
        bubbleVLG.padding               = new RectOffset(14, 14, 10, 10);
        bubbleVLG.childAlignment        = TextAnchor.UpperLeft;
        bubbleVLG.childForceExpandWidth  = true;
        bubbleVLG.childForceExpandHeight = false;
        bubbleVLG.childControlWidth      = true;
        bubbleVLG.childControlHeight     = true;

        ContentSizeFitter bubbleCSF = bubble.AddComponent<ContentSizeFitter>();
        bubbleCSF.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        bubbleCSF.verticalFit   = ContentSizeFitter.FitMode.PreferredSize;

        // BubbleText
        GameObject bubbleTxtGO = UIObj("BubbleText", bubble.transform);
        TextMeshProUGUI bubbleTmp = bubbleTxtGO.AddComponent<TextMeshProUGUI>();
        bubbleTmp.fontSize          = 28;
        bubbleTmp.color             = isUser ? ColUserText : ColSysText;
        bubbleTmp.enableWordWrapping = true;
        bubbleTmp.overflowMode      = TextOverflowModes.Overflow;
        bubbleTmp.alignment         = TextAlignmentOptions.MidlineLeft;
        if (font != null) bubbleTmp.font = font;

        // For user: Avatar must appear after Bubble (right side)
        if (isUser) avatar.transform.SetSiblingIndex(1);

        // Attach MessageItem and wire messageText
        MessageItem mi = root.AddComponent<MessageItem>();
        SerializedObject miSO = new SerializedObject(mi);
        miSO.FindProperty("messageText").objectReferenceValue = bubbleTmp;
        miSO.ApplyModifiedProperties();

        // Save prefab asset and clean up temp GO
        string path = "Assets/Prefabs/Chat/" + prefabName + ".prefab";
        GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefabAsset;
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    static GameObject UIObj(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        if (parent != null) go.transform.SetParent(parent, false);
        return go;
    }

    static void Stretch(GameObject go)
    {
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string[] parts   = path.Split('/');
        string   current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }

    static void SetProp(SerializedObject so, string propName, Object value)
    {
        SerializedProperty prop = so.FindProperty(propName);
        if (prop == null)
        {
            Debug.LogError($"[ChatSceneBuilder] SerializedProperty '{propName}' not found on {so.targetObject.GetType().Name}.");
            return;
        }
        prop.objectReferenceValue = value;
    }

    static TMP_FontAsset FindFont()
    {
        const string knownPath = "Assets/Res/Fonts/AlibabaPuHuiTi-3-45-Light SDF.asset";
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(knownPath);
        if (font != null) return font;

        string[] guids = AssetDatabase.FindAssets("t:TMP_FontAsset", new[] { "Assets/Res/Fonts" });
        if (guids.Length == 0)
        {
            Debug.LogWarning("[ChatSceneBuilder] No TMP_FontAsset found in Assets/Res/Fonts. Text will use TMP default font.");
            return null;
        }
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guids[0]));
        if (font == null)
            Debug.LogWarning("[ChatSceneBuilder] TMP_FontAsset found but failed to load. Text will use TMP default font.");
        return font;
    }
}
