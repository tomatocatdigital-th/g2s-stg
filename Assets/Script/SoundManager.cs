using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager I { get; private set; }

    [Header("Clips")]
    public AudioClip correctSound, incorrectSound, enterSound, clickSound;

    [Header("Per-clip Volumes")]
    [Range(0f,1f)] public float correctVolume = 1f;
    [Range(0f,1f)] public float incorrectVolume = 1f;
    [Range(0f,1f)] public float enterVolume = 1f;
    [Range(0f,1f)] public float clickVolume = 1f;

    float masterSfx = 1f;
    AudioSource source;

    void Awake()
    {
        if (I && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        source = GetComponent<AudioSource>();
        if (!source) source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
    }

    public void SetMasterSfx(float v) { masterSfx = Mathf.Clamp01(v); }

    public void PlayCorrect()   => Play(correctSound, correctVolume);
    public void PlayIncorrect() => Play(incorrectSound, incorrectVolume);
    public void PlayEnter()     => Play(enterSound, enterVolume);
    public void PlayClick()     => Play(clickSound, clickVolume);

    void Play(AudioClip clip, float vol)
    {
        if (clip && source) source.PlayOneShot(clip, vol * masterSfx);
    }
}