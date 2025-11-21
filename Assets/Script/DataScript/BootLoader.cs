using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System;
using System.Threading;
using System.Threading.Tasks;

#if FIREBASE_ENABLED
using Firebase;
using Firebase.Auth;
#endif

#if PLAY_GAMES_V2 && UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
#endif

public class BootLoader : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject spinner;
    [SerializeField] GameObject enterButton;
    [SerializeField] TextMeshProUGUI statusText;

    [Header("Next Scene")]
    [SerializeField] string nextScene = "MainMenu";

    [Header("Timeouts (sec)")]
    [SerializeField] float gpgsSignInTimeout   = 6f;   // ✅ เพิ่มสำหรับ GPGS
    [SerializeField] float firebaseInitTimeout = 6f;
    [SerializeField] float signInTimeout       = 5f;
    [SerializeField] float playerDataTimeout   = 5f;
    [SerializeField] float fallbackShowEnter   = 7f;

    bool _ready = false;
    bool _uiShown = false;
    bool _entered = false;                 // กันกดซ้ำ
    CancellationTokenSource _lifetimeCts;  // ยกเลิกงานเมื่อซีนนี้หาย

    async void Start()
    {
        _lifetimeCts = new CancellationTokenSource();

        try
        {
            if (spinner) spinner.SetActive(true);
            if (enterButton) enterButton.SetActive(false);
            SetStatus("Preparing...");

            await Task.Yield();

            var initTask = InitializeAllAsync(_lifetimeCts.Token);

            // กันค้าง: แสดงปุ่มช้าที่สุดไม่เกิน fallbackShowEnter
            var guard = Task.Delay(TimeSpan.FromSeconds(Mathf.Max(1f, fallbackShowEnter)), _lifetimeCts.Token);
            var first = await Task.WhenAny(initTask, guard);

            ShowEnterButton();

            // ปล่อยให้ init จบเองแบบไม่บล็อก UI
            _ = initTask.ContinueWith(t =>
            {
                if (t.IsFaulted) Debug.LogWarning("[Boot] Init finished with exception: " + t.Exception);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Boot] Start error: " + e.Message);
            ShowEnterButton();
        }
    }

    void OnDestroy()
    {
        // ยกเลิกงานที่ยังค้างอยู่ (เช่น timeout / network)
        try { _lifetimeCts?.Cancel(); _lifetimeCts?.Dispose(); } catch { }
    }

    void ShowEnterButton()
    {
        if (_uiShown) return;
        _uiShown = true;

        if (spinner) spinner.SetActive(false);
        if (enterButton) enterButton.SetActive(true);
        SetStatus(_ready ? "Ready! Tap to Start" : "Offline ready (Tap to Start)");
    }

    public void OnPressEnter()
    {
        if (_entered) return;
        _entered = true;

        if (enterButton) enterButton.SetActive(false); // กันกดรัว
        SceneManager.LoadScene(nextScene);
    }

    // -------- Core init --------
    async Task InitializeAllAsync(CancellationToken ct)
    {
        try
        {
            // ---------- 1) Google Play Games (Android only) ----------
#if PLAY_GAMES_V2 && UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                SetStatus("Signing in Google Play...");
                var gpgsStatus = await AwaitWithTimeout(GpgsSignInAsync(ct),
                                                        gpgsSignInTimeout,
                                                        "GPGS sign-in timeout",
                                                        ct);

                Debug.Log("[Boot] GPGS SignInStatus = " + gpgsStatus);

                if (gpgsStatus == SignInStatus.Success)
                    SetStatus("Google Play connected");
                else
                    SetStatus("GPGS skipped (" + gpgsStatus + ")");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[Boot] GPGS sign-in failed: " + ex.Message);
                SetStatus("GPGS offline");
            }
#else
            Debug.Log("[Boot] GPGS disabled or not Android runtime");
#endif

            // ---------- 2) Firebase / Auth ----------
#if FIREBASE_ENABLED
            SetStatus("Checking Firebase...");
            var depTask = FirebaseApp.CheckAndFixDependenciesAsync();
            await AwaitWithTimeout(depTask, firebaseInitTimeout, "Firebase init timeout", ct);

            if (depTask.Result == DependencyStatus.Available)
            {
                var auth = FirebaseAuth.DefaultInstance;
                if (auth.CurrentUser == null)
                {
                    SetStatus("Signing in (guest)...");
                    await AwaitWithTimeout(auth.SignInAnonymouslyAsync(),
                                           signInTimeout,
                                           "Sign-in timeout",
                                           ct);
                    Debug.Log("[Boot] Guest signed in: " + auth.CurrentUser?.UserId);
                }
            }
            else
            {
                Debug.LogWarning("[Boot] Firebase not available: " + depTask.Result);
                SetStatus("Offline mode");
            }
#else
            Debug.Log("[Boot] Firebase disabled");
#endif

            // ---------- 3) PlayerDataManager ----------
            if (PlayerDataManager.I == null)
            {
                var go = new GameObject("PlayerDataManager");
                go.AddComponent<PlayerDataManager>();
                DontDestroyOnLoad(go);
                Debug.Log("[Boot] PlayerDataManager created");
            }

            SetStatus("Loading player data...");
            await AwaitWithTimeout(PlayerDataManager.I.InitializeAsync(),
                                   playerDataTimeout,
                                   "Player data timeout",
                                   ct);
            SetStatus("Data ready");

            _ready = true;
        }
        catch (OperationCanceledException)
        {
            // ซีนเปลี่ยน / แอปปิด ไม่ต้องทำอะไรต่อ
        }
        catch (Exception e)
        {
            Debug.LogWarning("[Boot] InitializeAllAsync error → offline: " + e.Message);
            SetStatus("Offline mode");
            _ready = false;
        }
    }

    // -------- GPGS helper (callback -> Task) --------
