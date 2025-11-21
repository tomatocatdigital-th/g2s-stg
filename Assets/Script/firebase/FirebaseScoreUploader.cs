// FirebaseScoreUploader.cs
using UnityEngine;

public static class FirebaseScoreUploader
{
    // score = คะเเนนรอบนี้, best = best ใหม่, earned = เหรียญที่ได้ในรอบนี้,
    // wallet = เหรียญรวมหลังบวก
    public static void TryUpload(int score, int best, int earned, int wallet)
    {
#if FIREBASE_ENABLED
        // ตัวอย่างโค้ดเรียก service จริง
        //FirebaseScoreService.I?.SubmitScoreAndCoins(score, best, earned, wallet);
#else
        Debug.Log($"[LocalUpload] score={score} best={best} earned={earned} wallet={wallet}");
#endif
    }

    
}