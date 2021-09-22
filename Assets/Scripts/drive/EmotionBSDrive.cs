using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Newtonsoft.Json;

/*
Brief : 主要实现虚拟形象表情BS驱动
Author: 
Date  : 0908
note:核心功能尽量不采用MonoBehavior继承
*/
public class EmotionBSDrive 
{
    private Dictionary<string, int> em_id_dict;
    private Dictionary<string, int> eyeball_id_dict;
    private Dictionary<string, int> tongue_id_dict;

    public class EmotionBSInfo{

        public List<int> skIndex;   //就是 有哪些skinnedmeshrender 包含了该BS
        public List<int> skBSID;
        public string    bsName;
        public int       num;
        public string    RtType;    //旋转类型 比如 左眼 
        public EmotionBSInfo(){
            skIndex = new List<int>();
            skBSID = new List<int>();
            num = 0;
            RtType = "";
        }
    }


    //用于驱动面部表情的   //用于驱动眼球的
    private List<EmotionBSInfo>       m_EmBSList; 
    private List<EmotionBSInfo>       m_EyeBallBSList;

    //用于舌头驱动的
    public List<EmotionBSInfo> m_TongueLinkBSList;  //这个是记录跟舌头相关联的BS名称
    private List<EmotionBSInfo> m_TongueBSlist;      //这个是记录舌头


    private List<SkinnedMeshRenderer> m_EmotionSK;   //用于表情驱动的所有skinnedmeshrender组件

    public bool flag_em_initial = true;



    //跟舌头动画相关


    public string m_BuffAnimOfTongue = "";
    public string m_PreAnimNameOfTongue = "";
    public string m_CurAnimNameOfTongue = "";

    private float m_TongueBSAccVal = 0.0f;

    //动画正向播放标志
    //动画反向播放标志
    private bool m_TongueAnimPlayBack    = true;
    private bool m_TongueAnimStartPlayBack = false;
    private float m_TongueAnimSpeed = 10.0f;
   

    public EmotionBSDrive()
    {

    }

