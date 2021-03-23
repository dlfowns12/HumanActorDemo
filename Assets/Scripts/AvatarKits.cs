using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


/*
MonoBehavior 类是不能被new出来，只能通过 Instantiate
*/

namespace Avatar3D
{
    public class AvatarKits
    {
        /*************定义标志****************/

        UnityTest m_UnityTest = new UnityTest();


        /***********模型本身的定义****************/
        public string body_node = "standardbody";
        public string head_node = "standardhead";
        public string brow_node = "standardbrow";
        public string eyelash_node = "standardeyelash";
        public string eyel_node = "standardeyel";
        public string eyer_node = "standardeyer";
        public string oral_node = "standardoral";

        /************基模ab包对象******************/
        private AssetBundle mModel_ab;

        //*****************************************

        private string camera_node_name = "Main Camera";
        private string light_node_name = "Directional Light";

        //跟相机相关
        public CamManager mCamManager = null;

        public GameObject mLightGo = null;

        //跟背景相关的
        private BackGroundManager mBGObj = null;

        //初始化
        public AvatarKits(GameObject go)
        {
            scene_parent_node = go;
            initial();
        }


        private GameObject scene_parent_node = null;
        public bool flag_avatar_load = false;

        /*重要的一些对象定义*/
        //负责管理avatar相关的一些对象
        //根对象、各个mesh对象以及配饰对象
        AvatarManager m_AvatarManager = null;
        SkinnedMeshRenderer av_skHead = null, av_skBrow = null, av_skEyelash = null, av_skEyel = null, av_skEyer = null, av_skOral = null;
        private SkinnedMeshRenderer m_AvatarBodySK;


        /// <summary>
        /// 
        /// 初始化
        /// </summary>
        private void initial()
        {
            //avatar 自身相关的对象分配空间
            m_AvatarManager = new AvatarManager();
            m_AvatarManager.meshGo = new Dictionary<string, GameObject>();
            m_AvatarManager.meshRender = new Dictionary<string, SkinnedMeshRenderer>();
            //****************************************************************************
            //模块：场景管理之相机
            mCamManager = GameObject.Find(camera_node_name).GetComponent<CamManager>();

            mLightGo = GameObject.Find(light_node_name);
        }

        public void unInstall()
        {
            foreach (var tmp in m_AvatarManager.meshGo)
                GameObject.Destroy(tmp.Value);
            foreach (var tmp in m_AvatarManager.meshRender)
                GameObject.Destroy(tmp.Value);

            m_AvatarManager = null;
        }


        // Update is called once per frame
        /// <summary>
        /// 加载标模
        /// </summary>
        /// <param name="strSceneFile"></param>
        /// <returns></returns>
        /// 示例：loadStandardModel("c:/xxx.scene")
        public bool loadStandardModel(string strSceneFile)
        {
            if (scene_parent_node == null)
            {
                MsgEvent.SendCallBackMsg((int)AvatarID.Err_scene_node, AvatarID.Err_scene_node.ToString());
                return false;
            }
            //step1:

            bool res = parseSceneFile(strSceneFile);
            if (!res)
            {
                MsgEvent.SendCallBackMsg((int)AvatarID.Err_model_parse, AvatarID.Err_model_parse.ToString());
                return false;
            }

            flag_avatar_load = true;

            return true;
        }

        /// <summary>
        /// 更新骨骼用
        /// </summary>
        public void Update()
        {

        }


