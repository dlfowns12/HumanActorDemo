using UnityEngine;
using System.Collections.Generic;

public static class CameraAdjuster
{
    public static Camera GetMainCamera()
    {
        return Camera.main;
    }

    public static void SetFieldOfView(float fov)
    {
        Camera mainCamera = GetMainCamera();
        if (mainCamera != null)
        {
            mainCamera.fieldOfView = fov;
            Debug.Log($"已将MainCamera的FieldOfView设置为: {fov}");
        }
        else
        {
            Debug.LogError("未找到MainCamera！");
        }
    }

    public static float GetFieldOfView()
    {
        Camera mainCamera = GetMainCamera();
        if (mainCamera != null)
        {
            return mainCamera.fieldOfView;
        }
        else
        {
            Debug.LogError("未找到MainCamera！");
            return 0f;
        }
    }

    public static void IncreaseFieldOfView(float amount = 5f)
    {
        float currentFov = GetFieldOfView();
        if (currentFov > 0f)
        {
            SetFieldOfView(currentFov + amount);
        }
    }

    public static void DecreaseFieldOfView(float amount = 5f)
    {
        float currentFov = GetFieldOfView();
        if (currentFov > 0f)
        {
            SetFieldOfView(Mathf.Max(1f, currentFov - amount));
        }
    }
}

public class CameraAdjusterUI : MonoBehaviour
{
    [Range(1f, 179f)]
    public float fieldOfView = 60f;

    private float lastFieldOfView;

    private void Start()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            fieldOfView = mainCamera.fieldOfView;
            lastFieldOfView = fieldOfView;
        }
    }

    private void Update()
    {
        if (fieldOfView != lastFieldOfView)
        {
            CameraAdjuster.SetFieldOfView(fieldOfView);
            lastFieldOfView = fieldOfView;
        }
    }

    private void OnGUI()
    {
        GUI.Window(0, new Rect(10, 10, 300, 150), DrawCameraAdjusterWindow, "Camera调整工具");
    }

    private void DrawCameraAdjusterWindow(int windowID)
    {
        GUILayout.BeginVertical();
        
        GUILayout.Label("Field Of View:");
        fieldOfView = GUILayout.HorizontalSlider(fieldOfView, 1f, 179f);
        GUILayout.Label($"当前值: {fieldOfView:F1}");
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("增加FOV (+5)", GUILayout.Height(30)))
        {
            CameraAdjuster.IncreaseFieldOfView();
            fieldOfView = CameraAdjuster.GetFieldOfView();
        }
        
        if (GUILayout.Button("减少FOV (-5)", GUILayout.Height(30)))
        {
            CameraAdjuster.DecreaseFieldOfView();
            fieldOfView = CameraAdjuster.GetFieldOfView();
        }
        
        GUILayout.EndVertical();
        
        GUI.DragWindow();
    }
}
