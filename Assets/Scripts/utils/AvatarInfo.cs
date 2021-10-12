using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;




[Serializable]
public class AvatarManager
{

    public GameObject MeshRootNode;
    public Dictionary<string, GameObject> meshGo;
    public Dictionary<string, SkinnedMeshRenderer> meshRender;

    public Dictionary<string, int> bonesDic;

    public List<Transform> bonesTrans;

    public SkinnedMeshRenderer bodySKRender;

    //配饰相关
    public GameObject ConsumeRootNode;
    public Dictionary<string, GameObject> costumeGo;

    //模型本身顶点数据  //按照 头、眉毛、睫毛、左眼、右眼、口腔的数据存放
    public List<Vector3[]> vtList;

    public float rAngle;
    public float speedx;

    public AvatarManager()
    {
        rAngle = 0.0f;
        speedx = 5.0f;

    }

    public Vector3 localPos;
    public Vector3 localScale;
    public Quaternion qt;


    public float  trackYOffset;  //头部跟踪点(y轴)
    public float  trackNoseZ;        //鼻尖 深度值
    public float  faceWidth;   //脸部宽度
    public float  zoomScale;   //放大系数
    public float  trackxjitter;        //位置抖动系数
    public float  trackyjitter;        //位置抖动系数
    public float  scalejitter;        //抖动系数
    public string neckTransMatName;  //颈部透明材质


    public Material neckMat;
    public Material neckTransMat;

    public Material[] headTransMat;
    public Material[] headNormalMat;

    public void getBonesDict(SkinnedMeshRenderer body)
    {
        if (bonesDic == null)
        {
            bonesDic = new Dictionary<string, int>();
            bonesTrans = new List<Transform>();
        }
        int count = body.bones.Length;
        for(int i=0;i<count;i++)
        {
            string name = body.bones[i].name;
            bonesDic.Add(name, i);
            bonesTrans.Add(body.bones[i]);
        }
    }
   
}

//换装插槽类型
[Serializable]
public enum slot_type
{
    slot_suit = 0,
    slot_cloth,
    slot_pant,
    slot_shoe,
    slot_sock,
    slot_hair,
    slot_glass,
    slot_hat,
    slot_neck,
    slot_ear,
    slot_bracelet,

    count
}

//美妆(子对象 部位) 说明
[Serializable]
public enum makeup_head_type
{
    mu_face=0,
    mu_eyeshadow,
    mu_lip,
    mu_beard,

    count
}

/// <summary>
/// 新增
/// </summary>
public enum makeup_head_extra_type
{
    mu_eyeball=4,
    mu_eyelash,
    mu_eyebrow,

    count
}
[Serializable]
public class AvatarMUPartManager
{

    //需要与json文件里type定义对齐
    public string mu_face      = "mface";
    public string mu_eyeshadow = "meyeshadow";
    public string mu_lip       = "mlip";
    public string mu_beard     = "mbeard";

    //瞳孔******这个是针对美妆部件
    public string mu_pupil     = "mpupil";
    public string mu_eyelash   = "meyelash";
    public string mu_eyebrow   = "meyebrow";



    private List<string> mPartList;
    public AvatarMUPartManager()
    {
        mPartList = new List<string>();
        mPartList.Add(mu_face);  mPartList.Add(mu_eyeshadow);
        mPartList.Add(mu_lip);   mPartList.Add(mu_beard);
        mPartList.Add(mu_pupil); mPartList.Add(mu_eyelash);
        mPartList.Add(mu_eyebrow);
    }

    public int getPartIndex(string type)
    {
        int index = -1;
        for(int i=0;i<mPartList.Count;i++)
        {
            if (mPartList[i] == type)
            { index = i;break; }
        }
        return index;
    }

}
/********************************************************/


//形象美妆 类型定义
[Serializable]
public class AvatarMakeUp
{
    private SkinnedMeshRenderer BaseSK=null;  //skmeshRender组件
    private  Texture2D BaseTex = null;         //基本 纹理
    private  Material  BaseMat = null;         //基准 材质
    private  Material  RealMat = null;         //实时 材质

    private  RenderTexture renderTex;

