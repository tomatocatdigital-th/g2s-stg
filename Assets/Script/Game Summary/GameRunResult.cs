public static class GameRunResult
{
    public static int finalScore;
    public static int bestAfter;
    public static int coinsEarned;
    public static int walletAfter;

    public static void FillFromScoreManager()
    {
        var sm = ScoreManager.I;
        finalScore  = sm.GetLastRunScore();
        bestAfter   = sm.GetLastRunBest();
        coinsEarned = sm.GetLastRunCoinsEarned();
        walletAfter = sm.GetLastRunWalletAfter();
    }
}