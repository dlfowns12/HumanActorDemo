using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


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

    Err_invalid_data
}