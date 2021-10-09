
#define JOINRS_DRIVE_DEBUG_CODE
#define ENABLE_FINGER_DRIVE
#define ENABLE_RESET_BONE_WITH_ONLY_BODY



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Newtonsoft.Json;
using RootMotion.FinalIK;


using BoneHelper;




/*
Brief : 主要实现虚拟形象肢体驱动，包括 头部姿态、身体姿态
Author:
Date  : 0928
note:核心功能尽量不采用MonoBehavior继承
*/
public class BodyDrive 
{


#if JOINRS_DRIVE_DEBUG_CODE
    [System.Serializable]
    public class DebugInfo
    {
        public Vector3 displayOffset = new Vector3(0.4f, 0, 0);
        [Range(-180, 180)]
        public float xRotate = 0.0f;
        [Range(-180, 180)]
        public float yRotate = 0.0f;
        [Range(-180, 180)]
        public float zRotate = 0.0f;
        public bool enableScaleJointsPoint = true;
        public bool enableKalmanFilter = true;
        public bool enableJointsPointsAmend = true;

        public bool driveControl = false;
        public bool enableDrive = true;
        public bool enableBodyDrive = true;
        public bool enableHandDrive = true;
        public bool enableFootDrive = true;

        [Range(-90, 90)]
        public float upperBodyRotate = 0;
        [Range(-90, 90)]
        public float thighRotate = -5;
        [Range(-90, 90)]
        public float legRotate = 15;
    }
    public DebugInfo mDebugInfo = new DebugInfo();
#endif


    /*****************************************
     头部姿态
     *****************************************/
    //头部根骨骼
    private Transform  head_root_bone_trans = null;
    private string     head_root_bone_name;
    private Vector3    head_orig_rotation;

    private float pitchAngle;
    private float yawAngle;
    private float rollAngle;

    public bool flag_intial = true;


    // private GameObject m_BaseModelGo;

    private float boneCorrect_x = -18;
    private float boneCorrect_y = 10;
    private float boneCorrect_z = -4;


    /*****************************************
      肢体驱动
    *****************************************/
    private Dictionary<string, string> boneMapDic;
    public class JointsState
    {
        public Vector3     position;
        public Transform   transform;
    }

    private Dictionary<string, int> m_BonesIndexDic;
    private List<Transform> m_BonesTrans;

    private List<Transform> m_OrigBoneTrans;

    /********************************************/

    protected Vector3[] mJointsPoints;
    protected Vector3[] mOriginJointsPoints;
    protected bool mHasJoints2D = false;
    protected Vector2[] mJoints2DPoints;
    protected float[] mJointsScore;
    protected Rect mJoints2DBoundingBox;
    protected JointsState[] mInitJointsState;
    protected Dictionary<string, int> mBodyBoneIndices;
    protected Dictionary<DriveNodeType, GameObject> mDriveNodes;

    public GameObject mBaseModelObject;
    protected FullBodyBipedIK mFullBodyBipedIK;
    protected FingerRig[] mFingerRigs;
#if ENABLE_BODY_JUMP
    public class FrameInfo
    {
        public long frameTime;// ms
        public float y;
    }
    protected Vector3 mPlayerInitPosition;
    protected Queue<FrameInfo> mFrameInfos = new Queue<FrameInfo>();
    protected float mYScore = 0.0f;
#endif

    private Vector3 mForward;
    protected Vector3 mLeftHandForward;
    protected Vector3 mRightHandForward;
    protected Quaternion mLeftHandInvRotation;
    protected Quaternion mRightHandInvRotation;
    protected Quaternion mLeftAnkleInvRotation;
    protected Quaternion mRightAnkleInvRotation;

    protected Quaternion mLeftHipRotationCorrection;
    protected Quaternion mRightHipRotationCorrection;
    protected float mLeftShoulderAngleCorrection;
    protected float mRightShoulderAngleCorrection;
    protected float mLeftFootAngleCorrection;
    protected float mRightFootAngleCorrection;

    protected Vector3 mCurrentFulcrumOffset = Vector3.zero;
    protected Vector3 mCurrentLeftHandForward = Vector3.zero;
    protected Vector3 mCurrentRightHandForward = Vector3.zero;

    protected Vector3[] mJointKalmanP = new Vector3[(int)PlayerKeyJointSlot.Count];
    protected Vector3[] mJointKalmanX = new Vector3[(int)PlayerKeyJointSlot.Count];
    protected Vector3[] mJointKalmanK = new Vector3[(int)PlayerKeyJointSlot.Count];
    protected float Q = 0.001f;
    protected float R = 0.0015f;

    protected bool mEnableDrive = true;
    protected bool mEnableBodyDrive = true;
    protected bool mEnableHandDrive = true;
    protected bool mEnableFootDrive = true;
    protected bool mEnableHalfBodyDrive = false;
    protected float mDriveTotalDegree = 1.0f;

#if ENABLE_RESET_BONE_WITH_DRIVE_DEGREE
    protected float mEasingFunctionsParam = 1.0f;
    protected float mDriveDegreeAttenuation = 0.03f;
    protected bool mEnableDriveAttenuation = false;
#endif



    /***********************************************/




    /**********************************************/
    private static string sDriveObjectName = "BoneBodyControlObj";
    private static string sHipObjectName   = "BoneHipControlObj";
    private static string sRenderObjectName = "DebugRender";

    private bool flag_body_drive_enable   = false;

    private bool flag_bone_initial = false;


    skeletonJson m_SkeletonData;


    public BodyDrive()
    {

    }

    public void initial(string boneMapFile,SkinnedMeshRenderer skHead,GameObject baseModelGo,Dictionary<string,int> bonesDict,List<Transform> boneTrans)
    {
        head_root_bone_name = skHead.rootBone.name;
        head_root_bone_trans = skHead.rootBone;

        if(head_root_bone_name =="")
            flag_intial = false;


        head_orig_rotation = new Vector3(head_root_bone_trans.localEulerAngles.x, head_root_bone_trans.localEulerAngles.y, head_root_bone_trans.localEulerAngles.z);


        //*****************
        boneMapDic = new Dictionary<string, string>();

        //step1:读取 骨骼映射文件
        string strBoneData = File.ReadAllText(boneMapFile);
        BoneMapJson bonesData = JsonConvert.DeserializeObject<BoneMapJson>(strBoneData);

        if(bonesData == null)
        {
            flag_intial = false;
            MsgEvent.SendCallBackMsg((int)AvatarID.Err_bone_map_file, AvatarID.Err_bone_map_file.ToString());
        }

        for (int i = 0; i < bonesData.BoneMap.Count; i++)
        {
            string boneName    = bonesData.BoneMap[i].name;
            string bone_map_Name = bonesData.BoneMap[i].mapname;
            boneMapDic.Add(boneName, bone_map_Name);
        }

       // m_BaseModelGo = baseModelGo;
        mBaseModelObject = baseModelGo;
        //****肢体驱动初始化*****

        m_BonesIndexDic = bonesDict;
        m_BonesTrans = boneTrans;


        //初始化
        mJointsPoints = new Vector3[(int)PlayerKeyJointSlot.Count];
        mOriginJointsPoints = new Vector3[(int)PlayerKeyJointSlot.Count];
        mJoints2DPoints = new Vector2[(int)PlayerKeyJointSlot.Count];
        mJointsScore = new float[(int)PlayerKeyJointSlot.Count];



        flag_bone_initial = false;
    }


