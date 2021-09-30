using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BoneHelper 
{


    public enum PlayerKeyJointSlot
    {
        Shoulder_R,
        Elbow_R,
        Wrist_R,
        Wrist_R_scale,
        ThumbFinger1_R,
        ThumbFinger1_R_scale,
        ThumbFinger2_R,
        ThumbFinger3_R,
        ThumbFinger4_R,
        IndexFinger1_R,
        IndexFinger2_R,
        IndexFinger3_R,
        IndexFinger4_R,
        MiddleFinger1_R,
        MiddleFinger1_R_scale,
        MiddleFinger2_R,
        MiddleFinger3_R,
        MiddleFinger4_R,
        RingFinger1_R,
        RingFinger2_R,
        RingFinger3_R,
        RingFinger4_R,
        PinkyFinger1_R,
        PinkyFinger2_R,
        PinkyFinger3_R,
        PinkyFinger4_R,

        Shoulder_L,
        Elbow_L,
        Wrist_L,
        Wrist_L_scale,
        ThumbFinger1_L,
        ThumbFinger1_L_scale,
        ThumbFinger2_L,
        ThumbFinger3_L,
        ThumbFinger4_L,
        IndexFinger1_L,
        IndexFinger2_L,
        IndexFinger3_L,
        IndexFinger4_L,
        MiddleFinger1_L,
        MiddleFinger1_L_scale,
        MiddleFinger2_L,
        MiddleFinger3_L,
        MiddleFinger4_L,
        RingFinger1_L,
        RingFinger2_L,
        RingFinger3_L,
        RingFinger4_L,
        PinkyFinger1_L,
        PinkyFinger2_L,
        PinkyFinger3_L,
        PinkyFinger4_L,

        Spine1_M,

        Hip_R,
        Knee_R,
        Ankle_R,
        Toes_R,

        Hip_L,
        Knee_L,
        Ankle_L,
        Toes_L,

        nose_int,
        Root_M,
        Head_M,
        Neck_M,

        Count

    }
    public enum DriveNodeType
    {
        none,
        head,
        root,
        neck,
        spine,
        body,

        leftWrist,
        leftShoulder,
        leftElbow,
        rightWrist,
        rightShoulder,
        rightElbow,

        leftLeg,
        leftKnee,
        leftHip,
        rightLeg,
        rightKnee,
        rightHip,

        ThumbFinger1_L,
        ThumbFinger2_L,
        ThumbFinger3_L,
        ThumbFinger4_L,
        IndexFinger1_L,
        IndexFinger2_L,
        IndexFinger3_L,
        IndexFinger4_L,
        MiddleFinger1_L,
        MiddleFinger2_L,
        MiddleFinger3_L,
        MiddleFinger4_L,
        RingFinger1_L,
        RingFinger2_L,
        RingFinger3_L,
        RingFinger4_L,
        PinkyFinger1_L,
        PinkyFinger2_L,
        PinkyFinger3_L,
        PinkyFinger4_L,

        ThumbFinger1_R,
        ThumbFinger2_R,
        ThumbFinger3_R,
        ThumbFinger4_R,
        IndexFinger1_R,
        IndexFinger2_R,
        IndexFinger3_R,
        IndexFinger4_R,
        MiddleFinger1_R,
        MiddleFinger2_R,
        MiddleFinger3_R,
        MiddleFinger4_R,
        RingFinger1_R,
        RingFinger2_R,
        RingFinger3_R,
        RingFinger4_R,
        PinkyFinger1_R,
        PinkyFinger2_R,
        PinkyFinger3_R,
        PinkyFinger4_R,

        count
    }

    public class JointsDriveHelper
    {
        // 左手手指(用于索引手指检测的数据)
        public static PlayerKeyJointSlot[] sLeftFingerSlotsByHand = new PlayerKeyJointSlot[21] {
            PlayerKeyJointSlot.ThumbFinger1_L_scale,
            PlayerKeyJointSlot.ThumbFinger2_L,
            PlayerKeyJointSlot.ThumbFinger3_L,
            PlayerKeyJointSlot.ThumbFinger4_L,
            PlayerKeyJointSlot.IndexFinger1_L,
            PlayerKeyJointSlot.IndexFinger2_L,
            PlayerKeyJointSlot.IndexFinger3_L,
            PlayerKeyJointSlot.IndexFinger4_L,
            PlayerKeyJointSlot.MiddleFinger1_L_scale,
            PlayerKeyJointSlot.MiddleFinger2_L,
            PlayerKeyJointSlot.MiddleFinger3_L,
            PlayerKeyJointSlot.MiddleFinger4_L,
            PlayerKeyJointSlot.RingFinger1_L,
            PlayerKeyJointSlot.RingFinger2_L,
            PlayerKeyJointSlot.RingFinger3_L,
            PlayerKeyJointSlot.RingFinger4_L,
            PlayerKeyJointSlot.PinkyFinger1_L,
            PlayerKeyJointSlot.PinkyFinger2_L,
            PlayerKeyJointSlot.PinkyFinger3_L,
            PlayerKeyJointSlot.PinkyFinger4_L,
            PlayerKeyJointSlot.Wrist_L_scale,
        };
        // 左手手指(用于索引手指骨骼的数据)
        public static PlayerKeyJointSlot[] sLeftFingerSlotsByBone = new PlayerKeyJointSlot[21] {
            PlayerKeyJointSlot.ThumbFinger1_L,
            PlayerKeyJointSlot.ThumbFinger2_L,
            PlayerKeyJointSlot.ThumbFinger3_L,
            PlayerKeyJointSlot.ThumbFinger4_L,
            PlayerKeyJointSlot.IndexFinger1_L,
            PlayerKeyJointSlot.IndexFinger2_L,
            PlayerKeyJointSlot.IndexFinger3_L,
            PlayerKeyJointSlot.IndexFinger4_L,
            PlayerKeyJointSlot.MiddleFinger1_L,
            PlayerKeyJointSlot.MiddleFinger2_L,
            PlayerKeyJointSlot.MiddleFinger3_L,
            PlayerKeyJointSlot.MiddleFinger4_L,
            PlayerKeyJointSlot.RingFinger1_L,
            PlayerKeyJointSlot.RingFinger2_L,
            PlayerKeyJointSlot.RingFinger3_L,
            PlayerKeyJointSlot.RingFinger4_L,
            PlayerKeyJointSlot.PinkyFinger1_L,
            PlayerKeyJointSlot.PinkyFinger2_L,
            PlayerKeyJointSlot.PinkyFinger3_L,
            PlayerKeyJointSlot.PinkyFinger4_L,
            PlayerKeyJointSlot.Wrist_L,
        };

        // 下半身骨骼
        public static HashSet<PlayerKeyJointSlot> sLowerBodySlots = new HashSet<PlayerKeyJointSlot> {
            PlayerKeyJointSlot.Hip_L,
            PlayerKeyJointSlot.Hip_R,
            PlayerKeyJointSlot.Knee_L,
            PlayerKeyJointSlot.Knee_R,
            PlayerKeyJointSlot.Ankle_L,
            PlayerKeyJointSlot.Ankle_R,
            PlayerKeyJointSlot.Toes_L,
            PlayerKeyJointSlot.Toes_R,
        };

        public HashSet<string> sResetSlots = new HashSet<string>
        {
            PlayerKeyJointSlot.Shoulder_R.ToString(),
            PlayerKeyJointSlot.Elbow_R.ToString(),
            PlayerKeyJointSlot.Wrist_R.ToString(),
            PlayerKeyJointSlot.Wrist_R_scale.ToString(),
            PlayerKeyJointSlot.ThumbFinger1_R.ToString(),
            PlayerKeyJointSlot.ThumbFinger1_R_scale.ToString(),
            PlayerKeyJointSlot.ThumbFinger2_R.ToString(),
            PlayerKeyJointSlot.ThumbFinger3_R.ToString(),
            PlayerKeyJointSlot.ThumbFinger4_R.ToString(),
            PlayerKeyJointSlot.IndexFinger1_R.ToString(),
            PlayerKeyJointSlot.IndexFinger2_R.ToString(),
            PlayerKeyJointSlot.IndexFinger3_R.ToString(),
            PlayerKeyJointSlot.IndexFinger4_R.ToString(),
            PlayerKeyJointSlot.MiddleFinger1_R.ToString(),
            PlayerKeyJointSlot.MiddleFinger1_R_scale.ToString(),
            PlayerKeyJointSlot.MiddleFinger2_R.ToString(),
            PlayerKeyJointSlot.MiddleFinger3_R.ToString(),
            PlayerKeyJointSlot.MiddleFinger4_R.ToString(),
            PlayerKeyJointSlot.RingFinger1_R.ToString(),
            PlayerKeyJointSlot.RingFinger2_R.ToString(),
            PlayerKeyJointSlot.RingFinger3_R.ToString(),
            PlayerKeyJointSlot.RingFinger4_R.ToString(),
            PlayerKeyJointSlot.PinkyFinger1_R.ToString(),
            PlayerKeyJointSlot.PinkyFinger2_R.ToString(),
            PlayerKeyJointSlot.PinkyFinger3_R.ToString(),
            PlayerKeyJointSlot.PinkyFinger4_R.ToString(),

            PlayerKeyJointSlot.Shoulder_L.ToString(),
            PlayerKeyJointSlot.Elbow_L.ToString(),
            PlayerKeyJointSlot.Wrist_L.ToString(),
            PlayerKeyJointSlot.Wrist_L_scale.ToString(),
            PlayerKeyJointSlot.ThumbFinger1_L.ToString(),
            PlayerKeyJointSlot.ThumbFinger1_L_scale.ToString(),
            PlayerKeyJointSlot.ThumbFinger2_L.ToString(),
            PlayerKeyJointSlot.ThumbFinger3_L.ToString(),
            PlayerKeyJointSlot.ThumbFinger4_L.ToString(),
            PlayerKeyJointSlot.IndexFinger1_L.ToString(),
            PlayerKeyJointSlot.IndexFinger2_L.ToString(),
            PlayerKeyJointSlot.IndexFinger3_L.ToString(),
            PlayerKeyJointSlot.IndexFinger4_L.ToString(),
            PlayerKeyJointSlot.MiddleFinger1_L.ToString(),
            PlayerKeyJointSlot.MiddleFinger1_L_scale.ToString(),
            PlayerKeyJointSlot.MiddleFinger2_L.ToString(),
            PlayerKeyJointSlot.MiddleFinger3_L.ToString(),
            PlayerKeyJointSlot.MiddleFinger4_L.ToString(),
            PlayerKeyJointSlot.RingFinger1_L.ToString(),
            PlayerKeyJointSlot.RingFinger2_L.ToString(),
            PlayerKeyJointSlot.RingFinger3_L.ToString(),
            PlayerKeyJointSlot.RingFinger4_L.ToString(),
            PlayerKeyJointSlot.PinkyFinger1_L.ToString(),
            PlayerKeyJointSlot.PinkyFinger2_L.ToString(),
            PlayerKeyJointSlot.PinkyFinger3_L.ToString(),
            PlayerKeyJointSlot.PinkyFinger4_L.ToString(),

            PlayerKeyJointSlot.Spine1_M.ToString(),

            PlayerKeyJointSlot.Hip_R.ToString(),
            PlayerKeyJointSlot.Knee_R.ToString(),
            PlayerKeyJointSlot.Ankle_R.ToString(),
            PlayerKeyJointSlot.Toes_R.ToString(),

            PlayerKeyJointSlot.Hip_L.ToString(),
            PlayerKeyJointSlot.Knee_L.ToString(),
            PlayerKeyJointSlot.Ankle_L.ToString(),
            PlayerKeyJointSlot.Toes_L.ToString(),

            PlayerKeyJointSlot.nose_int.ToString(),
            PlayerKeyJointSlot.Root_M.ToString(),
            PlayerKeyJointSlot.Head_M.ToString(),
            PlayerKeyJointSlot.Neck_M.ToString(),
        };
        private static Dictionary<PlayerKeyJointSlot, List<PlayerKeyJointSlot>> sJointsTree = null;

        public static Dictionary<PlayerKeyJointSlot, List<PlayerKeyJointSlot>> GetRelationshipTree()
        {
            if (sJointsTree != null)
            {
                return sJointsTree;
            }
            sJointsTree = new Dictionary<PlayerKeyJointSlot, List<PlayerKeyJointSlot>>();
            var map = sJointsTree;
            map[PlayerKeyJointSlot.Shoulder_R] = new List<PlayerKeyJointSlot> { PlayerKeyJointSlot.Elbow_R };
            map[PlayerKeyJointSlot.Elbow_R] = new List<PlayerKeyJointSlot> { PlayerKeyJointSlot.Wrist_R };
            map[PlayerKeyJointSlot.Wrist_R] = new List<PlayerKeyJointSlot> {
                PlayerKeyJointSlot.Wrist_R_scale,
                PlayerKeyJointSlot.ThumbFinger1_R,
                PlayerKeyJointSlot.MiddleFinger1_R };
            map[PlayerKeyJointSlot.Wrist_R_scale] = new List<PlayerKeyJointSlot> {
                PlayerKeyJointSlot.ThumbFinger1_R_scale,
                PlayerKeyJointSlot.IndexFinger1_R,
                PlayerKeyJointSlot.MiddleFinger1_R_scale,
                PlayerKeyJointSlot.RingFinger1_R,
                PlayerKeyJointSlot.PinkyFinger1_R,
            };
            map[PlayerKeyJointSlot.ThumbFinger1_R] = new List<PlayerKeyJointSlot> { };
            map[PlayerKeyJointSlot.MiddleFinger1_R] = new List<PlayerKeyJointSlot> { };

            map[PlayerKeyJointSlot.Shoulder_L] = new List<PlayerKeyJointSlot> { PlayerKeyJointSlot.Elbow_L };
            map[PlayerKeyJointSlot.Elbow_L] = new List<PlayerKeyJointSlot> { PlayerKeyJointSlot.Wrist_L };
            map[PlayerKeyJointSlot.Wrist_L] = new List<PlayerKeyJointSlot> {
                PlayerKeyJointSlot.Wrist_L_scale,
                PlayerKeyJointSlot.ThumbFinger1_L,
                PlayerKeyJointSlot.MiddleFinger1_L
            };
            map[PlayerKeyJointSlot.Wrist_L_scale] = new List<PlayerKeyJointSlot> {
                PlayerKeyJointSlot.ThumbFinger1_L_scale,
                PlayerKeyJointSlot.IndexFinger1_L,
                PlayerKeyJointSlot.MiddleFinger1_L_scale,
                PlayerKeyJointSlot.RingFinger1_L,
                PlayerKeyJointSlot.PinkyFinger1_L,
            };
            map[PlayerKeyJointSlot.ThumbFinger1_L] = new List<PlayerKeyJointSlot> { };
            map[PlayerKeyJointSlot.MiddleFinger1_L] = new List<PlayerKeyJointSlot> { };

            for (int handIndex = 0; handIndex < 2; handIndex++)
            {
                int indexOffset = (handIndex == 0 ? 0 : PlayerKeyJointSlot.Wrist_L_scale - PlayerKeyJointSlot.Wrist_R_scale);
                for (int i = 0; i < 5; i++)
                {
                    var finger1 = sLeftFingerSlotsByHand[i * 4] - indexOffset;
                    var finger2 = sLeftFingerSlotsByHand[i * 4 + 1] - indexOffset;
                    var finger3 = sLeftFingerSlotsByHand[i * 4 + 2] - indexOffset;
                    var finger4 = sLeftFingerSlotsByHand[i * 4 + 3] - indexOffset;
                    map[finger1] = new List<PlayerKeyJointSlot> { finger2 };
                    map[finger2] = new List<PlayerKeyJointSlot> { finger3 };
                    map[finger3] = new List<PlayerKeyJointSlot> { finger4 };
                    map[finger4] = new List<PlayerKeyJointSlot> { };
                }
            }

            map[PlayerKeyJointSlot.Spine1_M] = new List<PlayerKeyJointSlot> { PlayerKeyJointSlot.Neck_M };

            map[PlayerKeyJointSlot.Hip_R] = new List<PlayerKeyJointSlot> { PlayerKeyJointSlot.Knee_R };
            map[PlayerKeyJointSlot.Knee_R] = new List<PlayerKeyJointSlot> { PlayerKeyJointSlot.Ankle_R };
            map[PlayerKeyJointSlot.Ankle_R] = new List<PlayerKeyJointSlot> { PlayerKeyJointSlot.Toes_R };
            map[PlayerKeyJointSlot.Toes_R] = new List<PlayerKeyJointSlot> { };

            map[PlayerKeyJointSlot.Hip_L] = new List<PlayerKeyJointSlot> { PlayerKeyJointSlot.Knee_L };
            map[PlayerKeyJointSlot.Knee_L] = new List<PlayerKeyJointSlot> { PlayerKeyJointSlot.Ankle_L };
            map[PlayerKeyJointSlot.Ankle_L] = new List<PlayerKeyJointSlot> { PlayerKeyJointSlot.Toes_L };
            map[PlayerKeyJointSlot.Toes_L] = new List<PlayerKeyJointSlot> { };

            map[PlayerKeyJointSlot.nose_int] = new List<PlayerKeyJointSlot> { };
            map[PlayerKeyJointSlot.Root_M] = new List<PlayerKeyJointSlot> { PlayerKeyJointSlot.Spine1_M, PlayerKeyJointSlot.Hip_L, PlayerKeyJointSlot.Hip_R };
            map[PlayerKeyJointSlot.Head_M] = new List<PlayerKeyJointSlot> { };
            map[PlayerKeyJointSlot.Neck_M] = new List<PlayerKeyJointSlot> { PlayerKeyJointSlot.Head_M, PlayerKeyJointSlot.nose_int, PlayerKeyJointSlot.Shoulder_L, PlayerKeyJointSlot.Shoulder_R };
            return map;
        }

        public static Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            Vector3 normal = Vector3.Cross(ab, ac);
            normal.Normalize();
            return normal;
        }

        public static GameObject GetObject(GameObject parent, string name)
        {
            var transform = parent.transform.Find(name);
            if (transform != null)
            {
                return transform.gameObject;
            }
            var obj = new GameObject(name);
            obj.transform.parent = parent.transform;
            obj.transform.localPosition = new Vector3();
            obj.transform.localRotation = Quaternion.Euler(0, 0, 0);
            obj.transform.localScale = new Vector3(1, 1, 1);
            return obj;
        }

        public static float EaseInOutExpo(float x)
        {
            if (x < 0.0001f)
            {
                return 0.0f;
            }
            if (x > 0.9999f)
            {
                return 1.0f;
            }
            if (x < 0.5f)
            {
                return Mathf.Pow(2.0f, 20.0f * x - 10.0f) / 2.0f;
            }
            else
            {
                return (2.0f - Mathf.Pow(2.0f, -20.0f * x + 10.0f)) / 2.0f;
            }
        }

        public static float EaseOutCubic(float x)
        {
            if (x < 0.0001f)
            {
                return 0.0f;
            }
            if (x > 0.9999f)
            {
                return 1.0f;
            }
            return 1.0f - Mathf.Pow(1.0f - x, 3.0f);
        }
    }





}
