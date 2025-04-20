using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundName { Attack, Damaged, Step, AlarmBeep, Click, Score, Slip }

[System.Serializable]
public class Sound 
{
    public string Name;
    public SoundName SName;
    public List<AudioClip> Clips; //Sounds can have multiple audioclips so the sound is a bit different each time
    public float Volume;
}

public class AudioManager : MonoBehaviour
{
    public List<Sound> Sounds = new List<Sound>();

    

    [SerializeField]
    private AudioSource SFXObject;
    public static AudioManager Instance;
    void Singleton()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this);
    }
    private void Awake()
    {
        Singleton();
    }

    #region Find sound in the list 
    Sound GetClip(SoundName name)
    {
        foreach (Sound v in Sounds)
        {
            if (v.SName == name) { return v; }
        }

        return null;
    }
    #endregion

    public void Play(SoundName name, Transform transform)
    {
        Sound sound = GetClip(name);
        if (sound == null) { Debug.LogError("No sound named " + name); return; }

        AudioSource source = Instantiate(SFXObject, transform);

        if (sound.Clips == null || sound.Clips.Count == 0) return;
        AudioClip clip = sound.Clips[Random.Range(0, sound.Clips.Count)];

        if (clip == null) { return; }

        source.clip = clip;        
        source.volume = sound.Volume;
        source.Play();

        float length = source.clip.length;
        Destroy(source, length);
    }
}
