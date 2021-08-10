using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    UnityTest mTest=new UnityTest();
    SceneController sCtrl = null;


    string control_node_name = "SceneController";
    
    //记录捏脸次数
    int facetypeRecord = 0;

    //换装次数
    int isuit = 0;
    int icloth = 0;
    int ipant = 0;
    int ishoe = 0;
    int ihair = 0;
    int ijhand = 0;
    int ijneck = 0;
    int iear = 0;

    int iglass = 0;

    //美妆次数
    int[] iMakeup = new int[5] {0, 0, 0, 0, 0 };

    int iAnimCount;

    //旋转
    float rotateRd = 5.0f;

    bool flag_load = false;


    public void Start()
    { 
    }
    //加载shader 功能测试
    public void loadshader_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        sCtrl.LoadResource(mTest.testshader);

        //test
        loadAvatar_click();

        flag_load = true;


    }

    //卸载形象 功能测试
    public void unloadAvatar_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        sCtrl.UnloadAvatar();
    }

    //加载形象 功能测试
    public void loadAvatar_click()
    {

        if (!sCtrl)
        {
            //Debug.Log("scenecontroller ");
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        }
        sCtrl.createAvatar(mTest.model);


    }

    //卸载资源 功能测试
    public void unLoadResource_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        sCtrl.UnloadResource();

    }

    /***********************捏脸相关测试****************************/


    //捏脸功能  测试
    public void tweakface_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

   
        if (facetypeRecord >= mTest.iFaceTypeNum)
            facetypeRecord = 0;

        if(false)
          sCtrl.TweakFaceType(mTest.strFaceType[facetypeRecord]);
        else
        {
            string strFace = File.ReadAllText(mTest.strFaceType[facetypeRecord]);

            sCtrl.TweakFaceSlider(strFace);
        }
        
        facetypeRecord = facetypeRecord + 1;
    }

    public void tweakface_restore_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        sCtrl.RestoreTweakFace("");
    }

    /***********************换装相关测试****************************/

    //换装 功能测试

    public void change_suit_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        if (isuit >= mTest.iCostumeNum)
            isuit = 0;
        sCtrl.ChangePendant(mTest.strSuit[isuit]);
        isuit = isuit + 1;

    }
    public void change_cloth_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        if (icloth>= mTest.iCostumeNum)
            icloth = 0;
        sCtrl.ChangePendant(mTest.strCloth[icloth]);
        icloth = icloth + 1;
    }

    //换裤子测试
    public void change_pant_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        if (ipant >= mTest.iCostumeNum)
            ipant = 0;
        sCtrl.ChangePendant(mTest.strPant[ipant]);
        ipant = ipant + 1;
    }

    public void unload_suit_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
    
        sCtrl.UnloadPendant("suit");
    
    }

    public void change_shoe_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        if (ishoe >= mTest.iCostumeNum)
            ishoe = 0;
        sCtrl.ChangePendant(mTest.strShoe[ishoe]);
        ishoe = ishoe + 1;
    }
    public void change_hair_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        if (ihair >= mTest.iCostumeNum)
            ihair = 0;
        sCtrl.ChangePendant(mTest.strHair[ihair]);
        ihair = ihair + 1;
    }

    public void change_jhand_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        if (ijhand >= mTest.iCostumeNum)
            ijhand = 0;
        sCtrl.ChangePendant(mTest.strJHand[ijhand]);
        ijhand = ijhand + 1;
    }

    public void change_jneck_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        if (ijneck >= mTest.iCostumeNum)
            ijneck = 0;
        sCtrl.ChangePendant(mTest.strJNeck[ijneck]);
        ijneck = ijneck + 1;
    }

    public void change_jear_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        if (iear >= mTest.iCostumeNum)
            iear = 0;
        sCtrl.ChangePendant(mTest.strJEar[iear]);
        iear = iear + 1;
    }

    public void change_glass_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        if (iglass >= mTest.iCostumeNum)
            iglass = 0;
        sCtrl.ChangePendant(mTest.strGlass[iglass]);
        iglass = iglass + 1;
    }



    /***********************美妆相关测试****************************/
    public void face_makeup_click()
    {
        int iMkType = 0;
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        sCtrl.ChangeMakeup(mTest.strFaceMU[iMakeup[iMkType]]);
        iMakeup[iMkType] = iMakeup[iMkType] + 1;

    }

    public void eyeshadow_makeup_click()
    {

        int iMkType = 1;
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        sCtrl.ChangeMakeup(mTest.strEyeShadowMU[iMakeup[iMkType]]);
        iMakeup[iMkType] = iMakeup[iMkType] + 1;

        if (iMakeup[iMkType] >= mTest.iMakeupNum)
            iMakeup[iMkType] = 0;

    }

    public void lip_makeup_click()
    {
        int iMkType = 2;

        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        if (iMakeup[iMkType] >= mTest.iMakeupNum)
            iMakeup[iMkType] = 0;

        sCtrl.ChangeMakeup(mTest.strLipMU[iMakeup[iMkType]]);
        iMakeup[iMkType] = iMakeup[iMkType] + 1;

    }
    public void beard_makeup_click()
    {
        int iMkType = 3;

        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        if (iMakeup[iMkType] >= mTest.iMakeupNum)
        {
            sCtrl.RestoreMakeup("mbeard");
            iMakeup[iMkType] = 0;
            return;
        }

        sCtrl.ChangeMakeup(mTest.strBeardMU[iMakeup[iMkType]]);
        iMakeup[iMkType] = iMakeup[iMkType] + 1;

        

    }
    public void pupil_makeup_click()
    {
        int iMkType = 4;

        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        if (iMakeup[iMkType] >= mTest.iMakeupNum)
        {
            sCtrl.RestoreMakeup("mpupil");
            iMakeup[iMkType] = 0;
            return;
        }
        sCtrl.ChangeMakeup(mTest.strPupilMU[iMakeup[iMkType]]);
        iMakeup[iMkType] = iMakeup[iMkType] + 1;

    }

    int iEyeLashCount = 0;
    public void eyeslash_makeup_click()
    {

        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        sCtrl.ChangeMakeup(mTest.strEyeLashMU[iEyeLashCount]);
        iEyeLashCount++;

        if(iEyeLashCount>= mTest.strEyeLashMU.Length)
           iEyeLashCount = 0;

    }

    int iEyeBrowCount = 0;
    public void eyebrow_makeup_click()
    {

        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        sCtrl.ChangeMakeup(mTest.strEyeBrowMU[iEyeBrowCount]);
        iEyeBrowCount++;

        if (iEyeBrowCount >= mTest.strEyeBrowMU.Length)
            iEyeBrowCount = 0;

    }

    /**************************

    换色
    ***************************/
    int iHairColorCount = 0;
    public void change_hair_color_click()
    {

        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        sCtrl.ChangePendantColor(mTest.strHairMap[iHairColorCount]);
        iHairColorCount++;

        if (iHairColorCount >= mTest.strHairMap.Length)
            iHairColorCount = 0;

    }

    public void pta_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        sCtrl.GenAvatar(mTest.genAvatarJFile);
    }

    //相机设置
    int camSetCount = 0;
    public void cam_set_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find("SceneController").GetComponent<SceneController>();

        if(camSetCount==0)
            sCtrl.SetCamera(mTest.strCamFile);
        else
            sCtrl.SetCamera(mTest.strCamFile);

        camSetCount++;
        if (camSetCount >= 2)
            camSetCount = 0;
    }

    public void rotate_avatar_left_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        sCtrl.RotateAvatar(rotateRd);
    }

    public void rotate_avatar_right_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        sCtrl.RotateAvatar(rotateRd * -1);
    }


    string strFaceArData = "";
    FaceARMultiJson strFaceArMultiData;
    int tNum = 0;
    public void face_ar_drive_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        if (strFaceArData == "")
        {
            strFaceArData = File.ReadAllText(mTest.arDriveJsonFile[0]);
            strFaceArMultiData = JsonUtility.FromJson<FaceARMultiJson>(strFaceArData);
        }

        string strFaceAROneJson = JsonUtility.ToJson(strFaceArMultiData.list[tNum]);
        sCtrl.ArFaceRealDrive(strFaceAROneJson);

        tNum = tNum + 1;
        if (tNum >= strFaceArMultiData.list.Count)
            tNum = 0;

        Debug.Log("tNum:" + tNum);
    }
    

    public void face_ar_drive_on_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        sCtrl.ArFaceDriveEnable("enable");
    }
    public void face_ar_drive_off_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        sCtrl.ArFaceDriveDisable("disable");
    }

    /**************************动画相关*****************************/

    public void anim_play()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        
       sCtrl.PlayAnimation(mTest.animFiles[iAnimCount]);

        iAnimCount = iAnimCount + 1;
        if (iAnimCount >= 2)
            iAnimCount = 0;
    }

    public void anim_stop()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        sCtrl.StopAnimation("stop");
    }

    /*****************跟录制相关************************/

    public void record_mp4_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        string mp4Data = File.ReadAllText(mTest.mp4file);

        sCtrl.RecordMP4Video(mp4Data);

    }

    public void stop_mp4_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        sCtrl.StopRecordMP4Video("222");

    }

    public void record_gif_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        string dataJson = File.ReadAllText(mTest.gifile);

        Debug.Log(dataJson);
        sCtrl.RecordGIF(dataJson);

       
    }


    public void stop_gif_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        sCtrl.StopRecordGIF("123");
    }

    public void record_png_click()
    {


        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        sCtrl.CaptureScreenToPNG("123");

    }


    /**************************************************************/


    public void export_glb_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();

        sCtrl.ExportMeshToGLB("glb");
    }


}
