using UnityEngine;

public class SpeedManager : MonoBehaviour
{
    public static SpeedManager I { get; private set; }

    [Header("Debug / Runtime")]
    [SerializeField]
    float currentSpeed = 5f;    // เอาไว้ดูใน Inspector อย่างเดียว

    public float CurrentSpeed => currentSpeed;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    /// <summary>
    /// เรียกจาก WaveController: ตั้งความเร็วฐานของ wave ปัจจุบัน
    /// </summary>
    public void SetBaseSpeed(float value)
    {
        currentSpeed = Mathf.Max(0f, value);
        Debug.Log($"[Speed] SetBaseSpeed from wave = {currentSpeed}");
    }

    /// <summary>
    /// hook จาก ScoreManager.OnResolve เดิม ถ้าอยาก “ไม่เร่ง/ไม่เบรก” ให้เว้นว่าง
    /// </summary>
    public void HandleResolve(bool correct)
    {
        // ไม่ทำอะไรเลย -> ความเร็วล็อกตาม wave อย่างเดียว
        // (จะลบ method นี้ทิ้งก็ได้ แต่ต้องถอด event ใน Inspector ด้วย)
    }
}