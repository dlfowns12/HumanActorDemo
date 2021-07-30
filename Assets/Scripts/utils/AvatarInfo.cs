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
