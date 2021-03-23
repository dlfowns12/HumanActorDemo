using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;


public class UnityTest
{
    public static string rootDir = "C:/work/test/";
    public string model = rootDir + "model/test.scene";

    public string testshader = rootDir + "shader/testshader";

    //相机位置设置
    public string strCamFile        = rootDir + "cam.scene";
    public string strCamBgColorFile = rootDir + "cam_color.scene";

    public static void testLoadModel(SceneController sCtrl, string strModel, int sex)
    {
        sCtrl.createAvatar(strModel);
    }

    public static void testLoadShader(SceneController sCtrl, string strShaderFile)
    {
        sCtrl.LoadResource(strShaderFile);
    }
}


