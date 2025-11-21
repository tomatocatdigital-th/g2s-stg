using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle hapticsToggle;

    void OnEnable()
    {
        var d = SettingsManager.I.Data;
        if (musicSlider)   musicSlider.SetValueWithoutNotify(d.masterMusic);
        if (sfxSlider)     sfxSlider.SetValueWithoutNotify(d.masterSfx);
        if (hapticsToggle) hapticsToggle.SetIsOnWithoutNotify(d.hapticsOn);

        if (musicSlider)   musicSlider.onValueChanged.AddListener(SettingsManager.I.SetMusic);
        if (sfxSlider)     sfxSlider.onValueChanged.AddListener(SettingsManager.I.SetSfx);
        if (hapticsToggle) hapticsToggle.onValueChanged.AddListener(SettingsManager.I.SetHaptics);
    }

    void OnDisable()
    {
        if (musicSlider)   musicSlider.onValueChanged.RemoveAllListeners();
        if (sfxSlider)     sfxSlider.onValueChanged.RemoveAllListeners();
        if (hapticsToggle) hapticsToggle.onValueChanged.RemoveAllListeners();
    }

    // ปุ่ม Close (ถ้ามี)
    public void OnClickClose() => gameObject.SetActive(false);

    // ปุ่ม Reset (ถ้ามี)
    public void OnClickReset()
    {
        SettingsManager.I.ResetToDefault();
        OnEnable(); // รีเฟรชค่าใน UI
        SoundManager.I?.PlayClick();
    }
}