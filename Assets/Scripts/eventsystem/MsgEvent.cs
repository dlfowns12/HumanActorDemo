using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MsgEvent 
{
    public static void SendCallBackMsg(int code, string msgDes)
    {
#if UNITY_EDITOR_WIN

    Debug.Log("type : " +code + ", reply : " + msgDes);

#elif UNITY_ANDROID
        AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.demo.UnityInvoker");
        AndroidJavaObject androidJavaObject = androidJavaClass.GetStatic<AndroidJavaObject>("s_instance");
        androidJavaObject.Call("onUnityEvent", code, msgDes);

#elif UNITY_IOS
       CallAppIOS.CalliOSEvent((int)type, msgDes);
#endif

    }



}
