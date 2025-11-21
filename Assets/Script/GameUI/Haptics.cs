// Assets/Scripts/Game System/Haptics.cs
using UnityEngine;

public static class Haptics
{
    /// <summary>
    /// สั่นสั้น ๆ (เคารพ SettingsManager: hapticsOn)
    /// </summary>
    public static void Vibrate()
    {
        if (SettingsManager.I && !SettingsManager.I.Data.hapticsOn) return;

#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            // ใช้ Vibrator (API 26+ มี VibrationEffect)
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var context = activity.Call<AndroidJavaObject>("getApplicationContext"))
            using (var vibrator = context.Call<AndroidJavaObject>("getSystemService", "vibrator"))
            {
                if (vibrator == null) return;

                int sdk = new AndroidJavaClass("android.os.Build$VERSION")
                          .GetStatic<int>("SDK_INT");

                if (sdk >= 26)
                {
                    using (var vibEffectClass = new AndroidJavaClass("android.os.VibrationEffect"))
                    {
                        var effect = vibEffectClass.CallStatic<AndroidJavaObject>("createOneShot", 30, 255);
                        vibrator.Call("vibrate", effect);
                    }
                }
                else
                {
                    vibrator.Call("vibrate", 30L);
                }
            }
        }
        catch { /* เงียบไว้ถ้าบางเครื่องไม่มี vibrator */ }

#elif UNITY_IOS && !UNITY_EDITOR
        // บน iOS ใช้ Handheld.Vibrate ได้ (ระดับพื้นฐาน)
        Handheld.Vibrate();

#else
        // ใน Editor หรือแพลตฟอร์มอื่น: ไม่สั่น (กันคอมไพล์เออเรอร์)
        // Debug.Log("[Haptics] Vibrate (noop on this platform).");
#endif
    }
}