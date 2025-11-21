using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Cart))]
public class CartDebugPrinter : MonoBehaviour
{
    void Start()
    {
        var cart  = GetComponent<Cart>();
        var multi = GetComponent<CartMultiGem>();

        string colorInfo;

        if (multi != null && multi.ColorsCount > 0)
        {
            // พิมพ์ลิสต์สีทั้งหมดของคันนี้
            colorInfo = string.Join(", ",
                multi.Colors.Select(c => c.ToString()));
        }
        else
        {
            // ไม่มี multi → ใช้ routeColor เดียว
            colorInfo = cart ? cart.routeColor.ToString() : "None";
        }

        Debug.Log($"[CartDebug] Spawned cart '{name}' colors = [{colorInfo}]");
    }
}