using UnityEngine;

public class CartWagonInfo : MonoBehaviour
{
    public int trainId;        // เลขขบวนเดียวกัน = เท่ากัน
    public int wagonIndex;     // 0 = หัวขบวน, 1, 2, ...
    public int wagonCount = 1; // จำนวนรวมในขบวน (ให้หัวขบวนรู้ไว้ เผื่อใช้ UI)

    // helper: ผูกกับ Cart เดิมของคุณได้ตามปกติ
    public Cart cart;          // ออปชัน
}