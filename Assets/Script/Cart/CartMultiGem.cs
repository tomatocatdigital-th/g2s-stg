using System.Collections.Generic;
using UnityEngine;

public class CartMultiGem : MonoBehaviour
{
    [Header("Gem Meshes (1–3)")]
    public MeshRenderer gem1;
    public MeshRenderer gem2;
    public MeshRenderer gem3;

    [Header("Materials")]
    public Material blueMat, yellowMat, redMat;

    [Header("Data")]
    public List<Route> gems = new List<Route>();
    public int gemCount = 1;

    [Header("UI (Bubble)")]
    public CartGemUI gemUI;

    public IReadOnlyList<Route> Colors => gems;
    public int ColorsCount => gemCount;

    // Start() จะใช้ตอนที่ prefab มีค่าเริ่มต้น (ไม่ใช่ตอน spawn จาก spawner)
    void Start()
    {
        ApplyToMeshes();

        if (gemUI)
            gemUI.Setup(gems);
    }

    /// <summary>ตั้งค่าลำดับสีของรถ + อัปเดต UI</summary>
    public void SetGems(IList<Route> sequence)
    {
        if (sequence == null || sequence.Count == 0)
            return;

        gems.Clear();
        gems.AddRange(sequence);
        gemCount = Mathf.Clamp(gems.Count, 1, 3);

        ApplyToMeshes();

        // อัปเดต UI แบบใหม่ (3-slot)
        if (gemUI != null)
            gemUI.Setup(gems);
    }

    void ApplyToMeshes()
    {
        var arr = new[] { gem1, gem2, gem3 };
        for (int i = 0; i < arr.Length; i++)
        {
            var r = arr[i];
            if (!r) continue;

            bool on = i < gemCount;
            r.gameObject.SetActive(on);

            if (!on) continue;

            var color = gems[Mathf.Clamp(i, 0, gems.Count - 1)];
            r.sharedMaterial = MatFrom(color);
        }
    }

    Material MatFrom(Route c) => c switch
    {
        Route.Blue => blueMat,
        Route.Yellow => yellowMat,
        Route.Red => redMat,
        _ => redMat
    };
}