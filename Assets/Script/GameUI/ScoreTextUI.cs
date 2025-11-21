// ScoreTextUI.cs
using TMPro;
using UnityEngine;

public class ScoreTextUI : MonoBehaviour
{
    public TMP_Text label;
    public string format = "{0}";   // เช่น "Score: {0}"

    public void Refresh(int value)
    {
        if (!label) return;
        label.SetText(format, value); // หรือ: label.text = string.Format(format, value);
    }
}