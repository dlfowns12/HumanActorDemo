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


    //用于驱动面部表情的
    private List<EmotionBSInfo>       m_EmBSList; 
    private List<SkinnedMeshRenderer> m_EmotionSK;   //用于表情驱动的所有skinnedmeshrender组件

    public bool flag_em_initial = true;  

    public EmotionBSDrive()
    {

    }

    public void emInitial(string emBSmapFile, List<SkinnedMeshRenderer> skList)
    {
        //step1:
        m_EmotionSK = skList;
        em_id_dict = new Dictionary<string, int>();
        m_EmBSList = new List<EmotionBSInfo>();

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
    }

    public void update()
    {

    }
}
