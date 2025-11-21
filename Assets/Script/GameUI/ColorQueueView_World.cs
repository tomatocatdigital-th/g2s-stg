using UnityEngine;
using System.Collections.Generic;

public class ColorQueueView_Material : MonoBehaviour
{
    [System.Serializable]
    public class SlotWorld
    {
        [Tooltip("Renderer ของหลอดไฟ/ฝาครอบไฟในฉาก (หนึ่งหรือหลายชิ้นก็ได้)")]
        public Renderer[] renderers;

        [Header("Optional Light")]
        public Light glowLight;          // ถ้ามีไฟจริงในฉาก
        [Range(0f,10f)] public float baseLightIntensity = 2.5f;
    }

    [Header("Bind")]
    public ColorQueueController controller;
    public SlotWorld[] slots = new SlotWorld[3];

    [Header("Materials")]
    public Material matEmpty;
    public Material matRed;
    public Material matYellow;
    public Material matBlue;

    [Header("Light Multiplier")]
    [Range(0f,3f)] public float onMul  = 1.0f;
    [Range(0f,1f)] public float offMul = 0.15f;

    void OnEnable()
    {
        if (controller) controller.OnQueueChanged += Refresh;
        Refresh(null); // เคลียร์เริ่มต้น
    }

    void OnDisable()
    {
        if (controller) controller.OnQueueChanged -= Refresh;
    }

    void Refresh(IReadOnlyList<Route> arr)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (slot == null) continue;

            // route ในตำแหน่ง i (ถ้าไม่มีให้ถือว่า Empty)
            Route r = Route.None;
            if (arr != null && i < arr.Count) r = arr[i];

            var m = MaterialForRoute(r);

            // สลับวัสดุให้ renderer ทั้งหมดในดวงนั้น
            if (slot.renderers != null)
            {
                foreach (var rend in slot.renderers)
                {
                    if (!rend || !m) continue;

                    // ใช้ .material เพื่อให้ได้ material instance ต่อ-อ็อบเจ็กต์
                    // (หลีกเลี่ยง .sharedMaterial ที่จะไปเปลี่ยนพรีแฟบ/อินสแตนซ์อื่น)
                    rend.material = m;
                }
            }

            // อัปเดต Light ถ้ามี
            if (slot.glowLight)
            {
                switch (r)
                {
                    case Route.Red:    slot.glowLight.color = Color.red;                                  break;
                    case Route.Yellow: slot.glowLight.color = new Color(1f, 0.9f, 0.2f);                  break;
                    case Route.Blue:   slot.glowLight.color = new Color(0.2f, 0.6f, 1f);                  break;
                    default:           slot.glowLight.color = new Color(0.15f, 0.15f, 0.15f);             break;
                }

                float mul = (r == Route.None) ? offMul : onMul;
                slot.glowLight.intensity = slot.baseLightIntensity * mul;
                slot.glowLight.enabled   = (mul > 0.01f);
            }
        }
    }

    Material MaterialForRoute(Route r)
    {
        switch (r)
        {
            case Route.Red:    return matRed ? matRed : matEmpty;
            case Route.Yellow: return matYellow ? matYellow : matEmpty;
            case Route.Blue:   return matBlue ? matBlue : matEmpty;
            default:           return matEmpty;
        }
    }
}