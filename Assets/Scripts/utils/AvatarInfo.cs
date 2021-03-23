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

    public SkinnedMeshRenderer bodySKRender;

    public float rAngle;
    public float speedx;
    public AvatarManager()
    {
        rAngle = 0.0f;
        speedx = 5.0f;

    }

   
}
