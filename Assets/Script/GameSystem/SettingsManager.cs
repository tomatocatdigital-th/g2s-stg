using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager I { get; private set; }
    const string KEY = "G2R_SETTINGS_V1";

    [Header("Refs")]
    [Tooltip("AudioSource เพลงพื้นหลังหลักของเกม (ถ้ามี)")]
    public AudioSource musicSource;   // ลาก BGM หลักมาใส่

    public SettingsData Data { get; private set; } = new SettingsData();

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        if (!musicSource)
        {
            var go = new GameObject("BGM_Player");
            go.transform.SetParent(transform, worldPositionStays:false);
            musicSource = go.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = true; 
        }
        Load();
        ApplyAll();
    }

    // ---------- Load/Save ----------
    public void Load()
    {
        if (PlayerPrefs.HasKey(KEY))
        {
            var json = PlayerPrefs.GetString(KEY, "{}");
            try { Data = JsonUtility.FromJson<SettingsData>(json) ?? new SettingsData(); }
            catch { Data = new SettingsData(); }
        }
        else Data = new SettingsData();
    }

    public void Save()
    {
        var json = JsonUtility.ToJson(Data);
        PlayerPrefs.SetString(KEY, json);
        PlayerPrefs.Save();
    }

    // ---------- Apply ----------
    public void ApplyAll() { ApplyAudio(); }

    void ApplyAudio()
    {
        if (musicSource) musicSource.volume = Mathf.Clamp01(Data.masterMusic);
        if (SoundManager.I) SoundManager.I.SetMasterSfx(Mathf.Clamp01(Data.masterSfx));
    }

    // ---------- Setters (UI เรียก) ----------
    public void SetMusic(float v)  { Data.masterMusic = Mathf.Clamp01(v); ApplyAudio(); Save(); }
    public void SetSfx(float v)    { Data.masterSfx   = Mathf.Clamp01(v); ApplyAudio(); Save(); }
    public void SetHaptics(bool on){ Data.hapticsOn   = on;               Save(); }

    // ใช้ตอนต้องการค่าเริ่มใหม่
    public void ResetToDefault()
    {
        Data = new SettingsData();
        ApplyAll();
        Save();
    }
}