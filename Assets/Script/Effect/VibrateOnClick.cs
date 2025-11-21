using UnityEngine;

public class VibrateOnClick : MonoBehaviour
{
    // เรียกจาก OnClick() ของ Button
    public void Vibrate()
    {
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }
}