    public void Update(int type )
    {
        if(type == 1)
           head_root_bone_trans.localEulerAngles = new Vector3(head_orig_rotation.x + pitchAngle, head_orig_rotation.y + yawAngle,
                                                            head_orig_rotation.z + rollAngle);
        if (type == 2)
            setBonesDriveData(m_SkeletonData);
      

    }

    /// <summary>
    /// 设定头部姿态
    /// </summary>
    /// <param name="pitchx"></param>
    /// <param name="yawy"></param>
    /// <param name="rollz"></param>



    public void setHeadGesture(float pitchx,float yawy,float rollz)
    {
         pitchAngle = pitchx;
         yawAngle   = yawy;
         rollAngle  = rollz;

}
    /// <summary>
    /// 恢复头部姿态到初始位置
    /// </summary>
    public void restoreHeadGesture()
    {
        head_root_bone_trans.localEulerAngles = new Vector3(head_orig_rotation.x, head_orig_rotation.y,head_orig_rotation.z );
    }

    /******************肢体驱动相关*****************/

    private void skeletonInitial()
    {
       
        if (flag_bone_initial)
        {
            if (!mFullBodyBipedIK.enabled)
            {
                mFullBodyBipedIK.enabled = true;
                EnableFingerDrive(true, 0);
                EnableFingerDrive(true, 1);
            }
            return;
        }
    
        flag_bone_initial = true;
        mInitJointsState = new JointsState[(int)PlayerKeyJointSlot.Count];

        for (var slot = PlayerKeyJointSlot.Shoulder_R; slot < PlayerKeyJointSlot.Count; slot++)
        {
            var name = slot.ToString();
            if (m_BonesIndexDic.ContainsKey(name))
            {
                int index = m_BonesIndexDic[name];
                var jointsState = new JointsState();
                jointsState.transform = m_BonesTrans[index];
                jointsState.position = m_BonesTrans[index].position;
                mInitJointsState[(int)slot]= jointsState;

            }
            else
                Debug.LogError("can not get the transform " + name);
        }
        /*********ADD 0323*************/
        // 获取身体骨骼(不包含头部), 用于无肢体数据时重置骨骼


        /**************************/
        mForward = JointsDriveHelper.GetNormal(
             mInitJointsState[(int)PlayerKeyJointSlot.Spine1_M].transform.position,
             mInitJointsState[(int)PlayerKeyJointSlot.Hip_L].transform.position,
             mInitJointsState[(int)PlayerKeyJointSlot.Hip_R].transform.position);

        mLeftAnkleInvRotation = Quaternion.Inverse(Quaternion.LookRotation(
            mInitJointsState[(int)PlayerKeyJointSlot.Ankle_L].transform.position -
            mInitJointsState[(int)PlayerKeyJointSlot.Toes_L].transform.position, mForward)) *
            mInitJointsState[(int)PlayerKeyJointSlot.Ankle_L].transform.rotation;

        mRightAnkleInvRotation = Quaternion.Inverse(Quaternion.LookRotation(
            mInitJointsState[(int)PlayerKeyJointSlot.Ankle_R].transform.position -
            mInitJointsState[(int)PlayerKeyJointSlot.Toes_R].transform.position, mForward)) *
            mInitJointsState[(int)PlayerKeyJointSlot.Ankle_R].transform.rotation;

        mLeftHandForward = JointsDriveHelper.GetNormal(
            mInitJointsState[(int)PlayerKeyJointSlot.Wrist_L].transform.position,
            mInitJointsState[(int)PlayerKeyJointSlot.MiddleFinger1_L].transform.position,
            mInitJointsState[(int)PlayerKeyJointSlot.ThumbFinger1_L].transform.position);
        mLeftHandInvRotation = Quaternion.Inverse(Quaternion.LookRotation(
            mInitJointsState[(int)PlayerKeyJointSlot.ThumbFinger1_L].transform.position -
            mInitJointsState[(int)PlayerKeyJointSlot.MiddleFinger1_L].transform.position, mLeftHandForward)) *
            mInitJointsState[(int)PlayerKeyJointSlot.Wrist_L].transform.rotation;

        mRightHandForward = JointsDriveHelper.GetNormal(
            mInitJointsState[(int)PlayerKeyJointSlot.Wrist_R].transform.position,
            mInitJointsState[(int)PlayerKeyJointSlot.ThumbFinger1_R].transform.position,
            mInitJointsState[(int)PlayerKeyJointSlot.MiddleFinger1_R].transform.position);
        mRightHandInvRotation = Quaternion.Inverse(Quaternion.LookRotation(
            mInitJointsState[(int)PlayerKeyJointSlot.ThumbFinger1_R].transform.position -
            mInitJointsState[(int)PlayerKeyJointSlot.MiddleFinger1_R].transform.position, mRightHandForward)) *
            mInitJointsState[(int)PlayerKeyJointSlot.Wrist_R].transform.rotation;

        // 计算标模左右臀部点与Root节点的旋转角度(AI检测这三个点几乎为一条直线), 用于添加在臀部检测点上
        Vector3 vHipRight2HipLeft = (mInitJointsState[(int)PlayerKeyJointSlot.Hip_L].transform.position - mInitJointsState[(int)PlayerKeyJointSlot.Hip_R].transform.position).normalized;
        Vector3 vRoot2HipLeft = (mInitJointsState[(int)PlayerKeyJointSlot.Hip_L].transform.position - mInitJointsState[(int)PlayerKeyJointSlot.Root_M].transform.position).normalized;
        Vector3 vHipLeft2HipRight = (mInitJointsState[(int)PlayerKeyJointSlot.Hip_R].transform.position - mInitJointsState[(int)PlayerKeyJointSlot.Hip_L].transform.position).normalized;
        Vector3 vRoot2HipRight = (mInitJointsState[(int)PlayerKeyJointSlot.Hip_R].transform.position - mInitJointsState[(int)PlayerKeyJointSlot.Root_M].transform.position).normalized;
        mLeftHipRotationCorrection = Quaternion.FromToRotation(vHipRight2HipLeft, vRoot2HipLeft);
        mRightHipRotationCorrection = Quaternion.FromToRotation(vHipLeft2HipRight, vRoot2HipRight);
        // 标模的肩膀,脖子,腰椎这三个点的角度与AI的检测结果有差异, 需补齐差距
        float aiShoulderAngle = 65;
        mLeftShoulderAngleCorrection = Vector3.Angle(
            mInitJointsState[(int)PlayerKeyJointSlot.Spine1_M].position - mInitJointsState[(int)PlayerKeyJointSlot.Neck_M].position,
            mInitJointsState[(int)PlayerKeyJointSlot.Shoulder_L].position - mInitJointsState[(int)PlayerKeyJointSlot.Neck_M].position);
        mLeftShoulderAngleCorrection = (mLeftShoulderAngleCorrection - aiShoulderAngle) * 0.8f;
        mRightShoulderAngleCorrection = Vector3.Angle(
            mInitJointsState[(int)PlayerKeyJointSlot.Spine1_M].position - mInitJointsState[(int)PlayerKeyJointSlot.Neck_M].position,
            mInitJointsState[(int)PlayerKeyJointSlot.Shoulder_R].position - mInitJointsState[(int)PlayerKeyJointSlot.Neck_M].position);
        mRightShoulderAngleCorrection = (mRightShoulderAngleCorrection - aiShoulderAngle) * 0.8f;
        // 标模脚掌的旋转角度有AI检测结果的有差异, 需补齐差距
        float aiFootAngle = 68;
        mLeftFootAngleCorrection = Vector3.Angle(
            mInitJointsState[(int)PlayerKeyJointSlot.Knee_L].position - mInitJointsState[(int)PlayerKeyJointSlot.Ankle_L].position,
            mInitJointsState[(int)PlayerKeyJointSlot.Toes_L].position - mInitJointsState[(int)PlayerKeyJointSlot.Ankle_L].position);
        mLeftFootAngleCorrection = (mLeftFootAngleCorrection - aiFootAngle) * 0.8f;
        mRightFootAngleCorrection = Vector3.Angle(
            mInitJointsState[(int)PlayerKeyJointSlot.Knee_R].position - mInitJointsState[(int)PlayerKeyJointSlot.Ankle_R].position,
            mInitJointsState[(int)PlayerKeyJointSlot.Toes_R].position - mInitJointsState[(int)PlayerKeyJointSlot.Ankle_R].position);
        mRightFootAngleCorrection = (mRightFootAngleCorrection - aiFootAngle) * 0.8f;

        mDriveNodes = GetDriveNodes();
        InitFullBodyBipedIK();


    }

