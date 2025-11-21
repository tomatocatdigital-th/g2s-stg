using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json; // ✅ ใช้ Newtonsoft แทน JsonUtility

public class LocalPlayerDataStorage : IPlayerDataStorage
{
    const string KEY = "playerData_v3"; // เปลี่ยน key ป้องกันข้อมูลเดิมที่ว่าง

    public PlayerData Load()
    {
        if (!PlayerPrefs.HasKey(KEY)) return PlayerData.Default();

        string json = PlayerPrefs.GetString(KEY);
        PlayerData data = null;

        try
        {
            data = JsonConvert.DeserializeObject<PlayerData>(json);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[LocalPlayerDataStorage] Deserialize error: " + ex.Message);
        }

        if (data == null || string.IsNullOrEmpty(data.playerName))
        {
            Debug.LogWarning("[LocalPlayerDataStorage] Data missing, creating default.");
            return PlayerData.Default();
        }

        return data;
    }

    public void Save(PlayerData data)
    {
        if (data == null) return;

        string json = JsonConvert.SerializeObject(data, Formatting.None);
        PlayerPrefs.SetString(KEY, json);
        PlayerPrefs.Save();
    }

    public async Task<PlayerData> LoadAsync(string uid = null)
        => await Task.FromResult(Load());

    public async Task SaveAsync(PlayerData data, string uid = null)
    {
        Save(data);
        await Task.CompletedTask;
    }

    public async Task AddRunResultAsync(string uid, int score, int wave, int duration, int version)
    {
        await Task.CompletedTask;
    }
}