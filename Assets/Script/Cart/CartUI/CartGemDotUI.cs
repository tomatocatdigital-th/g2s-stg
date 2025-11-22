using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CartGemDotUI : MonoBehaviour
{
    [Header("Refs")]
    public Image colorImage;
    public TextMeshProUGUI indexText;

    [Header("Colors by Route")]
    public Color redColor   = Color.red;
    public Color yellowColor = Color.yellow;
    public Color blueColor  = Color.blue;

    public void Setup(Route routeColor, int? index, bool showIndexNumber)
    {
        // ตั้งสีตาม route
        switch (routeColor)
        {
            case Route.Red:
                colorImage.color = redColor;
                break;
            case Route.Yellow:
                colorImage.color = yellowColor;
                break;
            case Route.Blue:
                colorImage.color = blueColor;
                break;
            default:
                colorImage.color = Color.white;
                break;
        }

        // ตั้งเลข
        if (showIndexNumber && index.HasValue)
        {
            indexText.gameObject.SetActive(true);
            indexText.text = index.Value.ToString();
        }
        else
        {
            indexText.gameObject.SetActive(false);
        }
    }
}