using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    SceneController sCtrl = null;

    string control_node_name = "SceneController";

    string model = "C:/work/test/model/test.scene";
    string testshader = "C:/work/test/shader/testshader";

    public void Start()
    { 
    }
    //加载shader 功能测试
    public void loadshader_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        sCtrl.LoadResource(testshader);
    }

    //卸载形象 功能测试
    public void unloadAvatar_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        sCtrl.UnloadAvatar();
    }

    //加载形象 功能测试
    public void loadAvatar_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        sCtrl.createAvatar(model);
    }

    //卸载资源 功能测试
    public void unLoadResource_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        sCtrl.UnloadResource();

    }
}
