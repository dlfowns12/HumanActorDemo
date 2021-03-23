using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    UnityTest mTest=new UnityTest();
    SceneController sCtrl = null;

    string control_node_name = "SceneController";

    //旋转
    float rotateRd = 5.0f;
    public void Start()
    { 
    }
    //加载shader 功能测试
    public void loadshader_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        sCtrl.LoadResource(mTest.testshader);

        //test
        loadAvatar_click();

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
        {
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        }
        sCtrl.createAvatar(mTest.model);


    }

    //卸载资源 功能测试
    public void unLoadResource_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        sCtrl.UnloadResource();

    }

    public void rotate_avatar_left_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        sCtrl.RotateAvatar(rotateRd);
    }

    public void rotate_avatar_right_click()
    {
        if (!sCtrl)
            sCtrl = GameObject.Find(control_node_name).GetComponent<SceneController>();
        sCtrl.RotateAvatar(rotateRd * -1);
    }
}
