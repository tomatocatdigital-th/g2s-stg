using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    public static BGMPlayer I { get; private set; }
    private AudioSource source;

    void Awake()
    {
        if (I && I != this)
        {
            Destroy(gameObject); // ถ้ามีตัวเก่าอยู่แล้ว ไม่ต้องสร้างซ้ำ
            return;
        }
        I = this;
        DontDestroyOnLoad(gameObject); // รอดตลอดทุก Scene

        source = GetComponent<AudioSource>();
        if (!source)
            source = gameObject.AddComponent<AudioSource>();
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (!clip) return;
        source.clip = clip;
        source.loop = loop;
        source.Play();
    }

    public void StopMusic()
    {
        source.Stop();
    }

    public void SetVolume(float v)
    {
        source.volume = Mathf.Clamp01(v);
    }
}