using System;
using UnityEngine;

[Serializable]
public class CartVariant
{
    public string id = "Blue";
    [Header("Cart Body")]
    public Material cartMaterial;

    [Header("GemItem")]
    public Material gemMaterial;

    [Header("น้ำหนัก")]
    public float weight = 1f;
}
