using UnityEngine;
using System.Runtime.InteropServices;       //DllImport的命名空间
using AOT;
using System;


public class CallAppIOS
{
#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void _CalliOSFromUnity(int type, string reply);    //IOS中方法


#endif

    public static void CalliOSEvent(int type, string reply)
    {
#if UNITY_IOS
            _CalliOSFromUnity(type, reply);
#endif
    }


}