    protected Dictionary<DriveNodeType, GameObject> GetDriveNodes()
    {
        string nodeName = "DriveObject";
        var gameObject = mBaseModelObject.transform.parent.gameObject;

        GameObject driveObject = JointsDriveHelper.GetObject(gameObject, nodeName);
        var currentScale = gameObject.transform.localScale;
        var inverScale = new Vector3(1.0f / currentScale.x, 1.0f / currentScale.y, 1.0f / currentScale.z);
        var inverEuler = gameObject.transform.localEulerAngles * -1;
        driveObject.transform.localRotation = Quaternion.Euler(inverEuler);
        driveObject.transform.localScale = inverScale;
        var driveNodes = new Dictionary<DriveNodeType, GameObject>();
        for (var type = DriveNodeType.none; type < DriveNodeType.count; type++)
        {
            string name = type.ToString();

#if ENABLE_FINGER_DRIVE
#else
            if (name.Contains("Finger"))
            {
                continue;
            }
#endif

            var transform = driveObject.transform.Find(name);
            GameObject node;
            do
            {
                if (transform != null)
                {
                    break;
                }
                node = JointsDriveHelper.GetObject(driveObject, name);
                transform = node.transform;
            } while (false);
            node = transform.gameObject;
            driveNodes[type] = node;
        }
        return driveNodes;
    }

    protected void InitFullBodyBipedIK()
    {
        var ik = mBaseModelObject.AddComponent<FullBodyBipedIK>();

        // 绑定 ik.references
        ik.references.pelvis = mInitJointsState[(int)PlayerKeyJointSlot.Root_M].transform;    // 盆骨

        ik.references.leftThigh = mInitJointsState[(int)PlayerKeyJointSlot.Hip_L].transform;  // 左大腿
        ik.references.leftCalf = mInitJointsState[(int)PlayerKeyJointSlot.Knee_L].transform;  // 左小腿
        ik.references.leftFoot = mInitJointsState[(int)PlayerKeyJointSlot.Ankle_L].transform; // 左脚

        ik.references.rightThigh = mInitJointsState[(int)PlayerKeyJointSlot.Hip_R].transform;
        ik.references.rightCalf = mInitJointsState[(int)PlayerKeyJointSlot.Knee_R].transform;
        ik.references.rightFoot = mInitJointsState[(int)PlayerKeyJointSlot.Ankle_R].transform;

        ik.references.leftUpperArm = mInitJointsState[(int)PlayerKeyJointSlot.Shoulder_L].transform; // 左上臂
        ik.references.leftForearm = mInitJointsState[(int)PlayerKeyJointSlot.Elbow_L].transform; // 左前臂
        ik.references.leftHand = mInitJointsState[(int)PlayerKeyJointSlot.Wrist_L].transform; // 左手

        ik.references.rightUpperArm = mInitJointsState[(int)PlayerKeyJointSlot.Shoulder_R].transform;
        ik.references.rightForearm = mInitJointsState[(int)PlayerKeyJointSlot.Elbow_R].transform;
        ik.references.rightHand = mInitJointsState[(int)PlayerKeyJointSlot.Wrist_R].transform;


        //暂时去掉，肢体驱动和表情驱动 有冲突
        //ik.references.head = mInitJointsState[(int)PlayerKeyJointSlot.Head_M].transform; // 头
        ik.references.spine = new Transform[1];
        ik.references.spine[0] = mInitJointsState[(int)PlayerKeyJointSlot.Spine1_M].transform;

        ik.references.root = mBaseModelObject.transform;
        ik.solver.rootNode = mInitJointsState[(int)PlayerKeyJointSlot.Root_M].transform;

        // 绑定驱动节点
        ik.SetReferences(ik.references, ik.solver.rootNode);
        ik.solver.bodyEffector.target = mDriveNodes[DriveNodeType.body].transform;

        ik.solver.leftHandEffector.target = mDriveNodes[DriveNodeType.leftWrist].transform;
        ik.solver.leftShoulderEffector.target = mDriveNodes[DriveNodeType.leftShoulder].transform;
        ik.solver.leftArmChain.bendConstraint.bendGoal = mDriveNodes[DriveNodeType.leftElbow].transform;

        ik.solver.rightHandEffector.target = mDriveNodes[DriveNodeType.rightWrist].transform;
        ik.solver.rightShoulderEffector.target = mDriveNodes[DriveNodeType.rightShoulder].transform;
        ik.solver.rightArmChain.bendConstraint.bendGoal = mDriveNodes[DriveNodeType.rightElbow].transform;

        ik.solver.leftFootEffector.target = mDriveNodes[DriveNodeType.leftLeg].transform;
        ik.solver.leftThighEffector.target = mDriveNodes[DriveNodeType.leftHip].transform;
        ik.solver.leftLegChain.bendConstraint.bendGoal = mDriveNodes[DriveNodeType.leftKnee].transform;

        ik.solver.rightFootEffector.target = mDriveNodes[DriveNodeType.rightLeg].transform;
        ik.solver.rightThighEffector.target = mDriveNodes[DriveNodeType.rightHip].transform;
        ik.solver.rightLegChain.bendConstraint.bendGoal = mDriveNodes[DriveNodeType.rightKnee].transform;

#if ENABLE_FINGER_DRIVE
        mFingerRigs = new FingerRig[2];
        for (int handIndex = 0; handIndex < 2; handIndex++)
        {
            var fingerRig = ik.gameObject.AddComponent<FingerRig>();
            fingerRig.fingers = new Finger[5];
            var leftFingerSlots = JointsDriveHelper.sLeftFingerSlotsByBone;
            int indexOffset = (handIndex == 0 ? 0 : PlayerKeyJointSlot.Wrist_L_scale - PlayerKeyJointSlot.Wrist_R_scale);
            var driveNode = (handIndex == 0 ? DriveNodeType.ThumbFinger4_L : DriveNodeType.ThumbFinger4_R);
            for (int i = 0; i < 5; i++)
            {
                fingerRig.fingers[i] = new Finger();
                var finger = fingerRig.fingers[i];
                finger.weight = 1.0f;
                finger.rotationWeight = 0.0f;
                finger.bone1 = mInitJointsState[(int)leftFingerSlots[i * 4] - indexOffset].transform;
                finger.bone2 = mInitJointsState[(int)leftFingerSlots[i * 4 + 1] - indexOffset].transform;
                finger.bone3 = mInitJointsState[(int)leftFingerSlots[i * 4 + 2] - indexOffset].transform;
                finger.tip = mInitJointsState[(int)leftFingerSlots[i * 4 + 3] - indexOffset].transform;
                finger.target = mDriveNodes[driveNode + i * 4].transform;
            }
            mFingerRigs[handIndex] = fingerRig;
        }

        EnableFingerDrive(true, 0);
        EnableFingerDrive(true, 1);
#endif


        ik.solver.iterations = 1;

        mFullBodyBipedIK = ik;

        mFullBodyBipedIK.enabled = true;

        UpdateIKParam();
    }

