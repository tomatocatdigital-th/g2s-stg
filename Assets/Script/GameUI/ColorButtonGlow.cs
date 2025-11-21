using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public enum RailColor { Blue, Yellow, Red }

public class ColorButtonGlow : MonoBehaviour, IPointerClickHandler
{
    [Header("Setup")]
    public RailColor color;
    public ButtonGroup group;   // ตัวกลาง

    [Header("UI Images")]
    public Image ringImage;   // วงแหวนรอบปุ่ม
    public Image iconImage;   // ไอคอนในปุ่ม

    [Header("Colors")]
    public Color ringNormal = new Color(1, 1, 1, 0.5f);
    public Color ringActive = Color.white;
    public Color iconNormal = new Color(1, 1, 1, 0.85f);
    public Color iconActive = Color.white;

    [Header("Animation")]
    public float scaleActive = 1.12f;
    public float tweenTime = 0.12f;
    public bool pulseWhileActive = true;
    public float pulseAmp = 0.06f;
    public float pulseSpeed = 3.5f;

    Vector3 _baseScale;
    Coroutine _animCo;
    bool _isActive;

    void Awake()
    {
        _baseScale = transform.localScale;
        SetVisual(false, instant: true);
    }

    // ให้กลุ่มเรียกใช้
    public void SetVisual(bool active, bool instant = false)
    {
        _isActive = active;

        if (ringImage) ringImage.color = active ? ringActive : ringNormal;
        if (iconImage) iconImage.color = active ? iconActive : iconNormal;

        Vector3 target = _baseScale * (active ? scaleActive : 1f);
        if (_animCo != null) StopCoroutine(_animCo);
        if (instant) transform.localScale = target;
        else _animCo = StartCoroutine(TweenScale(target));

        if (pulseWhileActive && active)
        {
            if (_animCo != null) StopCoroutine(_animCo);
            _animCo = StartCoroutine(Pulse());
        }
    }

    public void OnPointerClick(PointerEventData e)
    {
        if (group) group.MakeActive(this);   // แจ้งให้ตัวกลางสลับ
    }

    IEnumerator TweenScale(Vector3 target)
    {
        float t = 0f; Vector3 from = transform.localScale;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / tweenTime;
            transform.localScale = Vector3.LerpUnclamped(from, target, t);
            yield return null;
        }
        transform.localScale = target;
    }

    IEnumerator Pulse()
    {
        float baseScale = scaleActive;
        while (_isActive)
        {
            float s = baseScale + Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * pulseSpeed) * pulseAmp;
            transform.localScale = _baseScale * s;
            yield return null;
        }
        transform.localScale = _baseScale;
    }
}
