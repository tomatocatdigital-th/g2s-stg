using TMPro;
using UnityEngine;

public class CoinsHUD : MonoBehaviour
{
    public TextMeshProUGUI coinsText;   // ใส่ <Image coin> + Text ก็ได้
    public Animator anim;               // (ไม่จำเป็น) มี trigger ชื่อ "bump"
    int lastValue;

    void OnEnable()
    {
        if (ScoreManager.I)
            ScoreManager.I.OnCoinsChanged.AddListener(OnCoinsChanged);
    }

    void Start()
    {
        if (ScoreManager.I)
        {
            lastValue = ScoreManager.I.Coins;
            SetText(lastValue);
        }
    }

    void OnDisable()
    {
        if (ScoreManager.I)
            ScoreManager.I.OnCoinsChanged.RemoveListener(OnCoinsChanged);
    }

    void OnCoinsChanged(int v)
    {
        if (v > lastValue && anim) anim.SetTrigger("bump"); // เด้งนิด ๆ เวลาได้เงิน
        lastValue = v;
        SetText(v);
    }

    void SetText(int v) => coinsText.text = FormatCoins(v);

    // 999 -> 999 | 1,500 -> 1.5K | 2,000,000 -> 2M
    static string FormatCoins(int n)
    {
        if (n >= 1_000_000) return (n / 1_000_000f).ToString("0.#") + "M";
        if (n >= 1_000)     return (n / 1_000f).ToString("0.#") + "K";
        return n.ToString("N0");
    }
}