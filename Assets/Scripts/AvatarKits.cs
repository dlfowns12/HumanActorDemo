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
        private string ptaLinkFile = null;
        private string ptaMapFile = null;
        private string emBsMapFile = null;
        private string boneMapFile = null;
        private string boneSimFile = null;

        //*****************************************

        private string camera_node_name = "Main Camera";
        private string light_node_name = "Directional Light";

        //标模本身管理*****************************
        //存储模型本身的顶点数据
        //*****************************************
        //跟相机相关
        public CamManager mCamManager = null;

        public GameObject mLightGo = null;

        //跟背景相关的
        private BackGroundManager mBGObj = null;

        //跟录制相关

        private SenceRecorder m_SRecorder   = null;



        //抖动系数 尤其是针对AR数据，比如AI检测的 头像位置
        private float preTrackYJitter = -100.0f;
        private float preTrackXJitter = -100.0f;
        private float preScaleJitter = -1.0f;
   




        //初始化
        public AvatarKits(GameObject go)
        {
            scene_parent_node = go;
            initial();
        }


        private GameObject scene_parent_node = null;
        public bool flag_avatar_load = false;

        /************模块对象定义***********************/
        //**********动画对象
        private AnimControl m_AnimCtrl;


        //******头套功能********；
        private float pitchAngle;
        private float yawAngle;
        private float rollAngle;
        private Vector3 head_orig_rotation;
        private Transform head_root_bone_trans = null;

        // 简单头套
        private bool flag_ar_drive_update = false;

        private float m_LocalTrackx, m_LocalTracky, m_LocalTrackz, m_root_scale;
        private Vector3 pre_headPos;
        private Vector3 pre_headAngle;
        Quaternion pre_qt;
        Vector3 pre_direction = new Vector3();
        private Vector3 velocity= Vector3.zero;

        /*重要的一些对象定义*/
        //负责管理avatar相关的一些对象
        //根对象、各个mesh对象以及配饰对象
        AvatarManager m_AvatarManager = null;
        SkinnedMeshRenderer av_skHead = null, av_skBrow = null, av_skEyelash = null, av_skEyel = null, av_skEyer = null, av_skOral = null;
        private List<SkinnedMeshRenderer> m_AvatarHeadSK;
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
            m_AvatarHeadSK = new List<SkinnedMeshRenderer>();
            m_AvatarManager.vtList = new List<Vector3[]>();
            //****************************************************************************
            //模块：场景管理之相机
            mCamManager = GameObject.Find(camera_node_name).GetComponent<CamManager>();

            mLightGo = GameObject.Find(light_node_name);

            //模块：动画模块
            m_AnimCtrl = new AnimControl();

            //模块：录制模块
            m_SRecorder = new SenceRecorder(mCamManager.gameObject);

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
            //step2: 加载完形象之后，需要对其它模块进行初始化
            //***********************动画模块初始化*********************************
            m_AnimCtrl.initial(m_AvatarManager.MeshRootNode);

            head_root_bone_trans = av_skHead.rootBone;
            head_orig_rotation = new Vector3(head_root_bone_trans.localEulerAngles.x, head_root_bone_trans.localEulerAngles.y, head_root_bone_trans.localEulerAngles.z);


            flag_avatar_load = true;

            /**************************/

            return true;
        }

        /// <summary>
        /// 更新骨骼用
        /// </summary>
        public void Update()
        {

           

            if (flag_ar_drive_update)
            {

               
                Vector3 headMovePos = new Vector3(m_LocalTrackx, m_LocalTracky, m_LocalTrackz);

                scene_parent_node.transform.localPosition = Vector3.SmoothDamp(scene_parent_node.transform.localPosition, headMovePos, ref velocity,0.025f);

                //method1:
                // scene_parent_node.transform.localPosition = new Vector3(m_LocalTrackx, m_LocalTracky, m_LocalTrackz);
                //scene_parent_node.transform.localScale = new Vector3(m_AvatarManager.zoomScale, m_AvatarManager.zoomScale, m_AvatarManager.zoomScale);

                head_root_bone_trans.localEulerAngles = new Vector3(head_orig_rotation.x + pitchAngle, head_orig_rotation.y + yawAngle,
                                                            head_orig_rotation.z + rollAngle);

                flag_ar_drive_update = false;
      
            }   
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


            //相关联的几个文件
            ptaLinkFile = rootDir + "/" + modeljson.linkfile;
            ptaMapFile = rootDir + "/" + modeljson.mapfile;
            emBsMapFile = rootDir + "/" + modeljson.embsmapfile;
            boneMapFile = rootDir + "/" + modeljson.bonemapfile;
            boneSimFile = rootDir + "/" + modeljson.simfile;


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
                        if (ab_mat == null)
                        { Debug.Log(matName + "is loss!"); break; return false; }
                        Material matInst = GameObject.Instantiate(ab_mat);
                        matArr[i] = matInst;

                    }
                    tmp.Value.materials = matArr;
              
            }



            // Debug.Log("m_AvatarManager.meshRender.Count:" + m_AvatarManager.meshRender.Count);
            //单独提取skinnedmesh复制到对象上

            Debug.Log("m_AvatarManager.meshRender count:" + m_AvatarManager.meshRender.Count);

            if (m_AvatarManager.meshRender.Count >= 6)
            {
                m_AvatarManager.meshRender.TryGetValue(head_node, out av_skHead);
                m_AvatarManager.meshRender.TryGetValue(brow_node, out av_skBrow);
                m_AvatarManager.meshRender.TryGetValue(eyelash_node, out av_skEyelash);
                m_AvatarManager.meshRender.TryGetValue(eyel_node, out av_skEyel);
                m_AvatarManager.meshRender.TryGetValue(eyer_node, out av_skEyer);
                m_AvatarManager.meshRender.TryGetValue(oral_node, out av_skOral);
                m_AvatarManager.meshRender.TryGetValue(body_node, out m_AvatarBodySK);
                m_AvatarHeadSK.Add(av_skHead);
                m_AvatarHeadSK.Add(av_skBrow);
                m_AvatarHeadSK.Add(av_skEyelash);
                m_AvatarHeadSK.Add(av_skEyer);
                m_AvatarHeadSK.Add(av_skEyel);
                m_AvatarHeadSK.Add(av_skOral);

                //存储顶点
                for (int i = 0; i < m_AvatarHeadSK.Count; i++)
                {
                    Vector3[] vts = m_AvatarHeadSK[i].sharedMesh.vertices;
                    m_AvatarManager.vtList.Add(vts);
                }
            }
            else
            {
                MsgEvent.SendCallBackMsg((int)AvatarID.Err_model_skinmeshrender_count, AvatarID.Err_model_skinmeshrender_count.ToString());
                return false;
            }

            m_AvatarManager.bodySKRender = m_AvatarBodySK;
            m_AvatarManager.getBonesDict(m_AvatarBodySK);


            //ar相关的系数
            if (modeljson.trackoffset == null || modeljson.facewidth <= 0.0f || modeljson.zoomcoef <= 0.0f)
            {
                MsgEvent.SendCallBackMsg((int)AvatarID.Err_model_intial, AvatarID.Err_model_intial.ToString());
                Debug.Log("model intial error");
                return false;
            }
            m_AvatarManager.neckTransMatName = modeljson.necktransmat;
            m_AvatarManager.zoomScale    = modeljson.zoomcoef;
            m_AvatarManager.faceWidth    = modeljson.facewidth;
            m_AvatarManager.trackYOffset = modeljson.trackoffset;
            m_AvatarManager.trackNoseZ   = modeljson.trackz;
            m_AvatarManager.trackxjitter = modeljson.trackxjitter;
            m_AvatarManager.trackyjitter = modeljson.trackyjitter;
            m_AvatarManager.scalejitter  = modeljson.scalejitter;


            int headMatNum = av_skHead.sharedMaterials.Length;

            Material[] headNormMat = new Material[headMatNum];
            for (int i = 0; i < headMatNum; i++)
                headNormMat[i] = av_skHead.sharedMaterials[i];

            m_AvatarManager.headNormalMat = headNormMat;

            Material[] headTransMat = new Material[headMatNum];
            headTransMat[0] = av_skHead.sharedMaterials[0];

            Material matneck = mModel_ab.LoadAsset<Material>(m_AvatarManager.neckTransMatName);
            if (matneck == null)
            {
                MsgEvent.SendCallBackMsg((int)AvatarID.Err_head_material_num, AvatarID.Err_head_material_num.ToString());
                return true;
            }

            Material matInstneck = GameObject.Instantiate(matneck);
            if (matInstneck == null)
                MsgEvent.SendCallBackMsg((int)AvatarID.Err_head_material_num, AvatarID.Err_head_material_num.ToString());
            else
            {
                headTransMat[headMatNum - 1] = matInstneck;
                m_AvatarManager.headTransMat = headTransMat;
            }

      
            return true;
        }

        public void genAvatar(string strJsonFile)
        {
            //step1: 读取生成的形象数据
            string usr_data = File.ReadAllText(strJsonFile);
            AvatarVertexData userData = JsonUtility.FromJson<AvatarVertexData>(usr_data);
            if (userData == null)
            {
                MsgEvent.SendCallBackMsg(3, strJsonFile);
                MsgEvent.SendCallBackMsg((int)AvatarID.Err_pta_file_noexist, AvatarID.Err_pta_file_noexist.ToString());
                return;
            }

            //step2: 读取配置文件
            string head_dump_map = File.ReadAllText(ptaLinkFile);
            string obj_to_ab_map = File.ReadAllText(ptaMapFile);

            if (head_dump_map == null || obj_to_ab_map == null)
            {
                MsgEvent.SendCallBackMsg((int)AvatarID.Err_map_file_noexist, AvatarID.Err_map_file_noexist.ToString());
                return;
            }

            vertLinkMap headDump = JsonUtility.FromJson<vertLinkMap>(head_dump_map);
            AvatarObjMapAB obj2ab = JsonUtility.FromJson<AvatarObjMapAB>(obj_to_ab_map);

            if (headDump.head.Count == 0 || obj2ab.headTable.Count == 0)
            {
                MsgEvent.SendCallBackMsg((int)AvatarID.Err_map_file_noexist, AvatarID.Err_map_file_noexist.ToString());
                return;
            }

          
            List<int[]> vtFlags = new List<int[]>();
            for (int i = 0; i < m_AvatarManager.vtList.Count; i++)
            {
                int num = m_AvatarManager.vtList[i].Length;
                int[] vtflag = new int[num];
                Array.Clear(vtflag, 0, num); //数组每个元素置零
                vtFlags.Add(vtflag);

            
            }

            List<List<vertLinkParam>> headVtlinks = new List<List<vertLinkParam>>();
            List<List<int>> headTables = new List<List<int>>();
            List<List<AvatarPosParam>> exterAvatarPosData = new List<List<AvatarPosParam>>();
            exterAvatarPosData.Add(userData.head); exterAvatarPosData.Add(userData.brow);
            exterAvatarPosData.Add(userData.eyelash); exterAvatarPosData.Add(userData.eyeL);
            exterAvatarPosData.Add(userData.eyeR); exterAvatarPosData.Add(userData.oral);

            headVtlinks.Add(headDump.head); headVtlinks.Add(headDump.brow);
            headVtlinks.Add(headDump.eyelash); headVtlinks.Add(headDump.eyeR);
            headVtlinks.Add(headDump.eyeL); headVtlinks.Add(headDump.oral);

            headTables.Add(obj2ab.headTable); headTables.Add(obj2ab.browTable);
            headTables.Add(obj2ab.eyelashTable); headTables.Add(obj2ab.eyeLTable);
            headTables.Add(obj2ab.eyeRTable); headTables.Add(obj2ab.oralTable);

            for (int i = 0; i < m_AvatarHeadSK.Count; i++)
            {
                Mesh msh = m_AvatarHeadSK[i].sharedMesh;
                Vector3[] vertices = m_AvatarHeadSK[i].sharedMesh.vertices;


                for (int j = 0; j < exterAvatarPosData[i].Count; j++)
                {
                    int id = int.Parse(exterAvatarPosData[i][j].index);

                    float px = exterAvatarPosData[i][j].pos[0];
                    float py = exterAvatarPosData[i][j].pos[1];
                    float pz = exterAvatarPosData[i][j].pos[2];

                    int corrID = headTables[i][id];
                    int num = headVtlinks[i][corrID].linkIndex.Count;

                    for (int k = 0; k < num; k++)
                    {
                        int comIndex = headVtlinks[i][corrID].linkIndex[k];
                      
                        if (comIndex >= vtFlags[i].Length)
                        { MsgEvent.SendCallBackMsg((int)AvatarID.Err_model_index_out, AvatarID.Err_model_index_out.ToString()); return; }

                        if (vtFlags[i][comIndex] == 1)
                            continue;
                        else vtFlags[i][comIndex] = 1;

                        vertices[comIndex].x = px;
                        vertices[comIndex].y = py;
                        vertices[comIndex].z = pz;
                    }
                }
                m_AvatarHeadSK[i].sharedMesh.vertices = vertices;
                // m_AvatarHeadSK[i].sharedMesh.RecalculateNormals();


            }
            MsgEvent.SendCallBackMsg((int)AvatarID.Success, AvatarID.Success.ToString());
        }


        /******************************************************************************************************

           以下是跟相机相关的一些接口

        *******************************************************************************************************/
        /// <summary>
        /// 设置相机参数
        /// </summary>
        /// <param name="strJsonFile"></param>
        public void setCamera(string strJsonFile)
        {
            string strCamData = File.ReadAllText(strJsonFile);
            CamParam camData = JsonUtility.FromJson<CamParam>(strCamData);

            if (camData == null)
                return;

            mCamManager.setPositon(camData.distx, camData.disty, camData.distz);

            if (camData.rotx != 0.0 || camData.roty != 0.0 || camData.rotz != 0.0)
                mCamManager.setRotation(camData.rotx, camData.roty, camData.rotz);

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
        public void setCameraBackgroundColor(string strJsonFile)
        {
            string strCamBgColor = File.ReadAllText(strJsonFile);
            CamBgColorJson camColor = JsonUtility.FromJson<CamBgColorJson>(strCamBgColor);

            if (camColor == null)
                return;

            mCamManager.setCameraBgColor(camColor.red, camColor.green, camColor.blue);

            MsgEvent.SendCallBackMsg((int)AvatarID.Success, AvatarID.Success.ToString());
        }
        public void setCameraBackgroundHtmlStringColor(string HtmlStringColor)
        {
            Color pColor;
            bool res = ColorUtility.TryParseHtmlString(HtmlStringColor, out pColor);
            if (res)
            {
                mCamManager.setCameraBgColor(pColor.r, pColor.g, pColor.b);
                MsgEvent.SendCallBackMsg((int)AvatarID.Success, AvatarID.Success.ToString());
            }
        }



        public void setHeadGesture(float pitchx, float yawy, float rollz)
        {
            pitchAngle = pitchx;
            yawAngle = yawy;
            rollAngle = rollz;

        }

        public void restoreHeadGesture()
        {
            head_root_bone_trans.localEulerAngles = new Vector3(head_orig_rotation.x, head_orig_rotation.y, head_orig_rotation.z);
        }

        public void faceArDrive2(string arJson)
        {

            FaceARJson strFaceArData = JsonUtility.FromJson<FaceARJson>(arJson);
            float[] cameraMatrixArg = strFaceArData.camMatrix;

            Matrix4x4 cameraMatrix = new Matrix4x4();
            cameraMatrix.SetRow(0, new Vector4(cameraMatrixArg[0], cameraMatrixArg[1], cameraMatrixArg[2], cameraMatrixArg[3]));
            cameraMatrix.SetRow(1, new Vector4(cameraMatrixArg[4], cameraMatrixArg[5], cameraMatrixArg[6], cameraMatrixArg[7]));
            cameraMatrix.SetRow(2, new Vector4(cameraMatrixArg[8], cameraMatrixArg[9], cameraMatrixArg[10], cameraMatrixArg[11]));
            cameraMatrix.SetRow(3, new Vector4(cameraMatrixArg[12], cameraMatrixArg[13], cameraMatrixArg[14], cameraMatrixArg[15]));
            Camera.main.projectionMatrix = cameraMatrix.transpose;


            float[] poseMatrixArg = strFaceArData.poseMatrix;
            Matrix4x4 poseMatrix = new Matrix4x4();
            poseMatrix.SetRow(0, new Vector4(poseMatrixArg[0], poseMatrixArg[1], poseMatrixArg[2], poseMatrixArg[3]));
            poseMatrix.SetRow(1, new Vector4(poseMatrixArg[4], poseMatrixArg[5], poseMatrixArg[6], poseMatrixArg[7]));
            poseMatrix.SetRow(2, new Vector4(poseMatrixArg[8], poseMatrixArg[9], poseMatrixArg[10], poseMatrixArg[11]));
            poseMatrix.SetRow(3, new Vector4(poseMatrixArg[12], poseMatrixArg[13], poseMatrixArg[14], poseMatrixArg[15]));
            Matrix4x4 transposeMatrix = poseMatrix.transpose;



            Matrix4x4 matRot = Matrix4x4.identity;
            matRot.SetColumn(0, transposeMatrix.GetColumn(0));
            matRot.SetColumn(1, transposeMatrix.GetColumn(1));
            matRot.SetColumn(2, transposeMatrix.GetColumn(2));
            var mat22 = Matrix4x4.Rotate(Quaternion.AngleAxis(180, Vector3.up));

            matRot = matRot * mat22;

            //右手转左手
            Vector3 tmpt = new Vector3(transposeMatrix.GetColumn(3).x * 1, transposeMatrix.GetColumn(3).y, transposeMatrix.GetColumn(3).z * -1);
            Matrix4x4 matT = Matrix4x4.Translate(tmpt);

            var mat2 = Matrix4x4.Scale(new Vector3(1.0f, 1.0f, -1.0f));


            // 不同模型和算法标准人脸模型的offset调节
            var matTmp = Matrix4x4.identity;

            matRot = mat2 * (matRot * matTmp) * mat2;

            Matrix4x4 result = matT * matRot;

            Vector3 headPos = new Vector3(result.GetColumn(3).x, result.GetColumn(3).y, result.GetColumn(3).z);
            Vector3 headScale = new Vector3(strFaceArData.headScale, strFaceArData.headScale, strFaceArData.headScale);
            Vector3 eulerAngles = result.rotation.eulerAngles;

            Vector3 headAngle = new Vector3(-eulerAngles.x, eulerAngles.y - 180, -eulerAngles.z);

            setHeadGesture(headAngle.y, headAngle.z, headAngle.x + 360.0f);

            Quaternion qt = Quaternion.Euler(headAngle.y, headAngle.z, headAngle.x + 360.0f);
            Vector3 direction = new Vector3(0.0f, 0.0f, 0.0f);
            Vector3 rotatedDirection = qt * direction;
            Vector3 rotatedPoint = rotatedDirection;

            float xvalue = Mathf.Sin(Mathf.PI * headAngle.y / 180.0f);
            xvalue = Mathf.Abs(xvalue);
            

            float yvalue = Mathf.Sin(Mathf.PI * headAngle.x / 180.0f);
            yvalue = Mathf.Abs(yvalue);

            if (!flag_ar_drive_update)
            {

                if (headAngle.y > 0)
                    m_LocalTrackx = headPos.x +xvalue;
                else
                    m_LocalTrackx = headPos.x - xvalue;

                m_LocalTrackx = m_LocalTrackx + 0.0f;

                m_LocalTracky =  headPos.y;
                m_LocalTrackz = -headPos.z;

                pre_headPos   = headPos;
                pre_headAngle = headAngle;

                pre_qt = qt;
                pre_direction = direction;

            }
            flag_ar_drive_update = true;

        }


        /// <summary>
        /// ar驱动，实时跟踪摄像头画面人头位置，驱动虚拟形象头模
        /// </summary>
        public void faceArDrive(string arJson)
        {
            FaceARJson strFaceArData = JsonUtility.FromJson<FaceARJson>(arJson);

            if (strFaceArData.nCamWidth <= 0.0f || strFaceArData.nFaceWidth <= 0.0f)
            {
                return;
            }
            //摄像头画面人脸宽度 转换到 画布 下的宽度
            float headScalex = mBGObj.bgCanvasWidth / (float)strFaceArData.nCamWidth;
            float headScaley = mBGObj.bgCanvasHeight / (float)strFaceArData.nCamHeight;

            strFaceArData.nFaceWidth = (int)((float)strFaceArData.nFaceWidth * headScalex);
            strFaceArData.nFaceCenterY = (int)((float)strFaceArData.nFaceCenterY * headScaley);
            strFaceArData.nFaceCenterX = (int)((float)strFaceArData.nFaceCenterX * headScalex);

            float faceWidthInCanvas = strFaceArData.nFaceWidth * m_AvatarManager.zoomScale;
            float faceWidthInWorld = faceWidthInCanvas * mCamManager.CanvasInWorldWidth / (float)mCamManager.refCanvasWidth;
            float avatarFaceWidthInWorld = m_AvatarManager.faceWidth;
            float aScale = faceWidthInWorld / avatarFaceWidthInWorld;

            //缩放系数抖动处理*************add by 0721********

            aScale = processDataShake(aScale, preScaleJitter, m_AvatarManager.scalejitter);
            preScaleJitter = aScale;

            //*****************************

            float trackx, tracky, trackyy;
            if (!m_UnityTest.flag_ardrive_test)
            {
                strFaceArData.nFaceCenterX = mCamManager.refCanvasWidth - strFaceArData.nFaceCenterX;

                strFaceArData.headAttitudeAngle.rollAngle = 0 - strFaceArData.headAttitudeAngle.rollAngle;
                strFaceArData.headAttitudeAngle.pitchAngle = 0 - strFaceArData.headAttitudeAngle.pitchAngle;
                strFaceArData.headAttitudeAngle.yawAngle = 0 - strFaceArData.headAttitudeAngle.yawAngle;

                trackx = (mCamManager.refCanvasWidth / 2.0f - strFaceArData.nFaceCenterX) / mCamManager.refCanvasWidth * mCamManager.CanvasInWorldWidth;
                trackyy = (mCamManager.refCanvasHeight / 2.0f - strFaceArData.nFaceCenterY) / mCamManager.refCanvasHeight * mCamManager.CanvasInWorldHeight;
            }
            else
            {
                trackx = (mCamManager.refCanvasWidth / 2.0f - strFaceArData.nFaceCenterX) / mCamManager.refCanvasWidth * mCamManager.CanvasInWorldWidth;
                trackyy = (mCamManager.refCanvasHeight / 2.0f - strFaceArData.nFaceCenterY) / mCamManager.refCanvasHeight * mCamManager.CanvasInWorldHeight;
            }

            //位置系数抖动处理************* add by 0721*********

            trackx = processDataShake(trackx, preTrackXJitter, m_AvatarManager.trackxjitter);
            preTrackXJitter = trackx;

            trackyy = processDataShake(trackyy, preTrackYJitter, m_AvatarManager.trackyjitter);
            preTrackYJitter = trackyy;

            //*************************************************

            Debug.Log("trackx:" + trackx);
            Debug.Log("trackyy:" + trackyy);

            tracky = m_AvatarManager.trackYOffset * aScale + trackyy;
            Quaternion qt = Quaternion.Euler(strFaceArData.headAttitudeAngle.rollAngle, strFaceArData.headAttitudeAngle.pitchAngle, strFaceArData.headAttitudeAngle.yawAngle);
            Vector3 direction = new Vector3(0.0f, 0.0f, m_AvatarManager.trackNoseZ * aScale);
            Vector3 rotatedDirection = qt * direction;
            Vector3 rotatedPoint = rotatedDirection;

            //rollangle:  是按照x轴旋转的 x轴朝右正向； pitchangle:是按照y轴旋转的， y轴朝下正向；yawangle:是按照z轴旋转的，朝外为正。
            if (!flag_ar_drive_update)
            {
                setHeadGesture(strFaceArData.headAttitudeAngle.pitchAngle, strFaceArData.headAttitudeAngle.yawAngle, strFaceArData.headAttitudeAngle.rollAngle);
                m_LocalTrackx = trackx + rotatedDirection.x;
                m_LocalTracky = tracky + rotatedDirection.y; ;
                m_root_scale = aScale;
                flag_ar_drive_update = true;
            }

        }
        private float processDataShake(float curData,float preData,float threshold)
        {
            if (Mathf.Abs(curData - preData) <= threshold)
                return preData;
            else
                return curData;
        }

        public void setAvatarBodyHide()
        {
            m_AvatarBodySK.gameObject.SetActive(false);

            if (m_AvatarManager.headTransMat != null)
                av_skHead.materials = m_AvatarManager.headTransMat;
        }
        public void restoreAvatarShow()
        {
            m_AvatarBodySK.gameObject.SetActive(true);
            scene_parent_node.transform.localPosition = new Vector3(0, 0, 0);
            scene_parent_node.transform.localScale = new Vector3(1, 1, 1);
            Quaternion qt = Quaternion.Euler(0, 0, 0);
            scene_parent_node.transform.localRotation = qt;

            //头部姿态
            restoreHeadGesture();

            if (m_AvatarManager.headNormalMat != null)
                av_skHead.materials = m_AvatarManager.headNormalMat;

        }
        public void setAvatarShow(string strShow)
        {
            if (strShow == "0")
                scene_parent_node.SetActive(false);
            else
                scene_parent_node.SetActive(true);

        }

        public void playAnims(string animFile)
        {
            string strAnimData = File.ReadAllText(animFile);
            AnimationJson AnimData = JsonUtility.FromJson<AnimationJson>(strAnimData);
            string rootDir = Path.GetDirectoryName(animFile);
            m_AnimCtrl.playAnim(rootDir, AnimData.file, AnimData.isloop, AnimData.speed);

        }

        public void stopAnims()
        {
            m_AnimCtrl.stopAnim();
        }

        /******************************************************************************************************

         以下是跟录制相关的一些接口

         *******************************************************************************************************/
        public void recordMP4Video(string dataJson)
        {

            RecordVideoJson videoData = JsonUtility.FromJson<RecordVideoJson>(dataJson);
            if (videoData == null)
            {
                MsgEvent.SendCallBackMsg((int)AvatarID.Err_record_data, AvatarID.Err_record_data.ToString());
                return;
            }

            m_SRecorder.startMP4Record(videoData.path, videoData.width, videoData.height, videoData.frameRate, videoData.bitrate,
                                     videoData.sampleRate, videoData.channelCount, videoData.recordVoice);

            //m_SRecorder.startMP4Record("d:/223.mp4", 1080, 1920, 20,
            //                        0, 0, false);


            MsgEvent.SendCallBackMsg((int)AvatarID.Suc_mp4_recording, AvatarID.Suc_mp4_recording.ToString());
        }
        public void stopRecordMP4Video()
        {

            m_SRecorder.stopMP4Record();
            MsgEvent.SendCallBackMsg((int)AvatarID.Suc_mp4_recording_stop, AvatarID.Suc_mp4_recording_stop.ToString());
        }

        public void recordGIF(string dataJson)
        {
            RecordGifJson gifData = JsonUtility.FromJson<RecordGifJson>(dataJson);
            if (gifData == null)
            {
                MsgEvent.SendCallBackMsg((int)AvatarID.Err_record_data, AvatarID.Err_record_data.ToString());
                return;
            }
            m_SRecorder.startGIFRecord(gifData.path, gifData.width, gifData.height, gifData.fps);

            //m_SRecorder.startGIFRecord("d:/3333", 640, 480, 40);


        }
        public void stopRecordGIF()
        {
            m_SRecorder.stopGIFRecord();
        }

        public void captureScreenPng(string dataJson)
        {
            RecordPngJson pngData = JsonUtility.FromJson<RecordPngJson>(dataJson);
            if (pngData == null)
            {
                MsgEvent.SendCallBackMsg((int)AvatarID.Err_record_data, AvatarID.Err_record_data.ToString());
                return;
            }
            m_SRecorder.capturePNG(pngData.path, pngData.width, pngData.height);
           // m_SRecorder.capturePNG("d:/123.png", 520, 520);
            MsgEvent.SendCallBackMsg((int)AvatarID.Suc_png_capture, AvatarID.Suc_png_capture.ToString());

        }

        public void recordWebP(string dataJson)
        {
            RecordWebPJson webpData = JsonUtility.FromJson<RecordWebPJson>(dataJson);
            if (webpData == null)
            {
                MsgEvent.SendCallBackMsg((int)AvatarID.Err_record_data, AvatarID.Err_record_data.ToString());
                return;
            }
           // Debug.Log("recordWebP width:" + webpData.width + " height:" + webpData.height + " fps:" + webpData.fps);
            m_SRecorder.startWebPRecord(webpData.path, webpData.width, webpData.height, webpData.fps);

            //m_SRecorder.startGIFRecord("d:/3333", 640, 480, 40);


        }
        public void stopRecordWebP()
        {
            m_SRecorder.stopWebPRecord();
        }
    }
}
