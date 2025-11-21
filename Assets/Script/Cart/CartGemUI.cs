using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CartGemUI : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform dotContainer;
    public Image dotPrefab;
    public Image bubbleBg;
    public Image bubbleTail;

    [Header("Sprites (optional)")]
    public Sprite redDotSprite;
    public Sprite blueDotSprite;
    public Sprite yellowDotSprite;

    [Header("Layout")]
    public float dotSize = 0.12f;   // เมตร
    public float spacing = 0.04f;

    [Header("Matched Visual")]
    [Range(0f,1f)] public float matchedFade = 0.25f;
    public float matchedScale = 0.85f;

    readonly List<Image> _dots = new();
    int _matchedCount = 0;

    public void Setup(IList<Route> seq)
    {
        // layout
        var layout = dotContainer.GetComponent<HorizontalLayoutGroup>();
        if (layout) layout.spacing = spacing;

        // clear
        foreach (Transform c in dotContainer) Destroy(c.gameObject);
        _dots.Clear();

        // build dots
        foreach (var color in seq)
        {
            var img = Instantiate(dotPrefab, dotContainer);
            var rt  = (RectTransform)img.transform;
            rt.sizeDelta = new Vector2(dotSize, dotSize);
            img.raycastTarget = false;

            // ใช้สไปรท์ถ้ามี, ไม่งั้นลงสี
            var sp = SpriteFor(color);
            if (sp) { img.sprite = sp; img.color = Color.white; }
            else    { img.sprite = dotPrefab.sprite; img.color = ToColor(color); }

            _dots.Add(img);
        }

        _matchedCount = 0;
        ApplyMatchedVisual();
    }

    public void SetMatchedCount(int count)
    {
        _matchedCount = Mathf.Clamp(count, 0, _dots.Count);
        ApplyMatchedVisual();
    }

    public void FlashSuccess()
    {
        foreach (var d in _dots)
        {
            d.CrossFadeAlpha(1f, 0f, true);
            d.CrossFadeAlpha(matchedFade, 0.15f, false);
            d.transform.localScale = Vector3.one * matchedScale;
        }
    }
    public void FlashFail()
    {
        foreach (var d in _dots)
        {
            d.CrossFadeAlpha(1f, 0f, true);
            d.transform.localScale = Vector3.one;
        }
    }

    void ApplyMatchedVisual()
    {
        for (int i = 0; i < _dots.Count; i++)
        {
            bool matched = (i < _matchedCount);
            var img = _dots[i];
            img.canvasRenderer.SetAlpha(matched ? matchedFade : 1f);
            img.transform.localScale = matched ? Vector3.one * matchedScale : Vector3.one;
        }
    }

    Sprite SpriteFor(Route r) => r switch
    {
        Route.Red    => redDotSprite,
        Route.Blue   => blueDotSprite,
        Route.Yellow => yellowDotSprite,
        _            => null
    };

    Color ToColor(Route r) => r switch
    {
        Route.Red    => new Color(1f, .3f, .3f),
        Route.Blue   => new Color(.35f, .55f, 1f),
        Route.Yellow => new Color(1f, .95f, .3f),
        _            => Color.white
    };
}