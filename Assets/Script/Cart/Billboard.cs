using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera targetCamera;

    void LateUpdate()
    {
        var cam = targetCamera ? targetCamera : Camera.main;
        if (!cam) return;
        transform.rotation = Quaternion.LookRotation(transform.position - cam.transform.position);
    }
}