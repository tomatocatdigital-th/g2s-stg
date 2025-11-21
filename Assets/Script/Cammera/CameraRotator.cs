using UnityEngine;

public class CameraRotator : MonoBehaviour
{
[Header("Oscillation")]
    [Tooltip("องศาสูงสุดของการส่าย (เช่น 3-8 องศา)")]
    public float amplitudeDeg = 5f;

    [Tooltip("ความถี่การส่าย หน่วย Hz (รอบ/วินาที)")]
    public float frequency = 0.8f;

    [Tooltip("แกนหมุนที่ต้องการส่าย (ค่าเริ่ม = แกน Y)")]
    public Vector3 axis = Vector3.up;

    [Tooltip("เลื่อนเฟสเริ่มต้นของไซน์ (องศา)")]
    public float phaseOffsetDeg = 0f;

    [Tooltip("สุ่มเฟสเริ่มต้นเล็กน้อยเพื่อไม่ให้ทุกชิ้นส่ายพร้อมกัน")]
    public bool randomizeStartPhase = true;

    [Header("Behaviour")]
    [Tooltip("ใช้ unscaled time (ไม่หยุดเมื่อ Time.timeScale = 0)")]
    public bool useUnscaledTime = false;

    [Tooltip("เปิด/ปิดเอฟเฟกต์แบบนุ่ม ๆ")]
    public bool smoothEnableDisable = true;

    [Tooltip("ระยะเวลาค่อย ๆ blend เข้า/ออก (วินาที)")]
    public float blendDuration = 0.25f;

    Quaternion _baseLocalRot;
    float _blend;                // 0..1
    bool _targetOn = true;
    float _randPhaseRad;

    void Awake()
    {
        _baseLocalRot = transform.localRotation;
        if (randomizeStartPhase)
        {
            _randPhaseRad = Random.Range(0f, Mathf.PI * 2f);
        }
    }

    void OnEnable()
    {
        _targetOn = true;
    }

    void OnDisable()
    {
        // คืนค่าสภาพเดิมเมื่อถูกปิดใช้งาน (เพื่อไม่ทับมุมเดิมของฉาก/กล้อง)
        transform.localRotation = _baseLocalRot;
        _blend = 0f;
    }

    /// <summary>
    /// เรียกจากภายนอกเพื่อเปิด/ปิดเอฟเฟกต์ (จะ blend ถ้าตั้ง smoothEnableDisable)
    /// </summary>
    public void SetEnabled(bool on)
    {
        _targetOn = on;
        if (!smoothEnableDisable)
        {
            _blend = on ? 1f : 0f;
        }
    }

    void Update()
    {
        float t = useUnscaledTime ? Time.unscaledTime : Time.time;

        // blend เข้า/ออกนุ่ม ๆ
        if (smoothEnableDisable && blendDuration > 0f)
        {
            float delta = Time.deltaTime / Mathf.Max(0.0001f, blendDuration);
            _blend = Mathf.MoveTowards(_blend, _targetOn ? 1f : 0f, delta);
        }
        else
        {
            _blend = _targetOn ? 1f : 0f;
        }

        // คำนวณมุมส่าย (องศา)
        float phaseRad = _randPhaseRad + Mathf.Deg2Rad * phaseOffsetDeg;
        float angle = Mathf.Sin((Mathf.PI * 2f * frequency) * t + phaseRad) * amplitudeDeg * _blend;

        // ทำให้แกนเป็น normalized (กันกรณีแกนศูนย์)
        Vector3 nAxis = axis.sqrMagnitude < 1e-6f ? Vector3.up : axis.normalized;

        // สร้างมุมหมุนเพิ่ม แล้วคูณกับ localRotation เดิม
        Quaternion sway = Quaternion.AngleAxis(angle, nAxis);
        transform.localRotation = _baseLocalRot * sway;
    }

#if UNITY_EDITOR
    // อัปเดตฐานมุมถ้ามีการหมุนวัตถุใน Editor ระหว่างเล่น
    void LateUpdate()
    {
        if (!Application.isPlaying) return;
        // หากอยาก "ล็อก" มุมฐานใหม่ขณะรัน ให้กด R ใน Inspector เองตามต้องการ
    }

    [ContextMenu("Re-capture Base Rotation")]
    void RecaptureBaseRotation()
    {
        _baseLocalRot = transform.localRotation;
    }
#endif
}