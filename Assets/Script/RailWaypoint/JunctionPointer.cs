using UnityEngine;

public class JunctionPointer : MonoBehaviour
{
    public JunctionController junction;   // ลิงก์ pad หลัก
    public MeshRenderer rend;             // MeshRenderer ของ pointer

    [Header("Materials / Colors")]
    public Material matDefault;
    public Material matRed;
    public Material matYellow;
    public Material matBlue;

    [Header("Motion")]
    public float moveAmplitude = 0.1f;    // ระยะการขยับแกน Z
    public float moveSpeed = 2f;          // ความเร็วในการขยับ
    private Vector3 startPos;

    void Start()
    {
        if (!rend) rend = GetComponent<MeshRenderer>();
        startPos = transform.localPosition;
    }

    void Update()
    {
        if (!junction) return;

        // 🎨 เปลี่ยนสี pointer ตาม pad
        switch (junction.activeColor)
        {
            case Route.Red:    rend.sharedMaterial = matRed;    break;
            case Route.Yellow: rend.sharedMaterial = matYellow; break;
            case Route.Blue:   rend.sharedMaterial = matBlue;   break;
            default:           rend.sharedMaterial = matDefault; break;
        }

        // 💫 ขยับแกน Z ไป-มา (เช่น เด้งหน้า-หลัง)
        float offsetZ = Mathf.Sin(Time.time * moveSpeed) * moveAmplitude;
        transform.localPosition = startPos + new Vector3(0f, 0f, offsetZ);
    }
}