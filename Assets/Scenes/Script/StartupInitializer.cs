using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

#if FIREBASE_ENABLED
using Firebase;
using Firebase.Auth;
#endif

// เลือก API ของ Google Play Games:
// - ถ้าใช้ "GPGS เวอร์ชัน 2 (ใหม่)" ให้ใส่ Scripting Define Symbols: PLAY_GAMES_V2
// - ถ้าใช้ปลั๊กอิน GPGS แบบเดิม ให้ใส่: PLAY_GAMES_LEGACY
// (อย่างน้อยต้องมีอันใดอันหนึ่งสำหรับ Android)
#if PLAY_GAMES_LEGACY
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;
#endif

[DefaultExecutionOrder(-1000)]
public class StartupInitializer : MonoBehaviour
{
    [Header("Flow")]
    [Tooltip("ชื่อซีนถัดไปหลังจาก init สำเร็จ")]
    public string nextSceneName = "Home";

    [Tooltip("เวลาขั้นต่ำที่หน้าโหลดจะแสดง (กันจอดับไวเกิน)")]
    public float minSplashSeconds = 1.0f;

    [Header("Options")]
    [Tooltip("ถ้าเปิด จะขอ ServerAuthCode เพื่อนำไปผูก Firebase Auth ด้วย GPGS")]
    public bool requestServerAuthCode = true;

    [Tooltip("ล็อก verbose เพื่อดีบักบนเครื่องจริง")]
    public bool enableVerboseLog = true;

#if FIREBASE_ENABLED
    FirebaseAuth _auth;
#endif

    static bool _booted; // กันบูตซ้ำเมื่อเปลี่ยนซีนเร็ว

    void Awake()
    {
        if (_booted) { Destroy(gameObject); return; }
        _booted = true;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        StartCoroutine(BootFlow());
    }

