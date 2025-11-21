using System.Collections.Generic;
using UnityEngine;

public class TrainRequirement : MonoBehaviour
{
    public List<Route> sequence = new List<Route>();  // ลำดับสีของขบวน เช่น [Blue, Red, Yellow]
    public int nextIndex = 0;                          // คิวถัดไปที่ต้องการ

    public bool HasNext => nextIndex < sequence.Count;
    public Route CurrentRequired() => sequence[Mathf.Clamp(nextIndex, 0, sequence.Count - 1)];

    // เรียกเมื่อผ่าน Pad ถูกต้อง
    public void Advance() { if (nextIndex < sequence.Count) nextIndex++; }
}