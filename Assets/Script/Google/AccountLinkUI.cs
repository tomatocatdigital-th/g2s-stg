using System.Collections;
using UnityEngine;
using UnityEngine.UI;

#if FIREBASE_ENABLED
using Firebase.Auth;
#endif

#if PLAY_GAMES_V2 && UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

public class AccountLinkUI : MonoBehaviour
{
    [Header("UI")]
    public TMPro.TextMeshProUGUI linkStatusText;
    public Button linkButton;
    public TMPro.TextMeshProUGUI debugText;

    void OnEnable()
    {
        RefreshUI();
    }

    // -------------------------------
    // REFRESH UI
    // -------------------------------
    public void RefreshUI()
    {
#if FIREBASE_ENABLED
        var auth = FirebaseAuth.DefaultInstance;
        var user = auth?.CurrentUser;
        bool linked = (user != null && !user.IsAnonymous);

        if (linkStatusText)
            linkStatusText.text = linked ? "Linked to Google" : "Link account";

        if (linkButton)
            linkButton.gameObject.SetActive(!linked);

        Log(linked
            ? $"[Account] Linked (uid={user?.UserId})"
            : "[Account] Guest (not linked)");

        LogAuthState("[RefreshUI]");
#else
        // Firebase disabled → always local only
        if (linkStatusText) linkStatusText.text = "Local only";
        if (linkButton) linkButton.gameObject.SetActive(false);
        Log("[Account] FIREBASE NOT ENABLED");
#endif
    }

    // -------------------------------
    // ON BUTTON PRESS
    // -------------------------------
    public void PressLinkAccount()
    {
        StartCoroutine(LinkRoutine());
    }

    // -------------------------------
    // MAIN LINK ROUTINE
    // -------------------------------
    IEnumerator LinkRoutine()
    {
#if !(FIREBASE_ENABLED) || !(PLAY_GAMES_V2) || !UNITY_ANDROID
        Log("[Link] Firebase/GPGS v2 not available on this platform.");
        yield break;
#else
        if (linkButton) linkButton.interactable = false;
        Log("[Link] Starting link flow...");

        // -------------------------------
        // 1) GPGS INTERACTIVE SIGN-IN
        // -------------------------------
        bool signInDone = false;
        bool signInOK   = false;

        Log("[Link] Showing Google Play Games UI...");

        PlayGamesPlatform.Instance.ManuallyAuthenticate(status =>
        {
            signInOK = (status == SignInStatus.Success);
            Log("[Link] GPGS Result = " + status);
            signInDone = true;
        });

        while (!signInDone) yield return null;

        if (!signInOK)
        {
            Log("[Link] User canceled / GPGS sign-in failed.");
            if (linkButton) linkButton.interactable = true;
            yield break;
        }

        // -------------------------------
        // 2) REQUEST SERVER AUTH CODE
        // -------------------------------
        Log("[Link] Request Server Auth Code...");
        bool codeDone = false;
        string serverCode = null;

        PlayGamesPlatform.Instance.RequestServerSideAccess(false, code =>
        {
            serverCode = code;
            codeDone   = true;
        });

        while (!codeDone) yield return null;

        if (string.IsNullOrEmpty(serverCode))
        {
            Log("[Link] ERROR: Empty serverAuthCode → Check OAuth Web Client ID / SHA-1");
            if (linkButton) linkButton.interactable = true;
            yield break;
        }

        Log("[Link] Got serverAuthCode length=" + serverCode.Length);

        // -------------------------------
        // 3) LINK TO FIREBASE
        // -------------------------------
        var auth = FirebaseAuth.DefaultInstance;
        var currentUser = auth.CurrentUser;

        LogAuthState("[Link] Before Firebase link");
        Log("[Link] currentUser=" + (currentUser == null ? "NULL" : currentUser.UserId)
            + ", anon=" + (currentUser?.IsAnonymous ?? false));

        Log("[Link] Linking Firebase account...");
        var cred = PlayGamesAuthProvider.GetCredential(serverCode);

        System.Threading.Tasks.Task linkTask;
        if (currentUser != null && currentUser.IsAnonymous)
        {
            // อัปเกรดจาก guest → Google Play (รักษา UID)
            Log("[Link] Using LinkWithCredentialAsync (upgrade guest)");
            linkTask = currentUser.LinkWithCredentialAsync(cred);
        }
        else
        {
            // เข้าด้วย Google Play โดยตรง
            Log("[Link] Using SignInWithCredentialAsync");
            linkTask = auth.SignInWithCredentialAsync(cred);
        }

        while (!linkTask.IsCompleted) yield return null;

        if (linkTask.Exception != null)
        {
            var ex = linkTask.Exception;
            Log("[Link] FIREBASE LINK FAILED:\n" + ex);
            LogAuthState("[Link] After Firebase link FAILED");
        }
        else
        {
            Log("[Link] LINK SUCCESS! uid=" + auth.CurrentUser?.UserId);
            LogAuthState("[Link] After Firebase link SUCCESS");
        }

        RefreshUI();

        if (linkButton) linkButton.interactable = true;
        yield break;
#endif
    }

#if FIREBASE_ENABLED
    // -------------------------------
    // AUTH STATE DEBUG
    // -------------------------------
    void LogAuthState(string tag)
    {
        var auth = FirebaseAuth.DefaultInstance;
        var u = auth?.CurrentUser;

        if (u == null)
        {
            Log($"{tag} AuthState: user=NULL");
            return;
        }

        // รวบรวม providerIds (google.com, playgames.google.com ฯลฯ)
        string providers = "";
        foreach (var p in u.ProviderData)
        {
            providers += p.ProviderId + " ";
        }

        Log($"{tag} AuthState: uid={u.UserId}, anon={u.IsAnonymous}, provider={providers}");
    }
#endif

    // -------------------------------
    // DEBUG TEXT + LOG
    // -------------------------------
    void Log(string msg)
    {
        Debug.Log(msg);
        if (debugText) debugText.text = msg;
    }
}