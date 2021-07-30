using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;

using NatSuite.Recorders;
using NatSuite.Recorders.Clocks;
using NatSuite.Recorders.Inputs;
using NatSuite.Recorders.Internal;


using DVCRecorder;

public class SenceRecorder
{


 

    
    private GameObject    m_CameraGo;
    public  AudioListener m_AudioListener;
    private Camera        m_CameraComp;


    //0712新增
    private AudioSource m_AudioSource;
    private string m_DeviceName;


    // MP4 视频录制
    private IMediaRecorder mp4recorder = null;
    private CameraInput    mp4cameraInput = null;
    private AudioInput     mp4AudioInput = null;

    // gif 录制
    private GIFRecorder gifRecorder;
    private CameraInput gifCameraInput;

    private GifRecorder m_GifRecorder = null;
    private WebpRecorder m_WebpRecorder = null;

    //***************************
    bool flag_recorder_enable = false;


    bool flag_work= false;

    bool flag_mp4_work = false;
    bool flag_gif_work = false;
    bool flag_webp_work = false;




    public SenceRecorder(GameObject go)
    {
        m_AudioListener = null;
        m_CameraGo = go;

        if (m_CameraGo == null)
            MsgEvent.SendCallBackMsg((int)AvatarID.Err_camera_noexist, AvatarID.Err_camera_noexist.ToString());
        else
        {
            m_AudioListener = m_CameraGo.GetComponent<AudioListener>();
            m_CameraComp = m_CameraGo.GetComponent<Camera>();
            flag_recorder_enable = true;


            //获取audiosource组件  0712 新增
            m_AudioSource = m_CameraGo.GetComponent<AudioSource>();



            //m_GifRecorder = m_CameraGo.GetComponent<Recorder>();
            GameObject gifGO = new GameObject("GifRecorder");

            gifGO.AddComponent<GifRecorder>();
            m_GifRecorder = gifGO.GetComponent<GifRecorder>();
            m_GifRecorder.setCameraPixel(m_CameraComp.pixelWidth, m_CameraComp.pixelHeight);
            Debug.Log("SenceRecorder m_GifRecorder:" + m_GifRecorder);
            if (m_GifRecorder == null)
                MsgEvent.SendCallBackMsg((int)AvatarID.Err_gif_initial, AvatarID.Err_gif_initial.ToString());
            
          

            GameObject webpGO = new GameObject("WebpRecorder");
            //GameObject webpGO = GameObject.Find("WebpRecorder");
            webpGO.AddComponent<WebpRecorder>();
            m_WebpRecorder = webpGO.GetComponent<WebpRecorder>();
            m_WebpRecorder.setCameraPixel(m_CameraComp.pixelWidth, m_CameraComp.pixelHeight);

            Debug.Log("SenceRecorder m_WebpRecorder:" + m_WebpRecorder);
            if (m_WebpRecorder == null)
            {
                MsgEvent.SendCallBackMsg((int)AvatarID.Err_webp_initial, AvatarID.Err_webp_initial.ToString());
            }
        }

    }


    /// <summary>
    /// mp4 record
    /// </summary>
    /// <param name="path"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="frameRate"></param>
    /// <param name="sampleRate"></param>
    /// <param name="channelCount"></param>
    /// <param name="recordVoice"></param>
    public void startMP4Record(string path,int width,int height,int frameRate, int bitrate, int sampleRate,int channelCount,bool recordVoice)
    {
        if (flag_work)
            return;
      
        if (flag_recorder_enable)
        {
            var clock = new RealtimeClock();
            sampleRate = AudioSettings.outputSampleRate;

            if (frameRate < 10 && frameRate >= 40)
                frameRate = 25;


            path = path + ".mp4";

            Utility.SetExportPath(path);
        
            mp4recorder = new MP4Recorder(width, height, frameRate, sampleRate, channelCount);
            mp4cameraInput = new CameraInput(mp4recorder, clock, m_CameraComp);


            /*********add *******/

            if (recordVoice)
            {

            

                Microphone.End(null);
                if (m_AudioSource == null)
                    Debug.LogError("audioSource can not be null");


                if (Microphone.devices.Length <= 0)
                    Debug.LogError("Microphone devices count can not be zero");
                else
                {

        


                    int lengthSec = 60;
                    m_DeviceName = Microphone.devices[0];//获取麦克风名

                    Debug.Log("length:" + Microphone.devices.Length);
                    m_AudioSource.clip = Microphone.Start(m_DeviceName, true, lengthSec, sampleRate);//获取该麦克风获得的音乐片段
                    while (!(Microphone.GetPosition(m_DeviceName) > 0)) { }
                    m_AudioSource.Play();

 

                }
            }



            /****/



            if (recordVoice)
                // mp4AudioInput = new AudioInput(mp4recorder, clock, m_AudioListener);
                mp4AudioInput = new AudioInput(mp4recorder, clock,m_AudioSource,true);

            flag_work = true;
            flag_mp4_work = true;
        }
    }

