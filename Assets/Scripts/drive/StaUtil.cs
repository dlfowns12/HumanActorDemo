using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

using Avatar3D;




public class StaUtil 
{
    //三个工作状态:空闲、工作、暂停
    public enum StaState
    {
        Idle=0,
        Working,
        Pause
    }

    private List<StaParamJson> m_PlayDataList;
    private StaState  m_StaSate;
    private Coroutine m_Coroutine = null;

    private MonoBehaviour m_SceneCtrlBH = null;
    private GameObject    m_SceneCtrlGO = null;
    private AudioSource   m_AudioSource = null;

    private float timeOfPerFrame = 0.0f;
    private float m_AudioFps = 0.0f;
    private float m_AccTime = 0.0f;
    private int   m_PreFrameIndex = -1;

    public List<FrameBSParam> m_Frames;


    private AvatarKits avatarKits_ref;




    public StaUtil(MonoBehaviour goBH,GameObject go, AvatarKits kits)
    {
        m_SceneCtrlBH = goBH;
        m_SceneCtrlGO = go;

        avatarKits_ref = kits;

        intialSta();
    }
    public void intialSta()
    {
        if (m_SceneCtrlGO)
        {
            m_AudioSource = m_SceneCtrlBH.GetComponent<AudioSource>();

            if (m_AudioSource == null)
                m_AudioSource = m_SceneCtrlGO.AddComponent<AudioSource>();
            //初始化
            m_AudioSource.loop = false;
            m_AudioSource.volume = 1.0f;
            m_AudioSource.mute = false;     //是否静音

            m_PlayDataList = new List<StaParamJson>();

            //初始默认为空闲状态
            m_StaSate = StaState.Idle;
            //默认值
            timeOfPerFrame = 1.0f / 60.0f;

        }
    }

    public void control(int playCommand)
    {
        if(playCommand == 0){
            sta_play();
            return;}

        if (playCommand == 1){
            sta_stop();
            return;}

        if (playCommand == 2){
            sta_pause();
            return;}
        if (playCommand == 3){
            sta_resume();
            return;}
    }

    public void work(StaParamJson strStaData)
    {

        lock(m_PlayDataList)
        {
            m_PlayDataList.Add(strStaData);
        }

        if (m_StaSate == StaState.Idle)
            sta_play();

    }

    private void sta_play()
    {
        //开启一个协程 去辅助播放音频
        if (m_Coroutine == null)
        {
            m_StaSate = StaState.Working; //标记当前音频工作状态
            m_Coroutine = m_SceneCtrlBH.StartCoroutine(execAudioWebRequest());
        }
   
    }

    public IEnumerator execAudioWebRequest()
    {
  
        while(m_PlayDataList.Count>0)
        {
            string audioFilePath = m_PlayDataList[0].audioFilePath;
            string url = string.Format("file://{0}", audioFilePath);

            AudioType audioTp = AudioType.WAV;
            if(m_PlayDataList[0].audioFileType == "mp3")
                audioTp = AudioType.MPEG;

            if (m_AudioFps < 1.0f)
            {
                m_AudioFps = m_PlayDataList[0].frameRate;
                timeOfPerFrame = 1.0f / m_AudioFps;
            }
            UnityWebRequest unityWebRequest = UnityWebRequestMultimedia.GetAudioClip(url, audioTp);
            yield return unityWebRequest.SendWebRequest();
            if (unityWebRequest.result == UnityWebRequest.Result.ConnectionError)
            {
                updatePlayDataList();
                yield break;  //直接结束该协程的后续操作
            }
            Debug.Log("url:" + url);
            Debug.Log(unityWebRequest.result);

            AudioClip adClip = DownloadHandlerAudioClip.GetContent(unityWebRequest);
            //如果成功加载
            if (adClip.loadState == AudioDataLoadState.Loaded)
            {
                m_AudioSource.clip = adClip;
                //if (m_PlayDataList[0].volume > 1.0f)
                //    m_AudioSource.volume = m_PlayDataList[0].volume;
                //else
                m_AudioSource.volume = 1.0f;

                m_AudioSource.Play();
               
                m_AccTime = 0.0f;
                m_PreFrameIndex = -1;

                m_Frames = m_PlayDataList[0].audioFrames;

      
            }

            MsgEvent.SendCallBackMsg((int)AvatarID.Suc_sta_text, m_PlayDataList[0].audioClipText);

            yield return new WaitUntil(driveAvatarMouth);
            updatePlayDataList();
        }
    }


    private bool driveAvatarMouth()
    {
        if(m_StaSate != StaState.Working)
            return false;


        if (m_AccTime >= m_AudioSource.clip.length){
            m_AccTime = 0.0f;
            return true;
        }

        int frameIndex = (int)(m_AccTime / timeOfPerFrame);
        if (m_PreFrameIndex != frameIndex)
        {
            m_PreFrameIndex = frameIndex;
            if (frameIndex >= 0 && frameIndex < m_Frames.Count && m_Frames[frameIndex].data.Count == m_PlayDataList[0].bsNameList.Count)
                avatarKits_ref.staEmotionDrive(m_PlayDataList[0].bsNameList,  m_Frames[frameIndex].data);
        }
        m_AccTime = m_AccTime + Time.deltaTime;
        
        return false;
    }
    private void updatePlayDataList()
    {
        lock (m_PlayDataList)
        {
            if(m_PlayDataList.Count > 0)
                m_PlayDataList.RemoveAt(0);

            if (m_PlayDataList.Count == 0)
            {
                sta_stop();

                MsgEvent.SendCallBackMsg((int)AvatarID.Suc_sta_idle, AvatarID.Suc_sta_idle.ToString());
            }
        }
     
    }
    /// <summary>
    /// 停止操作
    /// </summary>
    private void sta_stop()
    {
        //处于工作中 才会停止
        if (m_StaSate == StaState.Working)
        {
            if (m_Coroutine != null)
                m_SceneCtrlBH.StopCoroutine(m_Coroutine);

            m_Coroutine = null;
            m_PlayDataList.Clear();

            m_StaSate = StaState.Idle;
            m_AudioSource.Stop();
            //恢复BS  to do ........
            avatarKits_ref.staRestoreEmotionBS();
        }
    }

    private void sta_pause()
    {
        if (m_StaSate == StaState.Working)
        {
            m_AudioSource.Pause();
            m_StaSate = StaState.Pause;

            //恢复 bs to do .......
            avatarKits_ref.staRestoreEmotionBS();
        }
    }
    private void sta_resume()
    {
        if (m_StaSate == StaState.Pause)
        {
            m_AudioSource.UnPause();
            m_StaSate = StaState.Working;
        }
    }
    public string get_sta_state()
    {
        return m_StaSate.ToString();
    }


}
