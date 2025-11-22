using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CartGemUI : MonoBehaviour
{
    [Header("Follow / Billboard")]
    public Transform followTarget;      // BubblePoint บน cart
    public Camera targetCamera;         // ปล่อยว่างได้ เดี๋ยวเลือก Camera.main ให้
    public RectTransform root;          // RectTransform ของ BubbleRoot

    [Header("Slots (fixed 3)")]
    public Image[] slotImages = new Image[3];   // ลาก Slot0, Slot1, Slot2 มาใส่

    [Header("Sprites by Route color")]
    public Sprite redSprite;
    public Sprite yellowSprite;
    public Sprite blueSprite;
    public Sprite emptySprite;          // sprite ว่าง (หรือปล่อยว่างแล้ว disable เอา)

    [Header("Options")]
    public bool hideWhenNoRoute = true;

    void Awake()
    {
        if (root == null)
            root = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        // ให้ bubble ตามตำแหน่ง BubblePoint
        if (followTarget != null && root != null)
        {
            root.position = followTarget.position;
        }

        // billboard หันเข้ากล้อง
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera != null && root != null)
        {
            Vector3 camPos = targetCamera.transform.position;
            Vector3 dir = root.position - camPos;
            if (dir.sqrMagnitude > 0.0001f)
            {
                root.rotation = Quaternion.LookRotation(dir);
            }
        }
    }

    public void Setup(IReadOnlyList<Route> routes)
    {
        if (slotImages == null || slotImages.Length == 0)
            return;

        if (routes == null || routes.Count == 0)
        {
            if (hideWhenNoRoute && root != null)
                root.gameObject.SetActive(false);

            // หรือเคลียร์ slot ให้ว่าง
            for (int i = 0; i < slotImages.Length; i++)
            {
                if (slotImages[i] == null) continue;
                slotImages[i].sprite = emptySprite;
                slotImages[i].enabled = (emptySprite != null);
            }
            return;
        }

        if (root != null)
            root.gameObject.SetActive(true);

        for (int i = 0; i < slotImages.Length; i++)
        {
            var img = slotImages[i];
            if (img == null) continue;

            if (i < routes.Count)
            {
                img.enabled = true;
                img.sprite = GetSpriteForRoute(routes[i]);
            }
            else
            {
                if (emptySprite != null)
                {
                    img.enabled = true;
                    img.sprite = emptySprite;
                }
                else
                {
                    img.enabled = false;
                }
            }
        }
    }

    Sprite GetSpriteForRoute(Route route)
    {
        switch (route)
        {
            case Route.Red:    return redSprite;
            case Route.Yellow: return yellowSprite;
            case Route.Blue:   return blueSprite;
            default:           return emptySprite;
        }
    }

    internal void Setup(object routesForThisCart)
    {
        throw new NotImplementedException();
    }
}