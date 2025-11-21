using UnityEngine;

[RequireComponent(typeof(Cart))]
public class CartMover : MonoBehaviour
{
    [Header("Path")]
    public WaypointNode current;          // <- CartSpawner จะเซ็ตตัวนี้ = lane.spawnAt

    [Header("Speed")]
    [Tooltip("ค่าความเร็ว fallback ถ้าไม่มี SpeedManager")]
    public float speed = 4f;
    [Tooltip("องศาที่หมุนได้ต่อวินาที")]
    public float rotateSpeed = 360f;
    public float arriveThreshold = 0.05f;
    public float laneMultiplier = 1f;

    bool isStopped = false;

    Cart cart;
    Rigidbody rb;

    void Awake()
    {
        cart = GetComponent<Cart>();
        rb = GetComponent<Rigidbody>();

        // ถ้ามี Rigidbody ให้ใช้ช่วย interpolate ให้ลื่นขึ้น
        if (rb)
        {
            rb.isKinematic = true;                       // เราคุมตำแหน่งเอง
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
        }
    }

    void Update()
    {
        // ถ้ามี Rigidbody เราจะขยับใน FixedUpdate แทน
        if (rb) return;
        if (isStopped || !current) return;

        float dt = Time.deltaTime;
        MoveAlongPath(dt, useRigidbody: false);
    }

    void FixedUpdate()
    {
        // เฉพาะกรณีมี Rigidbody
        if (!rb) return;
        if (isStopped || !current) return;

        float dt = Time.fixedDeltaTime;
        MoveAlongPath(dt, useRigidbody: true);
    }

    public void SetStopped(bool stop) => isStopped = stop;

    public void SetLaneMultiplier(float mul) => laneMultiplier = mul;

    // --- ดึง base speed ปัจจุบัน (จาก SpeedManager ถ้ามี) ---
    float GetBaseSpeed()
    {
        if (SpeedManager.I != null)
            return SpeedManager.I.CurrentSpeed;

        return speed; // fallback
    }

    void MoveAlongPath(float dt, bool useRigidbody)
    {
        if (!current) return;

        Vector3 pos = useRigidbody ? rb.position : transform.position;

        Vector3 targetPos = current.transform.position;
        Vector3 toTarget = targetPos - pos;
        float dist = toTarget.magnitude;

        // ถ้าเข้าใกล้ waypoint พอแล้ว -> ไป node ต่อไป
        if (dist <= arriveThreshold)
        {
            current = Next(current);
            if (!current) return;

            targetPos = current.transform.position;
            toTarget = targetPos - pos;
            dist = toTarget.magnitude;

            if (dist < 0.0001f) return;
        }

        Vector3 dir = toTarget / dist;

        // ===== ใช้ base speed จาก SpeedManager + laneMultiplier =====
        float baseSpeed = GetBaseSpeed();
        float spd = Mathf.Max(0f, baseSpeed * Mathf.Max(0f, laneMultiplier));
        float step = spd * dt;

        // เดินหน้า
        Vector3 newPos = Vector3.MoveTowards(pos, targetPos, step);

        if (useRigidbody)
            rb.MovePosition(newPos);
        else
            transform.position = newPos;

        // หมุนให้หันไปทางเดิน
        if (dir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            float maxAngleThis = rotateSpeed * dt;

            Quaternion newRot = Quaternion.RotateTowards(
                useRigidbody ? rb.rotation : transform.rotation,
                targetRot,
                maxAngleThis
            );

            if (useRigidbody)
                rb.MoveRotation(newRot);
            else
                transform.rotation = newRot;
        }
    }

    WaypointNode Next(WaypointNode node)
    {
        if (!node) return null;
        if (node.nextWaypoints != null && node.nextWaypoints.Length > 0)
            return node.nextWaypoints[0];
        return null;
    }
}