    public void emInitial(string emBSmapFile, List<SkinnedMeshRenderer> skList)
    {
        //step1:
        m_EmotionSK = skList;
        em_id_dict = new Dictionary<string, int>();
        eyeball_id_dict = new Dictionary<string, int>();
        tongue_id_dict = new Dictionary<string, int>();


        m_EmBSList = new List<EmotionBSInfo>();
        m_EyeBallBSList = new List<EmotionBSInfo>();

        m_TongueLinkBSList = new List<EmotionBSInfo>();
        m_TongueBSlist = new List<EmotionBSInfo>();



        //step1:读取 表情映射文件
        string strEmBsData = File.ReadAllText(emBSmapFile);
        EmotionBSJson emBsData = JsonConvert.DeserializeObject<EmotionBSJson>(strEmBsData);

        if(emBsData.EmotionBSMap.Count == 0){
            MsgEvent.SendCallBackMsg((int)AvatarID.Err_embs_map_file, AvatarID.Err_embs_map_file.ToString());
            flag_em_initial = false;
            return;
        }

        //step2:把sta 返回的 bs 名称 和 分配的 id 对应起来*********************************************************
        for (int i=0;i< emBsData.EmotionBSMap.Count;i++)
        {
            string bsName = emBsData.EmotionBSMap[i].name;
            string bsMapName = emBsData.EmotionBSMap[i].mapname;
            em_id_dict.Add(bsName, i);
            EmotionBSInfo embs = new EmotionBSInfo();
            for (int j=0;j< skList.Count;j++)
            {
                int id = skList[j].sharedMesh.GetBlendShapeIndex(bsMapName);
                if(id>=0){
                    embs.skIndex.Add(j);
                    embs.skBSID.Add(id);
                }
            }
            embs.num    = embs.skIndex.Count;
            embs.bsName = bsMapName;
            m_EmBSList.Add(embs);
        }

        //step3:用于眼球驱动的 BS 名称映射 （在51个表情里 头部已经包含了眼球转动的bs，为了区分，重新复制了这几个BS，并用了新的名字映射）
        for(int i=0;i<emBsData.EyeBallBSMap.Count;i++)
        {
            string bsName = emBsData.EyeBallBSMap[i].name;
            string bsMapName = emBsData.EyeBallBSMap[i].mapname;
            eyeball_id_dict.Add(bsName, i);
           
            EmotionBSInfo embs = new EmotionBSInfo();
            for (int j = 0; j < skList.Count; j++)
            {
                int id = skList[j].sharedMesh.GetBlendShapeIndex(bsMapName);
                if (id >= 0)
                {
                    embs.skIndex.Add(j);
                    embs.skBSID.Add(id);
                }
            }
            embs.num    = embs.skIndex.Count;
            embs.bsName = bsMapName;
            m_EyeBallBSList.Add(embs);
        }


        //step4:用于舌头驱动的 BS名称 映射**********************************************

        for(int i=0;i<emBsData.TongueBSMap.Count; i++)
        {
            string bsName    = emBsData.TongueBSMap[i].name;
            string bsMapName = emBsData.TongueBSMap[i].mapname;

            tongue_id_dict.Add(bsName, i);
            EmotionBSInfo embs = new EmotionBSInfo();
            for (int j = 0; j < skList.Count; j++)
            {
                int id = skList[j].sharedMesh.GetBlendShapeIndex(bsMapName);
                if (id >= 0)
                {
                    embs.skIndex.Add(j);
                    embs.skBSID.Add(id);
                }
            }
            embs.num = embs.skIndex.Count;
            embs.bsName = bsMapName;
            m_TongueBSlist.Add(embs);   
        }
        //跟舌头相关联的 BS 名称 列表
        for (int i = 0; i < emBsData.TongueLinkBSMap.Count; i++)
        {
            string bsName    = emBsData.TongueLinkBSMap[i].name;
            string bsMapName = emBsData.TongueLinkBSMap[i].mapname;

           
            EmotionBSInfo embs = new EmotionBSInfo();
            for (int j = 0; j < skList.Count; j++)
            {
                int id = skList[j].sharedMesh.GetBlendShapeIndex(bsMapName);

             
                if (id >= 0)
                {
                    embs.skIndex.Add(j);
                    embs.skBSID.Add(id);
                }
            }
            embs.num = embs.skIndex.Count;
            embs.bsName = bsMapName;
            m_TongueLinkBSList.Add(embs);
        }

        //************************************************************************
        if (m_EmotionSK.Count == 0)
            flag_em_initial = false;

    }

    public void emSetBSWeight(string bsName,float value)
    {

        int id = em_id_dict[bsName];

        if (id >=0)
        {
            value = Mathf.Abs(value);
            value = value * 100;
            EmotionBSInfo embs = m_EmBSList[id];
            if(embs.num ==1){
                int skid = embs.skIndex[0];
                int bsid = embs.skBSID[0];
                m_EmotionSK[skid].SetBlendShapeWeight(bsid, value);
            }
            else
                for (int i = 0; i < embs.num; i++)
                    m_EmotionSK[embs.skIndex[i]].SetBlendShapeWeight(embs.skBSID[i], value);
        }
    }

    public void emRestoreBS()
    {
        for (int j = 0; j < m_EmBSList.Count; j++)
        {
            EmotionBSInfo embs = m_EmBSList[j];
            if (embs.num == 1) {
                int skid = embs.skIndex[0];
                int bsid = embs.skBSID[0];
                m_EmotionSK[skid].SetBlendShapeWeight(bsid, 0.0f);
            }
            else
                for (int i = 0; i < embs.num; i++)
                    m_EmotionSK[embs.skIndex[i]].SetBlendShapeWeight(embs.skBSID[i], 0.0f);
        }

        //***用于眼球的****
        for (int j = 0; j < m_EyeBallBSList.Count; j++)
        {
            EmotionBSInfo embs = m_EyeBallBSList[j];
            if (embs.num == 1)
            {
                int skid = embs.skIndex[0];
                int bsid = embs.skBSID[0];
                m_EmotionSK[skid].SetBlendShapeWeight(bsid, 0.0f);
            }
            else
                for (int i = 0; i < embs.num; i++)
                    m_EmotionSK[embs.skIndex[i]].SetBlendShapeWeight(embs.skBSID[i], 0.0f);
        }

    }

