using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasCameraBinder : MonoBehaviour
{
    public Camera overrideCamera;
    public bool rebindEveryEnable = true;

    Canvas _canvas;

    void Awake()
    {
        _canvas = GetComponent<Canvas>();
        Bind();
    }

    void OnEnable()
    {
        if (rebindEveryEnable) Bind();
    }

    public void Bind()
    {
        var cam = overrideCamera ? overrideCamera : Camera.main;
        if (!cam) return;

        _canvas.worldCamera = cam;
    }
}