    protected void UpdateIKParam()
    {
        if (mFullBodyBipedIK == null)
        {
            return;
        }
        var ik = mFullBodyBipedIK;

        float bodyDegree = (mEnableDrive && mEnableBodyDrive) ? mDriveTotalDegree : 0.0f;
        ik.solver.bodyEffector.positionWeight = 1.0f * bodyDegree;
        ik.solver.spineStiffness = 1.0f * bodyDegree;
        ik.solver.pullBodyVertical = 0.5f * bodyDegree;
        ik.solver.pullBodyHorizontal = 0.0f * bodyDegree;

        float handDegree = (mEnableDrive && mEnableHandDrive) ? mDriveTotalDegree : 0.0f;
        ik.solver.leftHandEffector.positionWeight = 1.0f * handDegree;
        ik.solver.leftHandEffector.rotationWeight = 1.0f * handDegree;
        ik.solver.leftShoulderEffector.positionWeight = 1.0f * handDegree;
        ik.solver.leftArmChain.reach = 0.0f * handDegree;
        ik.solver.leftArmChain.bendConstraint.weight = 1.0f * handDegree;
        ik.solver.leftArmMapping.weight = 1.0f * handDegree;

        ik.solver.rightHandEffector.positionWeight = 1.0f * handDegree;
        ik.solver.rightHandEffector.rotationWeight = 1.0f * handDegree;
        ik.solver.rightShoulderEffector.positionWeight = 1.0f * handDegree;
        ik.solver.rightArmChain.reach = 0.0f * handDegree;
        ik.solver.rightArmChain.bendConstraint.weight = 1.0f * handDegree;
        ik.solver.rightArmMapping.weight = 1.0f * handDegree;

        float footDegree = (mEnableDrive && mEnableFootDrive) ? mDriveTotalDegree : 0.0f;
        ik.solver.leftFootEffector.positionWeight = 1.0f * footDegree;
        ik.solver.leftFootEffector.rotationWeight = 1.0f * footDegree;
        ik.solver.leftThighEffector.positionWeight = 1.0f * footDegree;
        ik.solver.leftLegChain.reach = 0.0f * footDegree;
        ik.solver.leftLegChain.bendConstraint.weight = 0.9f * footDegree;
        ik.solver.leftLegMapping.weight = 1.0f * footDegree;

        ik.solver.rightFootEffector.positionWeight = 1.0f * footDegree;
        ik.solver.rightFootEffector.rotationWeight = 1.0f * footDegree;
        ik.solver.rightThighEffector.positionWeight = 1.0f * footDegree;
        ik.solver.rightLegChain.reach = 0.0f * footDegree;
        ik.solver.rightLegChain.bendConstraint.weight = 0.9f * footDegree;
        ik.solver.rightLegMapping.weight = 1.0f * footDegree;


    }
    private Vector3 kalmanFilter(Vector3 joint, int index)
    {

        Vector3 res;
        mJointKalmanK[index].x = (mJointKalmanP[index].x + Q) / (mJointKalmanP[index].x + Q + R);
        mJointKalmanK[index].y = (mJointKalmanP[index].y + Q) / (mJointKalmanP[index].x + Q + R);
        mJointKalmanK[index].z = (mJointKalmanP[index].z + Q) / (mJointKalmanP[index].x + Q + R);

        mJointKalmanP[index].x = R * (mJointKalmanP[index].x + Q) / (R + mJointKalmanP[index].x + Q);
        mJointKalmanP[index].y = R * (mJointKalmanP[index].x + Q) / (R + mJointKalmanP[index].x + Q);
        mJointKalmanP[index].z = R * (mJointKalmanP[index].x + Q) / (R + mJointKalmanP[index].x + Q);

        res.x = mJointKalmanX[index].x + (joint.x - mJointKalmanX[index].x) * mJointKalmanK[index].x;
        res.y = mJointKalmanX[index].y + (joint.y - mJointKalmanX[index].y) * mJointKalmanK[index].y;
        res.z = mJointKalmanX[index].z + (joint.z - mJointKalmanX[index].z) * mJointKalmanK[index].z;

        mJointKalmanX[index].x = res.x;
        mJointKalmanX[index].y = res.y;
        mJointKalmanX[index].z = res.z;

        return res;

    }

    public void setBodyDriveEnable(bool enable)
    {
        if (mBaseModelObject == null)
        {
            Debug.Log("Base Model Go is not exist!");
            return;
        }
        flag_body_drive_enable = enable;

        var boneIk = mBaseModelObject.GetComponent<FullBodyBipedIK>();
        if (boneIk != null)
        {
            mFullBodyBipedIK.enabled = enable;
            EnableFingerDrive(enable, 0);
            EnableFingerDrive(enable, 1);
        }


        if (boneIk != null)
        {
            mFullBodyBipedIK.enabled = false;
            EnableFingerDrive(false, 0);
            EnableFingerDrive(false, 1);
        }


    }

    public void setFingerDriveEnable(bool enable)
    {
        if (mBaseModelObject == null)
        {
            Debug.Log("Base Model Go is not exist!");
            return;
        }
    }

