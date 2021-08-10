using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Drawing;

/*
Brief : 主要实现虚拟形象美化功能
Author: 
Date  : 0808
note:核心功能尽量不采用MonoBehavior继承
*/
/*********************************
脸部美妆采用Blend方式
眼球采用纹理替换方式
*********************************/

public class FaceMakeUp 
{
    //脸部美妆部件包括：-脸妆、-眼影、-唇色、-胡子   存放当前使用纹理
   // private Texture2D[] m_FaceMUTexture = new Texture2D[(int)head_makeup_type.count] { null, null, null, null };
    // 三个部位的美妆，其中 头部又分 以下几个子部位 ： 脸妆、眼影、唇色、胡子
    AvatarMakeUp m_HeadMU =null;   //头部
    AvatarMakeUp m_LEBallMU = null; //左眼
    AvatarMakeUp m_REBallMU = null; //右眼

    AvatarMakeUp m_EyeLashMU = null; //眼睫毛
    AvatarMakeUp m_EyeBrowMU = null; //眉毛

    public AvatarMUPartManager m_MUPartName = null;

    private bool flag_initial = false;
    //构造函数
    public FaceMakeUp()
    {
        m_MUPartName = new AvatarMUPartManager();
    }

    //初始化
    public void initial(SkinnedMeshRenderer faceSk, SkinnedMeshRenderer leyeballSK, SkinnedMeshRenderer reyeballSK, 
                        SkinnedMeshRenderer eyelashSK,SkinnedMeshRenderer eyebrowSK)
    {
        m_HeadMU   = new AvatarMakeUp(faceSk, (int)makeup_head_type.count);
        m_LEBallMU = new AvatarMakeUp(leyeballSK, 0);
        m_REBallMU = new AvatarMakeUp(reyeballSK, 0);

        m_EyeLashMU  = new AvatarMakeUp(eyelashSK,0);
        m_EyeBrowMU  = new AvatarMakeUp(eyebrowSK, 0);

        flag_initial = true;
    }
    ~FaceMakeUp(){}


    public bool changeMakeUp(string strImageFile, string muType)
    {
        if (!flag_initial)
        {
            MsgEvent.SendCallBackMsg((int)AvatarID.Err_makeup_intial, AvatarID.Err_makeup_intial.ToString());
            return false;
        }
        int iType = m_MUPartName.getPartIndex(muType);

        if (iType < 0)
        {
            MsgEvent.SendCallBackMsg((int)AvatarID.Err_makeup_config, AvatarID.Err_makeup_config.ToString());
            return false;
        }

        if (iType < (int)makeup_head_type.count)
            changeHeadMK(strImageFile, iType);
        else
        {
            switch(iType)
            {
                case (int)makeup_head_extra_type.mu_eyeball:
                    changePupilMK(strImageFile);
                    break;
                case (int)makeup_head_extra_type.mu_eyelash:
                    changeEyelashMK(strImageFile);
                    break;
                case (int)makeup_head_extra_type.mu_eyebrow:
                    changeEyeBrowMK(strImageFile);
                    break;
            }  
        }

        return true;
    }

    public void restoreMakeup(string MUType)
    {
        if (!flag_initial)
        {
            MsgEvent.SendCallBackMsg((int)AvatarID.Err_makeup_intial, AvatarID.Err_makeup_intial.ToString());
            return;
        }
        int iType = m_MUPartName.getPartIndex(MUType);
        if (iType < 0)
        {
            MsgEvent.SendCallBackMsg((int)AvatarID.Err_makeup_config, AvatarID.Err_makeup_config.ToString());
            return;
        }
        if (iType < (int)makeup_head_type.count)
            m_HeadMU.restoreBaseTexture(iType);
        else
        {
            if (iType == (int)makeup_head_extra_type.mu_eyeball)
            {
                m_LEBallMU.restoreBaseTexture();
                m_REBallMU.restoreBaseTexture();
            }
            if (iType == (int)makeup_head_extra_type.mu_eyelash)
                m_EyeLashMU.restoreBaseTexture();

            if (iType == (int)makeup_head_extra_type.mu_eyebrow)
                m_EyeBrowMU.restoreBaseTexture();

        }

        return;
    }

    //瞳孔
    private bool changePupilMK(string strFile)
    {
       // Debug.Log("nwidth,nheight:" + m_LEBallMU.nWidth);
        //读取文件
        Texture2D tex = getTexFromFile(strFile, m_LEBallMU.nWidth, m_LEBallMU.nHeight);
        if (tex != null){
            m_LEBallMU.updateMainTexture(tex);
            m_REBallMU.updateMainTexture(tex);
        }

        return true;
    }


    private bool changeEyelashMK(string strFile)
    {
     
        //读取文件
        Texture2D tex = getTexFromFile(strFile, m_EyeLashMU.nWidth, m_EyeLashMU.nHeight);
        if (tex != null)
            m_EyeLashMU.updateMainTexture(tex);

        return true;
    }

    private bool changeEyeBrowMK(string strFile)
    {

        //读取文件
        Texture2D tex = getTexFromFile(strFile, m_EyeBrowMU.nWidth, m_EyeBrowMU.nHeight);
        if (tex != null)
            m_EyeBrowMU.updateMainTexture(tex);

        return true;
    }


    //脸部
    private bool changeHeadMK(string strFile,int iType)
    {
        Texture2D tex = getTexFromFile(strFile, m_HeadMU.nWidth, m_HeadMU.nHeight);

        if (tex != null)
            m_HeadMU.updatePartTexture(tex, iType);
        return true;
    }



    private Texture2D getTexFromFile(string imgPath,int nwidth,int nheight)
    {
        //读取文件
        FileStream fs = new FileStream(imgPath, FileMode.Open, FileAccess.Read);
        int byteLength = (int)fs.Length;
        byte[] imgBytes = new byte[byteLength];
        fs.Read(imgBytes, 0, byteLength);
        fs.Close();
        fs.Dispose();

        Texture2D tex = new Texture2D(nwidth, nheight, TextureFormat.ARGB32,true);
        
        tex.LoadImage(imgBytes);
        tex.Apply();

   

        return tex;
    }




}
