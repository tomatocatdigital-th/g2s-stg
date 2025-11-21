using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WaveUIController : MonoBehaviour
{
    [Header("Bind")]
    //public CanvasGroup cg;            // ใส่บน GameObject เดียวกัน
    public Image badge;               // พื้นหลังคงที่ 1 รูป (ไม่มีก็ได้)
    public TextMeshProUGUI label;     // WAVE xx

    [Header("Text")]
    public string prefix = "";
    public float showDur = 0.9f;      // ค้างแป๊บเดียว
    public float fadeDur = 0.25f;     // เข้า/ออกไว ๆ
    public bool scalePunch = true;    // ขยายเข้าเล็กน้อย

    Vector3 _baseScale;
    Coroutine _seq;

    void Awake()
    {
        //if (!cg) cg = GetComponent<CanvasGroup>();
        _baseScale = transform.localScale;
        HideInstant();
    }

    public void ShowWave(int waveIndex)
    {
        // อัปเดตข้อความครั้งเดียว
        label.SetText(prefix);
        // ต่อด้วยเลข (เลี่ยง boxing/alloc โดยแยก set)
        label.text += waveIndex.ToString();

        if (_seq != null) StopCoroutine(_seq);
        _seq = StartCoroutine(Seq());
    }

    IEnumerator Seq()
    {
        //cg.blocksRaycasts = false;

        // prep
        //cg.alpha = 0f;
        transform.localScale = _baseScale * (scalePunch ? 0.92f : 1f);

        // fade in
        float t = 0f;
        while (t < fadeDur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeDur);
            //cg.alpha = k;
            if (scalePunch)
                transform.localScale = Vector3.Lerp(_baseScale * 0.92f, _baseScale, k);
            yield return null;
        }
        //cg.alpha = 1f;
        transform.localScale = _baseScale;

        // hold
        yield return new WaitForSeconds(showDur);

        // fade out
        t = 0f;
        while (t < fadeDur)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeDur);
            //cg.alpha = 1f - k;
            yield return null;
        }
        HideInstant();
        _seq = null;
    }

    void HideInstant()
    {
        //cg.alpha = 0f;
        transform.localScale = _baseScale;
    }
}