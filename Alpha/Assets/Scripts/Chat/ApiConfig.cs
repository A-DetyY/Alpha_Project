using UnityEngine;

[CreateAssetMenu(fileName = "ApiConfig", menuName = "Chat/ApiConfig")]
public class ApiConfig : ScriptableObject
{
    public string apiUrl = "https://oneapi.hk/v1/chat/completions";
    public string apiKey = "";
    public string model  = "claude-3-5-sonnet-20241022";
}
