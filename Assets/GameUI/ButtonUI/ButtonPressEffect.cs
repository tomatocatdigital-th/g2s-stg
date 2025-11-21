using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // กำหนดระยะที่ปุ่มจะขยับลงเมื่อถูกกด
    [SerializeField]
    private float pressDepth = 10f; 

    private Vector3 originalPosition;
    private RectTransform rectTransform;

    void Awake()
    {
        // รับ RectTransform ของปุ่มนี้
        rectTransform = GetComponent<RectTransform>();
        // บันทึกตำแหน่งเริ่มต้นของปุ่ม
        originalPosition = rectTransform.anchoredPosition3D;
    }

    // ------------------------------------------
    // ตรวจจับเมื่อผู้ใช้กดเมาส์/นิ้วลงบนปุ่ม
    // ------------------------------------------
    public void OnPointerDown(PointerEventData eventData)
    {
        // คำนวณตำแหน่งที่ขยับลง (แกน Y ลดลง)
        Vector3 pressedPosition = originalPosition;
        pressedPosition.y -= pressDepth;

        // กำหนดตำแหน่งใหม่
        rectTransform.anchoredPosition3D = pressedPosition;
    }

    // ------------------------------------------
    // ตรวจจับเมื่อผู้ใช้ปล่อยเมาส์/นิ้วออกจากปุ่ม
    // ------------------------------------------
    public void OnPointerUp(PointerEventData eventData)
    {
        // กำหนดตำแหน่งกลับไปที่ตำแหน่งเริ่มต้น
        rectTransform.anchoredPosition3D = originalPosition;
    }
}