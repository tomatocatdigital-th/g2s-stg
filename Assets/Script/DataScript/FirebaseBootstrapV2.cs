using Firebase;
using Firebase.Auth;
using UnityEngine;
using System.Threading.Tasks;

public class FirebaseBootstrapV2 : MonoBehaviour
{
    async void Start()
    {
        Debug.Log("<color=#8be9fd>[Firebase]</color> Checking dependencies...");

        var dep = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dep != DependencyStatus.Available)
        {
            Debug.LogError($"<color=#ff5555>[Firebase]</color> Dependencies not available: {dep}");
            return;
        }

        Debug.Log("<color=#50fa7b>[Firebase]</color> Dependencies OK ✅");

        FirebaseAuth auth = FirebaseAuth.DefaultInstance;

        if (auth.CurrentUser != null)
        {
            Debug.Log($"<color=#f1fa8c>[Firebase]</color> Already signed in as: {auth.CurrentUser.UserId}");
        }
        else
        {
            Debug.Log("<color=#8be9fd>[Firebase]</color> Signing in anonymously...");
            try
            {
                var result = await auth.SignInAnonymouslyAsync();
                Debug.Log($"<color=#50fa7b>[Firebase]</color> Sign-in success ✅ UID: {result.User.UserId}");
                Debug.Log($"<color=#bd93f9>Provider:</color> {result.User.ProviderId}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"<color=#ff5555>[Firebase]</color> Sign-in failed ❌: {ex.Message}");
                return;
            }
        }

        if (PlayerDataManager.I != null)
        {
            Debug.Log("<color=#8be9fd>[Firebase]</color> Initializing PlayerDataManager...");
            await PlayerDataManager.I.InitializeAsync();
            Debug.Log($"<color=#50fa7b>[PlayerData]</color> Ready ✅ | Coins: {PlayerDataManager.I.Data.wallet.coin}");
        }
        else
        {
            Debug.LogWarning("<color=#ffb86c>[PlayerData]</color> PlayerDataManager not found in scene!");
        }

        Debug.Log("<color=#50fa7b>[Firebase]</color> Bootstrap complete ✅");
    }
}