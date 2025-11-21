using UnityEngine;
using System;
using System.Threading.Tasks;
using GooglePlayGames;
using GooglePlayGames.BasicApi; // ยังใช้เพื่อเอา SignInStatus
using Firebase;
using Firebase.Auth;

public class GpgsBoot : MonoBehaviour
{
    static bool inited;
    FirebaseAuth auth;

    async void Start()
    {
        await InitAndSignInAsync();
    }

    private async Task InitAndSignInAsync()
    {
        // 0) Firebase deps
        var dep = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dep != DependencyStatus.Available)
        {
            Debug.LogError($"[Firebase] Dependencies not available: {dep}");
            return;
        }
        auth = FirebaseAuth.DefaultInstance;

        // 1) Init GPGS v2 (ไม่มี InitializeInstance/ClientConfiguration แล้ว)
        if (!inited)
        {
            PlayGamesPlatform.DebugLogEnabled = true;
            PlayGamesPlatform.Activate();
            inited = true;
        }

        // 2) Sign-in (หุ้ม callback เป็น Task)
        var status = await AuthenticateAsync();
        if (status != SignInStatus.Success)
        {
            Debug.LogWarning($"[GPGS] Sign-in failed: {status}");
            return;
        }
        Debug.Log("[GPGS] Signed in.");

        // 3) ขอ server auth code
        string serverAuthCode = await RequestServerAuthCodeAsync();
        if (string.IsNullOrEmpty(serverAuthCode))
        {
            Debug.LogWarning("[GPGS] No server auth code. เช็ก package name + SHA-1 ใน Firebase/Play Console");
            return;
        }

        // 4) เข้าสู่ระบบ Firebase ด้วย PlayGamesAuthProvider
        await SignInToFirebaseAsync(serverAuthCode);
    }

    private Task<SignInStatus> AuthenticateAsync()
    {
        var tcs = new TaskCompletionSource<SignInStatus>();
        // v2 มีโอเวอร์โหลดแบบกำหนด interactivity ได้เช่น:
        // PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptOnce, status => { ... });
        PlayGamesPlatform.Instance.Authenticate(status =>
        {
            tcs.TrySetResult(status);
        });
        return tcs.Task;
    }

    private Task<string> RequestServerAuthCodeAsync()
    {
        var tcs = new TaskCompletionSource<string>();
        PlayGamesPlatform.Instance.RequestServerSideAccess(false, code =>
        {
            tcs.TrySetResult(code); // อาจได้ "" ถ้าคอนฟิกไม่ครบ
        });
        return tcs.Task;
    }

    private async Task SignInToFirebaseAsync(string serverAuthCode)
    {
        try
        {
            var cred = PlayGamesAuthProvider.GetCredential(serverAuthCode);
            var user = await auth.SignInWithCredentialAsync(cred);
            Debug.Log($"[Firebase] Signed in via GPGS: {user.UserId}");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[Firebase] Sign-in failed: {e}");
        }
    }
    
}