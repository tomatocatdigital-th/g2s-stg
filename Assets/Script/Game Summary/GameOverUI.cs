using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("Cross-scene payload")]
    public GameRunResultSO run;   // ← ลาก asset GameRunResult.asset มาใส่

    [Header("Texts")]
    public TextMeshProUGUI currentScoreTMP;
    public TextMeshProUGUI topScoreTMP;
    public TextMeshProUGUI earnedCoinsTMP;
    public TextMeshProUGUI walletAfterTMP;

    [Header("Optional")]
    public GameObject loadingSpinner;    // ถ้ามีเอฟเฟกต์โหลด
    [SerializeField] string gameSceneName = "Game";
    [SerializeField] string mainMenuSceneName = "MainMenu";

    void Start()
    {
        Refresh();   // แสดงค่าทันทีที่เข้า scene
    }

    public void Refresh()
    {
        if (loadingSpinner) loadingSpinner.SetActive(false);

        if (!run)
        {
            Debug.LogWarning("[GameOverUI] run SO is null, fallback to ScoreManager.");
            var sm = ScoreManager.I;
            if (sm)
            {
                SetTexts(
                    sm.GetLastRunScore(),
                    sm.GetLastRunBest(),
                    sm.GetLastRunCoinsEarned(),
                    sm.GetLastRunWalletAfter()
                );
            }
            return;
        }

        SetTexts(run.finalScore, run.bestAfter, run.coinsEarned, run.walletAfter);
    }

    void SetTexts(int finalScore, int bestAfter, int coinsEarned, int walletAfter)
    {
        if (currentScoreTMP) currentScoreTMP.text = $"Total Score: {finalScore}";
        if (topScoreTMP) topScoreTMP.text = $"Top Score: {bestAfter}";
        if (earnedCoinsTMP) earnedCoinsTMP.text = $"Earned: +{coinsEarned} COINS";
        if (walletAfterTMP) walletAfterTMP.text = $"Total coin: {walletAfter} Coins";
    }

    // ===== Buttons =====
    public void PressRetry()
    {
        SceneManager.LoadScene(gameSceneName);
    }
    public void PressHome()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // (ถ้ามีปุ่ม Coins x2)
    public async void PressDoubleCoins()
    {
        // ตรงนี้คุณค่อยเชื่อมโฆษณา/เงื่อนไขได้
        // ตัวอย่างเรียกเพิ่มเหรียญแล้วรีเฟรช UI
        int add = run ? run.coinsEarned : (ScoreManager.I ? ScoreManager.I.GetCoinsEarnedThisRun() : 0);
        if (add > 0)
        {
            await PlayerDataManager.I.AddCoinsAsync(add); // เพิ่มอีกเท่าหนึ่ง
            if (run)
            {
                run.coinsEarned *= 2;
                run.walletAfter += add;
            }
            Refresh();
        }
    }
}