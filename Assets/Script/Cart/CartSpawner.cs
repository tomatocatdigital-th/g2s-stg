using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartSpawner : MonoBehaviour
{
    public static CartSpawner I { get; private set; }

    public enum LaneTag { Left, Mid, Right }

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    [System.Serializable]
    public class SpawnLane
    {
        public string name = "Lane";
        public LaneTag tag = LaneTag.Left;
        public WaypointNode spawnAt;
        public JunctionController junction;
        [Min(0f)] public float desiredGap = 4f;
        [Range(0f, 1f)] public float weight = 1f;

        [Header("UI Hint (optional)")]
        public SpawnHintController hint;

        [HideInInspector] public Transform lastCart;
        [HideInInspector] public bool enabledThisWave = true;
    }

    [Header("Prefab")]
    public GameObject cartPrefab;

    // ====== Wave permissions (ยังใช้ len เพื่อตัดสินใจจำนวนสี) ======
    public bool allowSingle = true;
    public bool allowDouble = true;
    public bool allowTriple = false;

    public List<SpawnLane> lanes = new List<SpawnLane>();

    static int sCartCounter = 1;

    // ---------- Manifest ----------
    [Header("Cart Color Manifest")]
    [Tooltip("ถ้าเปิด: len=2 -> [Blue,Red], len=3 -> [Blue,Red,Yellow]")]
    public bool useDefaultPattern = true;

    List<Route> _nextOverrideManifest; // one-shot

    /// <summary>ตั้งลำดับสีของ “คันถัดไป” โดยตรง (ความยาวต้องตรงกับ len ที่จะสั่งสปอน)</summary>
    public void SetNextManifest(IList<Route> sequence)
    {
        if (sequence == null || sequence.Count == 0) { _nextOverrideManifest = null; return; }
        if (_nextOverrideManifest == null) _nextOverrideManifest = new List<Route>(sequence.Count);
        else _nextOverrideManifest.Clear();
        _nextOverrideManifest.AddRange(sequence);
    }

    // ---------- API จาก WaveController ----------
    public void SetAllowedTypes(bool single, bool dbl, bool triple)
    {
        allowSingle = single;
        allowDouble = dbl;
        allowTriple = triple;
    }

    public void SetEnabledLaneMask(bool left, bool mid, bool right)
    {
        foreach (var ln in lanes)
        {
            bool on = (ln.tag == LaneTag.Left && left) ||
                      (ln.tag == LaneTag.Mid && mid) ||
                      (ln.tag == LaneTag.Right && right);
            ln.enabledThisWave = on;
            ln.hint?.Hide();
        }
    }

    // len = จำนวนสีในคันนี้ (1..3)
    public bool TrySpawnBySize(int len)
    {
        var lane = GetAnySpawnableLane();
        if (lane == null) return false;
        len = Mathf.Clamp(len, 1, 3);

        if ((len == 1 && !allowSingle) ||
            (len == 2 && !allowDouble) ||
            (len == 3 && !allowTriple))
            return false;

        SpawnSingleCartWithColors(lane, len);
        return true;
    }

    public bool TrySpawnBySize(int len, LaneTag laneTag)
    {
        var lane = GetSpawnableLaneByTag(laneTag);
        if (lane == null) return false;
        len = Mathf.Clamp(len, 1, 3);

        if ((len == 1 && !allowSingle) ||
            (len == 2 && !allowDouble) ||
            (len == 3 && !allowTriple))
            return false;

        SpawnSingleCartWithColors(lane, len);
        return true;
    }

    // ---------- Internal ----------
    bool CanSpawn(SpawnLane lane)
    {
        if (lane == null || !lane.spawnAt || !cartPrefab) return false;
        if (!lane.enabledThisWave) return false;
        if (!lane.lastCart) return true;

        float dist = Vector3.Distance(lane.lastCart.position, lane.spawnAt.transform.position);
        return dist >= lane.desiredGap;
    }

    SpawnLane GetAnySpawnableLane()
    {
        float total = 0f;
        for (int i = 0; i < lanes.Count; i++)
            if (lanes[i]?.enabledThisWave == true && CanSpawn(lanes[i]))
                total += Mathf.Max(0f, lanes[i].weight);

        if (total > 0f)
        {
            float r = Random.value * total;
            for (int i = 0; i < lanes.Count; i++)
            {
                var ln = lanes[i];
                if (ln == null || !ln.enabledThisWave || !CanSpawn(ln)) continue;
                float w = Mathf.Max(0f, ln.weight);
                if (r < w) return ln;
                r -= w;
            }
        }

        for (int i = 0; i < lanes.Count; i++)
            if (lanes[i] != null && lanes[i].enabledThisWave && CanSpawn(lanes[i]))
                return lanes[i];

        return null;
    }

    SpawnLane GetSpawnableLaneByTag(LaneTag tag)
    {
        foreach (var ln in lanes)
            if (ln != null && ln.tag == tag && ln.enabledThisWave && CanSpawn(ln))
                return ln;
        return null;
    }

    // ====== โหมดใหม่: สปอน "คันเดียว" แต่มีหลายสี ======
    void SpawnSingleCartWithColors(SpawnLane lane, int len)
    {
        int cartId = sCartCounter++;

        // เตรียมลิสต์สีสำหรับคันนี้
        var manifest = BuildManifest(len); // เช่น [Blue, Red, Yellow]

        // สร้างคันเดียว
        var go = Instantiate(cartPrefab, lane.spawnAt.transform.position, Quaternion.identity);
        go.name = $"Cart_{cartId}_{lane.name}";

        // ตั้งสีหลักของ Cart (ยังใช้ให้ระบบเก่า/หน้าตา)
        var cart = go.GetComponent<Cart>();
        if (cart) cart.SetRouteColor(manifest[0]);
        // ใส่ข้อมูลหลายสีลง CartMultiGem (ถ้ามี)
        var multi = go.GetComponent<CartMultiGem>();
        if (multi) multi.SetGems(manifest);

        // ให้มี CartWagonInfo ไว้สำหรับระบบที่อ้างถึง (AutoClose/EndOfWaypoint)
        var wi = go.GetComponent<CartWagonInfo>();
        if (!wi) wi = go.AddComponent<CartWagonInfo>();
        wi.trainId = cartId;      // ใช้เป็น id ของคันนี้
        wi.wagonIndex = 0;
        wi.wagonCount = 1;
        wi.cart = cart;

        // หน้าตา/วัสดุให้ตรงสีแรก (ถ้าคุณยังใช้ CartAppearance)
        var app = go.GetComponent<CartAppearance>();
        if (app) app.ApplyFromCart();

        // ให้เริ่มวิ่งจาก spawnAt
        var mover = go.GetComponent<CartMover>();
        if (mover) mover.current = lane.spawnAt;

        // กัน spawn ถี่เกินในเลนนั้น
        lane.lastCart = go.transform;
    }

    // ------------- สร้างลำดับสีของ "คันเดียว" -------------
    List<Route> BuildManifest(int len)
    {
        // one-shot override
        if (_nextOverrideManifest != null && _nextOverrideManifest.Count == len)
        {
            var seq = new List<Route>(_nextOverrideManifest);
            _nextOverrideManifest = null;
            return seq;
        }

        if (useDefaultPattern)
        {
            if (len == 1) return new List<Route> { PickRoute() };
            if (len == 2) return new List<Route> { Route.Blue, Route.Red };
            if (len == 3) return new List<Route> { Route.Blue, Route.Red, Route.Yellow };
        }

        var manifest = new List<Route>(len);
        for (int i = 0; i < len; i++) manifest.Add(PickRoute());
        return manifest;
    }

    Route PickRoute()
    {
        int r = Random.Range(0, 3);
        return r switch { 0 => Route.Blue, 1 => Route.Yellow, _ => Route.Red };
    }
    public void ResetSpawnerState()
    {
        foreach (var ln in lanes)
        {
            ln.lastCart = null;
            ln.enabledThisWave = true;
        }
    }
}