using UnityEngine;
using System.Collections;

public class CorrectHitFX : MonoBehaviour
{
    [Header("Refs")]
    public ParticleSystem particles; // ใส่ FX_Correct
    public Light flashLight;         // ไฟเล็ก ๆ  Range 3–6, Intensity 0

    [Header("Flash")]
    public float flashIntensity = 5f;
    public float flashTime = 0.15f;

    public void PlayFX()
    {
        if (particles) particles.Play();
        if (flashLight) StartCoroutine(Flash());
    }

    IEnumerator Flash()
    {
        float t = 0f;
        while (t < flashTime)
        {
            t += Time.deltaTime;
            float k = 1f - (t / flashTime); // เฟดลง
            flashLight.intensity = flashIntensity * k;
            yield return null;
        }
        flashLight.intensity = 0f;
    }
}