    public void ScaleJointsPointsToBaseModel()
    {

        var tree = JointsDriveHelper.GetRelationshipTree();
        var queue = new Queue<PlayerKeyJointSlot>();
        queue.Enqueue(PlayerKeyJointSlot.Root_M);

        //var fullBodyRotation = Quaternion.Euler(-(6+12), 10, -4);
         var fullBodyRotation = Quaternion.Euler(boneCorrect_x, boneCorrect_y, boneCorrect_z);


        if (mEnableHalfBodyDrive)
        {
            fullBodyRotation = Quaternion.Euler(0, 0, 0);
        }

        var fullBodyRotate = fullBodyRotation;
#if JOINRS_DRIVE_DEBUG_CODE
        var debugRotate = Quaternion.Euler(mDebugInfo.xRotate, mDebugInfo.yRotate, mDebugInfo.zRotate);
#else
        var debugRotate = Quaternion.Euler(0, 0, 0);
#endif
        fullBodyRotate = debugRotate * fullBodyRotation;

        var vCurrentHip = mJointsPoints[(int)PlayerKeyJointSlot.Hip_L] - mJointsPoints[(int)PlayerKeyJointSlot.Hip_R];
        var vBaseModelHip = mInitJointsState[(int)PlayerKeyJointSlot.Hip_L].position - mInitJointsState[(int)PlayerKeyJointSlot.Hip_R].position;
        // 上半身修正
#if JOINRS_DRIVE_DEBUG_CODE
        var upperBodyRotation = Quaternion.AngleAxis(mDebugInfo.upperBodyRotate, vCurrentHip);
#else
        var upperBodyRotation = Quaternion.AngleAxis(9, vCurrentHip);
#endif
        var upperBodyRotate = debugRotate * fullBodyRotation * upperBodyRotation; // 绕左右大腿连线
        // 肩膀修正
        var leftShoulderNormal = JointsDriveHelper.GetNormal(
            mJointsPoints[(int)PlayerKeyJointSlot.Neck_M],
            mJointsPoints[(int)PlayerKeyJointSlot.Spine1_M],
            mJointsPoints[(int)PlayerKeyJointSlot.Shoulder_L]);
        var leftShoulderRotation = Quaternion.AngleAxis(mLeftShoulderAngleCorrection, leftShoulderNormal);
        var leftShoulderRotate = debugRotate * fullBodyRotation * upperBodyRotation * leftShoulderRotation;
        var rightShoulderNormal = JointsDriveHelper.GetNormal(
            mJointsPoints[(int)PlayerKeyJointSlot.Neck_M],
            mJointsPoints[(int)PlayerKeyJointSlot.Spine1_M],
            mJointsPoints[(int)PlayerKeyJointSlot.Shoulder_R]);
        var rightShoulderRotation = Quaternion.AngleAxis(mRightShoulderAngleCorrection, rightShoulderNormal);
        var rightShoulderRotate = debugRotate * fullBodyRotation * upperBodyRotation * rightShoulderRotation;

        // 臀部修正
        var hipRotation = Quaternion.FromToRotation(vBaseModelHip, vCurrentHip);
        var leftHipRotate = hipRotation * mLeftHipRotationCorrection * Quaternion.Inverse(hipRotation);
        leftHipRotate = debugRotate * fullBodyRotation * leftHipRotate;
        var rightHipRotate = hipRotation * mRightHipRotationCorrection * Quaternion.Inverse(hipRotation);
        rightHipRotate = debugRotate * fullBodyRotation * rightHipRotate;
        // 腿部修正
        var leftLegNormal = JointsDriveHelper.GetNormal(
            mJointsPoints[(int)PlayerKeyJointSlot.Knee_L],
            mJointsPoints[(int)PlayerKeyJointSlot.Hip_L],
            mJointsPoints[(int)PlayerKeyJointSlot.Ankle_L]);
        var rightLegNormal = JointsDriveHelper.GetNormal(
            mJointsPoints[(int)PlayerKeyJointSlot.Knee_R],
            mJointsPoints[(int)PlayerKeyJointSlot.Hip_R],
            mJointsPoints[(int)PlayerKeyJointSlot.Ankle_R]);
        // 大腿修正
#if JOINRS_DRIVE_DEBUG_CODE
        float thighAngle = mDebugInfo.thighRotate;
#else
        float thighAngle = 0; // 绕大腿,膝盖,脚踝所在平面法线
#endif
        var leftThighRotation = Quaternion.Euler(0, 0, 0);
        if (leftLegNormal.magnitude > 0.5f)
        {
            leftThighRotation = Quaternion.AngleAxis(thighAngle, leftLegNormal);
        }
        var leftThighRotate = debugRotate * fullBodyRotation * leftThighRotation;
        var rightThighRotation = Quaternion.Euler(0, 0, 0);
        if (rightLegNormal.magnitude > 0.5f)
        {
            rightThighRotation = Quaternion.AngleAxis(thighAngle, rightLegNormal);
        }
        var rightThighRotate = debugRotate * fullBodyRotation * rightThighRotation;
        // 小腿修正
#if JOINRS_DRIVE_DEBUG_CODE
        float legAngle = mDebugInfo.legRotate;
#else
        float legAngle = 9; // 绕大腿,膝盖,脚踝所在平面法线
#endif
        var leftLegRotation = Quaternion.Euler(0, 0, 0);
        if (leftLegNormal.magnitude > 0.5f)
        {
            leftLegRotation = Quaternion.AngleAxis(legAngle, leftLegNormal);
        }
        var leftLegRotate = debugRotate * fullBodyRotation * leftThighRotation * leftLegRotation;
        var rightLegRotation = Quaternion.Euler(0, 0, 0);
        if (rightLegNormal.magnitude > 0.5f)
        {
            rightLegRotation = Quaternion.AngleAxis(legAngle, rightLegNormal);
        }
        var rightLegRotate = debugRotate * fullBodyRotation * rightThighRotation * rightLegRotation;
        // 脚掌修正
        var leftFootNormal = JointsDriveHelper.GetNormal(
            mJointsPoints[(int)PlayerKeyJointSlot.Ankle_L],
            mJointsPoints[(int)PlayerKeyJointSlot.Knee_L],
            mJointsPoints[(int)PlayerKeyJointSlot.Toes_L]);
        var leftFootRotation = Quaternion.AngleAxis(mLeftFootAngleCorrection, leftFootNormal);
        var leftFootRotate = debugRotate * fullBodyRotation * leftLegRotation * leftFootRotation;
        var rightAnkleNormal = JointsDriveHelper.GetNormal(
            mJointsPoints[(int)PlayerKeyJointSlot.Ankle_R],
            mJointsPoints[(int)PlayerKeyJointSlot.Knee_R],
            mJointsPoints[(int)PlayerKeyJointSlot.Toes_R]);
        var rightFootRotation = Quaternion.AngleAxis(mRightFootAngleCorrection, rightAnkleNormal);
        var rightFootRotate = debugRotate * fullBodyRotation * rightLegRotation * rightFootRotation;

        var jointsRotate = new Dictionary<PlayerKeyJointSlot, Quaternion>();
        // 头部
        jointsRotate[PlayerKeyJointSlot.nose_int] = upperBodyRotate;
        jointsRotate[PlayerKeyJointSlot.Head_M] = upperBodyRotate;
        jointsRotate[PlayerKeyJointSlot.Neck_M] = upperBodyRotate;
        // 上半身
        jointsRotate[PlayerKeyJointSlot.Shoulder_L] = leftShoulderRotate;
        jointsRotate[PlayerKeyJointSlot.Elbow_L] = upperBodyRotate;
        jointsRotate[PlayerKeyJointSlot.Wrist_L_scale] = upperBodyRotate;
        jointsRotate[PlayerKeyJointSlot.Shoulder_R] = rightShoulderRotate;
        jointsRotate[PlayerKeyJointSlot.Elbow_R] = upperBodyRotate;
        jointsRotate[PlayerKeyJointSlot.Wrist_R_scale] = upperBodyRotate;
        jointsRotate[PlayerKeyJointSlot.Spine1_M] = upperBodyRotate;
        // 下半身
        jointsRotate[PlayerKeyJointSlot.Hip_L] = leftHipRotate;
        jointsRotate[PlayerKeyJointSlot.Knee_L] = leftThighRotate;
        jointsRotate[PlayerKeyJointSlot.Ankle_L] = leftLegRotate;
        jointsRotate[PlayerKeyJointSlot.Toes_L] = leftFootRotate;
        jointsRotate[PlayerKeyJointSlot.Hip_R] = rightHipRotate;
        jointsRotate[PlayerKeyJointSlot.Knee_R] = rightThighRotate;
        jointsRotate[PlayerKeyJointSlot.Ankle_R] = rightLegRotate;
        jointsRotate[PlayerKeyJointSlot.Toes_R] = rightFootRotate;

        float alpha = 1.0f;
        Quaternion halfBodyRotate = Quaternion.identity;
        if (mEnableHalfBodyDrive)
        {
            var forward = JointsDriveHelper.GetNormal(
                mJointsPoints[(int)PlayerKeyJointSlot.Spine1_M],
                mJointsPoints[(int)PlayerKeyJointSlot.Hip_L],
                mJointsPoints[(int)PlayerKeyJointSlot.Hip_R]);
            var initForward = new Vector3(mForward.x, 0, mForward.z);
            initForward.Normalize();
            var currentForward = new Vector3(forward.x, 0, forward.z);
            currentForward.Normalize();
            var initBodyLook = Quaternion.LookRotation(mInitJointsState[(int)PlayerKeyJointSlot.Hip_R].position - mInitJointsState[(int)PlayerKeyJointSlot.Hip_L].position, initForward);
            var currentBodyLook = Quaternion.LookRotation(mJointsPoints[(int)PlayerKeyJointSlot.Hip_R] - mJointsPoints[(int)PlayerKeyJointSlot.Hip_L], currentForward);
            var bodyRotate = currentBodyLook * Quaternion.Inverse(initBodyLook);
            var euler = bodyRotate.ToEuler() / Mathf.Acos(-1.0f) * 180;
            alpha = Mathf.Clamp01(Mathf.Max(0, Mathf.Abs((euler.y)) - 20) / (90 - 20));
            halfBodyRotate = Quaternion.Euler(new Vector3(0, euler.y * alpha, 0));

        }

        // 检测点的枚举与标模骨骼节点名可能不一致, 以及部分骨骼节点可能不存在
        PlayerKeyJointSlot GetBodyBone(PlayerKeyJointSlot slot)
        {
            if (slot == PlayerKeyJointSlot.Wrist_L_scale
                || slot == PlayerKeyJointSlot.Wrist_R_scale
                || slot == PlayerKeyJointSlot.ThumbFinger1_L_scale
                || slot == PlayerKeyJointSlot.ThumbFinger1_R_scale
                || slot == PlayerKeyJointSlot.MiddleFinger1_L_scale
                || slot == PlayerKeyJointSlot.MiddleFinger1_R_scale)
            {
                return slot - 1;
            }
            if (slot == PlayerKeyJointSlot.nose_int)
            {
                return PlayerKeyJointSlot.Head_M;
            }
            return slot;
        }
        while (queue.Count != 0)
        {
            var parent = queue.Dequeue();
            if (!tree.ContainsKey(parent))
            {
                continue;
            }
            var parentBodyBone = GetBodyBone(parent);
            var subList = tree[parent];
            foreach (var sub in subList)
            {
                var subBodyBone = GetBodyBone(sub);
                Vector3 dir = mOriginJointsPoints[(int)sub] - mOriginJointsPoints[(int)parent]; // 检测数据朝向
                dir.Normalize();

                Quaternion rotate;
                if (jointsRotate.TryGetValue(subBodyBone, out rotate))
                {
                    dir = rotate * dir;
                }
                else
                {
                    dir = fullBodyRotate * dir;
                }

                if (mEnableHalfBodyDrive && JointsDriveHelper.sLowerBodySlots.Contains(subBodyBone))
                {
                    var initDir = mInitJointsState[(int)subBodyBone].position - mInitJointsState[(int)parentBodyBone].position;
                    initDir.Normalize();
                    if (subBodyBone.ToString().Contains("Hip_"))
                    {
                        dir = initDir * (1 - alpha) + dir * alpha;
                        dir = fullBodyRotate * dir;
                    }
                    else if (subBodyBone.ToString().Contains("Toes_"))
                    {
                        dir = halfBodyRotate * initDir;
                    }
                    else
                    {
                        dir = initDir;
                    }
                }

                Vector3 dis = mInitJointsState[(int)subBodyBone].position - mInitJointsState[(int)parentBodyBone].position;
                mJointsPoints[(int)sub] = mJointsPoints[(int)parent] + dir * dis.magnitude;
                queue.Enqueue(sub);
            }
        }

    }

