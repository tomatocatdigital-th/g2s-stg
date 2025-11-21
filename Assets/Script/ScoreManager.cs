using Unity.Burst.Intrinsics;
using UnityEngine;
using UnityEngine.Events;

public enum GameEndReason { None, LivesDepleted, TimeOut }
public enum CartType { Single = 1, Double = 2, Triple = 3 }

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager I { get; private set; }

    // --------- Last run cache ---------
    int _lastRunScore;
    int _lastRunBest;
    int _lastRunCoinsEarned;
    int _lastRunWalletAfter;

    public int GetLastRunScore()        => _lastRunScore;
    public int GetLastRunBest()         => _lastRunBest;
    public int GetLastRunCoinsEarned()  => _lastRunCoinsEarned;
    public int GetLastRunWalletAfter()  => _lastRunWalletAfter;

    [Header("Config Ref")]
    public GameBalanceConfig config;

    [Header("State")]
    public int Score  { get; private set; }
    public int Lives  { get; private set; }
    public int Coins  { get; private set; }

    public int CurrentScore => Score;
    public GameEndReason LastEndReason { get; private set; } = GameEndReason.None;

    [Header("Events")]
    public UnityEvent<int>  OnScoreChanged = new UnityEvent<int>();
    public UnityEvent<int>  OnLivesChanged = new UnityEvent<int>();
    public UnityEvent<int>  OnCoinsChanged = new UnityEvent<int>();
    public UnityEvent       OnGameOver     = new UnityEvent();
    public UnityEvent<bool> OnResolve      = new UnityEvent<bool>(); // true = correct

    [SerializeField] int   defaultStartLives   = 3;
    [SerializeField] float mistakeSpeedKeepMul = 0.80f;
    [SerializeField] int   scorePerCoin        = 10;   // 10 คะแนน = 1 เหรียญ
    [SerializeField] int   baseScorePerCart    = 1;    // base score ต่อ Cart

    const string BEST_KEY                    = "BEST_SCORE";
    const string COIN_KEY                    = "COINS_WALLET";
    const string LAST_RUN_SCORE_KEY          = "LAST_RUN_SCORE";
    const string LAST_RUN_BEST_KEY           = "LAST_RUN_BEST";
    const string LAST_RUN_COINS_EARNED_KEY   = "LAST_RUN_COINS_EARNED";
    const string LAST_RUN_WALLET_AFTER_KEY   = "LAST_RUN_WALLET_AFTER";

    bool _ended = false;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        Coins = PlayerPrefs.GetInt(COIN_KEY, 0);
        _lastRunScore       = PlayerPrefs.GetInt(LAST_RUN_SCORE_KEY, 0);
        _lastRunBest        = PlayerPrefs.GetInt(LAST_RUN_BEST_KEY, 0);
        _lastRunCoinsEarned = PlayerPrefs.GetInt(LAST_RUN_COINS_EARNED_KEY, 0);
        _lastRunWalletAfter = PlayerPrefs.GetInt(LAST_RUN_WALLET_AFTER_KEY, Coins);

        ResetAll();
        EmitSnapshot();
    }

    void Start() => EmitSnapshot();

    int   StartLives   => config ? Mathf.Max(1, config.startLives) : defaultStartLives;
    float BaseSpeed    => config ? config.baseSpeed : 5f;

    // --------- Public API ---------
    public void ResetAll()
    {
        Score = 0;
        Lives = StartLives;
        _ended = false;
        LastEndReason = GameEndReason.None;

        OnScoreChanged.Invoke(Score);
        OnLivesChanged.Invoke(Lives);
        OnCoinsChanged.Invoke(Coins);
    }

    public void EmitSnapshot()
    {
        OnScoreChanged.Invoke(Score);
        OnLivesChanged.Invoke(Lives);
        OnCoinsChanged.Invoke(Coins);
    }

    /// <summary>
    /// ResolveDelivery:
    /// - correct=true: เพิ่มคะแนนตาม CartType
    /// - correct=false: หัก 1 ชีวิต, ลดความเร็วลงชั่วคราว
    /// </summary>
    public void ResolveDelivery(bool correct, CartType type = CartType.Single)
    {
        if (_ended) return;

        OnResolve.Invoke(correct);

        if (correct)
        {
            int multiplier = (int)type;
            AddScoreInternal(baseScorePerCart * multiplier);
        }
        else
        {
            LoseLifeInternal(1);
            
        }

        if (Lives <= 0)
            EndGame(GameEndReason.LivesDepleted);
    }

    // รองรับสคริปต์เก่าที่เรียกแบบเดิม
    public void ResolveDelivery(bool correct) => ResolveDelivery(correct, CartType.Single);

    public void EndGame(GameEndReason reason)
    {
        if (_ended) return;
        _ended = true;
        LastEndReason = reason;

        FinalizeAndSave();
        OnGameOver.Invoke();
    }

    // คงไว้สำหรับโค้ดเก่าที่เรียก (แม้ไม่มีตัวจับเวลาแล้ว)
    public void EndByTimeout() => EndGame(GameEndReason.TimeOut);

    public void AddHearts(int v)
    {
        int maxLives = StartLives;
        Lives = Mathf.Clamp(Lives + v, 0, maxLives);
        OnLivesChanged.Invoke(Lives);
    }

    public int  ScorePerCoin             => Mathf.Max(1, scorePerCoin);
    public int  GetCoinsEarnedThisRun()  => Score / ScorePerCoin;

    public void AddCoins(int v)
    {
        if (v <= 0) return;
        Coins = Mathf.Max(0, Coins + v);
        PlayerPrefs.SetInt(COIN_KEY, Coins);
        PlayerPrefs.Save();
        OnCoinsChanged.Invoke(Coins);
    }

    // --------- Internal Ops ---------
    void SetScore(int value)
    {
        value = Mathf.Max(0, value);
        if (Score == value) return;
        Score = value;
        OnScoreChanged.Invoke(Score);
    }

    void AddScoreInternal(int amount)
    {
        if (amount <= 0) return;
        SetScore(Score + amount);
    }

    void LoseLifeInternal(int amount)
    {
        Lives = Mathf.Max(0, Lives - Mathf.Max(1, amount));
        OnLivesChanged.Invoke(Lives);
    }

    void FinalizeAndSave()
    {
        int best = PlayerPrefs.GetInt(BEST_KEY, 0);
        if (Score > best)
        {
            best = Score;
            PlayerPrefs.SetInt(BEST_KEY, best);
        }

        int earned = Score / Mathf.Max(1, scorePerCoin);
        Coins += earned;
        PlayerPrefs.SetInt(COIN_KEY, Coins);

        _lastRunScore       = Score;
        _lastRunBest        = best;
        _lastRunCoinsEarned = earned;
        _lastRunWalletAfter = Coins;

        PlayerPrefs.SetInt(LAST_RUN_SCORE_KEY,        _lastRunScore);
        PlayerPrefs.SetInt(LAST_RUN_BEST_KEY,         _lastRunBest);
        PlayerPrefs.SetInt(LAST_RUN_COINS_EARNED_KEY, _lastRunCoinsEarned);
        PlayerPrefs.SetInt(LAST_RUN_WALLET_AFTER_KEY, _lastRunWalletAfter);
        PlayerPrefs.Save();

        OnCoinsChanged.Invoke(Coins);
        EmitSnapshot();

        FirebaseScoreUploader.TryUpload(Score, best, earned, Coins);
    }

    public int GetBestLocal()  => PlayerPrefs.GetInt(BEST_KEY, 0);
    public int GetCoinsLocal() => PlayerPrefs.GetInt(COIN_KEY, 0);

    [ContextMenu("Force Game Over")]
    public void ForceGameOver() => EndGame(GameEndReason.LivesDepleted);
}