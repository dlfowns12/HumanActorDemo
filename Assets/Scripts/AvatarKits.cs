using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Avatar3D
{

    [Serializable]
    public class standardmodelJson
    {
        public string name;
        public string abfile;
    }

    public class AvatarKits
    {
        private AssetBundle mModel_ab;

        //初始化
        public AvatarKits(GameObject go)
        {
            scene_parent_node = go;
        }

        private GameObject scene_parent_node = null;
        public GameObject MeshRootNode;

        // Update is called once per frame
        /// <summary>
        /// 加载标模
        /// </summary>
        /// <param name="strSceneFile"></param>
        /// <returns></returns>
        /// 示例：loadStandardModel("c:/xxx.scene")
        public bool loadStandardModel(string strSceneFile)
        {
            return parseSceneFile(strSceneFile);
        }

        public void unloadStandardModel()
        {
            if (mModel_ab)
                mModel_ab.Unload(true);

            GameObject.Destroy(MeshRootNode);
            GameObject.Destroy(scene_parent_node);
        }
        /// <summary>
        /// 解析scene文件
        /// </summary>
        /// <param name="sceneFile"></param>
        private bool parseSceneFile(string strSceneFile)
        {
            string rootDir = Path.GetDirectoryName(strSceneFile);
            StreamReader sr = new StreamReader(strSceneFile);
            string jsonStr = sr.ReadToEnd();
            standardmodelJson modeljson = JsonUtility.FromJson<standardmodelJson>(jsonStr);
            mModel_ab = AssetBundle.LoadFromFile(rootDir + "/" + modeljson.abfile);
            MeshRootNode = GameObject.Instantiate<GameObject>(mModel_ab.LoadAsset<GameObject>(modeljson.name)) as GameObject;
            MeshRootNode.transform.SetParent(scene_parent_node.transform);
     
            return true;
        }
    }
}
