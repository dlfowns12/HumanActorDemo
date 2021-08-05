using UnityEngine;
using System.Collections.Generic;

public class BlendShapeStateManager
{
    private Dictionary<string, Dictionary<string, BlendShapeState>> blendShapeStates = new Dictionary<string, Dictionary<string, BlendShapeState>>();

    public void SaveBlendShapeState(GameObject gameObject)
    {
        if (gameObject == null)
        {
            Debug.LogError("要保存BlendShape状态的GameObject为null");
            return;
        }

        string key = gameObject.name + "_" + gameObject.GetInstanceID();
        Dictionary<string, BlendShapeState> meshStates = new Dictionary<string, BlendShapeState>();

        // 获取所有SkinnedMeshRenderer组件
        SkinnedMeshRenderer[] skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        
        foreach (SkinnedMeshRenderer smr in skinnedMeshRenderers)
        {
            if (smr.sharedMesh != null && smr.sharedMesh.blendShapeCount > 0)
            {
                string meshKey = smr.name + "_" + smr.GetInstanceID();
                BlendShapeState state = new BlendShapeState();
                state.blendShapeWeights = new float[smr.sharedMesh.blendShapeCount];
                
                for (int i = 0; i < smr.sharedMesh.blendShapeCount; i++)
                {
                    state.blendShapeWeights[i] = smr.GetBlendShapeWeight(i);
                }
                
                meshStates.Add(meshKey, state);
            }
        }

        if (meshStates.Count > 0)
        {
            blendShapeStates[key] = meshStates;
            Debug.Log($"已保存GameObject '{gameObject.name}' 的BlendShape状态，共 {meshStates.Count} 个SkinnedMeshRenderer");
        }
        else
        {
            Debug.LogWarning($"GameObject '{gameObject.name}' 没有包含BlendShape的SkinnedMeshRenderer");
        }
    }

    public void SaveBlendShapeStates(params GameObject[] gameObjects)
    {
        foreach (GameObject gameObject in gameObjects)
        {
            SaveBlendShapeState(gameObject);
        }
    }

    public bool RestoreBlendShapeState(GameObject gameObject)
    {
        if (gameObject == null)
        {
            Debug.LogError("要恢复BlendShape状态的GameObject为null");
            return false;
        }

        string key = gameObject.name + "_" + gameObject.GetInstanceID();
        if (!blendShapeStates.ContainsKey(key))
        {
            Debug.LogError($"未找到GameObject '{gameObject.name}' 的BlendShape状态");
            return false;
        }

        Dictionary<string, BlendShapeState> meshStates = blendShapeStates[key];
        SkinnedMeshRenderer[] skinnedMeshRenderers = gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
        int restoredCount = 0;

        foreach (SkinnedMeshRenderer smr in skinnedMeshRenderers)
        {
            string meshKey = smr.name + "_" + smr.GetInstanceID();
            if (meshStates.ContainsKey(meshKey))
            {
                BlendShapeState state = meshStates[meshKey];
                if (smr.sharedMesh != null && smr.sharedMesh.blendShapeCount == state.blendShapeWeights.Length)
                {
                    for (int i = 0; i < state.blendShapeWeights.Length; i++)
                    {
                        smr.SetBlendShapeWeight(i, state.blendShapeWeights[i]);
                    }
                    restoredCount++;
                }
            }
        }

        Debug.Log($"已恢复GameObject '{gameObject.name}' 的BlendShape状态，共 {restoredCount} 个SkinnedMeshRenderer");
        return true;
    }

    public void RestoreBlendShapeStates(params GameObject[] gameObjects)
    {
        foreach (GameObject gameObject in gameObjects)
        {
            RestoreBlendShapeState(gameObject);
        }
    }

    public bool HasBlendShapeState(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return false;
        }

        string key = gameObject.name + "_" + gameObject.GetInstanceID();
        return blendShapeStates.ContainsKey(key);
    }

    public void ClearBlendShapeState(GameObject gameObject)
    {
        if (gameObject == null)
        {
            Debug.LogError("要清除BlendShape状态的GameObject为null");
            return;
        }

        string key = gameObject.name + "_" + gameObject.GetInstanceID();
        if (blendShapeStates.ContainsKey(key))
        {
            blendShapeStates.Remove(key);
            Debug.Log($"已清除GameObject '{gameObject.name}' 的BlendShape状态");
        }
        else
        {
            Debug.LogWarning($"未找到GameObject '{gameObject.name}' 的BlendShape状态");
        }
    }

    public void ClearAllBlendShapeStates()
    {
        blendShapeStates.Clear();
        Debug.Log("已清除所有BlendShape状态");
    }

    public int GetSavedBlendShapeStateCount()
    {
        return blendShapeStates.Count;
    }
}

[System.Serializable]
public class BlendShapeState
{
    public float[] blendShapeWeights;
}

public class BlendShapeStateManagerExample : MonoBehaviour
{
    public GameObject[] charactersToSave;
    private BlendShapeStateManager blendShapeStateManager;

    private void Start()
    {
        blendShapeStateManager = new BlendShapeStateManager();
    }

    private void OnGUI()
    {
        GUI.Window(8, new Rect(630, 270, 300, 250), DrawBlendShapeStateWindow, "BlendShape状态管理工具");
    }

    private void DrawBlendShapeStateWindow(int windowID)
    {
        GUILayout.BeginVertical();
        
        GUILayout.Label($"已保存的BlendShape状态数: {blendShapeStateManager.GetSavedBlendShapeStateCount()}");
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("保存选中角色的BlendShape状态", GUILayout.Height(30)))
        {
            if (charactersToSave != null && charactersToSave.Length > 0)
            {
                blendShapeStateManager.SaveBlendShapeStates(charactersToSave);
            }
            else
            {
                Debug.LogError("请在Inspector中指定要保存BlendShape状态的角色");
            }
        }
        
        if (GUILayout.Button("恢复选中角色的BlendShape状态", GUILayout.Height(30)))
        {
            if (charactersToSave != null && charactersToSave.Length > 0)
            {
                blendShapeStateManager.RestoreBlendShapeStates(charactersToSave);
            }
            else
            {
                Debug.LogError("请在Inspector中指定要恢复BlendShape状态的角色");
            }
        }
        
        if (GUILayout.Button("清除所有BlendShape状态", GUILayout.Height(30)))
        {
            blendShapeStateManager.ClearAllBlendShapeStates();
        }
        
        GUILayout.EndVertical();
        
        GUI.DragWindow();
    }
}
