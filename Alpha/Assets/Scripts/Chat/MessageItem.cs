using TMPro;
using UnityEngine;

public class MessageItem : MonoBehaviour
{
    [SerializeField] private TMP_Text messageText;

    public void SetContent(string text)
    {
        if (messageText == null)
        {
            Debug.LogError($"[MessageItem] messageText is not assigned on {gameObject.name}.", this);
            return;
        }
        messageText.text = text;
    }
}
