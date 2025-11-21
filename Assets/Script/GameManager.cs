using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum GameState { Menu, Playing, GameOver }

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }

    [Header("Scene Names")]
    [SerializeField] string mainMenuSceneName = "MainMenu";
    [SerializeField] string gameOverSceneName = "GameOver";

    public CanvasGroup screenFade;   // UI ทับทั้งจอ, alpha=0, Active=true
    public float fadeDur = 0.35f;

    [Header("Refs (ในฉาก Game เท่านั้น)")]
    public ScoreManager score;
    public CartSpawner spawner;
    public WaveController wave;

    [Header("Cross-scene payload (optional)")]
    public GameRunResultSO runResult;

    [Header("Events")]
    public UnityEvent OnEnterPlaying;
    public UnityEvent OnEnterGameOver;

    public GameState state { get; private set; } = GameState.Menu;
    bool endHandled = false;

    // ✅ ใช้จับเวลาเล่นรอบนี้ (เพิ่มเข้ามาใหม่)
    float _runStartRealtime = 0f;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;

        Time.timeScale = 1f;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    void OnEnable()
    {
        if (score) score.OnGameOver.AddListener(HandleGameOver);
    }

    void OnDisable()
    {
        if (score) score.OnGameOver.RemoveListener(HandleGameOver);
    }

    void Start()
    {
        StartGame();
    }

    // ====================== GAME FLOW ======================

    public void StartGame()
    {
        if (spawner) spawner.ResetSpawnerState();
        state = GameState.Playing;
        endHandled = false;

        // รีเซ็ตสถานะเกม
        if (score) score.ResetAll();

        // เปิดสปอว์น + เริ่มเวฟ
        if (spawner) spawner.enabled = true;
        if (wave)    wave.StartWaves();

        // ✅ เริ่มจับเวลาเล่นรอบนี้
        _runStartRealtime = Time.realtimeSinceStartup;

        OnEnterPlaying?.Invoke();
    }

    void HandleGameOver() => EndGame();

    public async void EndGame()
    {
        if (endHandled) return;
        endHandled = true;

        state = GameState.GameOver;

        // หยุดระบบที่ต้องหยุด
        if (spawner) spawner.enabled = false;
        // TODO: ถ้าอยากให้ WaveController หยุดด้วยก็เพิ่มเมธอด StopRun() แล้วเรียกที่นี่

        OnEnterGameOver?.Invoke();

        // =================== บันทึกผลรอบนี้ ===================

        int finalScore = score ? score.CurrentScore : 0;

        // ถ้ามี property ใน WaveController เช่น CurrentWave / LastWave ให้เปลี่ยนตรงนี้ให้ตรงชื่อ
        int finalWave = 0;
        // ตัวอย่าง:
        // if (wave != null) finalWave = wave.CurrentWave;

        int durationSec = Mathf.Max(
            0,
            Mathf.RoundToInt(Time.realtimeSinceStartup - _runStartRealtime)
        );

        int buildVerInt = ParseVersionToInt(Application.version);

        Debug.Log($"[GM] EndGame -> score={finalScore}, wave={finalWave}, dur={durationSec}s, ver={buildVerInt}");

        if (PlayerDataManager.I != null)
        {
            // ✅ ใช้ระบบเซฟใหม่
            await PlayerDataManager.I.SaveRunAsync(
                finalScore,
                finalWave,
                durationSec,
                buildVerInt
            );
        }

        // ส่งค่าไป SO เพื่อให้หน้า GameOver อ่าน
        if (runResult && ScoreManager.I)
            runResult.FillFrom(ScoreManager.I);

        Time.timeScale = 0f;
        SceneManager.LoadScene(gameOverSceneName);
    }

    // แปลง "1.2.3" → 10203 ง่าย ๆ ไว้เก็บใน Firestore
    int ParseVersionToInt(string ver)
    {
        if (string.IsNullOrEmpty(ver)) return 0;
        var seg = ver.Split('.');
        int major = seg.Length > 0 && int.TryParse(seg[0], out var a) ? a : 0;
        int minor = seg.Length > 1 && int.TryParse(seg[1], out var b) ? b : 0;
        int patch = seg.Length > 2 && int.TryParse(seg[2], out var c) ? c : 0;
        return (major * 10000) + (minor * 100) + patch;
    }

    // ====================== BUTTONS (ใช้ในฉาก Game เท่านั้น) ======================

    public void PressHome()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}