using System.Collections.Generic;
using System.IO;
using UnityEngine;


/*
由于加载ab资源包，不能要求内部有同名的对象

如果想加载的ab包的内容不一样 但是名字一样的话

只能加载第一次加载的ab包的内容,所以 ab内的对象名称一定不能重复

否则 会出现问题.

GameObject.destroy()  之后，变量主动赋值为null;

*/

public class Costume
{
    //所有服饰类的资源包对象,,根据服饰的类型，预先定义
    private List<AssetBundle> consumeABS;

    private DynamicBone[] constumeDynamicBones;

    //为了获取配置文件中的插槽名称对应的索引
    private Dictionary<string, int> costumeSlotDic;

    //插槽下的服装对象,一个插槽只有一个对象,,这个是换装时候使用
    //服装完成的对象包含模型和骨骼
    private Dictionary<int, GameObject> costumeMeshGo;
    private Dictionary<int, GameObject> costumeRealMeshGo;


    private List<GameObject> costumeParentGo;


    //每一种类型下的额外骨骼 存储，方便资源卸载时候，删除对象 
    List<List<GameObject>> m_SlotExBonesList;


    //场景主节点
    private GameObject m_SceneRootNode;


    //这个是记录每次类型加载mesh名称，
    //目的是防止短时间内重复加载同样的mesh对象
    //解决不了根本，只能从素材本身防止重复名称出现;
    private List<string> m_listMeshRecord;

    //身体对象,服装需要绑定到身体骨架上
    private GameObject m_bodyGo;
    private SkinnedMeshRenderer m_BodySk = null;

    slot_names mSlotName;

    //仿真对象 



    UnityTest m_UnityTest = new UnityTest();

    //
    private AvatarManager m_Avatar_manager = null;
    //
    public bool flag_costume_initial = false;

    //预设骨骼上限值






    public Costume(GameObject sceneParentGo )
    {
        m_SceneRootNode = sceneParentGo;

        
    }

    public void initial(GameObject bodyGo, Dictionary<string, GameObject> costumeNodes, AvatarManager mAvataManager)
    {

        costumeSlotDic = new Dictionary<string, int>();
        mSlotName = new slot_names();
        costumeSlotDic.Add(mSlotName.slotSuit, 0);
        costumeSlotDic.Add(mSlotName.slotCloth, 1);
        costumeSlotDic.Add(mSlotName.slotPant, 2);
        costumeSlotDic.Add(mSlotName.slotShoe, 3);
        costumeSlotDic.Add(mSlotName.slotSock, 4);
        costumeSlotDic.Add(mSlotName.slotHair, 5);
        costumeSlotDic.Add(mSlotName.slotGlass, 6);
        costumeSlotDic.Add(mSlotName.slotHat, 7);
        costumeSlotDic.Add(mSlotName.slotNeck, 8);
        costumeSlotDic.Add(mSlotName.slotEar, 9);
        costumeSlotDic.Add(mSlotName.slotBracelet, 10);


        m_listMeshRecord = new List<string>();
        //初始化
        costumeMeshGo     = new Dictionary<int, GameObject>();
        costumeRealMeshGo = new Dictionary<int, GameObject>();

        m_SlotExBonesList = new List<List<GameObject>>();


        //有关配饰资源ab对象的定义，目的是为了释放ab内存
        consumeABS = new List<AssetBundle>();
        for (int i = 0; i < (int)slot_type.count; i++)
        {
            AssetBundle slot_ab = null;
            consumeABS.Add(slot_ab);
            string name = "";
            m_listMeshRecord.Add(name);

            List<GameObject> goList = new List<GameObject>();
            m_SlotExBonesList.Add(goList);


        }
        m_bodyGo = bodyGo;
        m_BodySk = m_bodyGo.GetComponent<SkinnedMeshRenderer>();

        //需要添加的 动态骨骼
        constumeDynamicBones = new DynamicBone[consumeABS.Count];

        costumeParentGo = new List<GameObject>();
        foreach (var tmp in costumeNodes)
        {
            costumeParentGo.Add(tmp.Value);
        }

        flag_costume_initial = true;

        m_Avatar_manager = mAvataManager;


    }

