using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class AvatarCommand : MonoBehaviour
{
#if UNITY_LINUX_RECORDER || UNITY_EDITOR
    private IPCServer mIpcServer = null;
    private const string mLocalHost = "127.0.0.1";
    private const int mPort = 5555;
    private SceneController mSceneContrl = null;
    private MessageQueue mMessageQueue = new MessageQueue();

    private class MessageQueue : Object
    {
        private ArrayList mMessageList;
        private Mutex mMutex;

        public MessageQueue()
        {
            mMutex = new Mutex();
            mMessageList = new ArrayList();
        }

        public void QueueMessage(JObject msg)
        {
            mMutex.WaitOne();
            mMessageList.Add(msg);
            mMutex.ReleaseMutex();

        }

        public JObject DequeueMessage()
        {
            JObject msg = null;
            mMutex.WaitOne();
            if (mMessageList.Count > 0)
            {
                msg = (JObject)mMessageList[0];
                mMessageList.RemoveAt(0);
            }
            mMutex.ReleaseMutex();

            return msg;
        }

        void Destroy()
        {
            
        }


    }

    void Awake()
    {
        mIpcServer = new IPCServer(mLocalHost, mPort);
        mIpcServer.setCommandListener(DispatchCommand);
        mIpcServer.run();
        GameObject sceneObj = GameObject.Find("SceneController");
        mSceneContrl = sceneObj.GetComponent<SceneController>();
        Debug.Log("AvatarCommand Awake ...");
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        JObject jData = mMessageQueue.DequeueMessage();
        if (jData != null)
        {
            Debug.Log("AvatarCommand Update jData : [ " + jData + " ]");
            string method = jData["Function"].ToString();
            Debug.Log("Function :[ " + method + " ]");
            string rootPath = jData["RootPath"].ToString();
            Debug.Log("RootPath :[ " + rootPath + " ]");
            string fileName = jData["Param"].ToString();
            string param = rootPath + fileName;
            Debug.Log("AvatarCommand Update param : [ " + param + " ]");
            if (method.Equals("createAvatar"))
            {
                mSceneContrl.createAvatar(param);
            }
            else if (method.Equals("UnloadAvatar"))
            {
                mSceneContrl.UnloadAvatar();
            }
            else if (method.Equals("LoadShader"))
            {
                mSceneContrl.LoadResource(param);
            }
            else if (method.Equals("UnloadResource"))
            {
                mSceneContrl.UnloadResource();
            }
            else if (method.Equals("TweakFaceType"))
            {
                mSceneContrl.TweakFaceType(param);
            }
            else if (method.Equals("RestoreTweakFace"))
            {
                mSceneContrl.RestoreTweakFace(param);
            }
            else if (method.Equals("ExportHeadEmotionBSMap"))
            {
                mSceneContrl.ExportHeadEmotionBSMap();
            }
            else if (method.Equals("ChangePendant"))
            {
                mSceneContrl.ChangePendant(param);
            }
            else if (method.Equals("ChangeMakeup"))
            {
                mSceneContrl.ChangeMakeup(param);
            }
            else if (method.Equals("RestoreMakeup"))
            {
                mSceneContrl.RestoreMakeup(param);
            }
            else if (method.Equals("UnloadScene"))
            {
                mSceneContrl.UnloadScene();
            }
            else if (method.Equals("SetSceneBackgroundColor"))
            {
                mSceneContrl.SetSceneBackgroundColor(param);
            }
            else if (method.Equals("SetSceneBackgroundHtmlStringColor"))
            {
                mSceneContrl.SetSceneBackgroundHtmlStringColor(param);
            }
            else if (method.Equals("GenAvatar"))
            {
                mSceneContrl.GenAvatar(param);
            }
            else if (method.Equals("SetCamera"))
            {
                mSceneContrl.SetCamera(param);
            }
            else if (method.Equals("RotateAvatar"))
            {
                mSceneContrl.RotateAvatar(param);
            }
            else if (method.Equals("EnableStaFunction"))
            {
                mSceneContrl.EnableStaFunction(param);
            }
            else if (method.Equals("StaWork"))
            {
                mSceneContrl.StaWork(param);
            }
            else if (method.Equals("StaPlayControl"))
            {
                mSceneContrl.StaPlayControl(param);
            }
            else if (method.Equals("GetStaPlayState"))
            {
                mSceneContrl.GetStaPlayState(param);
            }
            else if (method.Equals("EmotionRealDrive"))
            {
                mSceneContrl.EmotionRealDrive(param);
            }
            else if (method.Equals("EmotionRealDriveOff"))
            {
                mSceneContrl.EmotionRealDriveOff(param);
            }
            else if (method.Equals("ArFaceRealDrive"))
            {
                mSceneContrl.ArFaceRealDrive(param);
            }
            else if (method.Equals("BindCameraTexture"))
            {
                mSceneContrl.BindCameraTexture(param);
            }
            else if (method.Equals("PlayAnimation"))
            {
                mSceneContrl.PlayAnimation(param);
            }
            else if (method.Equals("StopAnimation"))
            {
                mSceneContrl.StopAnimation(param);
            }
            else if (method.Equals("RecordMP4Video"))
            {
                string jsonData = File.ReadAllText(param);
                mSceneContrl.RecordMP4Video(jsonData);
            }
            else if (method.Equals("StopRecordMP4Video"))
            {
                //string jsonData = File.ReadAllText(param);
                mSceneContrl.StopRecordMP4Video(param);
            }
            else if (method.Equals("RecordGIF"))
            {
                string jsonData = File.ReadAllText(param);
                mSceneContrl.RecordGIF(jsonData);
            }
            else if (method.Equals("StopRecordGIF"))
            {
                //string jsonData = File.ReadAllText(param);
                mSceneContrl.StopRecordGIF(param);
            }
            else if (method.Equals("RecordWebP"))
            {
                string jsonData = File.ReadAllText(param);
                mSceneContrl.RecordWebP(jsonData);
            }
            else if (method.Equals("StopRecordWebP"))
            {
                //string jsonData = File.ReadAllText(param);
                mSceneContrl.StopRecordWebP(param);
            }
            else if (method.Equals("CaptureScreenToPNG"))
            {
                string jsonData = File.ReadAllText(param);
                mSceneContrl.CaptureScreenToPNG(param);
            }
        }
    }

    public void DispatchCommand(string jsonData)
    {
        Debug.Log("AvatarCommand DispatchCommand : [ " + jsonData + " ]");
        
        if (!string.IsNullOrEmpty(jsonData))
        {
            JObject jData = (JObject)JsonConvert.DeserializeObject(jsonData);

            mMessageQueue.QueueMessage(jData);

        }
        
    }

    void OnDestroy()
    {
        mIpcServer.exit();
    }

    void CreateAvatarCmd(string jsonData)
    { 
        
    }
#endif
}
