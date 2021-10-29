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

    /********驱动开关 标志***********/

    private bool flag_emotion_drive_enable = false;
    private bool flag_ar_drive_enable = false;
    private bool flag_body_drive_enable = false;

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


        ////定义场景父节点,名称统一定义

#if UNITY_LINUX_RECORDER
        GameObject proxyGO = new GameObject("AvatarProxy");
        proxyGO.AddComponent<AvatarCommand>();
#elif UNITY_LINUX_RTC
        GameObject rtcGO = new GameObject("DVCStreamer");
        rtcGO.AddComponent<DVCStreamer>();
#endif
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
        //会做一些判断，用于实时的一些功能，必须面部实时驱动
        if (flag_emotion_drive_enable || flag_ar_drive_enable || flag_body_drive_enable)
        {
            if (commonAvatarKits != null)
                commonAvatarKits.Update();
        }
    }

    //private void FixedUpdate()
    //{

    //}

    //private void LateUpdate()
    //{
    //    //会做一些判断，用于实时的一些功能，必须面部实时驱动
    //    if (flag_emotion_drive_enable || flag_ar_drive_enable || flag_body_drive_enable)
    //    {
    //        if (commonAvatarKits != null)
    //            commonAvatarKits.Update();
    //    }

    //}



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

    public void ExportHeadEmotionBSMap()
    {
        if (commonAvatarKits != null)
            commonAvatarKits.exportEmotionBSMapFile();
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




    /******************************************************************************************************

     接口：形象美化 & 以及服饰资源换色

    *******************************************************************************************************/
    /// <summary>
    /// 改变妆容
    /// </summary>
    /// <param name="strConfigFile"></param>
    /// <returns></returns>
    public void ChangeMakeup(string strConfigFile)
    {
        if (commonAvatarKits != null)
            commonAvatarKits.changeMakeup(strConfigFile);

        return;
    }
    public void RestoreMakeup(string MUType)
    {
        if (commonAvatarKits != null)
            commonAvatarKits.restoreMakeup(MUType);

    }

    public void ChangePendantColor(string strConfigFile)
    {
        if (commonAvatarKits != null)
            commonAvatarKits.changeCostumeColor(strConfigFile);
    }



    /******************************************************************************************************

    以下是跟场景相关的接口    《 相机  灯光 》 

    *******************************************************************************************************/
    /// <summary>
    /// 卸载场景,删除shader，以及与场景相关的资源
    /// </summary>
    public void UnloadScene()
    {
        UnloadResource();
    }

    public void SetSceneBackgroundImage2(string jpgFile)
    {
        if (commonAvatarKits != null)
        {
            Texture2D tx = new Texture2D(1080, 1920);
            FileStream files = new FileStream(jpgFile, FileMode.Open);
            byte[] imgByte = new byte[files.Length];
            files.Read(imgByte, 0, imgByte.Length);
            files.Close();
            if (imgByte.Length > 0)
            {
                tx.LoadImage(imgByte);
                m_BGObj.setBackGroundTexture(tx);
                MsgEvent.SendCallBackMsg((int)AvatarID.Suc_camera_texture_bind, AvatarID.Suc_camera_texture_bind.ToString());
            }
        }
    }

    /// <summary>
    /// 设定背景
    /// </summary>
    public void SetSceneBackgroundColor(string strJsonFile)
    {
        if (commonAvatarKits != null)
        {
            commonAvatarKits.setCameraBackgroundColor(strJsonFile);
        }
    }

    /// <summary>
    /// 参数为 "#FFF7F4" 形式
    /// </summary>
    /// <param name="HtmlStringColor"></param>
    public void SetSceneBackgroundHtmlStringColor(string HtmlStringColor)
    {
        if (commonAvatarKits != null)
        {
            commonAvatarKits.setCameraBackgroundHtmlStringColor(HtmlStringColor);
        }
    }

    public void SetSceneLight(string strJsonFile)
    {
        if (commonAvatarKits != null)
            commonAvatarKits.setSceneLight(strJsonFile);

    }

    public void SetSceneBackgroundImage(string strJsonFile)
    {
        if (commonAvatarKits != null)
        {
            string strJsonData = File.ReadAllText(strJsonFile);
            BackGroundTextureJson textureData = JsonUtility.FromJson<BackGroundTextureJson>(strJsonData);
            Texture2D tx = new Texture2D(textureData.width, textureData.height);
            string rootDir = Path.GetDirectoryName(strJsonFile);
            string image_file = rootDir + "/" + textureData.name;

           

            FileStream files = new FileStream(image_file, FileMode.Open);
            byte[] imgByte = new byte[files.Length];
            files.Read(imgByte, 0, imgByte.Length);
            files.Close();
            if(imgByte.Length >0)
            {
                tx.LoadImage(imgByte);
                m_BGObj.setBackGroundTexture(tx);
                MsgEvent.SendCallBackMsg((int)AvatarID.Suc_camera_texture_bind, AvatarID.Suc_camera_texture_bind.ToString());
            }
        }
    }

    public void SetSceneBackgroundVideo(string strJsonFile)
    {
        if (commonAvatarKits != null)
        {
            string strJsonData = File.ReadAllText(strJsonFile);
            BackGroundTextureJson textureData = JsonUtility.FromJson<BackGroundTextureJson>(strJsonData);

            string rootDir    = Path.GetDirectoryName(strJsonFile);
            string video_file = rootDir + "/" + textureData.name;


            m_BGObj.setBackGroundTextureFromVideo(video_file,textureData.width,textureData.height);

        }

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

    public void RotateAvatar(string dist)
    {
        if (commonAvatarKits != null)
        {
            float dst = float.Parse(dist);
            commonAvatarKits.rotateAvatar(dst);
        }
    }
    /******************************************************************************************************

    以下是跟STA相关的功能接口

    *******************************************************************************************************/
    public void EnableStaFunction(string strEmpty)
    {
        if (commonAvatarKits != null)
        {
            commonAvatarKits.enableSta(this, this.gameObject);

        }
    }

    public void StaWork(string strStaJson)
    {

        if (commonAvatarKits != null)
        {
            commonAvatarKits.playAudio(strStaJson);
        }

    }

    //"0": 播放；"1"：停止；"2"：暂停；"3"：恢复
    public void StaPlayControl(string strControl)
    {
        if (commonAvatarKits != null)
        {
            commonAvatarKits.playControl(strControl);
        }
    }
    public void GetStaPlayState(string strEmpty)
    {
        if (commonAvatarKits != null)
        {
            commonAvatarKits.getStaPlayState();
        }
    }

    /******************************************************************************************************

    以下是跟 表情 & 肢体 驱动相关的功能接口  (表情肢体实时驱动)

    *******************************************************************************************************/

    public void EmotionRealDrive(string faceBSJson)
    {
        if (commonAvatarKits != null)
        {
            commonAvatarKits.faceExpressDrive(faceBSJson);

            if (!flag_emotion_drive_enable)
                flag_emotion_drive_enable = true;
        }

    }

    public void EmotionRealDriveOff(string strRestore)
    {
        if (commonAvatarKits != null)
        {
            commonAvatarKits.faceExpressDriveRestore();

            if (flag_emotion_drive_enable)
                flag_emotion_drive_enable = false;
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
           // commonAvatarKits.faceArDrive(arJson);
            commonAvatarKits.faceArDrive2(arJson);

        }
    }
    public void ArFaceDriveEnable(string strEnable)
    {
        if (commonAvatarKits != null)
        {
            flag_ar_drive_enable = true;

            //method1:用该方案
          //  commonAvatarKits.mCamManager.setCameraToOrthographic();
            commonAvatarKits.setAvatarBodyHide();

            MsgEvent.SendCallBackMsg((int)AvatarID.Suc_camera_bind, AvatarID.Suc_camera_bind.ToString());
        }
    }


    public void ArFaceOffset(float value, int type)
    {
        if (commonAvatarKits != null && flag_ar_drive_enable)
            commonAvatarKits.faceArSetOffset(value, type);
    }

    public void ArFaceDriveDisable(string strDisable)
    {
        if (commonAvatarKits != null)
        {
            flag_ar_drive_enable = false;
            commonAvatarKits.mCamManager.restoreCameraSet();
            commonAvatarKits.restoreAvatarShow();
            commonAvatarKits.setAvatarShow("1");
            m_BGObj.restoreBackGroundTexture();

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

    public void BindCameraTexture(string dataJson)
    {
        if (m_UnityTest.flag_ardrive_test)
        {

            int width = m_CamManager.refCanvasWidth;
            int height = m_CamManager.refCanvasHeight;
           // Texture2D tex = Resources.Load<Texture2D>("Textures/1669173194466");
            Texture2D tex = Resources.Load<Texture2D>("Textures/19116");
            //cameraTexture = Texture2D.CreateExternalTexture(1080, 1920, TextureFormat.RGBA32, false, false, (IntPtr)tex.GetNativeTexturePtr());
            cameraTexture = Texture2D.CreateExternalTexture(width, height, TextureFormat.RGBA32, false, false, (IntPtr)tex.GetNativeTexturePtr());

            m_BGObj.bgCanvasWidth = width;
            m_BGObj.bgCanvasHeight = height;


        }
        else
        {
            CameraTextureJson textureData = JsonUtility.FromJson<CameraTextureJson>(dataJson);
            cameraTexture = Texture2D.CreateExternalTexture(textureData.width, textureData.height, TextureFormat.RGBA32, false, false, (IntPtr)textureData.textureid);

            m_BGObj.bgCanvasWidth  = textureData.canvasWidth;
            m_BGObj.bgCanvasHeight = textureData.canvasHeight;


        }
        m_BGObj.setBackGroundTexture(cameraTexture);
        MsgEvent.SendCallBackMsg((int)AvatarID.Suc_camera_texture_bind, AvatarID.Suc_camera_texture_bind.ToString());

    }



    public void savebgToPng(string strFileName)
    {
        m_BGObj.saveBgTexture(strFileName);

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

    以下是跟肢体驱动相关

  *******************************************************************************************************/

    public void SetBodyDriveEnable(string strEnable)
    {

        if (commonAvatarKits != null)
        {
            commonAvatarKits.setBodyDriveEnable();

            flag_body_drive_enable = true;
        }
    }

    public void SetBodyDriveDisable(string strDisable)
    {

        if (commonAvatarKits != null)
        {
            commonAvatarKits.setBodyDriveDisable();
            flag_body_drive_enable = false;
        }
    }

    public void BodyRealDrive(string dataJson)
    {
        if (commonAvatarKits != null)
        {
            commonAvatarKits.setBodyDriveData(dataJson);
        }

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