    public Vector3 GetFulcrumOffset()
    {
        if (mEnableHalfBodyDrive)
        {
            return mInitJointsState[(int)PlayerKeyJointSlot.Root_M].position - mJointsPoints[(int)PlayerKeyJointSlot.Root_M];
        }
        float GetScore(Vector3 detectRoot, Vector3 detectAnkle, Vector3 boneRoot, Vector3 boneAnkle)
        {
            return 50.0f - Vector3.Angle(detectRoot, detectAnkle) - Vector3.Angle(boneRoot, boneAnkle);
        }
        var root = mJointsPoints[(int)PlayerKeyJointSlot.Root_M];
        var leftAnkle = mJointsPoints[(int)PlayerKeyJointSlot.Ankle_L];
        var rightAnkle = mJointsPoints[(int)PlayerKeyJointSlot.Ankle_R];
        var rootByBone = mInitJointsState[(int)PlayerKeyJointSlot.Root_M].position;
        var leftAnkleByBone = mInitJointsState[(int)PlayerKeyJointSlot.Ankle_L].position;
        var rightAnkleByBone = mInitJointsState[(int)PlayerKeyJointSlot.Ankle_R].position;
        var leftScore = GetScore(root, leftAnkle, rootByBone, leftAnkleByBone);
        var rightScore = GetScore(root, rightAnkle, rootByBone, rightAnkleByBone);

        var result = (leftAnkle * leftScore + rightAnkle * rightScore) * -(1.0f / (leftScore + rightScore))
            + (leftAnkleByBone + rightAnkleByBone) * 0.5f;
        return result;
    }