    public async void stopMP4Record()
    {
        if (flag_work && flag_mp4_work)
        {
            mp4cameraInput.Dispose();
            mp4cameraInput = null;

            if (mp4AudioInput != null)
            {
                mp4AudioInput.Dispose();
                mp4AudioInput = null;

                m_AudioSource.Stop();

                if (m_DeviceName != null)
                    Microphone.End(null);

                Debug.Log("stop audio record!");
            }
            Debug.Log("stopMP4Record ...");
            await mp4recorder.FinishWriting();

            mp4recorder   = null;
            flag_work     = false;
            flag_mp4_work = false;
        }
    }


    public void startGIFRecord(string path, int width, int height, int fps)
    {
        if (flag_work)
            return;

        if (flag_recorder_enable)
        {
            m_GifRecorder.StartRecord(path, width, height, fps);
            flag_work = true;
            flag_gif_work = true;

            MsgEvent.SendCallBackMsg((int)AvatarID.Suc_gif_recording, AvatarID.Suc_gif_recording.ToString());

        }
    }
    public  void stopGIFRecord()
    {
        if (flag_work && flag_gif_work)
        {

            flag_gif_work = false;
            m_GifRecorder.StopRecord();

            Debug.Log("start to save gif!");
            m_GifRecorder.GenerateOutFile((gifBytes, gifSavePath) =>
            {
                Debug.Log("gif file：" + gifSavePath);
            
            });

            flag_work = false;
       

        }

    }
    ///
    public void startGIFRecord1(string path, int width, int height, int fps)
    {
        if (flag_work)
            return;

        if (flag_recorder_enable)
        {

            path = path + ".gif";
            Utility.SetExportPath(path);

            Debug.Log("path:" + path);

            float rato = fps / 25.0f;
            int skipnum =(int)( 4 * rato + 0.5f);
            gifRecorder = new GIFRecorder(width, height, 0.04f);
            gifCameraInput = new CameraInput(gifRecorder, new RealtimeClock(), m_CameraComp);
            // Get a real GIF look by skipping frames
            gifCameraInput.frameSkip = skipnum;

            flag_work     = true;
            flag_gif_work = true;

        }
    }
    public async void stopGIFRecord1()
    {
        if (flag_work && flag_gif_work)
        {
            gifCameraInput.Dispose();
            gifCameraInput = null;

            await gifRecorder.FinishWriting();
            gifRecorder = null;

            flag_work = false;
            flag_gif_work = false;
        }
    }
    public void capturePNG(string path,int width,int height)
    {

        if(flag_recorder_enable)
        {
            path = path + ".png";

            Debug.Log("path:" + path);

            ScreenCapture.CaptureScreenshot(path);
            return;

            RenderTexture rt = new RenderTexture(width, height, 32);
            m_CameraComp.targetTexture = rt;
            m_CameraComp.Render();
            RenderTexture.active = rt;
            Texture2D t = new Texture2D(width, height, TextureFormat.ARGB32, false, false);
            t.ReadPixels(new Rect(0, 0, t.width, t.height), 0, 0);
            t.Apply();

            
            System.IO.File.WriteAllBytes(path, t.EncodeToJPG());
            m_CameraComp.targetTexture = null;

        }
    }

    public void startWebPRecord(string path, int width, int height, int fps)
    {
        if (flag_work)
            return;

        if (flag_recorder_enable)
        {
            Debug.Log("startWebPRecord m_WebpRecorder:" + m_WebpRecorder);
            m_WebpRecorder.StartRecord(path, width, height, fps);
            flag_work = true;
            flag_webp_work = true;

            MsgEvent.SendCallBackMsg((int)AvatarID.Suc_webp_recording, AvatarID.Suc_webp_recording.ToString());

        }
    }
    public void stopWebPRecord()
    {
        if (flag_work && flag_webp_work)
        {

            flag_webp_work = false;
            m_WebpRecorder.StopRecord();

            Debug.Log("start to save webp!");
            m_WebpRecorder.GenerateOutFile((webpBytes, webpSavePath) =>
            {
                Debug.Log("webp file：" + webpSavePath);

            });

            flag_work = true;


        }

    }















}