    IEnumerator BootFlow()
    {
        float start = Time.realtimeSinceStartup;
        if (enableVerboseLog) Debug.Log("[BOOT] Start boot flow");

        // 1) Init Google Play Games (เฉพาะ Android)
#if UNITY_ANDROID && (PLAY_GAMES_V2 || PLAY_GAMES_LEGACY)
        yield return StartCoroutine(InitGPGS());
#else
        if (enableVerboseLog) Debug.Log("[BOOT] Skip GPGS (not Android or no symbols)");
#endif

        // 2) Init Firebase (ถ้ามี)
#if FIREBASE_ENABLED
        yield return StartCoroutine(InitFirebase());
#else
        if (enableVerboseLog) Debug.Log("[BOOT] Skip Firebase (FIREBASE_ENABLED not defined)");
#endif

        // 3) รอให้หน้าโหลดแสดงอย่างน้อย N วิ
        float elapsed = Time.realtimeSinceStartup - start;
        if (elapsed < minSplashSeconds)
            yield return new WaitForSecondsRealtime(minSplashSeconds - elapsed);

        if (enableVerboseLog) Debug.Log("[BOOT] Done. Loading next scene: " + nextSceneName);

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }

#if UNITY_ANDROID && (PLAY_GAMES_V2 || PLAY_GAMES_LEGACY)
    IEnumerator InitGPGS()
    {
        if (enableVerboseLog) Debug.Log("[BOOT][GPGS] Initializing...");

#if PLAY_GAMES_LEGACY
        // ====== ปลั๊กอินแบบเดิม ======
        var configBuilder = new PlayGamesClientConfiguration.Builder();
        if (requestServerAuthCode)
            configBuilder = configBuilder.RequestServerAuthCode(false); // false=ไม่ force refresh token ทุกครั้ง

        var config = configBuilder.Build();
        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = enableVerboseLog;
        PlayGamesPlatform.Activate();

        bool done = false;
        Social.localUser.Authenticate(success =>
        {
            if (enableVerboseLog) Debug.Log("[BOOT][GPGS-LEGACY] SignIn: " + success);
            done = true;
        });

        // เผื่อบางดีไวซ์ authenticate async ช้า
        float timeout = 10f;
        while (!done && timeout > 0f) { timeout -= Time.unscaledDeltaTime; yield return null; }
        if (!done) Debug.LogWarning("[BOOT][GPGS-LEGACY] SignIn timeout");

#elif PLAY_GAMES_V2
        // ====== GPGS v2 ======
        // หมายเหตุ: ชื่อ namespace/method อาจต่างไปตามแพ็กเกจที่คุณใช้
        // ด้านล่างเป็น pattern ทั่วไป: เรียก Initialize/SignIn แบบ async แล้วรอผล
        bool signedIn = false;
        string warn = null;

        // ตัวอย่างโค้ดกึ่งจำลอง (เพราะ API ของ v2 แตกต่างกันตามแพ็กเกจ)
        // แนะนำอแดปเตอร์ของคุณเอง เช่น GpgsV2Wrapper.Initialize/SignIn
        try
        {
            if (enableVerboseLog) Debug.Log("[BOOT][GPGS-V2] Init...");
            // TODO: เรียกฟังก์ชัน Init ของ GPGS v2 ที่คุณใช้
            // ex: await PlayGamesClient.Initialize();

            if (enableVerboseLog) Debug.Log("[BOOT][GPGS-V2] SignIn...");
            // TODO: เรียก SignIn (silent → interactive fallback)
            // ex: var result = await PlayGamesClient.SignInAsync();
            // signedIn = result.Success;

            // ชั่วคราวให้ผ่าน (คุณค่อยแทนที่ด้วย API จริงของ v2)
            signedIn = true;
        }
        catch (System.Exception e)
        {
            warn = e.Message;
        }
        yield return new WaitForSecondsRealtime(0.25f);

        if (enableVerboseLog) Debug.Log("[BOOT][GPGS-V2] SignIn: " + signedIn + (warn != null ? $" ({warn})" : ""));
#endif

        yield break;
    }
#endif // UNITY_ANDROID...

#if FIREBASE_ENABLED
    IEnumerator InitFirebase()
    {
        if (enableVerboseLog) Debug.Log("[BOOT][FB] Initializing Firebase...");

        var task = FirebaseApp.CheckAndFixDependenciesAsync();
        while (!task.IsCompleted) yield return null;

        if (task.Result != DependencyStatus.Available)
        {
            Debug.LogWarning("[BOOT][FB] Firebase dependencies not available: " + task.Result);
            yield break;
        }

        _auth = FirebaseAuth.DefaultInstance;

#if UNITY_ANDROID && (PLAY_GAMES_V2 || PLAY_GAMES_LEGACY)
        // ถ้าต้องการผูก Auth ด้วย GPGS (ดีกว่า Anonymous)
        if (requestServerAuthCode)
        {
            string serverAuthCode = null;

    #if PLAY_GAMES_LEGACY
            // legacy plugin
            try
            {
                serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
            }
            catch { }
    #elif PLAY_GAMES_V2
            // TODO: ดึง Server Auth Code จาก GPGS v2 ตาม API ของแพ็กเกจคุณ
            // serverAuthCode = await PlayGamesClient.GetServerAuthCodeAsync();
    #endif

            if (!string.IsNullOrEmpty(serverAuthCode))
            {
                var cred = GoogleAuthProvider.GetCredential(null, serverAuthCode);
                var signTask = _auth.SignInWithCredentialAsync(cred);
                while (!signTask.IsCompleted) yield return null;

                if (signTask.Exception != null)
                {
                    Debug.LogWarning("[BOOT][FB] SignInWithCredential failed, fallback to anonymous. " + signTask.Exception.Message);
                    yield return StartCoroutine(FirebaseAnon());
                }
                else
                {
                    if (enableVerboseLog) Debug.Log("[BOOT][FB] Firebase signed-in via GPGS: " + _auth.CurrentUser?.UserId);
                }
            }
            else
            {
                // ไม่มี auth code → ลอง anonymous
                yield return StartCoroutine(FirebaseAnon());
            }
        }
        else
        {
            // ไม่ขอ server auth code → anonymous ไปก่อน
            yield return StartCoroutine(FirebaseAnon());
        }
#else
        // แพลตฟอร์มอื่น → anonymous
        yield return StartCoroutine(FirebaseAnon());
#endif
        yield break;
    }

    IEnumerator FirebaseAnon()
    {
        var t = _auth.SignInAnonymouslyAsync();
        while (!t.IsCompleted) yield return null;

        if (t.Exception != null)
            Debug.LogWarning("[BOOT][FB] Anonymous sign-in failed: " + t.Exception.Message);
        else if (enableVerboseLog)
            Debug.Log("[BOOT][FB] Firebase signed-in (anon): " + _auth.CurrentUser?.UserId);
    }
#endif
}