using UnityEngine;

[CreateAssetMenu(fileName="GameRunResult", menuName="BoxTO/Game Run Result")]
public class GameRunResultSO : ScriptableObject
{
    [Header("Filled when a run ends")]
    public int finalScore;
    public int bestAfter;
    public int coinsEarned;
    public int walletAfter;

    public void FillFrom(ScoreManager sm)
    {
        finalScore  = sm.GetLastRunScore();
        bestAfter   = sm.GetLastRunBest();
        coinsEarned = sm.GetLastRunCoinsEarned();
        walletAfter = sm.GetLastRunWalletAfter();
    }
}