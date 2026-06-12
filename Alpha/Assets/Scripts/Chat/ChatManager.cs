using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChatManager : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Transform messageContainer;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private GameObject systemMessagePrefab;
    [SerializeField] private GameObject userMessagePrefab;

    private SystemResponder _systemResponder;
    private UnityAction<string> _onSubmitHandler;
    public SystemResponder GetSystemResponder() => _systemResponder;

    private void Awake()
    {
        Debug.Assert(scrollRect != null, "scrollRect not assigned", this);
        Debug.Assert(messageContainer != null, "messageContainer not assigned", this);
        Debug.Assert(inputField != null, "inputField not assigned", this);
        Debug.Assert(sendButton != null, "sendButton not assigned", this);
        Debug.Assert(systemMessagePrefab != null, "systemMessagePrefab not assigned", this);
        Debug.Assert(userMessagePrefab != null, "userMessagePrefab not assigned", this);

        _systemResponder = new SystemResponder();
        _onSubmitHandler = _ => OnSendClicked();
        sendButton.onClick.AddListener(OnSendClicked);
        inputField.onSubmit.AddListener(_onSubmitHandler);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        sendButton.onClick.RemoveListener(OnSendClicked);
        inputField.onSubmit.RemoveListener(_onSubmitHandler);
    }

    private void OnSendClicked()
    {
        string text = inputField.text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        AddMessage(text, userMessagePrefab);
        inputField.text = string.Empty;
        inputField.ActivateInputField();
        StartCoroutine(ShowSystemReply(text));
    }

    private IEnumerator ShowSystemReply(string userMessage)
    {
        MessageItem thinking = AddMessage("思考中…", systemMessagePrefab);
        string reply = null;
        yield return _systemResponder.GetReplyCoroutine(userMessage, r => reply = r);
        Destroy(thinking.gameObject);
        AddMessage(reply, systemMessagePrefab);
    }

    private MessageItem AddMessage(string text, GameObject prefab)
    {
        var item = Instantiate(prefab, messageContainer).GetComponent<MessageItem>();
        item.SetContent(text);
        Canvas.ForceUpdateCanvases();
        scrollRect.normalizedPosition = Vector2.zero;
        return item;
    }
}
