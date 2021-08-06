using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System;
using Newtonsoft.Json;

public class ClothSimulator 
{

    private  AvatarManager m_Avatar_manager_clothsim;

    private bool flag_initial = false;




    public void initial(string boneFile, AvatarManager amg)
    {
        flag_initial = true;
        m_Avatar_manager_clothsim = amg;


        //step1:¶ÁÈ¡ ¹Ç÷ÀÅö×²Ìå
        string strBoneData = File.ReadAllText(boneFile);
        BodyColliderJson boneColliderData = JsonConvert.DeserializeObject<BodyColliderJson>(strBoneData);

        //step2: Îª¹Ç÷ÀÌí¼ÓÅö×²Ìå
        for (int i=0;i<boneColliderData.collider.Count;i++){
            int id =  amg.bonesDic[boneColliderData.collider[i].name];
            if(id>=0){
                GameObject boneGO =  amg.bonesTrans[id].gameObject;
                DynamicBoneCollider  dyBoneCollider = boneGO.AddComponent<DynamicBoneCollider>();
                dyBoneCollider.m_Direction = (DynamicBoneColliderBase.Direction)(boneColliderData.collider[i].direction - 1);
                dyBoneCollider.m_Center.x = boneColliderData.collider[i].center.x;
                dyBoneCollider.m_Center.y = boneColliderData.collider[i].center.y;
                dyBoneCollider.m_Center.z = boneColliderData.collider[i].center.z;

                dyBoneCollider.m_Radius = boneColliderData.collider[i].radius;
                dyBoneCollider.m_Radius2 = boneColliderData.collider[i].radius2;
                dyBoneCollider.m_Height = boneColliderData.collider[i].height;
            }
        }
    }

    public void addSimulationParamForGo(DynamicBone dyBoneComponent,  CostumeSimParamJson dataJson,List<Transform> clothBones,Dictionary<string,int> clothBoneDict)
    {

        dyBoneComponent.m_Roots = new List<Transform>();
        dyBoneComponent.m_Colliders = new List<DynamicBoneColliderBase>();
   

        for (int i=0;i<dataJson.extraRoots.Count;i++)
        {
            int boneID;

            if (clothBoneDict.TryGetValue(dataJson.extraRoots[i], out boneID))
                dyBoneComponent.m_Roots.Add(clothBones[boneID]);
        }

        for(int i=0;i< dataJson.colliders.Count;i++)
        {
           int boneID;
           if( m_Avatar_manager_clothsim.bonesDic.TryGetValue(dataJson.colliders[i], out boneID))
            {
                GameObject go = m_Avatar_manager_clothsim.bonesTrans[boneID].gameObject;
                DynamicBoneCollider  dyBoneCollider = go.GetComponent<DynamicBoneCollider>();

                dyBoneComponent.m_Colliders.Add(dyBoneCollider);
            }
        }

        if(dataJson.updateRate !=0.0)
           dyBoneComponent.m_UpdateRate = dataJson.updateRate;

   

        if (dataJson.damping != 0.0)
            dyBoneComponent.m_Damping    = dataJson.damping;

        if (dataJson.elasticity != 0.0)
            dyBoneComponent.m_Elasticity = dataJson.elasticity;

        if (dataJson.inert != 0.0)
            dyBoneComponent.m_Inert = dataJson.inert;

        if (dataJson.friction != 0.0)
            dyBoneComponent.m_Friction = dataJson.friction;

        if (dataJson.radius != 0.0)
            dyBoneComponent.m_Radius = dataJson.radius;

        if (dataJson.gravity != null)
            dyBoneComponent.m_Gravity = new Vector3(dataJson.gravity.x, dataJson.gravity.y, dataJson.gravity.z);

        if (dataJson.externalForce != null)
            dyBoneComponent.m_Force = new Vector3(dataJson.externalForce.x,
                                              dataJson.externalForce.y, dataJson.externalForce.z);
        if (dataJson.distantDisable)
            dyBoneComponent.m_DistantDisable = dataJson.distantDisable;

        if (dataJson.distanceToObject != 0)
            dyBoneComponent.m_DistanceToObject = dataJson.distanceToObject;


    }


}
