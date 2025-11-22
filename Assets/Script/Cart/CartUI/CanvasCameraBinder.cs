using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasCameraBinder : MonoBehaviour
{
    public Camera overrideCamera;   // ปล่อยว่างได้

    void Start()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas == null || canvas.renderMode != RenderMode.WorldSpace)
            return;

        Camera cam = overrideCamera != null ? overrideCamera : Camera.main;
        if (cam != null)
        {
            canvas.worldCamera = cam;
        }
        else
        {
            Debug.LogWarning("[CanvasCameraBinder] No camera found for world-space canvas.", this);
        }
    }
}