    /**********************************************************************************
     
     用于实时表情驱动
    
     **********************************************************************************/

    public void emSetBSWeightForEyeBall(string bsName, float value)
    {

        int id = eyeball_id_dict[bsName];

        if (id >= 0)
        {
            value = value * 100;
            EmotionBSInfo embs = m_EyeBallBSList[id];

            if (embs.num == 1)
            {
                int skid = embs.skIndex[0];
                int bsid = embs.skBSID[0];
                m_EmotionSK[skid].SetBlendShapeWeight(bsid, value);
            }
            else
                for (int i = 0; i < embs.num; i++)
                    m_EmotionSK[embs.skIndex[i]].SetBlendShapeWeight(embs.skBSID[i], value);
        }
    }


    /// <summary>
    /// 眼睛bs列表
    /// </summary>
    /// <param name="eyeballBSList"></param>
    public void eyeBallRotation(List<EmBSInfo> eyeballBSList)
    {
        for (int i = 0; i < eyeballBSList.Count; i++)
        {
            emSetBSWeightForEyeBall(eyeballBSList[i].name, Mathf.Abs(eyeballBSList[i].value));
        }
    }


    public void setTongueAnim(string animBSName)
    {
        m_BuffAnimOfTongue = animBSName;
        //m_CurAnimNameOfTongue = animBSName;
    }
    public void update()
    {

        //一个动画 
        if (m_TongueAnimPlayBack)
        {

           
            m_CurAnimNameOfTongue = m_BuffAnimOfTongue;

   

            if (m_BuffAnimOfTongue == "")
                return;

            m_TongueAnimPlayBack = false;
            m_TongueAnimStartPlayBack = false;

           
        }

        if (m_CurAnimNameOfTongue != m_BuffAnimOfTongue)
        {
            if (!m_TongueAnimPlayBack)
            {
                playTongueAnim(-1, m_CurAnimNameOfTongue);
                m_TongueAnimStartPlayBack = true;
            }

        }
        else
        {
            if (!m_TongueAnimPlayBack && m_TongueAnimStartPlayBack ) 
                playTongueAnim(-1, m_CurAnimNameOfTongue);
            else
                playTongueAnim(1, m_CurAnimNameOfTongue);
        }








    }

    private void playTongueAnim(int sign,string animName)
    {


        if (animName == "")
            return;
        float value = 0;
        int id = tongue_id_dict[animName];
        if (id >= 0)
        {

           // if (sign > 0)
            {
                for (int i = 1; i < m_TongueLinkBSList.Count; i++)
                {
                    EmotionBSInfo embs1 = m_TongueLinkBSList[i];
                    for(int j=0;j<embs1.num;j++)
                       m_EmotionSK[embs1.skIndex[j]].SetBlendShapeWeight(embs1.skBSID[j], 100);

                }
            }

            m_TongueBSAccVal = m_TongueBSAccVal + Time.deltaTime * sign * m_TongueAnimSpeed;
           
            if (sign > 0)
            {
                if (m_TongueBSAccVal >= 1.0f)
                    m_TongueBSAccVal = 1.0f;
            }
            else
            {
                if (m_TongueBSAccVal <= 0.0f)
                {
                    m_TongueBSAccVal = 0.0f;
                    m_TongueAnimPlayBack = true;

                    //恢复舌头抬高BS  在配置文件里，有关舌头抬高的BS 放在 第1个映射之后
                    for (int i = 1; i < m_TongueLinkBSList.Count; i++)
                    {
                        EmotionBSInfo embs2 = m_TongueLinkBSList[i];
                        if(embs2.skIndex.Count>0)
                           m_EmotionSK[embs2.skIndex[0]].SetBlendShapeWeight(embs2.skBSID[0], 0.0f);
                    }
                }
            }

            value = m_TongueBSAccVal * 100;

            EmotionBSInfo embs = m_TongueBSlist[id];
            m_EmotionSK[embs.skIndex[0]].SetBlendShapeWeight(embs.skBSID[0], value);

        }

    }

    public bool getTongueAnimState()
    {
        return m_TongueAnimPlayBack;
    }











}
