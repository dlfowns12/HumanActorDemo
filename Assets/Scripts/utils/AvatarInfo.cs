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
   
}
