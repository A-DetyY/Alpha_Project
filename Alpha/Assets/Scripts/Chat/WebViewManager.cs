using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WebViewManager : MonoBehaviour
{
    [SerializeField] GameObject closeOverlay;
    [SerializeField] Button     closeButton;

#if UNITY_ANDROID && !UNITY_EDITOR
    AndroidJavaObject _webView;
    AndroidJavaObject _activity;
#endif

    void Awake()
    {
        Debug.Assert(closeOverlay != null, "closeOverlay not assigned", this);
        Debug.Assert(closeButton  != null, "closeButton not assigned",  this);
        closeOverlay.SetActive(false);
        closeButton.onClick.AddListener(Close);
    }

    void OnDestroy()
    {
        closeButton.onClick.RemoveListener(Close);
    }

    public IEnumerator Open(string absolutePath, Action<string> onResult)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        bool done = false;
        string error = null;

        _activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
            .GetStatic<AndroidJavaObject>("currentActivity");

        _activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            try
            {
                if (_webView != null)
                {
                    _webView.Call("destroy");
                    _webView = null;
                }

                _webView = new AndroidJavaObject("android.webkit.WebView", _activity);

                var settings = _webView.Call<AndroidJavaObject>("getSettings");
                settings.Call("setJavaScriptEnabled", true);
                settings.Call("setAllowFileAccess", true);
                settings.Call("setAllowFileAccessFromFileURLs", true);
                settings.Call("setAllowUniversalAccessFromFileURLs", true);

                var decorView = _activity.Call<AndroidJavaObject>("getWindow")
                                         .Call<AndroidJavaObject>("getDecorView");
                var rootView  = decorView.Call<AndroidJavaObject>("getRootView");
                var lp        = new AndroidJavaObject("android.view.ViewGroup$LayoutParams", -1, -1);
                rootView.Call("addView", _webView, lp);

                _webView.Call("loadUrl", "file://" + absolutePath);
            }
            catch (Exception e) { error = e.Message; }
            finally { done = true; }
        }));

        yield return new WaitUntil(() => done);

        if (error != null)
        {
            onResult("[error] WebView 打开失败：" + error);
            yield break;
        }

        closeOverlay.SetActive(true);
        onResult("ok");
#else
        Debug.Log("[WebViewManager] WebView only on Android. Path: " + absolutePath);
        onResult("[error] WebView 仅在 Android 真机上可用");
        yield break;
#endif
    }

    public void Close()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_webView == null) return;
        _activity?.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            try
            {
                var decorView = _activity.Call<AndroidJavaObject>("getWindow")
                                         .Call<AndroidJavaObject>("getDecorView");
                decorView.Call<AndroidJavaObject>("getRootView").Call("removeView", _webView);
                _webView.Call("destroy");
            }
            finally { _webView = null; }
        }));
#endif
        closeOverlay.SetActive(false);
    }

    public IEnumerator EvalJs(string script, Action<string> onResult)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_webView == null) { onResult("[error] WebView 未打开"); yield break; }

        bool done = false;
        _activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
        {
            try { _webView.Call("evaluateJavascript", script, null); }
            finally { done = true; }
        }));

        yield return new WaitUntil(() => done);
        onResult("ok");
#else
        onResult("[error] WebView 仅在 Android 真机上可用");
        yield break;
#endif
    }
}