    public void setBonesDriveDataJson(skeletonJson jointsList)
    {
        m_SkeletonData = jointsList;
    }
    //public void SetJointPoints(List<JointsParameter> jointsList)


    int exnum = 0;
    public void setBonesDriveData(skeletonJson jointsList)
    {

        if (!flag_body_drive_enable)
            return;

        //初始化
        skeletonInitial();


        mEnableHalfBodyDrive = jointsList.EnableLowerBody;
        mEnableHandDrive     = jointsList.EnableFingure;

        boneCorrect_x = jointsList.RotationCorrect.x;
        boneCorrect_y = jointsList.RotationCorrect.y;
        boneCorrect_z = jointsList.RotationCorrect.z;


        if (mFingerRigs[0].enabled != mEnableHandDrive)
        {
            EnableFingerDrive(mEnableHandDrive, 0);
            EnableFingerDrive(mEnableHandDrive, 1);
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 5; i++)
                {
                    var finger = mFingerRigs[j].fingers[i];
                    finger.weight = mEnableHandDrive ? 1.0f : 0.0f;
                }
            }
        }

        //设定数据
        Vector3 scale = new Vector3(-1, -1, -1);
        for (int i = 0; i < jointsList.BoneData.Count; i++)
        {
            int index;
            string boneName = null;
            bool bres = boneMapDic.TryGetValue(jointsList.BoneData[i].name, out boneName);

            if (!bres)
                index = (int)Enum.Parse(typeof(PlayerKeyJointSlot), jointsList.BoneData[i].name);
            else
                index = (int)Enum.Parse(typeof(PlayerKeyJointSlot), boneMapDic[jointsList.BoneData[i].name]);

            if (index < (int)PlayerKeyJointSlot.Count)
            {
                mOriginJointsPoints[index].x = jointsList.BoneData[i].x;
                mOriginJointsPoints[index].y = jointsList.BoneData[i].y;
                mOriginJointsPoints[index].z = jointsList.BoneData[i].z;
                mOriginJointsPoints[index].Scale(scale);
                mJointsPoints[index] = mOriginJointsPoints[index];
            }
        }



        


        SwitchDriveAndUpdateParam();
        // 数据矫正及防抖
        ScaleJointsPointsToBaseModel();
       
        KalmanFilter(); // 单帧处理不能使用卡尔曼
        // 驱动
        //UpdateJump();
        UpdateDriveNode();




    }
    public void KalmanFilter()
    {
        for (int i = 0; i < mJointsPoints.Length; i++)
        {
            mJointsPoints[i] = KalmanFilter(mJointsPoints[i], i);
        }
    }
    protected Vector3 KalmanFilter(Vector3 joint, int index)
    {
        Vector3 res;
        mJointKalmanK[index].x = (mJointKalmanP[index].x + Q) / (mJointKalmanP[index].x + Q + R);
        mJointKalmanK[index].y = (mJointKalmanP[index].y + Q) / (mJointKalmanP[index].x + Q + R);
        mJointKalmanK[index].z = (mJointKalmanP[index].z + Q) / (mJointKalmanP[index].x + Q + R);

        mJointKalmanP[index].x = R * (mJointKalmanP[index].x + Q) / (R + mJointKalmanP[index].x + Q);
        mJointKalmanP[index].y = R * (mJointKalmanP[index].x + Q) / (R + mJointKalmanP[index].x + Q);
        mJointKalmanP[index].z = R * (mJointKalmanP[index].x + Q) / (R + mJointKalmanP[index].x + Q);

        res.x = mJointKalmanX[index].x + (joint.x - mJointKalmanX[index].x) * mJointKalmanK[index].x;
        res.y = mJointKalmanX[index].y + (joint.y - mJointKalmanX[index].y) * mJointKalmanK[index].y;
        res.z = mJointKalmanX[index].z + (joint.z - mJointKalmanX[index].z) * mJointKalmanK[index].z;

        mJointKalmanX[index].x = res.x;
        mJointKalmanX[index].y = res.y;
        mJointKalmanX[index].z = res.z;

        return res;

    }
    public void SwitchDriveAndUpdateParam()
    {
        var ik = mFullBodyBipedIK;
        ik.solver.leftHandEffector.rotationWeight = (mFingerRigs != null && mFingerRigs[0].enabled) ? 1 : 0;
        ik.solver.rightHandEffector.rotationWeight = (mFingerRigs != null && mFingerRigs[1].enabled) ? 1 : 0;

#if ENABLE_FINGER_DRIVE
        bool[] enableFingerDrive = new bool[2];
        for (int handIndex = 0; handIndex < 2; handIndex++)
        {
            enableFingerDrive[handIndex] = false;
            var leftFingerSlots = JointsDriveHelper.sLeftFingerSlotsByHand;
            int indexOffset = (handIndex == 0 ? 0 : PlayerKeyJointSlot.Wrist_L_scale - PlayerKeyJointSlot.Wrist_R_scale);
            foreach (var slot in leftFingerSlots)
            {
                int index = (int)slot - indexOffset;
                var joint = mJointsPoints[index];
                if (joint.x >= 0.01f || joint.y >= 0.01f || joint.z >= 0.01f)
                {
                    enableFingerDrive[handIndex] = true;
                    break;
                }
            }
        }
        EnableFingerDrive(enableFingerDrive[0], 0);
        EnableFingerDrive(enableFingerDrive[1], 1);
#endif


    }

    public void EnableFingerDrive(bool enable, int index = -1)
    {
#if ENABLE_FINGER_DRIVE
        if (mFingerRigs == null)
        {
            return;
        }
        if (index < 0)
        {
            for (int i = 0; i < mFingerRigs.Length; i++)
            {
                mFingerRigs[i].enabled = enable;
            }
        }
        else if (index < mFingerRigs.Length)
        {
            mFingerRigs[index].enabled = enable;
        }
#endif
    }

    public void UpdateDriveNode()
    {
        mCurrentFulcrumOffset = GetFulcrumOffset();
       
        {
            mDriveNodes[DriveNodeType.leftLeg].transform.localPosition = Get(PlayerKeyJointSlot.Ankle_L);
            mDriveNodes[DriveNodeType.rightLeg].transform.localPosition = Get(PlayerKeyJointSlot.Ankle_R);
            mDriveNodes[DriveNodeType.leftKnee].transform.localPosition = Get(PlayerKeyJointSlot.Knee_L);
            mDriveNodes[DriveNodeType.rightKnee].transform.localPosition = Get(PlayerKeyJointSlot.Knee_R);
            mDriveNodes[DriveNodeType.leftHip].transform.localPosition = Get(PlayerKeyJointSlot.Hip_L);
            mDriveNodes[DriveNodeType.rightHip].transform.localPosition = Get(PlayerKeyJointSlot.Hip_R);

            mDriveNodes[DriveNodeType.leftLeg].transform.localRotation = Quaternion.LookRotation(
                mJointsPoints[(int)PlayerKeyJointSlot.Ankle_L] - mJointsPoints[(int)PlayerKeyJointSlot.Toes_L],
                mJointsPoints[(int)PlayerKeyJointSlot.Knee_L] - mJointsPoints[(int)PlayerKeyJointSlot.Ankle_L]) * mLeftAnkleInvRotation;
            mDriveNodes[DriveNodeType.rightLeg].transform.localRotation = Quaternion.LookRotation(
                mJointsPoints[(int)PlayerKeyJointSlot.Ankle_R] - mJointsPoints[(int)PlayerKeyJointSlot.Toes_R],
                mJointsPoints[(int)PlayerKeyJointSlot.Knee_R] - mJointsPoints[(int)PlayerKeyJointSlot.Ankle_R]) * mRightAnkleInvRotation;
        }

        Vector3 Get(PlayerKeyJointSlot slot)
        {
            return mJointsPoints[(int)slot] + mCurrentFulcrumOffset;
        }
        // 位移
        mDriveNodes[DriveNodeType.head].transform.localPosition = Get(PlayerKeyJointSlot.Head_M);
        mDriveNodes[DriveNodeType.root].transform.localPosition = Get(PlayerKeyJointSlot.Root_M);
        mDriveNodes[DriveNodeType.neck].transform.localPosition = Get(PlayerKeyJointSlot.Neck_M);
        mDriveNodes[DriveNodeType.spine].transform.localPosition = Get(PlayerKeyJointSlot.Spine1_M);

        mDriveNodes[DriveNodeType.body].transform.localPosition = Get(PlayerKeyJointSlot.Root_M);


        mDriveNodes[DriveNodeType.leftWrist].transform.localPosition = Get(PlayerKeyJointSlot.Wrist_L);
        mDriveNodes[DriveNodeType.rightWrist].transform.localPosition = Get(PlayerKeyJointSlot.Wrist_R);
        mDriveNodes[DriveNodeType.leftElbow].transform.localPosition = Get(PlayerKeyJointSlot.Elbow_L);
        mDriveNodes[DriveNodeType.rightElbow].transform.localPosition = Get(PlayerKeyJointSlot.Elbow_R);
        mDriveNodes[DriveNodeType.leftShoulder].transform.localPosition = Get(PlayerKeyJointSlot.Shoulder_L);
        mDriveNodes[DriveNodeType.rightShoulder].transform.localPosition = Get(PlayerKeyJointSlot.Shoulder_R);

        mCurrentLeftHandForward = mLeftHandForward;
        mCurrentRightHandForward = mRightHandForward;

        if (mFingerRigs != null && mFingerRigs[0].enabled)
        {
            mCurrentLeftHandForward = JointsDriveHelper.GetNormal(
                mJointsPoints[(int)PlayerKeyJointSlot.Wrist_L_scale],
                mJointsPoints[(int)PlayerKeyJointSlot.MiddleFinger1_L_scale],
                mJointsPoints[(int)PlayerKeyJointSlot.ThumbFinger1_L_scale]);
            mDriveNodes[DriveNodeType.leftWrist].transform.localRotation = Quaternion.LookRotation(
                mJointsPoints[(int)PlayerKeyJointSlot.ThumbFinger1_L_scale] - mJointsPoints[(int)PlayerKeyJointSlot.MiddleFinger1_L_scale], mCurrentLeftHandForward) * mLeftHandInvRotation;
        }

        if (mFingerRigs != null && mFingerRigs[1].enabled)
        {
            mCurrentRightHandForward = JointsDriveHelper.GetNormal(
            mJointsPoints[(int)PlayerKeyJointSlot.Wrist_R_scale],
            mJointsPoints[(int)PlayerKeyJointSlot.ThumbFinger1_R_scale],
            mJointsPoints[(int)PlayerKeyJointSlot.MiddleFinger1_R_scale]);
            mDriveNodes[DriveNodeType.rightWrist].transform.localRotation = Quaternion.LookRotation(
                mJointsPoints[(int)PlayerKeyJointSlot.ThumbFinger1_R_scale] - mJointsPoints[(int)PlayerKeyJointSlot.MiddleFinger1_R_scale], mCurrentRightHandForward) * mRightHandInvRotation;
        }

#if ENABLE_FINGER_DRIVE
        for (int handIndex = 0; handIndex < 2; handIndex++)
        {
            if (mFingerRigs == null || !mFingerRigs[handIndex].enabled)
            {
                continue;
            }
            int nodeIndexOffset = (handIndex == 0 ? 0 : DriveNodeType.ThumbFinger1_L - DriveNodeType.ThumbFinger1_R);
            int indexOffset = (handIndex == 0 ? 0 : PlayerKeyJointSlot.Wrist_L_scale - PlayerKeyJointSlot.Wrist_R_scale);
            for (int fingerIndex = 0; fingerIndex < 5; fingerIndex++)
            {
                var handForward = (handIndex == 0
                    ? JointsDriveHelper.GetNormal(
                    mJointsPoints[(int)PlayerKeyJointSlot.Wrist_L_scale],
                    mJointsPoints[(int)PlayerKeyJointSlot.MiddleFinger1_L_scale],
                    mJointsPoints[(int)PlayerKeyJointSlot.ThumbFinger1_L_scale])
                    : JointsDriveHelper.GetNormal(
                    mJointsPoints[(int)PlayerKeyJointSlot.Wrist_R_scale],
                    mJointsPoints[(int)PlayerKeyJointSlot.ThumbFinger1_R_scale],
                    mJointsPoints[(int)PlayerKeyJointSlot.MiddleFinger1_R_scale]));
                var handInverseRotation = handIndex == 0 ? mLeftHandInvRotation : mRightHandInvRotation;
                var lastBoneSlot = (handIndex == 0 ? PlayerKeyJointSlot.Wrist_L_scale : PlayerKeyJointSlot.Wrist_R_scale);
                for (int i = 0; i < 4; i++)
                {
                    int index = fingerIndex * 4 + i;
                    var nodeType = DriveNodeType.ThumbFinger1_L + index - nodeIndexOffset;
                    var slot = JointsDriveHelper.sLeftFingerSlotsByHand[index] - indexOffset;
                    var driveNode = mDriveNodes[nodeType];
                    driveNode.transform.localPosition = Get(slot);

                    var firstBoneSlot = JointsDriveHelper.sLeftFingerSlotsByHand[fingerIndex * 4] - indexOffset;
                    driveNode.transform.localRotation = Quaternion.LookRotation(
                        mJointsPoints[(int)slot] - mJointsPoints[(int)firstBoneSlot], handForward) * handInverseRotation;

                    lastBoneSlot = slot;
                }
            }
        }
#endif


    }



}
