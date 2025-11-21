using UnityEngine;
using System.Linq;

public class ButtonGroup : MonoBehaviour
{
    [Header("Targets")]
    public ColorQueueController colorQueue;   // ❗ลากตัวนี้ถ้าจะใช้ 'คิว 3 ช่อง'
    public JunctionController junction;       // ใช้เฉพาะโหมดเดิม (ไม่มีคิว)

    [Header("Buttons in this group")]
    public ColorButtonGlow[] buttons;

    [Header("Behaviour (legacy pad only)")]
    [Tooltip("ใช้เฉพาะตอนไม่มีคิว: เคลียร์ pad เป็น None เมื่อ ClearActive ถูกเรียกหรือไม่")]
    public bool clearPadOnClear = false;

    private ColorButtonGlow _active;

    void Awake()
    {
        if (buttons == null || buttons.Length == 0)
            buttons = GetComponentsInChildren<ColorButtonGlow>(true);

        foreach (var b in buttons)
            if (b) b.group = this;
    }

    void Start()
    {
        // ถ้ามีคิว: ไม่ต้องไปแตะ pad/ปุ่ม
        if (colorQueue != null) return;

        // ไม่มีคิว -> โหมด legacy: sync ปุ่มกับ pad ครั้งแรก
        if (junction && junction.activeColor != Route.None)
            SetFromRoute(ToRailColor(junction.activeColor));
        else
            SetButtonsVisual(false);
    }

    // === ใช้กับ UI Button OnClick ===
    public void PressRed()    => HandlePress(RailColor.Red);
    public void PressYellow() => HandlePress(RailColor.Yellow); // GOLD
    public void PressBlue()   => HandlePress(RailColor.Blue);

    void HandlePress(RailColor c)
    {
        // โหมดคิว (แนะนำ)
        if (colorQueue != null)
        {
            Debug.Log("[BTN] Enqueue " + c);
            colorQueue.Enqueue(ToRoute(c));  // เติมลงคิว
            return;
        }

        // โหมดเดิม (ตั้งสีให้ pad)
        SelectByColor_Legacy(c);
    }

    // ---------- Legacy Pad Mode ----------
    void SelectByColor_Legacy(RailColor c)
    {
        var btn = buttons?.FirstOrDefault(b => b && b.color == c);
        if (btn) MakeActive(btn);
        else if (junction) ApplyToJunction(c);
    }

    public void SetFromRoute(RailColor route)
    {
        var btn = buttons?.FirstOrDefault(b => b && b.color == route);
        if (btn) MakeActive(btn);
        else if (buttons != null && buttons.Length > 0) MakeActive(buttons[0]);
    }

    public void MakeActive(ColorButtonGlow btn)
    {
        if (!btn) return;

        foreach (var b in buttons)
            if (b && b != btn) b.SetVisual(false, instant: true);

        _active = btn;
        _active.SetVisual(true);
        ApplyToJunction(_active.color);
    }

    void ApplyToJunction(RailColor color)
    {
        if (!junction)
        {
            Debug.LogWarning("[BTN] junction is null (legacy mode)");
            return;
        }

        Debug.Log($"[BTN] set {color} -> {junction.name}");
        switch (color)
        {
            case RailColor.Blue:   junction.SetBlue();   break;
            case RailColor.Yellow: junction.SetYellow(); break;
            case RailColor.Red:    junction.SetRed();    break;
        }
    }

    public void ClearActive()
    {
        _active = null;
        SetButtonsVisual(false);

        if (colorQueue == null && clearPadOnClear && junction)
            junction.ClearPad();
    }

    void SetButtonsVisual(bool on)
    {
        if (buttons == null) return;
        foreach (var b in buttons)
            if (b) b.SetVisual(on, instant: true);
    }

    // ---------- Helpers ----------
    static RailColor ToRailColor(Route r) => r switch
    {
        Route.Blue   => RailColor.Blue,
        Route.Yellow => RailColor.Yellow,
        Route.Red    => RailColor.Red,
        _            => RailColor.Yellow
    };

    static Route ToRoute(RailColor c) => c switch
    {
        RailColor.Blue   => Route.Blue,
        RailColor.Yellow => Route.Yellow,
        RailColor.Red    => Route.Red,
        _                => Route.Yellow
    };
    
}