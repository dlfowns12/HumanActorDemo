using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


/**********************************
 形象生成(PTA)变量类型，用来解析json文件
 **********************************/

//模型自身 重复顶点查找

[Serializable]
public class vertLinkParam
{
    public string index;
    public List<int> linkIndex;
}

[Serializable]
public class vertLinkMap
{
    public List<vertLinkParam> head;
    public List<vertLinkParam> brow;
    public List<vertLinkParam> eyelash;
    public List<vertLinkParam> eyeL;
    public List<vertLinkParam> eyeR;
    public List<vertLinkParam> oral;
}

//输出的是obj模型到ab模型的映射
[Serializable]
public class AvatarObjMapAB
{
    public List<int> headTable;
    public List<int> browTable;
    public List<int> eyelashTable;
    public List<int> eyeLTable;
    public List<int> eyeRTable;
    public List<int> oralTable;
}

[Serializable]
public class AvatarPosParam
{
    public string index;
    public List<float> pos;
}

[Serializable]
public class AvatarVertexData
{
    public List<AvatarPosParam> head;
    public List<AvatarPosParam> brow;
    public List<AvatarPosParam> eyelash;
    public List<AvatarPosParam> eyeL;
    public List<AvatarPosParam> eyeR;
    public List<AvatarPosParam> oral;
}

/**********************************
 形象加载变量类型，用来解析json文件
 **********************************/
[Serializable]
public class entityJson
{
    public string node;
    public string name;
    public List<string> mat;
}
[Serializable]
public class standardmodelJson
{
    public string name;
    public List<entityJson> entity;
    public string abfile;
    public string sex;
    public string version;
    public string linkfile;
    public string mapfile;
    public string embsmapfile;
    public string bonemapfile;

    //ar相关
    public float trackoffset;
    public float trackz;
    public float facewidth;
    public float zoomcoef;
    public float trackxjitter; //位置抖动
    public float trackyjitter; //位置抖动
    public float scalejitter; //系数抖动
    public string necktransmat;


    //仿真相关
    public string simfile;

}

[Serializable]
/// <summary>
/// 对性别的枚举定义
/// 主要包括三种，男性，女性，通用
/// </summary>
public class SexType
{
     public int male = 0;
     public int female = 1;
     public int common = 2;
};

/*************************************

相机参数定义

*************************************/
[Serializable]
public class CamParam
{
    public float distx;
    public float disty;
    public float distz;

    public float rotx;
    public float roty;
    public float rotz;

}

[Serializable]
public class CamBgColorJson
{
    public float red;
    public float green;
    public float blue;
}



/*************************************

scenecontroller内函数返回的结果类型定义

*************************************/
[Serializable]
/// <summary>
/// 返回的信息
/// </summary>
public enum ResID
{
        Success = 0,                         // 操作成功
        Error,                               // 操作失败
        ERR_sex,                             // 性别错误
        Err_dir_not_exist,                   // 出错：目录不存在
        Err_file_not_exist,                  // 出错：文件不存在
        Err_not_set_avatar,                  // 出错：未设定角色场景
        Err_avatar_res_not_matching,         // 出错：角色与资源不配套
        Err_res_loading,                     // 出错：资源加载出错
        Err_not_support,                     // 出错：功能未支持
        Err_invalid_data                     // 出错：非法数据
};

[Serializable]
public class NameMapInfo
{
   public string name;
   public string mapname;
}

[Serializable]
public class NameMapInfo2
{
    public string name;
    public string mapname;
    public string type;
}

[Serializable]
public class BoneMapJson
{
    public List<NameMapInfo> BoneMap;
}

/**********************************
 用来解析json文件
 **********************************/
[Serializable]
public class KeyPointInfo
{
    public float x;
    public float y;
}
[Serializable]
public class AngleInfo
{
    public float pitchAngle;
    public float yawAngle;
    public float rollAngle;
}

[Serializable]
public class ARJson
{
    public float[] camMatrix;
    public float[] poseMatrix;
    public float headScale;

}


[Serializable]
public class FaceARJson
{
    public AngleInfo           headAttitudeAngle;

    public int nCamWidth;    //摄像头宽度
    public int nCamHeight;   //摄像头高度

    public int nFaceWidth;
    public int nFaceCenterX;  //记录人脸中心点（纵向）
    public int nFaceCenterY;  //记录人脸中心点（纵向）

    public float[] camMatrix;
    public float[] poseMatrix;

    public float headScale;
}

/// <summary>
/// ar数据类型
/// </summary>
[Serializable]
public class FaceARMultiJson
{
    public List<FaceARJson> list;
}


[Serializable] 
public class AnimationJson
{
    public string file;
    public int    isloop;
    public float  speed;
}

/**********************************
录制功能定义，用来解析json文件
 **********************************/
[Serializable]
public class RecordVideoJson
{
    public String path;
    public int    width;
    public int    height;
    public int    frameRate;
    public int    bitrate;

    public int    sampleRate;
    public int    channelCount;
    public bool   recordVoice;
}
[Serializable]
public class RecordGifJson
{
    public String path;
    public int    width;
    public int    height;
    public int    fps;
}
[Serializable]
public class RecordPngJson
{
    public String path;
    public int    width;
    public int    height;
}

[Serializable]
public class RecordWebPJson
{
    public String path;
    public int width;
    public int height;
    public int fps;
}

/*******************************

avatarkits 函数返回的结果类型定义

********************************/
[Serializable]
public enum AvatarID
{
    Success = 1000,
    Err_file_no_exist,

    Err_scene_node = 2000,                //形象场景主节点不存在
    Err_model_parse,               //模型解析出错
    Err_model_intial,
    Err_map_file_noexist,
    Err_model_index_out,
    Err_head_material_num,
    Err_model_skinmeshrender_count,
    Err_pta_file_noexist,

    //场景录制
    Err_camera_noexist = 3000,
    Err_audio_noexist,
    Err_record_data,

    Suc_mp4_recording,
    Suc_mp4_recording_stop,

    Err_gif_initial,
    Suc_gif_recording,
    Suc_gif_recording_stop,
    Suc_png_capture,

    Err_webp_initial,
    Suc_webp_recording,
    Suc_webp_recording_stop,

    Err_invalid_data
}