using System.Collections;
using UnityEngine;

#if FIREBASE_ENABLED
using Firebase.Auth;
#endif

#if PLAY_GAMES_v2 && UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

public class GpgsBootstrap : MonoBehaviour
{
    public TMPro.TextMeshProUGUI statusText;

    void Start() => StartCoroutine(BootGpgs());

    IEnumerator BootGpgs()
    {
#if !PLAY_GAMES || !UNITY_ANDROID || UNITY_EDITOR
        Log("[GPGS] Skipped (not Android target or PLAY_GAMES undefined).");
        yield break;
#else

    #if PLAY_GAMES_V2
        // -------- v2 path (callback = bool), ไม่มี PlayGamesClientConfiguration --------
        Log("[GPGS] Init v2...");
        PlayGamesPlatform.Activate();

        bool done = false;
        PlayGamesPlatform.Instance.Authenticate(success =>
        {
            Log(success ? "[GPGS] Sign-in success (v2)" : "[GPGS] Sign-in failed (v2)");
            done = true;
        });
        while (!done) yield return null;

        var u2 = PlayGamesPlatform.Instance?.localUser;
        Log($"[GPGS] user={u2?.userName} ({u2?.id})");
        yield break;

    #elif PLAY_GAMES_V1
        // -------- v1 legacy path (SignInStatus, InitializeInstance, Configuration) --------
        Log("[GPGS] Init v1...");

        var config = new PlayGamesClientConfiguration.Builder()
            .RequestServerAuthCode(false) // ต้องใส่เพื่อให้ GetServerAuthCode ใช้ได้
            .RequestEmail()
            .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();

        bool done = false;
        PlayGamesPlatform.Instance.Authenticate(
            SignInInteractivity.NoPrompt,
            status =>
            {
                Log($"[GPGS] Silent sign-in: {status}");
                done = true;
            });

        while (!done) yield return null;

        var u1 = PlayGamesPlatform.Instance?.localUser;
        Log($"[GPGS] user={u1?.userName} ({u1?.id})");
        yield break;

    #else
        // ถ้าไม่ได้เซ็ต V1 หรือ V2 อย่างใดอย่างหนึ่ง
        Log("[GPGS] Please define PLAY_GAMES_V1 or PLAY_GAMES_V2 in Player Settings.");
        yield break;
    #endif

#endif
    }

    void Log(string s) { Debug.Log(s); if (statusText) statusText.text = s; }
}