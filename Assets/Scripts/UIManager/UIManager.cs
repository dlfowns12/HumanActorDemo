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
    int iAnimCount;

    //旋转
    float rotateRd = 5.0f;

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

}
