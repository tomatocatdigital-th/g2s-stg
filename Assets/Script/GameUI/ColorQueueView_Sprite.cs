using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ColorQueueView_Sprite : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        public Image image;           // รูปวงกลมในช่อง
    }

    [Header("Bind")]
    public ColorQueueController controller;
    public SlotUI[] slots = new SlotUI[3];

    [Header("Sprites")]
    public Sprite spriteEmpty;   // ไฟว่าง (วงเทา หรือดับ)
    public Sprite spriteRed;
    public Sprite spriteYellow;
    public Sprite spriteBlue;

    void OnEnable()
    {
        if (controller) controller.OnQueueChanged += Refresh;
        Refresh(null);
    }

    void OnDisable()
    {
        if (controller) controller.OnQueueChanged -= Refresh;
    }

    void Refresh(IReadOnlyList<Route> arr)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s == null || s.image == null) continue;

            Route r = Route.None;
            if (arr != null && i < arr.Count) r = arr[i];

            s.image.sprite = SpriteForRoute(r);
            s.image.enabled = true;
        }
    }

    Sprite SpriteForRoute(Route r)
    {
        return r switch
        {
            Route.Red => spriteRed,
            Route.Yellow => spriteYellow,
            Route.Blue => spriteBlue,
            _ => spriteEmpty,
        };
    }
    
}