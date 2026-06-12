using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmDialog : MonoBehaviour
{
    [SerializeField] GameObject    panel;
    [SerializeField] TMP_Text      messageText;
    [SerializeField] Button        allowButton;
    [SerializeField] Button        denyButton;

    bool? _result;

    void Awake()
    {
        Debug.Assert(panel       != null, "panel not assigned",       this);
        Debug.Assert(messageText != null, "messageText not assigned", this);
        Debug.Assert(allowButton != null, "allowButton not assigned", this);
        Debug.Assert(denyButton  != null, "denyButton not assigned",  this);
        panel.SetActive(false);
    }

    public IEnumerator Ask(string message, Action<bool> onResult)
    {
        _result = null;
        messageText.text = message;
        panel.SetActive(true);

        allowButton.onClick.AddListener(OnAllow);
        denyButton.onClick.AddListener(OnDeny);

        try
        {
            yield return new WaitUntil(() => _result.HasValue);
        }
        finally
        {
            allowButton.onClick.RemoveListener(OnAllow);
            denyButton.onClick.RemoveListener(OnDeny);
            panel.SetActive(false);
        }

        onResult(_result.Value);
    }

    void OnAllow() => _result = true;
    void OnDeny()  => _result = false;
}
