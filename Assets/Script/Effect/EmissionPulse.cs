using UnityEngine;

public class EmissionPulse : MonoBehaviour
{
    [Header("Targets")]
    public Renderer[] targetRenderers;

    [Header("Pulse Settings")]
    public Color baseColor = Color.yellow;
    public float intensity = 2f;
    public float pulseSpeed = 2f;

    // Internal caches
    MaterialPropertyBlock[] blocks;
    float[] phaseOffsets;

    void Start()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
            return;

        int count = targetRenderers.Length;
        blocks = new MaterialPropertyBlock[count];
        phaseOffsets = new float[count];

        for (int i = 0; i < count; i++)
        {
            if (targetRenderers[i] == null) continue;

            // เตรียม MPB สำหรับแต่ละ Renderer
            blocks[i] = new MaterialPropertyBlock();
            targetRenderers[i].GetPropertyBlock(blocks[i]);

            // สุ่มระยะเฟสให้แต่ละอันพัลส์ไม่พร้อมกัน
            phaseOffsets[i] = Random.value * Mathf.PI * 2f;
        }
    }

    void Update()
    {
        if (targetRenderers == null || blocks == null)
            return;

        for (int i = 0; i < targetRenderers.Length; i++)
        {
            if (targetRenderers[i] == null || blocks[i] == null)
                continue;

            // สร้างค่าพัลส์แบบสุ่มเวลาแต่ละอัน
            float t = Mathf.Sin(Time.time * pulseSpeed + phaseOffsets[i]) * 0.5f + 0.5f;
            Color final = baseColor * Mathf.LinearToGammaSpace(1f + t * intensity);

            // อัปเดต Emission โดยไม่สร้าง Material ใหม่
            blocks[i].SetColor("_EmissionColor", final);
            targetRenderers[i].SetPropertyBlock(blocks[i]);
        }
    }
}