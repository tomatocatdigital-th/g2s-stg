using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

public class LoginUI : MonoBehaviour
{
    [Header("UI References")]
    public Button googleLoginButton;
    public Button guestLoginButton;
    public GameObject loadingSpinner;   // วงล้อโหลด

    [Header("Account Status")]
    public TextMeshProUGUI statusText;  // แสดงสถานะบัญชี

    FirebaseAuth auth;

    void Awake()
    {
        // Init Firebase
        auth = FirebaseAuth.DefaultInstance;

        // Init GPGS v2 (ไม่ต้องมี config)
        PlayGamesPlatform.Activate();

        if (googleLoginButton) googleLoginButton.onClick.AddListener(OnGoogleLoginClicked);
        if (guestLoginButton)  guestLoginButton.onClick.AddListener(OnGuestLoginClicked);

#if UNITY_EDITOR
        if (googleLoginButton) googleLoginButton.interactable = false; // ใช้บนเครื่องจริงเท่านั้น
#endif
    }

    void Start()
    {
        UpdateAccountStatus();
    }

    // =====================================================
    // PLAY AS GUEST
    // =====================================================
    public async void OnGuestLoginClicked()
    {
        SetBusy(true);
        try
        {
            if (auth.CurrentUser == null)
                await auth.SignInAnonymouslyAsync();

            await FirestoreManager.I.EnsureUserDocAsync();
            UpdateAccountStatus();
            GoMainMenu();
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Guest login failed: {e.Message}");
        }
        finally { SetBusy(false); }
    }

    // =====================================================
    // LOGIN WITH GOOGLE (GPGS v2 + FIREBASE)
    // =====================================================
    public async void OnGoogleLoginClicked()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        SetBusy(true);
        try
        {
            // 1) GPGS sign-in (v2)
            var status = await AuthenticateGpgsAsync();
            if (status != SignInStatus.Success)
            {
                Debug.LogWarning($"⚠️ GPGS sign-in failed: {status}");
                return;
            }

            // 2) ขอ server-side auth code (ไม่ใช่ idToken)
            string serverCode = await RequestServerAuthCodeAsync(forceRefresh: true);
            if (string.IsNullOrEmpty(serverCode))
            {
                Debug.LogError("❌ Server auth code is empty.");
                return;
            }

            // 3) ทำ Firebase Credential จาก serverCode
            var credential = PlayGamesAuthProvider.GetCredential(serverCode);

            // 4) ถ้าเป็น guest ให้ link ก่อน; ถ้า link ไม่ได้ค่อย sign-in
            var user = auth.CurrentUser;
            if (user != null && user.IsAnonymous)
            {
                try
                {
                    await user.LinkWithCredentialAsync(credential);
                    Debug.Log("✅ Linked Google with existing guest UID.");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Link failed ({ex.Message}) → SignInWithCredential.");
                    await auth.SignInWithCredentialAsync(credential);
                }
            }
            else
            {
                await auth.SignInWithCredentialAsync(credential);
                Debug.Log("✅ Signed in with Google.");
            }

            await FirestoreManager.I.EnsureUserDocAsync();
            UpdateAccountStatus();
            GoMainMenu();
        }
        catch (Exception e)
        {
            Debug.LogError($"❌ Google login failed: {e.Message}");
        }
        finally { SetBusy(false); }
#else
        Debug.LogWarning("Google login ใช้ได้เฉพาะบน Android Device (ต้อง Build ลงเครื่องจริง)");
#endif
    }

    // ----------------- GPGS helpers -----------------
    private Task<SignInStatus> AuthenticateGpgsAsync()
    {
        var tcs = new TaskCompletionSource<SignInStatus>();
        PlayGamesPlatform.Instance.Authenticate(status => tcs.TrySetResult(status));
        return tcs.Task;
    }

    private Task<string> RequestServerAuthCodeAsync(bool forceRefresh)
    {
        var tcs = new TaskCompletionSource<string>();
        PlayGamesPlatform.Instance.RequestServerSideAccess(forceRefresh, code =>
        {
            tcs.TrySetResult(code); // code อาจเป็น null/empty ถ้าขอไม่สำเร็จ
        });
        return tcs.Task;
    }

    // ----------------- Account status UI -----------------
    void UpdateAccountStatus()
    {
        if (!statusText) return;

        var user = auth.CurrentUser;
        if (user == null)
        {
            statusText.text = "❌ Not signed in";
            statusText.color = Color.gray;
            return;
        }

        bool linkedGoogle = false;
        foreach (var p in user.ProviderData)
            if (p.ProviderId == "google.com") { linkedGoogle = true; break; }

        if (linkedGoogle)
        {
            statusText.text = "✅ Linked with Google";
            statusText.color = new Color(0.2f, 0.9f, 0.3f);
        }
        else if (user.IsAnonymous)
        {
            statusText.text = "☁️ Playing as Guest\nTap to Link Google";
            statusText.color = new Color(1f, 0.85f, 0.2f);
        }
        else
        {
            statusText.text = $"👤 {user.DisplayName}";
            statusText.color = Color.white;
        }
    }

    // ----------------- Utility -----------------
    void SetBusy(bool busy)
    {
        if (loadingSpinner) loadingSpinner.SetActive(busy);
        if (googleLoginButton) googleLoginButton.interactable = !busy;
        if (guestLoginButton)  guestLoginButton.interactable = !busy;
    }

    void GoMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
}