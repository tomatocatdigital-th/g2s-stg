using System.Collections.Generic;
using System.Threading.Tasks;
#if FIREBASE_ENABLED
using Firebase.Firestore;
#endif

public class FirestorePlayerDataStorage : IPlayerDataStorage
{
#if FIREBASE_ENABLED
    FirebaseFirestore db;
#endif

    public FirestorePlayerDataStorage()
    {
#if FIREBASE_ENABLED
        db = FirebaseFirestore.DefaultInstance;
#endif
    }

    public async Task<PlayerData> LoadAsync(string uid = null)
    {
#if FIREBASE_ENABLED
        if (string.IsNullOrEmpty(uid)) return null;

        var doc = await db.Collection("playerData").Document(uid).GetSnapshotAsync();
        if (!doc.Exists) return null;

        return doc.ConvertTo<PlayerData>();
#else
        return null;
#endif
    }

    public async Task SaveAsync(PlayerData data, string uid = null)
    {
#if FIREBASE_ENABLED
        if (string.IsNullOrEmpty(uid) || data == null) return;

        var docRef = db.Collection("playerData").Document(uid);

        // ใช้ SetAsync โดยตรง (เพราะเราใช้ property + FirestoreProperty)
        await docRef.SetAsync(data, SetOptions.MergeAll);
#endif
    }

    public async Task AddRunResultAsync(string uid, int score, int wave, int duration, int version)
    {
#if FIREBASE_ENABLED
        var run = new Dictionary<string, object>
        {
            { "score", score },
            { "wave", wave },
            { "durationSec", duration },
            { "ver", version },
            { "createdAt", FieldValue.ServerTimestamp }
        };

        await db.Collection("playerData").Document(uid)
            .Collection("runs").AddAsync(run);
#endif
    }
}