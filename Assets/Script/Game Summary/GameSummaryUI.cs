using TMPro;
using UnityEngine;

public class GameSummaryUI : MonoBehaviour
{
    [Header("TMP Refs")]
    public TextMeshProUGUI currentScoreTMP;
    public TextMeshProUGUI topScoreTMP;
    public TextMeshProUGUI earnedCoinsTMP;

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        var sm = ScoreManager.I;
        if (sm == null) return;

        // ค่ารอบที่เพิ่งจบ (ScoreManager.FinalizeAndSave() อัปเดตไว้แล้ว)
        int cur   = sm.GetLastRunScore();
        int best  = sm.GetLastRunBest();
        int earn  = sm.GetLastRunCoinsEarned();

        // กันกรณี UI ถูกเปิดก่อน FinalizeAndSave()
        if (cur == 0 && sm.Score > 0) {
            cur  = sm.Score;
            earn = sm.Score / sm.ScorePerCoin;
            best = Mathf.Max(best, cur);
        }

        if (currentScoreTMP) currentScoreTMP.text = cur.ToString();
        if (topScoreTMP)     topScoreTMP.text     = best.ToString();
        if (earnedCoinsTMP)  earnedCoinsTMP.text  = $"+{earn} COINS";
    }
}