    public bool changeCloth(string abfile, string meshName, string slotName,
                           List<exBoneParam> exRootBones,string simFile, ClothSimulator sim)
    {

        if (!flag_costume_initial)
        {
            MsgEvent.SendCallBackMsg((int)AvatarID.Err_cloth_initial, AvatarID.Err_cloth_initial.ToString());
            return false;
        }
        int index = getSlotIndex(slotName);
        if (index < 0)
        {
            MsgEvent.SendCallBackMsg((int)AvatarID.Err_cloth_slotname, AvatarID.Err_cloth_slotname.ToString());
            return false;
        }
        //step1:加载模型


        if (m_listMeshRecord[index] == meshName) {
            MsgEvent.SendCallBackMsg((int)AvatarID.Err_cloth_repeatload, AvatarID.Err_cloth_repeatload.ToString());
            return false;
        }

        AssetBundle[] loadedABs = Resources.FindObjectsOfTypeAll<AssetBundle>();
        if (loadedABs != null) {
            foreach (var item in loadedABs) {
                if (item.Contains(Path.GetFileName(abfile))) {
                    item.Unload(true); break; } } }


        AssetBundle tempab = AssetBundle.LoadFromFile(abfile);
        GameObject clothGOInAB = tempab.LoadAsset<GameObject>(meshName);

        if (clothGOInAB == null)
        {
            tempab.Unload(true); //销毁ab资源
            MsgEvent.SendCallBackMsg((int)AvatarID.Err_cloth_abfile, AvatarID.Err_cloth_abfile.ToString());
            return false;
        }
        GameObject clothGoShow = GameObject.Instantiate<GameObject>(clothGOInAB) as GameObject;
        clothGoShow.transform.SetParent(costumeParentGo[index].transform);


        //暂时取消--- 采用了共享骨骼的方式,这个是非共享骨骼的方式
        clothGoShow.GetComponent<Transform>().rotation = m_SceneRootNode.GetComponent<Transform>().rotation;

        //替换材质************************
        //找到fbx包含的mesh
        GameObject clothMainGo = null;
        clothMainGo = GameObject.Find(meshName);

        ////第二种方案
        //Transform[] mtran = clothGoShow.GetComponentsInChildren<Transform>();
        //for (int i = 0; i < mtran.Length; i++)
        //    if (mtran[i].FindChild(meshName))
        //        clothMainGo = mtran[i].FindChild(meshName).gameObject;

        SkinnedMeshRenderer clothSKmesh = null;
        if (clothMainGo)
        {
            clothSKmesh = clothMainGo.GetComponent<SkinnedMeshRenderer>();
            if (clothSKmesh != null)
            {
                Material[] matArr = new Material[clothSKmesh.materials.Length];
                bool findMat = false;
                for (int i = 0; i < matArr.Length; i++)
                {
                    string matName = clothSKmesh.materials[i].name;
                    matName = matName.Replace(" (Instance)", "");
                    Material ab_mat = tempab.LoadAsset<Material>(matName);
                    if (ab_mat != null)
                    {
                        Material matInst = GameObject.Instantiate(ab_mat);
                        matArr[i] = matInst;
                        findMat = true;
                    }
                }
                if (findMat)
                    clothSKmesh.materials = matArr;
            }
        }
        else
            MsgEvent.SendCallBackMsg((int)AvatarID.Err_cloth_mesh_noexist, AvatarID.Err_cloth_mesh_noexist.ToString());

        //*******************************
        GameObject preClothGO;
        costumeMeshGo.TryGetValue(index, out preClothGO);
        if (preClothGO != null)
        {
            GameObject.Destroy(preClothGO);
            consumeABS[index].Unload(true);
            costumeMeshGo[index] = clothGoShow;

            costumeRealMeshGo[index] = clothMainGo;

        }
        else
        {
            costumeMeshGo.Add(index, clothGoShow);
            costumeRealMeshGo.Add(index, clothMainGo);
        }

        //如果slot_name = suit 套装，则需要判断 上装、下装是否为空，
        if (index == 0) //为套装
        {
            //判断上装和下装节点是否为空
            deleteSlotEntity(mSlotName.slotCloth);
            deleteSlotEntity(mSlotName.slotPant);
        }
        if (index == 1 || index == 2)
            deleteSlotEntity(mSlotName.slotSuit);

        //*******************************************************
        consumeABS[index] = tempab;
        m_listMeshRecord[index] = meshName;

        //共享骨骼
        Dictionary<string, int> clothBoneDict = new Dictionary<string, int>();
        List<Transform> clothBones = new List<Transform>();

        clearSimulatorInfo(index);

        if(clothMainGo)
        {
            updateCosumeBone(clothMainGo,index, m_BodySk, exRootBones, clothBoneDict, clothBones);
            if (m_UnityTest.flag_cloth_sim_test && exRootBones.Count>0)
            { 
                //是否需要为此资源添加物理参数
                if (simFile != "" )
                {
                    string dataJson = File.ReadAllText(simFile);
                    CostumeSimParamJson clothSimData = JsonUtility.FromJson<CostumeSimParamJson>(dataJson);
                    constumeDynamicBones[index] = m_SceneRootNode.AddComponent<DynamicBone>();
                    sim.addSimulationParamForGo(constumeDynamicBones[index],clothSimData, clothBones, clothBoneDict);
                }
            }
        }

        return true;
    }

    private void clearSimulatorInfo(int iClothIndex)
    {
        if (iClothIndex < 0)
            return;

        //step1: 销毁index物体 对应的外骨骼对象
        for (int i = 0; i < m_SlotExBonesList[iClothIndex].Count; i++)
        {
           if(m_SlotExBonesList[iClothIndex][i])
             GameObject.Destroy(m_SlotExBonesList[iClothIndex][i]);
        }
        //step2:
        if (constumeDynamicBones[iClothIndex])
            GameObject.Destroy(constumeDynamicBones[iClothIndex]);

    }

