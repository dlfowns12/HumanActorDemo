using UnityEngine;
using System.Collections.Generic;

public class SkeletonStateManager
{
    private Dictionary<string, Dictionary<string, TransformState>> skeletonStates = new Dictionary<string, Dictionary<string, TransformState>>();

    public void SaveSkeletonState(GameObject gameObject)
    {
        if (gameObject == null)
        {
            Debug.LogError("要保存骨骼状态的GameObject为null");
            return;
        }

        string key = gameObject.name + "_" + gameObject.GetInstanceID();
        Dictionary<string, TransformState> boneStates = new Dictionary<string, TransformState>();

        // 获取所有骨骼
        Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
        
        foreach (Transform transform in transforms)
        {
            string boneKey = transform.name + "_" + transform.GetInstanceID();
            TransformState state = new TransformState
            {
                position = transform.localPosition,
                rotation = transform.localRotation,
                scale = transform.localScale
            };
            boneStates.Add(boneKey, state);
        }

        skeletonStates[key] = boneStates;
        Debug.Log($"已保存GameObject '{gameObject.name}' 的骨骼状态，共 {boneStates.Count} 个骨骼");
    }

    public void SaveSkeletonStates(params GameObject[] gameObjects)
    {
        foreach (GameObject gameObject in gameObjects)
        {
            SaveSkeletonState(gameObject);
        }
    }

    public bool RestoreSkeletonState(GameObject gameObject)
    {
        if (gameObject == null)
        {
            Debug.LogError("要恢复骨骼状态的GameObject为null");
            return false;
        }

        string key = gameObject.name + "_" + gameObject.GetInstanceID();
        if (!skeletonStates.ContainsKey(key))
        {
            Debug.LogError($"未找到GameObject '{gameObject.name}' 的骨骼状态");
            return false;
        }

        Dictionary<string, TransformState> boneStates = skeletonStates[key];
        Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
        int restoredCount = 0;

        foreach (Transform transform in transforms)
        {
            string boneKey = transform.name + "_" + transform.GetInstanceID();
            if (boneStates.ContainsKey(boneKey))
            {
                TransformState state = boneStates[boneKey];
                transform.localPosition = state.position;
                transform.localRotation = state.rotation;
                transform.localScale = state.scale;
                restoredCount++;
            }
        }

        Debug.Log($"已恢复GameObject '{gameObject.name}' 的骨骼状态，共 {restoredCount} 个骨骼");
        return true;
    }

    public void RestoreSkeletonStates(params GameObject[] gameObjects)
    {
        foreach (GameObject gameObject in gameObjects)
        {
            RestoreSkeletonState(gameObject);
        }
    }

    public bool HasSkeletonState(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return false;
        }

        string key = gameObject.name + "_" + gameObject.GetInstanceID();
        return skeletonStates.ContainsKey(key);
    }

    public void ClearSkeletonState(GameObject gameObject)
    {
        if (gameObject == null)
        {
            Debug.LogError("要清除骨骼状态的GameObject为null");
            return;
        }

        string key = gameObject.name + "_" + gameObject.GetInstanceID();
        if (skeletonStates.ContainsKey(key))
        {
            skeletonStates.Remove(key);
            Debug.Log($"已清除GameObject '{gameObject.name}' 的骨骼状态");
        }
        else
        {
            Debug.LogWarning($"未找到GameObject '{gameObject.name}' 的骨骼状态");
        }
    }

    public void ClearAllSkeletonStates()
    {
        skeletonStates.Clear();
        Debug.Log("已清除所有骨骼状态");
    }

    public int GetSavedSkeletonStateCount()
    {
        return skeletonStates.Count;
    }
}

[System.Serializable]
public class TransformState
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
}

public class SkeletonStateManagerExample : MonoBehaviour
{
    public GameObject[] charactersToSave;
    private SkeletonStateManager skeletonStateManager;

    private void Start()
    {
        skeletonStateManager = new SkeletonStateManager();
    }

    private void OnGUI()
    {
        GUI.Window(7, new Rect(630, 10, 300, 250), DrawSkeletonStateWindow, "骨骼状态管理工具");
    }

    private void DrawSkeletonStateWindow(int windowID)
    {
        GUILayout.BeginVertical();
        
        GUILayout.Label($"已保存的骨骼状态数: {skeletonStateManager.GetSavedSkeletonStateCount()}");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("保存选中角色的骨骼状态", GUILayout.Height(30)))
        {
            if (charactersToSave != null && charactersToSave.Length > 0)
            {
                skeletonStateManager.SaveSkeletonStates(charactersToSave);
            }
            else
            {
                Debug.LogError("请在Inspector中指定要保存骨骼状态的角色");
            }
        }
        
        if (GUILayout.Button("恢复选中角色的骨骼状态", GUILayout.Height(30)))
        {
            if (charactersToSave != null && charactersToSave.Length > 0)
            {
                skeletonStateManager.RestoreSkeletonStates(charactersToSave);
            }
            else
            {
                Debug.LogError("请在Inspector中指定要恢复骨骼状态的角色");
            }
        }
        
        if (GUILayout.Button("清除所有骨骼状态", GUILayout.Height(30)))
        {
            skeletonStateManager.ClearAllSkeletonStates();
        }
        
        GUILayout.EndVertical();
        
        GUI.DragWindow();
    }
}
