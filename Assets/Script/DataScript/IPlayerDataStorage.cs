using System.Threading.Tasks;

public interface IPlayerDataStorage
{
    /// <summary>
    /// โหลดข้อมูลผู้เล่น
    /// uid: ใช้สำหรับ cloud (เช่น Firebase UID), local สามารถไม่สนใจได้
    /// </summary>
    Task<PlayerData> LoadAsync(string uid = null);

    /// <summary>
    /// บันทึกข้อมูลผู้เล่น
    /// uid: ใช้สำหรับ cloud (เช่น Firebase UID), local สามารถไม่สนใจได้
    /// </summary>
    Task SaveAsync(PlayerData data, string uid = null);

    /// <summary>
    /// บันทึกผลการเล่น 1 รอบ (ใช้กับ cloud เป็นหลัก)
    /// score: คะแนนรวมรอบนี้
    /// wave: wave ที่ไปถึง
    /// duration: เวลาเล่นเป็นวินาที
    /// version: เวอร์ชันเกม (int ที่เราแปลงจาก Application.version)
    /// </summary>
    Task AddRunResultAsync(string uid, int score, int wave, int duration, int version);
}