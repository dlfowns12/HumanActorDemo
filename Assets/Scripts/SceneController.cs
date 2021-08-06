using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

using Avatar3D;

public class SceneController : MonoBehaviour
{

    UnityTest m_UnityTest = new UnityTest();

    private AvatarKits commonAvatarKits = null;

    private SexType m_sexType;
    private int curSex = -1;

    private bool flag_shader_load = false;
    private bool flag_avatar_load = false;

    private string avatar_parent_node_name = "AvatarController";
    private string background_canvas_name  = "BgCanvas";
    private string camera_node_name = "Main Camera";

    private GameObject sceneParentGO;
    private Texture2D cameraTexture;
    private bool flag_ar_drive_enable = false;

    /**************************/

    //跟背景相关的
    private BackGroundManager m_BGObj = null;
    private CamManager        m_CamManager = null;

    /************************/
    private void Awake()
    {
        //关闭
        UnityEngine.Analytics.Analytics.enabled = false;
        UnityEngine.Analytics.Analytics.deviceStatsEnabled = false;
        UnityEngine.Analytics.Analytics.initializeOnStartup = false;
        UnityEngine.Analytics.Analytics.limitUserTracking = false;
        UnityEngine.Analytics.PerformanceReporting.enabled = false;

        Debug.Log("QualitySettings#shadowDistance: " + QualitySettings.shadowDistance);
        Debug.Log("QualitySettings#antiAliasing: " + QualitySettings.antiAliasing);

    }




    // Start is called before the first frame update
    void Start()
    {
        m_sexType = new SexType();

        //Debug.Log("screenW:" + Screen.width);
        //Debug.Log("screenH:" + Screen.height);


        m_BGObj = GameObject.Find(background_canvas_name).GetComponent<BackGroundManager>();

        m_CamManager = GameObject.Find(camera_node_name).GetComponent<CamManager>();


    }

    bool flag_dir = false;
    // Update is called once per frame
    void Update()
    {
        if (flag_ar_drive_enable)
        {
            if (commonAvatarKits != null)
                commonAvatarKits.Update();
        }
    }


    /******************************************************************************************************

    接口：形象生成 

    *******************************************************************************************************/
    /// <summary>
    /// 创建形象
    /// </summary>
    /// <param name="strSceneFile":*.scene文件></param>
    /// 示例：createAvatar("c:/xxx.scene")
    public bool createAvatar(string strSceneFile)
    {
        bool res = false;
        if (commonAvatarKits == null)
        {
            if (sceneParentGO == null)
                sceneParentGO = new GameObject(avatar_parent_node_name);

            commonAvatarKits = new AvatarKits(sceneParentGO);
            res = commonAvatarKits.loadStandardModel(strSceneFile);
        }
        if (res)
            MsgEvent.SendCallBackMsg((int)AvatarID.Success, AvatarID.Success.ToString());


        return res;
    }


    /// <summary>
    /// 卸载形象
    /// </summary>
    /// <param name="sex"></param>
    public void UnloadAvatar()
    {
        if (commonAvatarKits != null)
        {
            commonAvatarKits.unloadStandardModel();
            commonAvatarKits.unInstall();
            commonAvatarKits = null;
            System.GC.Collect();

            //删除所有资源
            //unloadResource();
        }
    }

    /// <summary>
    /// ab文件名
    /// </summary>
    /// <param name="strABFile"></param>
    /// 示例：LoadResource("c://xxxx")
    public void LoadResource(string strABFile)
    {
        if (!flag_shader_load)
        {
            AssetBundle.LoadFromFile(strABFile);
            flag_shader_load = true;
        }
    }

    /// <summary>
    /// 卸载资源
    /// </summary>
    public void UnloadResource()
    {
        if (flag_shader_load)
        {
            AssetBundle.UnloadAllAssetBundles(true);
            flag_shader_load = false;
        }
    }

    /******************************************************************************************************

     接口：形象捏脸 

    *******************************************************************************************************/
    /// <summary>
    /// 
    /// </summary>
    /// <param name="strJsonFile">完整json文件</param>
    /// <returns></returns>
    /// 示例：
    public void TweakFaceType(string strJsonFile)
    {
        bool res = false;
        if (commonAvatarKits != null)
        {
            res = commonAvatarKits.tweakFace(strJsonFile);
        }

        if (res)
            MsgEvent.SendCallBackMsg((int)AvatarID.Success, AvatarID.Success.ToString());

        return;
    }
    public void TweakFaceSlider(string strJsonData)
    {
        bool res = false;
        if (commonAvatarKits != null)
        {
            res = commonAvatarKits.tweakFaceSlider(strJsonData);
        }

        if (res)
            MsgEvent.SendCallBackMsg((int)AvatarID.Success, AvatarID.Success.ToString());

        return;
    }

    /// <summary>
    /// 恢复脸型为默认脸型
    /// </summary>
    public void RestoreTweakFace(string strEmpty)
    {
        if (commonAvatarKits != null)
        {
            commonAvatarKits.restoreFace();
        }

    }


