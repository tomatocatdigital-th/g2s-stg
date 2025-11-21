using UnityEngine;
using System.Collections;

public class WaveController : MonoBehaviour
{
    [Header("Config")]
    public WaveTableSO waveTable;

    [Header("Refs")]
    public CartSpawner spawner;     // ลากเข้ามา
    public SpeedManager speed;      // เซ็ต baseSpeed ต่อเวฟ
    public ScoreManager score;      // ให้รางวัลท้ายเวฟ (ถ้ามี)

    [Header("Control")]
    public bool autoStart = true;
    public float tailClearBuffer = 0.75f;  // กันชนท้ายเวฟเล็กน้อย

    public int CurrentWave { get; private set; } = 0;
    public bool IsRunning { get; private set; } = false;
    public bool Paused { get; private set; } = false;

    public WaveUIController waveUI;

    

    void Awake()
    {
        if (!spawner) spawner = FindObjectOfType<CartSpawner>();
    }

    void Start()
    {
        if (autoStart) StartWaves(1);
    }

    public void StartWaves(int startWave = 1)
    {
        // กันค้างจากรอบเก่า
        StopAllCoroutines();
        IsRunning = false;
        Paused = false;

        // รีเฟรชหา CartSpawner ใหม่ในฉากปัจจุบัน
        if (!spawner)
            spawner = FindObjectOfType<CartSpawner>();

        if (!spawner)
        {
            Debug.LogError("[Wave] CartSpawner not found in scene.");
            return;
        }

        if (!waveTable)
        {
            Debug.LogError("[Wave] WaveTable is missing.");
            return;
        }

        if (IsRunning) return;
        CurrentWave = Mathf.Max(1, startWave);
        StartCoroutine(RunWavesCo());
    }

    public void Pause() { Paused = true; }
    public void Resume() { Paused = false; }

    IEnumerator RunWavesCo()
    {
        if (!spawner)
        {
            Debug.LogError("[Wave] RunWavesCo abort: CartSpawner is null.");
            yield break;
        }
        IsRunning = true;
        

        while (CurrentWave <= waveTable.Count)
        {
            // ====== Load wave row ======
            if (waveUI) waveUI.ShowWave(CurrentWave);
            var row = waveTable.Get(CurrentWave);

            // 1) Base speed ต่อเวฟ
            if (speed) speed.SetBaseSpeed(row.baseSpeed);
            

            // 2) เปิดเลนตามเวฟ
            spawner.SetEnabledLaneMask(row.laneLeft, row.laneMid, row.laneRight);

            // 3) อนุญาตขนาดขบวนจาก weight (on/off)
            bool allowS = row.singleWeight > 0;
            bool allowD = row.doubleWeight > 0;
            bool allowT = row.tripleWeight > 0;
            spawner.SetAllowedTypes(allowS, allowD, allowT);

            // 4) ยิงตามจำนวน/interval (Wave-driven)
            for (int i = 0; i < row.cartsInWave; i++)
            {
                while (Paused) yield return null;

                int len = PickSize(row);
                bool ok = spawner.TrySpawnBySize(len);

                // ถ้า spawn ไม่ได้เพราะเลนติด ให้หน่วงสั้นๆ แล้วลองใหม่ (ไม่นับจำนวนเพิ่ม)
                if (!ok)
                {
                    yield return new WaitForSeconds(Mathf.Min(0.15f, row.spawnInterval * 0.3f));
                    i--; // retry slot นี้
                }
                else
                {
                    yield return new WaitForSeconds(row.spawnInterval);
                }
            }

            // 5) กันท้ายเวฟเล็กน้อย
            if (tailClearBuffer > 0f)
                yield return new WaitForSeconds(tailClearBuffer);

            // 6) แจกของรางวัลท้ายเวฟ
            if (row.bonusHearts > 0 && score) score.AddHearts(row.bonusHearts);
            if (row.extraCoins > 0 && score) score.AddCoins(row.extraCoins);

            // 7) พักหลังเวฟ — ควบคุมโดย WaveController (ตามโจทย์)
            if (row.postWaveBreak > 0f)
            {
                float t = 0f;
                while (t < row.postWaveBreak)
                {
                    while (Paused) yield return null;
                    t += Time.deltaTime;
                    yield return null;
                }
            }
            

            CurrentWave++;
        }

        IsRunning = false;
    }

    // เลือกขนาดขบวนตาม weight ของเวฟ
    int PickSize(WaveTableSO.WaveRow row)
    {
        int total = row.TotalWeight;
        int r = Random.Range(1, total + 1);
        if (r <= row.singleWeight) return 1;
        r -= row.singleWeight;
        if (r <= row.doubleWeight) return 2;
        return 3;
    }
    
}