#if PLAY_GAMES_V2 && UNITY_ANDROID && !UNITY_EDITOR
    Task<SignInStatus> GpgsSignInAsync(CancellationToken ct)
    {
        var tcs = new TaskCompletionSource<SignInStatus>();

        PlayGamesPlatform.Activate();

        // ถ้าจะใช้ config เพิ่มสามารถปรับ PlayGamesClientConfiguration ตรงนี้ได้
        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            if (ct.IsCancellationRequested)
            {
                tcs.TrySetCanceled(ct);
                return;
            }
            tcs.TrySetResult(status);
        });

        ct.Register(() =>
        {
            tcs.TrySetCanceled(ct);
        });

        return tcs.Task;
    }
#endif

    // -------- helpers --------
    async Task<T> AwaitWithTimeout<T>(Task<T> task, float seconds, string timeoutMessage, CancellationToken ct)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var delay = Task.Delay(TimeSpan.FromSeconds(Mathf.Max(0.5f, seconds)), timeoutCts.Token);

        var finished = await Task.WhenAny(task, delay);
        if (finished == delay) throw new TimeoutException(timeoutMessage);

        timeoutCts.Cancel();      // ยกเลิก delay
        ct.ThrowIfCancellationRequested();
        return await task;        // bubble exceptions + return result
    }

    async Task AwaitWithTimeout(Task task, float seconds, string timeoutMessage, CancellationToken ct)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        var delay = Task.Delay(TimeSpan.FromSeconds(Mathf.Max(0.5f, seconds)), timeoutCts.Token);

        var finished = await Task.WhenAny(task, delay);
        if (finished == delay) throw new TimeoutException(timeoutMessage);

        timeoutCts.Cancel();      // ยกเลิก delay
        ct.ThrowIfCancellationRequested();
        await task;               // bubble exceptions
    }

    void SetStatus(string s)
    {
        if (statusText) statusText.text = s;
        Debug.Log("[BootUI] " + s);
    }
}