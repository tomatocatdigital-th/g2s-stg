using UnityEngine;

public class JunctionAutoClose : MonoBehaviour
{
    [Header("Refs")]
    public JunctionController junction;
    public ColorQueueController colorQueue;   // << NEW: อ้างอิงคิวเพื่อเคลียร์

    [Header("Timing")]
    [Tooltip("กันกดย้ำ/ปิดถี่เกิน")]
    public float cooldown = 0.1f;

    [Tooltip("เวลาหน่วงก่อนปิด เมื่อผลลัพธ์ \"ถูก\" และตั้ง onlyCloseOnWrong=true")]
    public float holdAfterCorrect = 0.5f;

    [Tooltip("หาก >= 0 จะปิด pad หลังชนทุกครั้ง (ถูก/ผิด) ตามเวลานี้ — ตั้งเป็น -1 เพื่อไม่ใช้")]
    public float closeAfterAnyHit = -1f;

    [Header("Policy")]
    [Tooltip("เปิดไว้ = ปิดเฉพาะตอนผลลัพธ์ผิด, ถูกจะถือไฟไว้ตาม holdAfterCorrect")]
    public bool onlyCloseOnWrong = true;

    [Header("Queue Policy")]
    [Tooltip("เปิดไว้เพื่อเคลียร์คิวทุกครั้งเมื่อมีรถชน (ถูก/ผิด)")]
    public bool clearQueueOnAnyHit = true;   // << NEW: ค่าเริ่มต้นเปิด

    float _nextAllowed;
    float _holdUntil = 0f;

    void OnDisable()
    {
        CancelInvoke(nameof(ClosePadNow));
    }

    /// <summary>
    /// ให้ JunctionController เรียกหลังตัดสินถูก/ผิดเสมอ
    /// </summary>
    public void NotifyHeadHit(bool correct)
    {
        // << NEW: เคลียร์คิวทุกครั้งเมื่อมีรถชน
        if (clearQueueOnAnyHit)
            colorQueue?.ClearAll();

        // ถ้าเซ็ต closeAfterAnyHit >= 0 ให้ปิดตามเวลานี้ทุกเคส (เหนือกว่านโยบายอื่น)
        if (closeAfterAnyHit >= 0f)
        {
            CancelInvoke(nameof(ClosePadNow));
            Invoke(nameof(ClosePadNow), closeAfterAnyHit);
            return;
        }

        if (!correct)
        {
            // ผิด → ปิดทันที (ข้าม cooldown)
            ForceCloseNow(ignoreCooldown: true);
            return;
        }

        // ถูก →
        if (onlyCloseOnWrong)
        {
            // ถือไฟไว้ช่วงหนึ่ง (ไม่ปิดเองจนกว่าจะพ้น hold)
            _holdUntil = Time.time + holdAfterCorrect;
            CancelInvoke(nameof(ClosePadNow));
            Invoke(nameof(ClosePadNow), holdAfterCorrect);
        }
        else
        {
            // ไม่ถือไฟ → ปิดตาม holdAfterCorrect (0 = ปิดทันที)
            CancelInvoke(nameof(ClosePadNow));
            Invoke(nameof(ClosePadNow), Mathf.Max(0f, holdAfterCorrect));
        }
    }

    /// <summary>
    /// ปิดเดี๋ยวนี้จากภายนอก (เช่นตอนผิด)
    /// </summary>
    public void ForceCloseNow(bool ignoreCooldown = true)
    {
        CancelInvoke(nameof(ClosePadNow));
        if (!junction) return;

        // << NEW: เผื่อความชัวร์ เคลียร์คิวด้วยเมื่อมีการปิดแบบ force
        if (clearQueueOnAnyHit)
            colorQueue?.ClearAll();

        if (!ignoreCooldown && Time.time < _nextAllowed) return;

        junction.ClearPad();
        _nextAllowed = Time.time + cooldown;

#if UNITY_ANDROID || UNITY_IOS
        Haptics.Vibrate();
#endif
    }

    void ClosePadNow()
    {
        if (!junction) return;

        // << NEW: เผื่อความชัวร์ เคลียร์คิวด้วยเมื่อปิดตามเวลา
        if (clearQueueOnAnyHit)
            colorQueue?.ClearAll();

        // ถ้าถูกและยังอยู่ในช่วง hold (และตั้ง policy ให้ถือไฟ) ก็ยังไม่ปิด
        if (onlyCloseOnWrong && Time.time < _holdUntil) return;
        if (Time.time < _nextAllowed) return;

        junction.ClearPad();
        _nextAllowed = Time.time + cooldown;

#if UNITY_ANDROID || UNITY_IOS
        Haptics.Vibrate();
#endif
    }
}