using TMPro;
using UnityEngine;

public class ScoreHUD : MonoBehaviour
{
    [SerializeField] TMP_Text label;      // หรือ TextMeshProUGUI ก็ได้
    [SerializeField] string format = "{0}"; // เช่น "Score: {0}"

    void Awake()
    {
        if (!label) label = GetComponentInChildren<TMP_Text>(true);
    }

    void OnEnable()
    {
        if (ScoreManager.I != null)
        {
            ScoreManager.I.OnScoreChanged.AddListener(Refresh);
            // sync ค่าเริ่มต้นทันที
            Refresh(ScoreManager.I.Score);
        }
    }

    void OnDisable()
    {
        if (ScoreManager.I != null)
            ScoreManager.I.OnScoreChanged.RemoveListener(Refresh);
    }

    // เมธอดเดียว ใช้ได้ทั้งจาก event และเรียกเอง
    public void Refresh(int value)
    {
        if (!label) return;
        // ใช้อย่างใดอย่างหนึ่ง
        label.SetText(format, value);                // TMP เวอร์ชันมีโอเวอร์โหลดนี้
        // หรือ: label.text = string.Format(format, value);
    }
}