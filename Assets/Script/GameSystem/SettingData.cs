using System;
using UnityEngine;

[Serializable]
public class SettingsData
{
    [Range(0f,1f)] public float masterMusic = 1f;
    [Range(0f,1f)] public float masterSfx   = 1f;
    public bool hapticsOn = true;
}