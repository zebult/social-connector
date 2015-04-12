using UnityEngine;
using System;

#if UNITY_IPHONE

using System.Runtime.InteropServices;

#endif
public class SocialConnector
{
#if UNITY_IPHONE
    private delegate void ActivityViewCompletionHandler(string activityType, bool completed);

    [DllImport("__Internal")]
    private static extern void SocialConnector_Share(string text, string url, string textureUrl, ActivityViewCompletionHandler callback);

#elif UNITY_ANDROID
    private static AndroidJavaObject clazz = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    private static AndroidJavaObject activity = clazz.GetStatic<AndroidJavaObject>("currentActivity");
#endif

#if UNITY_IPHONE

    private static void _Share(string text, string url, string textureUrl)
    {
        SocialConnector_Share(text, url, textureUrl, ShareCallback);
    }

    [AOT.MonoPInvokeCallbackAttribute(typeof(ActivityViewCompletionHandler))]
    static void ShareCallback(string stringActivityType, bool completed) {
        if(Callback == null) return;
        Callback(completed, StringToActivityTypeiOS(stringActivityType), stringActivityType);
    }

    static ActivityType StringToActivityTypeiOS(string activityType) {
        ActivityType result = ActivityType.Unknown;
        if(activityType.EndsWith("Message")) result = ActivityType.Message;
        if(activityType.EndsWith("Mail")) result = ActivityType.Mail;
        if(activityType.EndsWith("Facebook")) result = ActivityType.Facebook;
        if(activityType.EndsWith("Twitter")) result = ActivityType.Twitter;
        return result;
    }

#elif UNITY_ANDROID

    private static void _Share(string text, string url, string textureUrl)
    {
        using (var intent = new AndroidJavaObject("android.content.Intent"))
        {
            intent.Call<AndroidJavaObject>("setAction", "android.intent.action.SEND");
            intent.Call<AndroidJavaObject>("setType", string.IsNullOrEmpty(textureUrl) ? "text/plain" : "image/png");

            if (!string.IsNullOrEmpty(url))
                text += "\t" + url;
            if (!string.IsNullOrEmpty(text))
                intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.TEXT", text);

            if (!string.IsNullOrEmpty(textureUrl))
            {
                var uri = new AndroidJavaClass("android.net.Uri");
                var file = new AndroidJavaObject("java.io.File", textureUrl);
                intent.Call<AndroidJavaObject>("putExtra", "android.intent.extra.STREAM", uri.CallStatic<AndroidJavaObject>("fromFile", file));
            }
            var chooser = intent.CallStatic<AndroidJavaObject>("createChooser", intent, "");
            chooser.Call<AndroidJavaObject>("putExtra", "android.intent.extra.EXTRA_INITIAL_INTENTS", intent);
            activity.Call("startActivity", chooser);
        }
    }
#endif

    public static void Share(string text)
    {
        Share(text, null, null);
    }

    public static void Share(string text, string url)
    {
        Share(text, url, null);
    }

    public static void Share(string text, string url, string textureUrl)
    {
#if UNITY_ANDROID || UNITY_IPHONE
        _Share(text, url, textureUrl);
#endif
    }

    public static Action<bool, ActivityType, string> Callback;

    public enum ActivityType
    {
        Message,
        Mail,
        Facebook,
        Twitter,
        Unknown,
    }
}
