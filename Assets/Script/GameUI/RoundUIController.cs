using UnityEngine;
using TMPro;
using System.Collections;

public class RoundUIController : MonoBehaviour
{
    [Header("UI")]
    public CanvasGroup group;
    public TMP_Text roundText;

    [Header("Display Settings")]
    public float fadeInTime = 0.3f;
    public float holdTime = 1.5f;
    public float fadeOutTime = 0.3f;

    Coroutine routine;

    void Awake()
    {
        if (group)
        {
            group.alpha = 0f;
            group.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// เรียกเมื่อต้องการโชว์รอบ เช่น ShowRound(1)
    /// </summary>
    public void ShowRound(int roundNumber)
    {
        if (routine != null) StopCoroutine(routine);
        routine = StartCoroutine(ShowRoutine(roundNumber));
    }

    IEnumerator ShowRoutine(int n)
    {
        group.gameObject.SetActive(true);
        if (roundText) roundText.text = $"ROUND {n}";

        // fade in
        float t = 0;
        while (t < fadeInTime)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(0, 1, t / fadeInTime);
            yield return null;
        }
        group.alpha = 1;

        yield return new WaitForSeconds(holdTime);

        // fade out
        t = 0;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            group.alpha = Mathf.Lerp(1, 0, t / fadeOutTime);
            yield return null;
        }
        group.alpha = 0;
        group.gameObject.SetActive(false);
    }
}