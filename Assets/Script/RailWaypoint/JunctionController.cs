using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class JunctionController : MonoBehaviour
{
    [Header("Queue Mode (แนะนำให้ใช้)")]
    public ColorQueueController colorQueue;     // คิวสีจากปุ่ม
    public JunctionAutoClose autoClose;        // คุมการปิด pad
    [Tooltip("ถ้าเปิด = ใช้คิวเป็นตัวตัดสินหลัก, pad เป็นแค่ visual")]
    public bool queueFirstMode = true;

    [Header("Checkpoint Pad (Renderer ที่เปลี่ยนสี)")]
    public MeshRenderer padRenderer;

    [Header("Pad Materials")]
    public Material matDefault;
    public Material matBlue;
    public Material matYellow;
    public Material matRed;

    [Header("Swatches (optional)")]
    public MeshRenderer swatchRed;
    public MeshRenderer swatchYellow;
    public MeshRenderer swatchBlue;
    public Material swatchDefault;

    [Header("Light (optional)")]
    public Light displayLight;
    public float lightIntensity = 60f;
    public Color lightBlue   = new(0.35f, 0.75f, 1f);
    public Color lightYellow = new(1f, 0.9f, 0.35f);
    public Color lightRed    = new(1f, 0.35f, 0.35f);

    [Header("Legacy Pad State (ใช้เมื่อไม่มีคิว)")]
    public Route activeColor = Route.None;

    // กันคอลลิเดอร์ลูกของรถคันเดียวกันยิงซ้ำ
    readonly HashSet<int> _seenCart = new();

    void Awake()
    {
        var col = GetComponent<Collider>();
        if (col) col.isTrigger = true;
    }

    void Start()
    {
        UpdateVisual_LegacyPad();
    }

    // ----------------------------------------------------------------------
    //  API สำหรับปุ่ม (ButtonGroup.cs) - รองรับทั้งโหมด pad และโหมดคิว
    // ----------------------------------------------------------------------
    public void SetBlue()
    {
        if (colorQueue != null && queueFirstMode)
            colorQueue.Enqueue(Route.Blue);
        else
            SetActiveColor(Route.Blue);
    }

    public void SetYellow()
    {
        if (colorQueue != null && queueFirstMode)
            colorQueue.Enqueue(Route.Yellow);
        else
            SetActiveColor(Route.Yellow);
    }

    public void SetRed()
    {
        if (colorQueue != null && queueFirstMode)
            colorQueue.Enqueue(Route.Red);
        else
            SetActiveColor(Route.Red);
    }

    public void ClearPad()
    {
        // เคลียร์ทั้ง logic (คิว) + visual
        colorQueue?.ClearAll();
        SetActiveColor(Route.None);
    }

    public void SetActiveColor(Route color)
    {
        activeColor = color;
        UpdateVisual_LegacyPad();
    }

    // ----------------------------------------------------------------------
    //  Visual (ใช้เฉพาะตอนทำงานโหมด pad เดิม หรือเคสต้องการให้ไฟดับ)
    // ----------------------------------------------------------------------
    void UpdateVisual_LegacyPad()
    {
        bool inQueueMode = (colorQueue != null && queueFirstMode);

        // ถ้าใช้คิวเป็นหลัก เราจะไม่ทาสี pad ตาม activeColor
        // แต่อนุญาตให้ "ClearPad()" ดับไฟ (activeColor = None)
        if (inQueueMode)
        {
            if (activeColor == Route.None)
            {
                if (padRenderer)  padRenderer.sharedMaterial  = matDefault;
                if (swatchRed)    swatchRed.sharedMaterial    = swatchDefault ? swatchDefault : matDefault;
                if (swatchYellow) swatchYellow.sharedMaterial = swatchDefault ? swatchDefault : matDefault;
                if (swatchBlue)   swatchBlue.sharedMaterial   = swatchDefault ? swatchDefault : matDefault;
                if (displayLight) displayLight.intensity      = 0f;
            }
            return;
        }

        // --- โหมด pad เดิม: ทาสีตาม activeColor ---
        if (padRenderer)
        {
            padRenderer.sharedMaterial = activeColor switch
            {
                Route.Blue   => matBlue,
                Route.Yellow => matYellow,
                Route.Red    => matRed,
                _            => matDefault
            };
        }

        if (swatchRed)
            swatchRed.sharedMaterial    = (activeColor == Route.Red)    ? matRed    : (swatchDefault ? swatchDefault : matDefault);
        if (swatchYellow)
            swatchYellow.sharedMaterial = (activeColor == Route.Yellow) ? matYellow : (swatchDefault ? swatchDefault : matDefault);
        if (swatchBlue)
            swatchBlue.sharedMaterial   = (activeColor == Route.Blue)   ? matBlue   : (swatchDefault ? swatchDefault : matDefault);

        if (displayLight)
        {
            if (activeColor == Route.None)
            {
                displayLight.intensity = 0f;
            }
            else
            {
                displayLight.intensity = lightIntensity;
                displayLight.color = activeColor switch
                {
                    Route.Blue   => lightBlue,
                    Route.Yellow => lightYellow,
                    Route.Red    => lightRed,
                    _            => lightBlue
                };
            }
        }
    }

    // ----------------------------------------------------------------------
    //  ตรวจตอน Cart ชน (รองรับรถคันเดียวหลายสี)
    // ----------------------------------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        // รองรับ child collider บนรถ
        var cart = other.GetComponentInParent<Cart>();
        if (!cart) return;

        int key = cart.GetInstanceID();
        if (_seenCart.Contains(key)) return; // กันยิงซ้ำคันเดิม
        _seenCart.Add(key);

        // --- ดึงลิสต์สีของรถคันนี้ ---
        IReadOnlyList<Route> seq = null;
        Route mainColor = cart.routeColor;

        // ถ้ามี CartMultiGem ให้ใช้ลิสต์สีจากมัน
        // (ใช้ InChildren กันกรณี script อยู่ที่ลูก)
        var multi = cart.GetComponentInChildren<CartMultiGem>();
        if (multi != null)
        {
            seq = multi.Colors;
            if (multi.ColorsCount > 0 && seq != null && seq.Count > 0)
                mainColor = seq[0];
        }

        bool correct;

        if (colorQueue != null && queueFirstMode)
        {
            // ===== โหมดหลัก: ใช้คิวเป็นตัวตัดสิน =====
            if (seq != null && seq.Count > 1)
            {
                // รถคันเดียวหลายสี → เทียบทั้งลิสต์
                correct = colorQueue.ResolveForTrain(seq, resetAfter: true);
                Debug.Log($"[PAD] multi-cart '{cart.name}' need = [{string.Join(", ", seq)}] -> {correct}");
            }
            else
            {
                // รถมีสีเดียว → เทียบ 1 สี
                correct = colorQueue.ResolveForSingleCart(mainColor, resetAfter: true);
                Debug.Log($"[PAD] single-cart '{cart.name}' need = {mainColor} -> {correct}");
            }
        }
        else if (colorQueue != null && !queueFirstMode)
        {
            // ===== Hybrid / fallback: ยังใช้คิว แต่เทียบสีเดียว (mainColor) =====
            correct = colorQueue.ResolveForSingleCart(mainColor, resetAfter: true);
        }
        else
        {
            // ===== Legacy: ไม่มีคิว → เทียบกับ activeColor แบบเก่า =====
            correct = (activeColor != Route.None && mainColor == activeColor);
        }

        // ส่งผลให้ระบบอื่น
        ScoreManager.I?.ResolveDelivery(correct);
        cart.OnResolved(correct);

        // บอก AutoClose ว่า head ชนแล้ว (ให้มันจัดการเวลาปิด pad เอง)
        if (autoClose)
            autoClose.NotifyHeadHit(correct);
        else
        {
            // ถ้าไม่มี AutoClose ใช้แบบง่าย: ปิด pad + ล้างคิวทุกครั้งที่มีการชน
            ClearPad();
        }
    }

    // ใช้ตอนรีเซ็ตเวฟ ฯลฯ
    public void ResetProcessedSet() => _seenCart.Clear();

    public void Disarm()
    {
        _seenCart.Clear();
        ClearPad();
    }
}