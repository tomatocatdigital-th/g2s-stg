using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EndOfWaypoint : MonoBehaviour
{
    [Header("Behavior")]
    [Tooltip("ทำลายเฉพาะหัวขบวน หรือทุกคันที่เข้าโซน")]
    public bool headOnly = false;

    [Tooltip("หน่วงก่อนทำลาย (เผื่ออยากปล่อยให้รถวิ่งเข้าเป้าสวย ๆ)")]
    public float destroyDelay = 0f;

    [Header("FX (optional)")]
    public GameObject hitEffectPrefab;
    public Transform effectPoint;
    public AudioClip sfxArrive;

    // ป้องกันทริกเกอร์ซ้ำคันเดิม
    readonly HashSet<int> _seen = new();

    void Awake()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // รองรับกรณีมี child collider
        var cart = other.GetComponentInParent<Cart>();
        if (!cart) return;

        int key = cart.GetInstanceID();
        if (_seen.Contains(key)) return; // กันซ้ำ
        _seen.Add(key);

        var wi = other.GetComponentInParent<CartWagonInfo>();
        if (headOnly && wi && wi.wagonIndex > 0) return; // ต้องหัวขบวนเท่านั้น

        // เล่น SFX/VFX เบา ๆ
        if (sfxArrive) AudioSource.PlayClipAtPoint(sfxArrive, transform.position, 1f);
        if (hitEffectPrefab)
        {
            var pos = effectPoint ? effectPoint.position : other.ClosestPoint(transform.position);
            var fx = Instantiate(hitEffectPrefab, pos, Quaternion.identity);
            Destroy(fx, 1.5f);
        }

        // ทำลายคันที่ชน (รองรับ Pool ถ้ามี)
        Kill(cart.gameObject);

        // ถ้าอยาก “เก็บทั้งขบวน” เมื่อหัวเข้าถึงเป้า
        if (headOnly && wi != null)
            DestroyWholeTrain(wi.trainId);
    }

    void Kill(GameObject go)
    {
        // ถ้ามีระบบ Pooling ลองคืนเข้าพูลก่อน
        var pooled = go.GetComponent<IPoolable>();
        if (pooled != null) { pooled.ReturnToPool(); return; }

        // เผื่อคันประกอบกันหลายชิ้น เลือกทำลาย root
        var root = go.transform.root ? go.transform.root.gameObject : go;
        if (destroyDelay > 0f) Destroy(root, destroyDelay);
        else Destroy(root);
    }

    void DestroyWholeTrain(int trainId)
    {
        if (trainId < 0) return;
        // หา CartWagonInfo ทุกคันในซีนที่ trainId ตรงกัน แล้วทำลาย
        var wagons = FindObjectsOfType<CartWagonInfo>();
        foreach (var w in wagons)
            if (w.trainId == trainId && w && w.gameObject)
                Kill(w.gameObject);
    }
}

/// <summary>
/// ถ้ามีระบบพูล ให้คอมโพเนนต์รถ implement ตัวนี้
/// </summary>
public interface IPoolable
{
    void ReturnToPool();
}