using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MsgEvent 
{
    public static void SendCallBackMsg(int code, string msgDes)
    {
        Debug.Log("type : " +code + ", reply : " + msgDes);
    }
}
