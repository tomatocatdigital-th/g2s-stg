// WaveConfig.cs
using UnityEngine;

[CreateAssetMenu(menuName = "G2R/Wave Config Set")]
public class WaveConfig : ScriptableObject
{
    [Tooltip("ลิสต์เวฟที่ประกอบเป็น 1 รอบ")]
    public WaveDef[] waves;

    // ใช้ bitmask เปิดปิดเลน: Left, Mid, Right
    [System.Flags]
    public enum LaneMask { None = 0, Left = 1<<0, Mid = 1<<1, Right = 1<<2, All = Left | Mid | Right }

    [System.Serializable]
    public class WaveDef
    {
        [Header("Meta")]
        public string name = "Wave";

        [Header("Target / Win Condition")]
        [Tooltip("จำนวนการส่งถูกที่ต้องทำให้ครบเพื่อผ่านเวฟ")]
        public int targetDeliver = 10;

        [Tooltip("กำหนดเลนที่อนุญาตในเวฟนี้ (เวฟง่ายเปิด 2 เลน, เวฟยากเปิด 3 เลน)")]
        public LaneMask enabledLanes = LaneMask.Left | LaneMask.Mid;

        [Header("Pacing & Telegraph")]
        [Tooltip("ตัวคูณความถี่การเกิด (1=ปกติ, 1.3=ถี่ขึ้น)")]
        public float spawnRateMul = 1f;

        [Tooltip("เวลานำก่อนเกิดจริงสำหรับลูกศรเตือน (วินาที)")]
        [Range(0.2f, 1.2f)] public float hintLeadTime = 0.7f;

        [Tooltip("เพิ่มความเร็วฐานของเกมในเวฟนี้ (เช่น +2.5, +3.5)")]
        public float speedBonusAbs = 0f;

        [Header("Cart Types")]
        [Tooltip("อนุญาตขบวนเดี่ยว")]
        public bool allowSingle = true;
        [Tooltip("อนุญาตขบวน 2 คันพ่วง")]
        public bool allowDouble = true;
        [Tooltip("อนุญาตขบวน 3 คันพ่วง")]
        public bool allowTriple = false;

        [Header("Boss / Rewards")]
        [Tooltip("ถ้าเป็นเวฟบอสจะไว้ใช้ทำกติกาพิเศษภายหลัง")]
        public bool isBoss = false;

        [Tooltip("+หัวใจเมื่อเคลียร์เวฟนี้ (0 = ไม่ได้รางวัลหัวใจ)")]
        public int heartRewardOnClear = 0;
    }
}