    private  Texture2D CurTex=null;        //针对眼球 定义的当前纹理，使用时，先清空，在赋值
    private  Texture2D[] PartCurTex;       //当前存放的纹理

    public int nWidth;
    public int nHeight;

    //部件skinmeshRender组件，一个是该组件支持的部分
    public AvatarMakeUp(SkinnedMeshRenderer sk,int partNum)
    {
        if (sk == null) return;
        BaseSK = sk;

        int nSize = sk.sharedMaterials.Length;
        nSize = 1;

        BaseMat   = sk.sharedMaterials[nSize -1];
        BaseTex = BaseMat.mainTexture as Texture2D;
        RealMat = sk.materials[nSize - 1];

 

        nWidth  = BaseTex.width;
        nHeight = BaseTex.height;

        if (partNum == 0) return;

        PartCurTex = new Texture2D[partNum];
        Array.Clear(PartCurTex,0,partNum);
    }
    ~AvatarMakeUp(){

    }
    public void updateMainTexture(Texture2D tex)
    {
        RealMat.mainTexture = tex;
        //如果这个不处理，将会导致内存增加
        if (CurTex == null)
            CurTex = tex;
        else
        {
            GameObject.Destroy(CurTex);
            CurTex = tex;
        }
    }

    //这个是对美妆对象部件的更新
    public void updatePartTexture(Texture2D tex,int iType)
    {
        if (PartCurTex[iType] == null)
            PartCurTex[iType] = tex;
        else{
            GameObject.Destroy(PartCurTex[iType]);
            PartCurTex[iType] = tex;
        }

        updateRenderTexture();
    }

    /// <summary>
    /// 核心功能
    /// </summary>
    private void updateRenderTexture(){

        if (renderTex)
            GameObject.Destroy(renderTex);

        //Texture2D tex = (Texture2D)RealMat.mainTexture;
        //byte[] bytes = tex.EncodeToPNG();
        //System.IO.File.WriteAllBytes("d:/123.png", bytes);

   
        renderTex = new RenderTexture(nWidth, nHeight, 32);
        RealMat.mainTexture = renderTex;

        RenderTexture.active = renderTex;
        Graphics.Blit(BaseMat.mainTexture, renderTex);
        RenderTexture.active = renderTex;
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, nWidth, nHeight, 0);

        for (int i = 0; i < PartCurTex.Length; i++)
        {
            if (PartCurTex[i])
            {
                Graphics.DrawTexture(new Rect(0, 0, nWidth, nHeight), PartCurTex[i]);
                //byte[] bytess = PartCurTex[i].EncodeToPNG();
                //System.IO.File.WriteAllBytes("d:/1234.png", bytess);
            }
        }
            GL.PopMatrix();
        RenderTexture.active = null;
    }
    public void restoreBaseTexture()
    {
        RealMat.mainTexture = BaseTex;
    }
    public void restoreBaseTexture(int iType)
    {
        GameObject.Destroy(PartCurTex[iType]);
        PartCurTex[iType] = null;
        updateRenderTexture();
    }

}

/******************************************
 
 服饰配置文件中的slot名称 必须以下面的为准
 
 ******************************************/
[Serializable]
public class slot_names
{
    public string slotSuit     = "suit";
    public string slotCloth    = "cloth";
    public string slotPant     = "pant";
    public string slotShoe     = "shoe";
    public string slotSock     = "sock";
    public string slotHair     = "hair";
    public string slotGlass    = "glass";
    public string slotHat      = "hat";
    public string slotNeck     = "neck";
    public string slotEar      = "ear";
    public string slotBracelet = "bracelet";
}

//给形象配饰预先分配 AssetBundle对象 
[Serializable]
public class consume_assetbunddle
{
    AssetBundle ab_suit;
    AssetBundle ab_cloth;
    AssetBundle ab_pant;
    AssetBundle ab_shoe;
    AssetBundle ab_sock;
    AssetBundle ab_hair;
    AssetBundle ab_glass;
    AssetBundle ab_hat;
    AssetBundle ab_neck;
    AssetBundle ab_ear;
    AssetBundle ab_bracelet;
}


/******************************************
 
 用于表情驱动中一些特别定义的名称 必须以下面的为准
 
 ******************************************/


