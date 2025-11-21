using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

/// <summary>
/// ศูนย์กลางการเขียน/อ่าน Firestore ของเกม
/// - เรียกใช้หลังจาก FirebaseInit เซ็นอินเรียบร้อยแล้ว
/// - เก็บ user doc, บันทึกรอบการเล่น, อัปเดตสถิติ, อัปเดตอัปเกรด, เปลี่ยนชื่อ
/// </summary>
public class FirestoreManager : MonoBehaviour
{
    public static FirestoreManager I { get; private set; }

    FirebaseFirestore db;
    FirebaseAuth auth;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        db = FirebaseFirestore.DefaultInstance;
        auth = FirebaseAuth.DefaultInstance;
        DontDestroyOnLoad(gameObject);
    }

    string Uid => auth.CurrentUser?.UserId;

    // ---------- Utilities ----------
    string SafeName(string name) => string.IsNullOrWhiteSpace(name) ? GenerateNickname() : name.Trim();

    string GenerateNickname()
    {
        var n = Random.Range(1000, 9999);
        return $"Player{n}";
    }

    // =========================================================
    // 1) Ensure user document (สร้างครั้งแรก + อัปเดต lastLoginAt)
    // =========================================================
    public async Task EnsureUserDocAsync()
    {
        if (string.IsNullOrEmpty(Uid)) return;

        // ถ้า Auth ยังไม่มีชื่อ ให้สุ่มตั้งชื่อครั้งแรก
        if (string.IsNullOrEmpty(auth.CurrentUser.DisplayName))
            await auth.CurrentUser.UpdateUserProfileAsync(new UserProfile { DisplayName = GenerateNickname() });

        var userRef = db.Collection("users").Document(Uid);
        var snap = await userRef.GetSnapshotAsync();

        if (!snap.Exists)
        {
            var data = new Dictionary<string, object>
            {
                { "displayName", SafeName(auth.CurrentUser.DisplayName) },
                { "createdAt",  Timestamp.GetCurrentTimestamp() },
                { "lastLoginAt", Timestamp.GetCurrentTimestamp() },
                { "renameLeft",  1 },

                { "stats", new Dictionary<string, object> {
                    { "totalRuns", 0 },
                    { "bestScore", 0 },
                    { "totalCoins", 0 },
                    { "totalPlaySeconds", 0 }
                }},

                { "upgrades", new Dictionary<string, object> {
                    { "speedLv", 0 },
                    { "drainLv", 0 },
                    { "energyLv", 0 }
                }}
            };

            await userRef.SetAsync(data);
            Debug.Log("🧾 Created new user doc");
        }
        else
        {
            await userRef.UpdateAsync(new Dictionary<string, object> {
                { "lastLoginAt", Timestamp.GetCurrentTimestamp() }
            });
        }
    }

    // =========================================================
    // 2) Save one run + update aggregated stats (Transaction)
    // =========================================================
    /// <summary>
    /// บันทึกรอบการเล่น และอัปเดตสถิติรวมแบบ atomic
    /// </summary>
    public async Task SaveRunAndUpdateAsync(
        int score, int coins, int secondsPlayed,
        int speedLv, int drainLv, int energyLv,
        string mapId = "stage1", string mode = "classic")
    {
        if (string.IsNullOrEmpty(Uid)) return;

        var userRef = db.Collection("users").Document(Uid);
        var runRef  = userRef.Collection("runs").Document(); // auto-id

        await db.RunTransactionAsync(async tr =>
        {
            // write run (ใช้ Dictionary ทุกชั้น)
            tr.Set(runRef, new Dictionary<string, object>
            {
                { "score", score },
                { "coins", coins },
                { "secondsPlayed", secondsPlayed },
                { "mapId", mapId },
                { "mode", mode },
                { "endedAt", Timestamp.GetCurrentTimestamp() },
                { "upgradesSnapshot", new Dictionary<string, object> {
                    { "speedLv", speedLv },
                    { "drainLv", drainLv },
                    { "energyLv", energyLv }
                }}
            });

            // read current stats
            var snap = await tr.GetSnapshotAsync(userRef);
            int best = 0, runs = 0, coinsTotal = 0, secsTotal = 0;

            if (snap.Exists)
            {
                // อ่าน nested field แบบปลอดภัย
                snap.TryGetValue("stats.bestScore", out best);
                snap.TryGetValue("stats.totalRuns", out runs);
                snap.TryGetValue("stats.totalCoins", out coinsTotal);
                snap.TryGetValue("stats.totalPlaySeconds", out secsTotal);
            }

            var newBest = Mathf.Max(best, score);

            // update stats + snapshot upgrades + lastRunAt
            tr.Update(userRef, new Dictionary<string, object>
            {
                { "lastRunAt", Timestamp.GetCurrentTimestamp() },
                { "stats", new Dictionary<string, object> {
                    { "bestScore", newBest },
                    { "totalRuns", runs + 1 },
                    { "totalCoins", coinsTotal + coins },
                    { "totalPlaySeconds", secsTotal + secondsPlayed }
                }},
                { "upgrades", new Dictionary<string, object> {
                    { "speedLv", speedLv },
                    { "drainLv",  drainLv },
                    { "energyLv", energyLv }
                }}
            });
        });

        Debug.Log("✅ Saved run & updated stats");
    }

    // =========================================================
    // 3) Update upgrades only (เช่นตอนผู้เล่นอัปจากเมนู)
    // =========================================================
    public async Task UpdateUpgradesAsync(int speedLv, int drainLv, int energyLv)
    {
        if (string.IsNullOrEmpty(Uid)) return;
        var userRef = db.Collection("users").Document(Uid);

        await userRef.UpdateAsync(new Dictionary<string, object> {
            { "upgrades", new Dictionary<string, object> {
                { "speedLv", speedLv },
                { "drainLv",  drainLv },
                { "energyLv", energyLv }
            }}
        });

        Debug.Log("🛠️ Upgrades updated");
    }

    // =========================================================
    // 4) Change player display name (ใช้โควตา renameLeft)
    // =========================================================
    public async Task<bool> TryChangeDisplayNameAsync(string newName)
    {
        if (string.IsNullOrEmpty(Uid)) return false;
        newName = SafeName(newName);

        var userRef = db.Collection("users").Document(Uid);

        bool success = false;
        await db.RunTransactionAsync(async tr =>
        {
            var snap = await tr.GetSnapshotAsync(userRef);
            int left = 0; if (snap.Exists) snap.TryGetValue("renameLeft", out left);
            if (left <= 0) return;

            tr.Update(userRef, new Dictionary<string, object> {
                { "displayName", newName },
                { "renameLeft", left - 1 }
            });

            success = true;
        });

        if (success)
            await auth.CurrentUser.UpdateUserProfileAsync(new UserProfile { DisplayName = newName });

        return success;
    }
}