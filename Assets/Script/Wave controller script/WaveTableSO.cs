using UnityEngine;

[CreateAssetMenu(fileName = "WaveTable", menuName = "G2R/Wave Table")]
public class WaveTableSO : ScriptableObject
{
    [System.Serializable]
    public class WaveRow
    {
        [Tooltip("Wave index (เริ่ม 1)")]
        public int wave = 1;

        [Header("Pacing")]
        public float baseSpeed = 13f;        // ความเร็วฐานของเวฟนี้
        public int cartsInWave = 20;         // จำนวนคาร์ตที่จะเกิด
        public float spawnInterval = 3.0f;   // ระยะห่างการเกิด (วินาที)

        [Header("Lane mask (เลือกเลนที่เปิดในเวฟนี้)")]
        public bool laneLeft = true;
        public bool laneMid = true;
        public bool laneRight = true;

        [Header("Train size composition (weight %)")]
        [Range(0,100)] public int singleWeight = 70;
        [Range(0,100)] public int doubleWeight = 25;
        [Range(0,100)] public int tripleWeight = 5;

        [Header("Rewards/Perks (optional)")]
        public int bonusHearts = 0;
        public int extraCoins = 0;

        [Header("Break หลังจบเวฟนี้ (วินาที)")]
        public float postWaveBreak = 1.0f;

        public int TotalWeight => Mathf.Max(1, singleWeight + doubleWeight + tripleWeight);
    }

    public WaveRow[] rows;

    public int Count => rows != null ? rows.Length : 0;

    public WaveRow Get(int waveIndex1Based)
    {
        int idx = Mathf.Clamp(waveIndex1Based - 1, 0, Mathf.Max(0, Count - 1));
        return rows[idx];
    }
}