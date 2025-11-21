using System.Threading.Tasks;
using UnityEngine;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager I { get; private set; }

    public PlayerData Data { get; private set; }
    public string UserId { get; private set; } = "local_guest";

    IPlayerDataStorage local;
    IPlayerDataStorage cloud;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        local = new LocalPlayerDataStorage();
#if FIREBASE_ENABLED
        cloud = new FirestorePlayerDataStorage();
#endif

        Data = PlayerData.Default();
    }

    public async Task InitializeAsync()
    {
        // load local 100%
        Data = await local.LoadAsync();

#if FIREBASE_ENABLED
        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (user != null)
        {
            UserId = user.UserId;
            await SyncWithCloudAsync();
        }
#endif
    }

    public async Task SyncWithCloudAsync()
    {
#if FIREBASE_ENABLED
        var cloudData = await cloud.LoadAsync(UserId);
        if (cloudData != null)
            MergePreferCloud(Data, cloudData);

        await local.SaveAsync(Data);
        await cloud.SaveAsync(Data, UserId);
#endif
    }

    void MergePreferCloud(PlayerData local, PlayerData cloud)
    {
        if (!string.IsNullOrEmpty(cloud.playerName))
            local.playerName = cloud.playerName;

        if (cloud.wallet != null)
        {
            local.wallet.coin = Mathf.Max(local.wallet.coin, cloud.wallet.coin);
            local.wallet.gem  = Mathf.Max(local.wallet.gem, cloud.wallet.gem);
        }

        if (cloud.progress != null)
        {
            local.progress.topScore = Mathf.Max(local.progress.topScore, cloud.progress.topScore);
            local.progress.topWave  = Mathf.Max(local.progress.topWave, cloud.progress.topWave);
        }

        if (cloud.settings != null)
        {
            local.settings.lang  = cloud.settings.lang ?? local.settings.lang;
            local.settings.sfx   = cloud.settings.sfx;
            local.settings.music = cloud.settings.music;
        }

        if (cloud.device != null)
        {
            local.device.os    = cloud.device.os    ?? local.device.os;
            local.device.model = cloud.device.model ?? local.device.model;
            local.device.ver   = cloud.device.ver   ?? local.device.ver;
        }
    }

    public async Task SaveRunAsync(int score, int wave, int duration, int version)
    {
        Data.progress.lastScore = score;
        Data.progress.lastWave  = wave;

        if (score > Data.progress.topScore) Data.progress.topScore = score;
        if (wave  > Data.progress.topWave)  Data.progress.topWave  = wave;

        int earned = Mathf.FloorToInt(score / 10f);
        Data.wallet.coin += earned;

        Data.lastLoginAt = PlayerData.NowMs();

        await local.SaveAsync(Data);

#if FIREBASE_ENABLED
        if (!string.IsNullOrEmpty(UserId))
        {
            await cloud.SaveAsync(Data, UserId);
            await cloud.AddRunResultAsync(UserId, score, wave, duration, version);
        }
#endif
    }

    public async Task AddCoinsAsync(int add)
    {
        Data.wallet.coin += add;
        Data.lastLoginAt = PlayerData.NowMs();

        await local.SaveAsync(Data);

#if FIREBASE_ENABLED
        if (!string.IsNullOrEmpty(UserId))
            await cloud.SaveAsync(Data, UserId);
#endif
    }
}