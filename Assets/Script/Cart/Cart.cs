using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class Cart : MonoBehaviour
{
    [Header("State")]
    CartMultiGem multi;
    public Route routeColor = Route.Red;      // สีของรถคันนี้

    public bool Resolved { get; private set; }  // ถูกเช็คไปแล้วหรือยัง

    [Header("Visual (optional)")]
    public Renderer rend;
    public Material matRed, matYellow, matBlue;

    [Header("Lifecycle")]
    [Tooltip("ให้ลบรถหลังถูกเช็คหรือไม่ (ค่าเริ่ม = ไม่ลบ ให้วิ่งต่อ)")]
    public bool destroyOnResolve = false;
    [Tooltip("ใส่ดีเลย์ถ้าต้องการลบหลังผ่านไป X วินาที")]
    public float destroyDelay = 0f;
    public float autoDespawnY = 40f; // กันตกค้างถ้าหลุดฉาก (กรณีฉากคุณแกน Z ก็แทบไม่โดน)

    void Reset()
    {
        var rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        var col = GetComponent<Collider>();
        col.isTrigger = false; // รถเป็น collider ปกติ
    }

    void Start() => ApplyVisual();



    void ApplyVisual()
    {
        if (!rend) rend = GetComponentInChildren<Renderer>();
        if (!rend) return;

        var m = routeColor switch
        {
            Route.Blue => matBlue,
            Route.Yellow => matYellow,
            Route.Red => matRed,
            _ => null
        };
        if (m) rend.sharedMaterial = m;
    }

    /// <summary>
    /// เรียกจาก JunctionController หลังเช็คถูก/ผิดแล้ว
    /// </summary>
    public void OnResolved(bool correct)
    {
        if (Resolved) return;      // กัน Resolve ซ้ำ
        Resolved = true;

        // TODO: เล่น FX/เสียงตาม correct ถ้าต้องการ
        // EffectManager.I?.PlayCorrectFX(...); / PlayWrongFX(...);
        // ถ้าไม่ลบ → รถจะยังวิ่งต่อไปตามรางได้ปกติ
    }

    void Update()
    {
        if (transform.position.y > autoDespawnY)
            Destroy(gameObject);
    }



    void Awake()
    {
        multi = GetComponent<CartMultiGem>();
    }

    public void Init(Route color)
    {
        routeColor = color;
        ApplyVisual();

        // ถ้ามี multi-gem แต่เป็นรถสีเดียว
        if (multi)
            multi.SetGems(new[] { color });
    }
    public void SetRouteColor(Route color)
    {
        routeColor = color;
        ApplyVisual();
    }
}