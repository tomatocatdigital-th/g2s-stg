using TMPro;
using UnityEngine;

public class WalletText : MonoBehaviour
{
    public TextMeshProUGUI text;
    const string COIN_KEY = "COINS_WALLET";

    void OnEnable()
    {
        int coins = PlayerPrefs.GetInt(COIN_KEY, 0);
        text.text = coins.ToString("N0");
    }
}