using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;


public class UnityTest
{

    public bool flag_ardrive_test   = true;

    public static string rootDir = "C:/work/test/";
    public string model = rootDir + "model/test.scene";

    public string testshader = rootDir + "shader/testshader";

    //相机位置设置
    public string strCamFile        = rootDir + "cam.scene";
    public string strCamBgColorFile = rootDir + "cam_color.scene";

    public string genAvatarJFile = "C:/work/test/test.json";

    public string[] arDriveJsonFile = { rootDir + "ardrive/test.json"};
    public string[] arDriveBground = { rootDir + "ardrive/test.scene" };

    public string[] animFiles = { rootDir + "anims/" + "config.scene"};

    public string gifile = rootDir + "record/gif.json";

    public string mp4file = rootDir + "record/mp4.json";


    public string bgImageFile = rootDir + "bg/img/config.scene";

    public string bgVideoFile = rootDir + "bg/vid/config.scene";



    public static void testLoadModel(SceneController sCtrl, string strModel, int sex)
    {
        sCtrl.createAvatar(strModel);
    }

    public static void testLoadShader(SceneController sCtrl, string strShaderFile)
    {
        sCtrl.LoadResource(strShaderFile);
    }

}


