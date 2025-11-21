using UnityEngine;

[CreateAssetMenu(fileName = "GameBalanceConfig", menuName = "BoxTO/Game Balance Config")]
public class GameBalanceConfig : ScriptableObject
{
    [Header("Base")]
    public int   startLives   = 3;
    public float baseSpeed    = 18f;
    public float maxSpeed     = 60f;

    [Header("Round Scaling")]
    [Tooltip("บวกความเร็วต่อ 1 รอบ (0 = ไม่สเกลตามรอบ)")]
    public float speedPerRound = 1.5f;

    /// <summary>
    /// คืนค่าความเร็วเป้าหมายสำหรับรอบ/เวฟนี้
    /// roundIndex = 0-based, waveSpeedBonus มาจาก WaveDef.speedBonusAbs
    /// </summary>
    public float GetSpeedFor(int roundIndex, float waveSpeedBonus)
    {
        float s = baseSpeed + Mathf.Max(0, roundIndex) * Mathf.Max(0f, speedPerRound) + waveSpeedBonus;
        return Mathf.Min(s, Mathf.Max(1f, maxSpeed));
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        maxSpeed     = Mathf.Max(1f, maxSpeed);
        speedPerRound= Mathf.Max(0f, speedPerRound);
        baseSpeed    = Mathf.Max(0.1f, baseSpeed);
    }
#endif
}