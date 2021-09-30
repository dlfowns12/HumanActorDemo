using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;


public class UnityTest
{

    public bool flag_makeup_enable  = false;
    public bool flag_ardrive_test   = true;
    public bool flag_cloth_sim_test = true;


    public static string rootDir = "C:/work/test/";
    public string model = rootDir + "model/test.scene";

    public string testshader = rootDir + "shader/testshader";


    /***************换装*****************/
    public int iCostumeNum = 1;

    public string[] strSuit = {rootDir + "garments/" + "suit/config.scene"};

    public string[] strCloth = {rootDir + "garments/" + "up/config.scene"};

    public string[] strPant =  {rootDir + "garments/" + "pant/config.scene"};

    public string[] strShoe = { rootDir + "garments/" + "shoe/config.scene"};

    public string[] strHair = { rootDir + "garments/" + "hair/config.scene"};

    public string[] strGlass = { rootDir + "garments/" + "glasses/config.scene"};


    public string[] strJHand = { rootDir + "garments/" + "jhand/config.scene"};

    public string[] strJNeck = { rootDir + "garments/" + "jneck/config.scene"};


    public string[] strJEar = { rootDir + "garments/" + "jear/config.scene"};


    /***************捏脸*****************/
    public int iFaceTypeNum = 1;
    public string[] strFaceType = {rootDir + "facetype/" + "face.json"};

    /***************美装*****************/
    public int iMakeupNum = 1;

    public string[] strBeardMU = {  rootDir + "makeup/beard/" + "config.scene"};

    public string[] strPupilMU = { rootDir + "makeup/eyeball/" + "config.scene"};

    public string[] strEyeShadowMU = { rootDir + "makeup/eyeline/" + "config.scene"};

    public string[] strLipMU = {rootDir + "makeup/lipstick/" + "config.scene"};


    public string[] strFaceMU = { rootDir + "makeup/rouge/" + "config.scene"};


    public string[] strEyeLashMU = { rootDir + "makeup/eyelash/" + "config.scene"};

    public string[] strEyeBrowMU = { rootDir + "makeup/eyebrow/"   + "config.scene"};



    public string[] strHairMap = { rootDir + "makeup/hair/" + "config.scene"};

    //相机位置设置
    public string strCamFile        = rootDir + "cam.scene";
    public string strCamBgColorFile = rootDir + "cam_color.scene";

    //灯光设置
    public string strLightFile = rootDir + "light.scene";

    public string genAvatarJFile = "C:/work/test/test.json";


    public string[] staTestJsonFile = {  rootDir + "asr/test.json"};



    public string[] faceExpressTestJsonFile = { "C:/work/test/express/test.json" };

    public string[] faceExpressMultiJsonFile = { rootDir + "express/test.json"};



    public string[] arDriveJsonFile = { rootDir + "ardrive/test.json"};


    public string[] arDriveBground = { rootDir + "ardrive/test.scene" };



    public string[] animFiles = { rootDir + "anims/" + "config.scene"};


    public string[] bodyDriveFile = { rootDir + "body/" + "all.json"};

    public string[] halfbodyDriveFile = { rootDir + "body/" + "half.json" };



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