    /******************************************************************************************************

     接口：形象换装 

    *******************************************************************************************************/
    /// <summary>
    /// 改变挂件(服饰类)
    /// </summary>
    /// <param name="strABfile"></param>
    /// <returns></returns>
    /// 
    public void ChangePendant(string strABConfigFile)
    {
        bool res = false;
        if (commonAvatarKits != null)
        {

            res = commonAvatarKits.changeCostume(strABConfigFile);
        }

        if (res)
            MsgEvent.SendCallBackMsg((int)AvatarID.Success, AvatarID.Success.ToString());
        return;
    }
    public void UnloadPendant(string strClothType)
    {

        bool res = false;
        if (commonAvatarKits != null)
           commonAvatarKits.unloadCostume(strClothType);

        MsgEvent.SendCallBackMsg((int)AvatarID.Success, AvatarID.Success.ToString());

    }

    /// <summary>
    /// 卸载场景,删除shader，以及与场景相关的资源
    /// </summary>
    public void UnloadScene()
    {
        UnloadResource();
    }


    /******************************************************************************************************

    以下是跟形象生成相关的接口

    *******************************************************************************************************/
    /// <summary>
    /// 基于json数据生成特定的虚拟形象
    /// </summary>
    /// <param name="strJsonFile"></param>
    public void GenAvatar(string strJsonFile)
    {
        if (commonAvatarKits != null)
        {
            commonAvatarKits.genAvatar(strJsonFile);
        }
    }



    /******************************************************************************************************

    以下是跟相机相关的接口

    *******************************************************************************************************/
    public void SetCamera(string strJsonFile)
    {
        if (commonAvatarKits != null)
        {
            commonAvatarKits.setCamera(strJsonFile);
        }

    }

    public void RotateAvatar(float dist)
    {
        if (commonAvatarKits != null)
        {
            commonAvatarKits.rotateAvatar(dist);
        }
    }
 
    /// <summary>
    /// AR虚拟形象驱动
    /// </summary>
    /// <param name="arJson"></param>
    public void ArFaceRealDrive(string arJson)
    {
        if (commonAvatarKits != null && flag_ar_drive_enable)
        {
            commonAvatarKits.faceArDrive2(arJson);
        }
    }
    public void ArFaceDriveEnable(string strEnable)
    {
        if (commonAvatarKits != null)
        {
            flag_ar_drive_enable = true;
            commonAvatarKits.setAvatarBodyHide();
        }
    }


    public void ArFaceDriveDisable(string strDisable)
    {
        if (commonAvatarKits != null)
        {
            flag_ar_drive_enable = false;
            commonAvatarKits.mCamManager.restoreCameraSet();
            commonAvatarKits.restoreAvatarShow();
            commonAvatarKits.setAvatarShow("1");

        }
    }


    /// <summary>
    /// "0":不显示；"1"：显示
    /// </summary>
    /// <param name="strShow"></param>
    public void ArFaceShow(string strShow)
    {
        if (commonAvatarKits != null)
            commonAvatarKits.setAvatarShow(strShow);

    }

    /******************************************************************************************************

    以下是跟动画相关

    *******************************************************************************************************/
    public void PlayAnimation(string strAnimationFile)
    {
        if (commonAvatarKits != null)
            commonAvatarKits.playAnims(strAnimationFile);

    }

    public void StopAnimation(string strStop)
    {
        if (commonAvatarKits != null)
            commonAvatarKits.stopAnims();
    }

    /******************************************************************************************************

    以下是跟录制相关

    *******************************************************************************************************/

    public void RecordMP4Video(string dataJson)
    {
        if (commonAvatarKits != null)
            commonAvatarKits.recordMP4Video(dataJson);
    }
    public void StopRecordMP4Video(string data)
    {
        if (commonAvatarKits != null)
            commonAvatarKits.stopRecordMP4Video();
    }
    public void RecordGIF(string dataJson)
    {
        if (commonAvatarKits != null)
            commonAvatarKits.recordGIF(dataJson);
    }
    public void StopRecordGIF(string data)
    {
        if (commonAvatarKits != null)
            commonAvatarKits.stopRecordGIF();
    }

    public void CaptureScreenToPNG(string data)
    {
        if (commonAvatarKits != null)
            commonAvatarKits.captureScreenPng(data);
    }

    public void RecordWebP(string dataJson)
    {
        if (commonAvatarKits != null)
            commonAvatarKits.recordWebP(dataJson);
    }
    public void StopRecordWebP(string data)
    {
        if (commonAvatarKits != null)
            commonAvatarKits.stopRecordWebP();
    }

    /******************************************************************************************************

    以下是跟模型导出相关

    *******************************************************************************************************/

    public  void ExportMeshToGLB(string strFileName)
    {
        if (commonAvatarKits != null)
        {
            commonAvatarKits.exportGlb(strFileName);
        }

    }


}
