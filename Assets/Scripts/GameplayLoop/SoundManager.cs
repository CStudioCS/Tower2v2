using System;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.Member;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    private AudioSource[] sources;
    
    [SerializeField] private int bufferSize = 10;
    [SerializeField] private int index = 0;

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
    }

    [SerializeField] private Sound[] sounds;

    private Dictionary<string, Sound> soundDict;



    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        sources = new AudioSource[bufferSize];

        for (int i = 0; i < bufferSize; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();
        }

        soundDict = new Dictionary<string, Sound>();

        foreach (Sound s in sounds)
        {
            soundDict[s.name] = s;
        }
    }

    public int PlaySound(string name)
    {
        if (soundDict.TryGetValue(name, out Sound sound))
        {
            sources[index].PlayOneShot(sound.clip,sound.volume);
        }
        else
        {
            Debug.LogError("Sound : '" + name + "' was not found.");
            return -1;
        }

        int i = index;

        index++;
        if (index >= sources.Length)
        {
            index = 0;
        }

        return i;
    }

    public void StopSound(int i)
    {
        sources[i].Stop();
    }
}