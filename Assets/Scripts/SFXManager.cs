using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager I; //singleton for easy access

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject); 
    }

    /// <summary>
    ///Play a sound at world position (for 3D sounds).
    /// </summary>
    public void PlayAt(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        GameObject temp = new GameObject("SFX_" + clip.name);
        AudioSource a = temp.AddComponent<AudioSource>();
        a.clip = clip;
        a.volume = volume;
        a.pitch = pitch;
        a.spatialBlend = 1f; // 3D sound
        a.transform.position = position;

        a.Play();
        Destroy(temp, clip.length / pitch);
    }

    /// <summary>
    ///Play a 2D sound (ignores position, e.g. UI or win sound).
    /// </summary>
    public void Play2D(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;

        GameObject temp = new GameObject("SFX2D_" + clip.name);
        AudioSource a = temp.AddComponent<AudioSource>();
        a.clip = clip;
        a.volume = volume;
        a.pitch = pitch;
        a.spatialBlend = 0f; //2D sound

        a.Play();
        Destroy(temp, clip.length / pitch);
    }
}
