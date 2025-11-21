using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if FIREBASE_ENABLED
using Firebase;
using Firebase.Auth;
#endif

public class FirebaseBootstrap : MonoBehaviour
{
    [Header("Next scene after boot")]
    [SerializeField] string nextScene = "MainMenu";

    [Header("Optional UI")]
    [SerializeField] GameObject loadingSpinner;
    [SerializeField] TMPro.TextMeshProUGUI statusText;

    [Header("Safety")]
    [SerializeField] bool autoCreatePlayerDataManager = true;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (loadingSpinner) loadingSpinner.SetActive(true);
        SetStatus("Starting...");
    }

    void Start() => StartCoroutine(BootFlow());

    IEnumerator BootFlow()
    {
        Log("[Boot] Begin");
#if FIREBASE_ENABLED
        // Verbose log ของ Firebase
        Firebase.FirebaseApp.LogLevel = Firebase.LogLevel.Debug;

        SetStatus("Checking Firebase...");
        var depTask = FirebaseApp.CheckAndFixDependenciesAsync();
        while (!depTask.IsCompleted) yield return null;

        if (depTask.Result != DependencyStatus.Available)
        {
            LogWarning($"[Boot] Firebase dependency not available: {depTask.Result}. Running local-only.");
            SetStatus("Offline (local only)");
        }
        else
        {
            Log("[Boot] Firebase dependencies OK");
            // ----- Sign-in (Anonymous) -----
            var auth = FirebaseAuth.DefaultInstance;
            if (auth.CurrentUser == null)
            {
                SetStatus("Signing in (guest)...");
                var signInTask = auth.SignInAnonymouslyAsync();
                while (!signInTask.IsCompleted) yield return null;

                if (signInTask.Exception != null)
                {
                    LogError("[Boot] Anonymous sign-in FAILED\n" + signInTask.Exception);
                    SetStatus("Guest sign-in failed (local only)");
                }
                else
                {
                    var u = auth.CurrentUser;
                    Log($"[Boot] Anonymous sign-in SUCCESS uid={u?.UserId}, isAnonymous={u?.IsAnonymous}");
                    SetStatus("Guest signed in");
                }
            }
            else
            {
                var u = auth.CurrentUser;
                Log($"[Boot] Already signed in uid={u?.UserId}, isAnonymous={u?.IsAnonymous}");
                SetStatus("Signed in");
            }
        }
#else
        Log("[Boot] FIREBASE_ENABLED not defined → local-only");
        SetStatus("Local only");
#endif

        // ----- Ensure PlayerDataManager -----
        if (PlayerDataManager.I == null)
        {
            if (autoCreatePlayerDataManager)
            {
                Log("[Boot] PlayerDataManager not found → create one.");
                var go = new GameObject("PlayerDataManager");
                go.AddComponent<PlayerDataManager>();
                DontDestroyOnLoad(go);
            }
            else
            {
                LogError("[Boot] PlayerDataManager is NULL and autoCreate disabled. Abort boot.");
                yield break;
            }
        }

        // ----- Initialize PlayerDataManager -----
        SetStatus("Sync player data...");
        var initTask = PlayerDataManager.I.InitializeAsync();
        while (!initTask.IsCompleted) yield return null;

        if (initTask.Exception != null)
        {
            LogError("[Boot] PlayerDataManager.InitializeAsync FAILED\n" + initTask.Exception);
            SetStatus("Data sync failed (local only)");
        }
        else
        {
            Log("[Boot] PlayerDataManager.InitializeAsync DONE");
            SetStatus("Ready");
        }

        if (loadingSpinner) loadingSpinner.SetActive(false);
        SceneManager.LoadScene(nextScene);
    }

    // ---------- Helpers ----------
    void SetStatus(string s)
    {
        if (statusText) statusText.text = s;
        Log("[BootUI] " + s);
    }
    void Log(string s)       => Debug.Log(s);
    void LogWarning(string s)=> Debug.LogWarning(s);
    void LogError(string s)  => Debug.LogError(s);
}