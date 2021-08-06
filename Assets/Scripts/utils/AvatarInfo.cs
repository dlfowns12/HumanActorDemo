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