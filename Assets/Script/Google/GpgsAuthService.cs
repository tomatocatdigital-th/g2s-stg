using System;
using System.Collections;
using UnityEngine;

#if PLAY_GAMES_V2 && UNITY_ANDROID
using GooglePlayGames;                 // Unity v2 plugin
using GooglePlayGames.BasicApi;        // SignInStatus
#endif

#if FIREBASE_ENABLED
using Firebase.Auth;                   // PlayGamesAuthProvider
#endif

// -------------------------------
// ANDROID + GPGS V2 (โค้ดจริง)
// -------------------------------
#if PLAY_GAMES_V2 && UNITY_ANDROID

public class GpgsAuthService : MonoBehaviour
{
    public static GpgsAuthService I { get; private set; }

    [Header("Optional UI")]
    public TMPro.TextMeshProUGUI statusText;

    [Header("Boot options")]
    public bool silentSignInOnStart = true;

    static bool s_activated;
    void EnsureActivated()
    {
        if (s_activated) return;
        PlayGamesPlatform.DebugLogEnabled = true;   // ปิดได้ตอนโปรดักชัน
        PlayGamesPlatform.Activate();
        s_activated = true;
    }

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (silentSignInOnStart) StartCoroutine(BootAndSilentSignIn());
    }

    public bool IsAuthenticated
    {
        get { return PlayGamesPlatform.Instance?.localUser?.authenticated ?? false; }
    }

    public string UserId
    {
        get { return PlayGamesPlatform.Instance?.localUser?.id; }
    }

    public string UserName
    {
        get { return PlayGamesPlatform.Instance?.localUser?.userName; }
    }

    // ---------- Boot & Silent sign-in ----------
    IEnumerator BootAndSilentSignIn()
    {
        EnsureActivated();
        SetStatus("GPGS silent sign-in ...");

        bool done = false;
        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            Log($"[GPGS] Silent sign-in: {status}");
            done = true;
        });

        while (!done) yield return null;

        SetStatus(IsAuthenticated ? $"Signed in: {UserName}" : "Guest (not linked)");
        yield break;
    }

    // ---------- Interactive sign-in ----------
    public IEnumerator SignInInteractive(Action<bool> onDone = null)
    {
        EnsureActivated();
        SetStatus("Signing in (interactive) ...");

        bool finished = false;
        bool ok = false;

        PlayGamesPlatform.Instance.ManuallyAuthenticate(status =>
        {
            ok = (status == SignInStatus.Success);
            Log($"[GPGS] Interactive sign-in: {status}");
            finished = true;
        });

        while (!finished) yield return null;

        SetStatus(ok ? $"Signed in: {UserName}" : "Sign-in canceled/failed");
        onDone?.Invoke(ok);
        yield break;
    }

    // ---------- Server Auth Code ----------
    public IEnumerator RequestServerAuthCode(bool forceRefresh, Action<string> onResult)
    {
        if (!IsAuthenticated)
        {
            Log("[GPGS] Not signed in. Cannot request server auth code.");
            onResult?.Invoke(null);
            yield break;
        }

        string received = null;
        bool finished = false;

        PlayGamesPlatform.Instance.RequestServerSideAccess(forceRefresh, code =>
        {
            received = code;
            Log($"[GPGS] Server auth code: {(string.IsNullOrEmpty(code) ? "NONE" : "RECEIVED")}");
            finished = true;
        });

        while (!finished) yield return null;
        onResult?.Invoke(received);
        yield break;
    }

    // ---------- Sign-out ----------
    // v2 ไม่มี API sign-out แล้ว
    public void SignOutNotice() => Log("[GPGS] Sign-out is no longer supported in Play Games Services v2.");

#if FIREBASE_ENABLED
    // ---------- Link Firebase ----------
    public IEnumerator LinkFirebaseCurrentUser(Action<bool, string> onDone)
    {
        yield return RequestServerAuthCode(false, async code =>
        {
            if (string.IsNullOrEmpty(code))
            {
                onDone?.Invoke(false, "Empty server auth code");
                return;
            }

            try
            {
                var cred = PlayGamesAuthProvider.GetCredential(code);
                var auth = FirebaseAuth.DefaultInstance;
                var user = auth.CurrentUser;

                if (user == null) await auth.SignInWithCredentialAsync(cred);
                else              await user.LinkWithCredentialAsync(cred);

                onDone?.Invoke(true, null);
            }
            catch (Exception e)
            {
                onDone?.Invoke(false, e.Message);
            }
        });
    }
#endif

    void SetStatus(string s) { if (statusText) statusText.text = s; Debug.Log("[GPGS] " + s); }
    void Log(string s)       { Debug.Log(s); if (statusText) statusText.text = s; }
}

#else

// -------------------------------
// iOS / Editor / อื่น ๆ (dummy)
// -------------------------------
public class GpgsAuthService : MonoBehaviour
{
    public static GpgsAuthService I { get; private set; }

    [Header("Optional UI")]
    public TMPro.TextMeshProUGUI statusText;

    [Header("Boot options")]
    public bool silentSignInOnStart = false;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        Log("[GPGS] Skipped (not Android / PLAY_GAMES_V2 not defined).");
    }

    public bool IsAuthenticated => false;
    public string UserId => null;
    public string UserName => null;

    public IEnumerator SignInInteractive(Action<bool> onDone = null)
    {
        Log("[GPGS] SignInInteractive skipped (no GPGS on this platform).");
        onDone?.Invoke(false);
        yield break;
    }

    public IEnumerator RequestServerAuthCode(bool forceRefresh, Action<string> onResult)
    {
        onResult?.Invoke(null);
        yield break;
    }

    public void SignOutNotice() => Log("[GPGS] Sign-out not supported on this platform.");

#if FIREBASE_ENABLED
    public IEnumerator LinkFirebaseCurrentUser(Action<bool, string> onDone)
    {
        onDone?.Invoke(false, "GPGS not available on this platform.");
        yield break;
    }
#endif

    void SetStatus(string s) { if (statusText) statusText.text = s; Debug.Log("[GPGS] " + s); }
    void Log(string s)       { Debug.Log(s); if (statusText) statusText.text = s; }
}

#endif