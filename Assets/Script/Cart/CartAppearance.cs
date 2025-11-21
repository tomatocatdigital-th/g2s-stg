using UnityEngine;

/// <summary>
/// ใช้กำหนดสีและวัสดุของ Cart ตาม Route ที่ถืออยู่
/// </summary>
[RequireComponent(typeof(Cart))]
public class CartAppearance : MonoBehaviour
{
    [Header("Renderer ของกล่อง/เพชร")]
    public MeshRenderer gemRenderer;

    [Header("Material ตามสีของ Route")]
    public Material blueMat;
    public Material yellowMat;
    public Material redMat;
    public Material trashMat;   // เผื่อรองรับกล่องขยะในอนาคต

    private Cart cart;

    void Awake()
    {
        cart = GetComponent<Cart>();
    }

    /// <summary>
    /// เรียกตอน spawn เพื่อให้ตรงกับสีของ cart.routeColor
    /// </summary>
    public void ApplyFromCart()
    {
        if (!cart) cart = GetComponent<Cart>();
        Apply(cart.routeColor);
    }

    /// <summary>
    /// ใช้ตอนต้องการเซ็ตสีใหม่ (manual)
    /// </summary>
    public void Apply(Route color)
    {
        if (!gemRenderer) return;

        Material mat = null;
        switch (color)
        {
            case Route.Blue:   mat = blueMat; break;
            case Route.Yellow: mat = yellowMat; break;
            case Route.Red:    mat = redMat; break;
            case Route.Trash:  mat = trashMat; break;
            default:           mat = null; break;
        }

        if (mat)
            gemRenderer.sharedMaterial = mat;

        // อัปเดตค่า logic ให้ตรงกัน (กรณีเรียกจากภายนอก)
        if (cart) cart.routeColor = color;
    }

#if UNITY_EDITOR
    // ปุ่มทดสอบใน Inspector (กดแล้วเปลี่ยนสีทันที)
    [ContextMenu("Apply Current RouteColor")]
    void ApplyCurrent()
    {
        ApplyFromCart();
    }
#endif
}