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
    [Range(1, 3)] public int gemCount = 1;
    public List<Route> gems = new List<Route>();

    [Header("UI")]
    public CartGemUI bubblePrefab;
    public Transform bubbleAnchor;
    CartGemUI _bubble;

    // ---- ให้คนอื่นอ่านลำดับสีจากตรงนี้ ----
    public IReadOnlyList<Route> Colors => gems;
    public int ColorsCount => gemCount;

    void Start()
    {
        // กันกรณีลืมเซ็ตใน prefab
        if (gems.Count == 0)
            gems.Add(Route.Red);

        gemCount = Mathf.Clamp(gems.Count, 1, 3);

        ApplyToMeshes();
        SpawnOrBindBubble();
    }

    /// <summary>ตั้งลำดับสีให้รถคันนี้ (เรียกจาก CartSpawner)</summary>
    public void SetGems(IList<Route> sequence)
    {
        if (sequence == null || sequence.Count == 0)
            return;

        gems.Clear();
        gems.AddRange(sequence);
        gemCount = Mathf.Clamp(gems.Count, 1, 3);

        ApplyToMeshes();

        if (_bubble)
            _bubble.Setup(gems);
        else
            SpawnOrBindBubble();
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
        Route.Blue   => blueMat,
        Route.Yellow => yellowMat,
        Route.Red    => redMat,
        _            => redMat
    };

    void SpawnOrBindBubble()
    {
        var existed = GetComponentInChildren<CartGemUI>(includeInactive: true);
        CartGemUI ui = existed;

        if (!ui && bubblePrefab && bubbleAnchor)
        {
            ui = Instantiate(bubblePrefab, bubbleAnchor);
        }
        else if (ui && bubbleAnchor && ui.transform.parent != bubbleAnchor)
        {
            ui.transform.SetParent(bubbleAnchor, worldPositionStays: false);
        }

        if (!ui) return;

        var rt = ui.transform as RectTransform;
        if (rt)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition3D = Vector3.zero;
            rt.localRotation = Quaternion.identity;

            if (rt.localScale.x > 5f)
                rt.localScale = Vector3.one * 0.01f;
        }

        var binder = ui.GetComponent<CanvasCameraBinder>();
        if (binder) binder.Bind();
        else
        {
            var canvas = ui.GetComponent<Canvas>();
            if (canvas && !canvas.worldCamera)
                canvas.worldCamera = Camera.main;
        }

        _bubble = ui;
        _bubble.Setup(gems);
    }
}