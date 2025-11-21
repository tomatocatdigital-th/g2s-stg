// WaveManager.cs (เวอร์ชัน no-timer)
using UnityEngine;
using System.Collections;

public class WaveManager : MonoBehaviour
{
    public static WaveManager I { get; private set; }

    [Header("Config")]
    public WaveConfig config;
    public float baseSpeedFallback = 5f;

    [Header("Loop")]
    public int wavesPerRound = 5;
    public float interWaveBreak = 1.5f;
    public float preWaveDelay = 0.5f;

    [Header("UI")]
    public RoundUIController roundUI;

    // Optional UI Hooks
    public System.Action<int, int, int> OnWaveStartUI;
    public System.Action OnWaveClearedUI;
    public System.Action OnRunFailedUI;
    public System.Action<int> OnRoundStartUI;

    int roundIndex = 0;
    int waveIndex = 0;
    int deliveredThisWave = 0;
    bool runningWave = false;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        if (ScoreManager.I)
            ScoreManager.I.OnResolve.AddListener(OnResolveFromScore);
    }

    void OnDisable()
    {
        if (ScoreManager.I)
            ScoreManager.I.OnResolve.RemoveListener(OnResolveFromScore);
    }

    [ContextMenu("Start Run (debug)")]
    public void StartRun()
    {
        StopAllCoroutines();
        roundIndex = 0;
        waveIndex = 0;
        StartCoroutine(Co_Run());
    }

    IEnumerator Co_Run()
    {
        if (config == null || config.waves == null || config.waves.Length == 0)
        {
            Debug.LogError("[WaveManager] ❌ No config/waves assigned!");
            yield break;
        }
        if (wavesPerRound <= 0) wavesPerRound = config.waves.Length;

        while (true)
        {
            // เริ่มรอบใหม่
            yield return StartCoroutine(Co_BeginRound(roundIndex));

            for (waveIndex = 0; waveIndex < wavesPerRound; waveIndex++)
            {
                var def = config.waves[waveIndex % config.waves.Length];

                // เตรียมเวฟ
                deliveredThisWave = 0;
                runningWave = true;

                // ความเร็ว/การเกิด
                if (SpeedManager.I != null)
                    SpeedManager.I.SetBaseSpeed(def.speedBonusAbs);


                // UI เริ่มเวฟ (1-based)
                OnWaveStartUI?.Invoke(roundIndex + 1, waveIndex + 1, def.targetDeliver);

                // รอผลเวฟ
                while (runningWave)
                {
                    // แพ้เพราะชีวิตหมด
                    if (ScoreManager.I && ScoreManager.I.Lives <= 0)
                    {
                        runningWave = false;
                        OnRunFailedUI?.Invoke();
                        ScoreManager.I.EndByTimeout(); // หรือ LivesDepleted แล้วแต่เมธอดที่มี
                        yield break;
                    }

                    // ผ่านเวฟ
                    if (deliveredThisWave >= def.targetDeliver)
                    {
                        runningWave = false;

                        if (def.heartRewardOnClear > 0)
                            ScoreManager.I.AddHearts(def.heartRewardOnClear);

                        OnWaveClearedUI?.Invoke();

                        if (waveIndex < wavesPerRound - 1)
                            yield return new WaitForSeconds(interWaveBreak);
                        break;
                    }

                    yield return null;
                }
            }

            // จบรอบ → ไปอีกรอบ (ขึ้น Wave/ความยากใหม่ตาม config)
            roundIndex++;
        }
    }

    IEnumerator Co_BeginRound(int rIndex0Based)
    {
        int roundNumber = rIndex0Based + 1;
        OnRoundStartUI?.Invoke(roundNumber);
        if (roundUI != null) roundUI.ShowRound(roundNumber);
        if (preWaveDelay > 0f) yield return new WaitForSeconds(preWaveDelay);
    }

    void OnResolveFromScore(bool correct)
    {
        if (!runningWave) return;
        if (correct) deliveredThisWave++;
    }
    
    string[] NamesFromMask(WaveConfig.LaneMask m)
{
    // map เป็นชื่อเลนที่ตั้งไว้ใน CartSpawner.lanes: "Left","Mid","Right"
    var list = new System.Collections.Generic.List<string>(3);
    if ((m & WaveConfig.LaneMask.Left)  != 0) list.Add("Left");
    if ((m & WaveConfig.LaneMask.Mid)   != 0) list.Add("Mid");
    if ((m & WaveConfig.LaneMask.Right) != 0) list.Add("Right");
    return list.ToArray();
}


    // ===== Spawner Hooks =====
}