        public void unloadStandardModel()
        {
            if (mModel_ab)
                mModel_ab.Unload(true);

            GameObject.Destroy(m_AvatarManager.MeshRootNode);
            GameObject.Destroy(scene_parent_node);
            flag_avatar_load = false;

        }
        /// <summary>
        /// 解析scene文件
        /// </summary>
        /// <param name="sceneFile"></param>
        private bool parseSceneFile(string strSceneFile)
        {
            string rootDir = Path.GetDirectoryName(strSceneFile);

            StreamReader sr = new StreamReader(strSceneFile);
            if (sr == null)
            {
                MsgEvent.SendCallBackMsg((int)AvatarID.Err_file_no_exist, AvatarID.Err_file_no_exist.ToString());
                return false;
            }
            string jsonStr = sr.ReadToEnd();

            standardmodelJson modeljson = JsonUtility.FromJson<standardmodelJson>(jsonStr);

            if (modeljson.name == null|| modeljson.entity.Count <= 1)
                return false;


            //step1:加载模型
            string abmodel_file = rootDir + "/" + modeljson.abfile;
            mModel_ab = AssetBundle.LoadFromFile(abmodel_file);

            m_AvatarManager.MeshRootNode = GameObject.Instantiate<GameObject>(mModel_ab.LoadAsset<GameObject>(modeljson.name)) as GameObject;


            if (m_AvatarManager.MeshRootNode == null)
                return false;
            m_AvatarManager.MeshRootNode.transform.SetParent(scene_parent_node.transform);

            //step2:处理avatarManager 对象,获取entity相关信息
            for (int i = 0; i < modeljson.entity.Count; i++)
            {
                string node = modeljson.entity[i].node;
                string name = modeljson.entity[i].name;

                GameObject go = GameObject.Find(name);
                if (go == null)
                { Debug.Log("name:" + name); break; }

                m_AvatarManager.meshGo.Add(node, go);

                SkinnedMeshRenderer mRender = go.GetComponent<SkinnedMeshRenderer>();
     
                if (mRender == null)
                {
                    Debug.Log("modeljson entity:" + i + " " + name);
                    break;
                }
                m_AvatarManager.meshRender.Add(node, mRender);
            }

            foreach (var tmp in m_AvatarManager.meshRender)
            {
                    tmp.Value.updateWhenOffscreen = true;
                    Material[] matArr = new Material[tmp.Value.materials.Length];

                    for (int i = 0; i < matArr.Length; i++)
                    {
                        string matName = tmp.Value.materials[i].name;
                        matName = matName.Replace(" (Instance)", "");
                        Material ab_mat = mModel_ab.LoadAsset<Material>(matName);
                        Material matInst = GameObject.Instantiate(ab_mat);
                        matArr[i] = matInst;

                    }
                    tmp.Value.materials = matArr;
            }

            m_AvatarManager.meshRender.TryGetValue(head_node, out av_skHead);
            m_AvatarManager.meshRender.TryGetValue(brow_node, out av_skBrow);
            m_AvatarManager.meshRender.TryGetValue(eyelash_node, out av_skEyelash);
            m_AvatarManager.meshRender.TryGetValue(eyel_node, out av_skEyel);
            m_AvatarManager.meshRender.TryGetValue(eyer_node, out av_skEyer);
            m_AvatarManager.meshRender.TryGetValue(oral_node, out av_skOral);
            m_AvatarManager.meshRender.TryGetValue(body_node, out m_AvatarBodySK);
            m_AvatarManager.bodySKRender = m_AvatarBodySK;
     
            return true;
        }


        public void rotateAvatar(float dist)
        {
            if (m_AvatarManager != null)
            {
                m_AvatarManager.rAngle += dist * m_AvatarManager.speedx;
                if (m_AvatarManager.rAngle >= 360.0f)
                    m_AvatarManager.rAngle -= 360.0f;
                if (m_AvatarManager.rAngle <= -360.0f)
                    m_AvatarManager.rAngle += 360.0f;

                Quaternion rotation = Quaternion.Euler(0, m_AvatarManager.rAngle, 0);
                // m_AvatarManager.MeshRootNode.GetComponent<Transform>().rotation = rotation;
                scene_parent_node.GetComponent<Transform>().rotation = rotation;

            }
        }

        public void setAvatarShow(string strShow)
        {
            if (strShow == "0")
                scene_parent_node.SetActive(false);
            else
                scene_parent_node.SetActive(true);

        }
    }
}
