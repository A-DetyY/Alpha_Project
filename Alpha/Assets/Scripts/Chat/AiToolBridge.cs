using UnityEngine;

public class AiToolBridge : MonoBehaviour
{
    [SerializeField] ConfirmDialog  confirmDialog;
    [SerializeField] ChatManager    chatManager;
    [SerializeField] WebViewManager webViewManager;

    void Start()
    {
        Debug.Assert(confirmDialog  != null, "confirmDialog not assigned",  this);
        Debug.Assert(chatManager    != null, "chatManager not assigned",    this);
        Debug.Assert(webViewManager != null, "webViewManager not assigned", this);

        string workspace = System.IO.Path.Combine(
            Application.persistentDataPath, "ai_workspace");

        var fileTools    = new FileTools(workspace);
        var webViewTools = new WebViewTools(webViewManager, workspace);
        var dispatcher   = new ToolDispatcher(fileTools, confirmDialog, webViewTools);
        chatManager.GetSystemResponder().SetDispatcher(dispatcher);
    }
}
