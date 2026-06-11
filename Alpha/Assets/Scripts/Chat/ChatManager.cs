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
        yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        AddMessage(_systemResponder.GetReply(userMessage), systemMessagePrefab);
    }

    private void AddMessage(string text, GameObject prefab)
    {
        Instantiate(prefab, messageContainer).GetComponent<MessageItem>().SetContent(text);
        Canvas.ForceUpdateCanvases();
        scrollRect.normalizedPosition = Vector2.zero;
    }
}
