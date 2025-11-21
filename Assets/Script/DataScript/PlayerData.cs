using Firebase.Firestore;
using UnityEngine;
using System;

[FirestoreData]
[Serializable]
public class PlayerData
{
    [FirestoreProperty] public string playerName { get; set; }
    [FirestoreProperty] public string localId    { get; set; }

    [FirestoreProperty] public Wallet wallet     { get; set; } = new Wallet();
    [FirestoreProperty] public Progress progress { get; set; } = new Progress();
    [FirestoreProperty] public Settings settings { get; set; } = new Settings();
    [FirestoreProperty] public DeviceInfo device { get; set; } = new DeviceInfo();

    [FirestoreProperty] public long createdAt    { get; set; }
    [FirestoreProperty] public long lastLoginAt  { get; set; }

    public static PlayerData Default()
    {
        return new PlayerData
        {
            playerName  = "Player" + UnityEngine.Random.Range(1000, 9999),
            createdAt   = NowMs(),
            lastLoginAt = NowMs(),
            wallet      = new Wallet { coin = 0, gem = 0 },
            progress    = new Progress { lastWave = 0, topWave = 0, lastScore = 0, topScore = 0 },
            settings    = new Settings { lang = "th", sfx = true, music = true },
            device      = new DeviceInfo { os = SystemInfo.operatingSystem, model = SystemInfo.deviceModel, ver = Application.version }
        };
    }

    public static long NowMs()
    {
        var epoch = new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc);
        return (long)(DateTime.UtcNow - epoch).TotalMilliseconds;
    }
}

[FirestoreData, Serializable]
public class Wallet
{
    [FirestoreProperty] public int coin { get; set; }
    [FirestoreProperty] public int gem  { get; set; }
}

[FirestoreData, Serializable]
public class Progress
{
    [FirestoreProperty] public int lastWave  { get; set; }
    [FirestoreProperty] public int topWave   { get; set; }
    [FirestoreProperty] public int lastScore { get; set; }
    [FirestoreProperty] public int topScore  { get; set; }
}

[FirestoreData, Serializable]
public class Settings
{
    [FirestoreProperty] public string lang { get; set; }
    [FirestoreProperty] public bool sfx    { get; set; }
    [FirestoreProperty] public bool music  { get; set; }
}

[FirestoreData, Serializable]
public class DeviceInfo
{
    [FirestoreProperty] public string os    { get; set; }
    [FirestoreProperty] public string model { get; set; }
    [FirestoreProperty] public string ver   { get; set; }
}