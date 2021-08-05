using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Brief : 主要实现虚拟形象捏脸功能
Author: 
Date  : 0805
note:核心功能尽量不采用MonoBehavior继承
*/

public class TwistFace 
{
    // Start is called before the first frame update

    /// <summary>
    /// 设置 bs 系数
    /// </summary>
    /// <param name="idx"></param>
    /// <param name="value"></param>
    /// <param name="skinMesh"></param>
    public void SetBSWeight(int idx,float value,SkinnedMeshRenderer skinMesh)
    {
        value = value * 100;
        if (value >= 0.0 && value <= 100.0f)
            skinMesh.SetBlendShapeWeight(idx, value);
    }

    /// <summary>
    /// 获取 bs 系数
    /// </summary>
    /// <param name="idx"></param>
    /// <param name="skinMesh"></param>
    /// <returns></returns>
    public float GetBSWeight(int idx,SkinnedMeshRenderer skinMesh)
    {
        float value = 0.0f;
        if (idx != -1)
            value = skinMesh.GetBlendShapeWeight(idx);

        return value;
    }
    /// <summary>
    /// 恢复bs系数,如果sk组件里包含了多种类型的BS 这种方法就不适用了
    /// </summary>
    /// <param name="skinMesh"></param>
    public void RestoreBSWeight(SkinnedMeshRenderer skinMesh)
    {
        int num = skinMesh.sharedMesh.blendShapeCount;
        for (int i = 0; i < num; i++)
            skinMesh.SetBlendShapeWeight(i, 0.0f);
    }
}
