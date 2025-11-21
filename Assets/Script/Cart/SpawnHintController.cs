using UnityEngine;

public class SpawnHintController : MonoBehaviour
{
    public CanvasGroup cg;       // world- หรือ screen-space ก็ได้
    public float pulseSpeed = 6f;
    float untilSpawn = -1f;

    void Awake() { if (cg) cg.alpha = 0f; }
    void Update()
    {
        if (untilSpawn < 0f) return;
        untilSpawn -= Time.deltaTime;
        if (!cg) return;

        float a = 0.4f + 0.6f * Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed));
        cg.alpha = a;

        if (untilSpawn <= 0f) Hide();
    }

    public void ShowFor(float leadTime)
    {
        if (!cg) return;
        untilSpawn = Mathf.Max(0.02f, leadTime);
        cg.gameObject.SetActive(true);
        cg.alpha = 1f;
        transform.localScale = Vector3.one; // อยากทำ pop scale ก็เพิ่มได้
    }

    public void Hide()
    {
        untilSpawn = -1f;
        if (!cg) return;
        cg.alpha = 0f;
        cg.gameObject.SetActive(false);
    }
}