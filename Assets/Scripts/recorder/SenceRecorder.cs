using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;

using NatSuite.Recorders;
using NatSuite.Recorders.Clocks;
using NatSuite.Recorders.Inputs;
using NatSuite.Recorders.Internal;

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

    //***************************
    bool flag_recorder_enable = false;

    bool flag_work= false;

    bool flag_mp4_work = false;


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

}
