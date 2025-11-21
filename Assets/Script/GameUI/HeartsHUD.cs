// HeartsHUD.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeartsHUD : MonoBehaviour
{
    [Header("Config")]
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("Refs")]
    public List<Image> hearts;   // ลาก Image 3 ดวงมา

    public void Refresh(int lives)
    {
        for (int i = 0; i < hearts.Count; i++)
            hearts[i].sprite = (i < lives) ? fullHeart : emptyHeart;
    }
    void OnEnable()
    {
        // สมัครฟัง event ชีวิตเปลี่ยน
        if (ScoreManager.I)
            ScoreManager.I.OnLivesChanged.AddListener(Refresh);
    }

    void OnDisable()
    {
        if (ScoreManager.I)
            ScoreManager.I.OnLivesChanged.RemoveListener(Refresh);
    }

    void Start()
    {
        // รีเฟรชครั้งแรกตามค่าปัจจุบัน
        if (ScoreManager.I) Refresh(ScoreManager.I.Lives);
    }
}