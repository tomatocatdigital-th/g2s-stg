using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class ColorQueueView : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        public Image box;           // สี่เหลี่ยมสี (ช่องคิว)
        public TMP_Text orderText;  // ตัวเลข 1/2/3 เหนือกล่อง
        public CanvasGroup fxPulse; // ใช้ทำกระพริบเมื่อเติมครบ (optional)
    }

    [Header("Bind")]
    public ColorQueueController controller;
    public SlotUI[] slots = new SlotUI[3];

    [Header("Colors")]
    public Color colorRed   = new(0.92f, 0.20f, 0.18f);
    public Color colorBlue  = new(0.22f, 0.40f, 0.94f);
    public Color colorGold  = new(0.98f, 0.86f, 0.30f);
    public Color colorEmpty = new(0.90f, 0.90f, 0.90f);

    [Header("FX")]
    public float fullPulseDuration = 0.35f;

    void OnEnable()
    {
        if (controller) controller.OnQueueChanged += Refresh;
        // รีเฟรชครั้งแรก
        Refresh(null);
    }

    void OnDisable()
    {
        if (controller) controller.OnQueueChanged -= Refresh;
    }

    void Refresh(IReadOnlyList<Route> arr)
    {
        // ถ้า controller ส่ง null มา ให้ดึง count เองโดยวนช่องว่าง
        // ที่นี่เราจะอ่านจาก event อย่างเดียวก็ได้ → ถ้า null แปลว่าล้าง
        int nonEmpty = 0;

        for (int i = 0; i < slots.Length; i++)
        {
            var s = slots[i];
            if (s == null || s.box == null) continue;

            Route r = Route.None;
            if (arr != null && i < arr.Count) r = arr[i];

            // เติมสี
            s.box.color = ToColor(r);

            // หมายเลข 1/2/3 ให้ขึ้นเฉพาะช่องที่ “มีสี”
            if (s.orderText)
            {
                if (r == Route.None) { s.orderText.gameObject.SetActive(false); }
                else
                {
                    nonEmpty++;
                    s.orderText.gameObject.SetActive(true);
                    s.orderText.text = nonEmpty.ToString(); // 1..n ตามลำดับซ้าย→ขวา
                }
            }

            // ปิดเอฟเฟกต์ถ้ามี
            if (s.fxPulse) s.fxPulse.alpha = 0f;
        }

        // ถ้าคิว “เต็ม” ทั้ง 3 ช่อง → กระพริบเบาๆ ให้ feedback
        if (arr != null && CountNonEmpty(arr) >= slots.Length)
            StartCoroutine(PulseAll());
    }

    IEnumerator PulseAll()
    {
        float t = 0f;
        while (t < fullPulseDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.PingPong(t * 6f, 1f); // 0..1 เร็วๆ
            foreach (var s in slots) if (s?.fxPulse) s.fxPulse.alpha = a;
            yield return null;
        }
        foreach (var s in slots) if (s?.fxPulse) s.fxPulse.alpha = 0f;
    }

    int CountNonEmpty(IReadOnlyList<Route> arr)
    {
        int c = 0;
        for (int i = 0; i < arr.Count; i++)
            if (arr[i] != Route.None) c++;
        return c;
    }

    Color ToColor(Route r)
    {
        return r switch
        {
            Route.Red    => colorRed,
            Route.Blue   => colorBlue,
            Route.Yellow => colorGold,
            _            => colorEmpty
        };
    }
}