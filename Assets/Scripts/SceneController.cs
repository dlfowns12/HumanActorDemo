using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

using Avatar3D;

public class SceneController : MonoBehaviour
{
    private AvatarKits commonAvatarKits = null;
    private bool flag_shader_load = false;
    private string avatar_parent_node_name = "AvatarController";
    private GameObject sceneParentGO;
    private void Awake()
    {
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    /// <summary>
    /// 创建形象
    /// </summary>
    /// <param name="strSceneFile":*.scene文件></param>
    /// 示例：createAvatar("c:/xxx.scene")
    public bool createAvatar(string strSceneFile)
    {
        bool res = false;
        if (commonAvatarKits == null)
        {
            if (sceneParentGO == null)
                sceneParentGO = new GameObject(avatar_parent_node_name);

            commonAvatarKits = new AvatarKits(sceneParentGO);
            res = commonAvatarKits.loadStandardModel(strSceneFile);
        }

        return res;
    }


    /// <summary>
    /// 卸载形象
    /// </summary>
    public void UnloadAvatar()
    {
        commonAvatarKits.unloadStandardModel();
        commonAvatarKits = null;
    }

    /// <summary>
    /// ab文件名
    /// </summary>
    /// <param name="strABFile"></param>
    /// 示例：LoadResource("c://xxxx")
    public void LoadResource(string strABFile)
    {
        if (!flag_shader_load)
        {
            AssetBundle.LoadFromFile(strABFile);
            flag_shader_load = true;
        }
    }

    /// <summary>
    /// 卸载资源
    /// </summary>
    public void UnloadResource()
    {
        if (flag_shader_load)
        {
            AssetBundle.UnloadAllAssetBundles(true);
            flag_shader_load = false;
        }
    }
}