    public void cancelCostume(string strClothType)
    {

        int index = getSlotIndex(strClothType);
        if (index < 0)
        {
            MsgEvent.SendCallBackMsg((int)AvatarID.Err_cloth_slotname, AvatarID.Err_cloth_slotname.ToString());
            return;
        }

    
        deleteSlotEntity(strClothType);

    }

    private void updateCosumeBone(GameObject srcGO, int goID,SkinnedMeshRenderer target, List<exBoneParam> exBones,
                                  Dictionary<string, int> clothBoneDic, List<Transform> clothBones)
    {
        SkinnedMeshRenderer srcSK = srcGO.GetComponent<SkinnedMeshRenderer>();
      
        for (int i = 0; i < srcSK.bones.Length; ++i)
        {
            clothBoneDic.Add(srcSK.bones[i].name, i);
            clothBones.Add(srcSK.bones[i]);
        }

        int pboneID;
        int boneID;

        //step2:先将服装骨骼的初始位置与模型骨骼的位置对齐
        for (int i = 0; i < srcSK.bones.Length; ++i)
        {
            if (m_Avatar_manager.bonesDic.TryGetValue(srcSK.bones[i].name, out boneID))
            {
                srcSK.bones[i].position = target.bones[boneID].position;
                srcSK.bones[i].rotation = target.bones[boneID].rotation;
            }
        }

        //step3: 将外骨骼 的父节点挂接到 avatar 骨骼上
        for (int i = 0; i < exBones.Count; i++)
        {
            boneID = clothBoneDic[exBones[i].name];
            string pname = exBones[i].parent;
            if (m_Avatar_manager.bonesDic.TryGetValue(pname, out pboneID))
            {
                clothBones[boneID].parent = m_Avatar_manager.bonesTrans[pboneID];
                m_SlotExBonesList[goID].Add(clothBones[boneID].gameObject);
            }
        }

  
        //step4: 共享骨骼
        Transform[] newBones = new Transform[srcSK.bones.Length];
        for (int i = 0; i < srcSK.bones.Length; ++i)
        {
            if (m_Avatar_manager.bonesDic.TryGetValue(srcSK.bones[i].name, out boneID))
                newBones[i] = target.bones[boneID];
            else
                // newBones[i] = GameObject.Find(srcSK.bones[i].name).transform;
                newBones[i] = srcSK.bones[i];
        }



        srcSK.bones = newBones;


    }


    private void deleteSlotEntity(string slotname)
    {
        int index = getSlotIndex(slotname);
        GameObject go;
        costumeMeshGo.TryGetValue(index, out go);
        if (go != null)
        {
            GameObject.Destroy(go);
            consumeABS[index].Unload(true);
            m_listMeshRecord[index] = "";
            costumeMeshGo.Remove(index);
            costumeRealMeshGo.Remove(index);

            //物理仿真
            clearSimulatorInfo(index);
        }

        return;
    }
    /// <summary>
    /// 获取服装插槽索引
    /// </summary>
    /// <param name="slotName"></param>
    /// <returns></returns>
    private int getSlotIndex(string slotName)
    {
        int value = 0;
        costumeSlotDic.TryGetValue(slotName, out value);
        return value;
    }   
    
    /**************************************************************/
    /// <summary>
    /// 
    /// </summary>
    /// <param name="slotType">插槽类型</param>
    /// <param name="strTexFile">贴图文件</param>
    public void changeSlotColor(string slotType,string strTexFile)
    {
        int index = getSlotIndex(slotType);

        GameObject go;
        costumeRealMeshGo.TryGetValue(index, out go);

        if (go == null)
            return;
        SkinnedMeshRenderer hairSK = go.GetComponent<SkinnedMeshRenderer>();

        if(hairSK)
        {
            Material RealMat = hairSK.sharedMaterials[0];
            int nWidth  = RealMat.mainTexture.width;
            int nHeight = RealMat.mainTexture.height;

            Texture2D tex = convertImageToTex(strTexFile,nWidth,nHeight);

            RealMat.mainTexture = tex;
        }
    }

    private Texture2D convertImageToTex(string imgPath, int nwidth, int nheight)
    {
        //读取文件
        FileStream fs = new FileStream(imgPath, FileMode.Open, FileAccess.Read);
        int byteLength = (int)fs.Length;
        byte[] imgBytes = new byte[byteLength];
        fs.Read(imgBytes, 0, byteLength);
        fs.Close();
        fs.Dispose();

        Texture2D tex = new Texture2D(nwidth, nheight, TextureFormat.ARGB32, true);

        tex.LoadImage(imgBytes);
        tex.Apply();

        return tex;
    }

}
