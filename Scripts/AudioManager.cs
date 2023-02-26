using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class AudioManager : ScriptableObject
{
    public float musicVolume = 1;
    public float soundVolume = 1;

    public List<SoundData> soundDatas = new List<SoundData>();
    public List<SoundData> musicDatas = new List<SoundData>();

    Dictionary<Transform, SoundObjectData> soundObjects = new Dictionary<Transform, SoundObjectData>();
    Dictionary<string, LoopingSoundData> loopingSounds = new Dictionary<string, LoopingSoundData>();

    AudioSource musicSource;


    public void ResetSoundObjects()
    {
        foreach (SoundObjectData soundObjData in soundObjects.Values)
        {
            DestroyImmediate(soundObjData.gameObject);
        }
        soundObjects = new Dictionary<Transform, SoundObjectData>();
    }

    public void StopAllLoops()
    {
        string[] loops = new string[loopingSounds.Count];
        for (int i = 0; i < loopingSounds.Count; i++)
        {
            loopingSounds.Keys.CopyTo(loops, i);
            StopLoopingSound(loops[i]);
        }
    }

    SoundObjectData GetSoundObject(Transform position, bool makeObject, bool dontDestroyOnLoad)
    {
        // if there is no sound object with the transform
        if (!soundObjects.ContainsKey(position) || makeObject)
        {
            GameObject newObject = new GameObject();
            if (dontDestroyOnLoad)
                DontDestroyOnLoad(newObject);
            //newObject.transform.parent = position;
            newObject.transform.localPosition = Vector3.zero;
            SoundObjectData newObjectData = new SoundObjectData(newObject, new List<AudioSource>(), new List<int>());
            soundObjects.Add(position, newObjectData);
        }
        return soundObjects[position];
    }

    int GetAudioSource(SoundObjectData objectData)
    {
        int sourceIdx;

        // if no available sources, creates a new one
        if (objectData.availableSources.Count == 0)
        {
            AudioSource source = objectData.gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            objectData.audioSources.Add(source);
            sourceIdx = objectData.audioSources.Count - 1;
        }
        // otherwise get the next available one
        else
        {
            sourceIdx = objectData.availableSources[0];
            objectData.availableSources.RemoveAt(0);
        }

        return sourceIdx;
    }

    AudioSource InitializeAudioSource(bool loopingSound, SoundData soundData, SoundObjectData objectData, int sourceIdx, float volume, float pitch, float spacialBlend)
    {
        // gets source
        AudioSource source = objectData.audioSources[sourceIdx];

        // assign values and play
        source.clip = soundData.sound;
        source.volume = (volume == -1 ? soundData.volume : volume) * (loopingSound ? musicVolume : soundVolume);
        source.pitch = pitch == -4 ? soundData.pitch : pitch;
        source.spatialBlend = spacialBlend == -1 ? soundData.spacialBlend : spacialBlend;

        return source;
    }

    public void PlaySound(MonoBehaviour mono, Transform position, string name, float volume = -1, float pitch = -4, float spacialBlend = -1)
    {
        mono.StartCoroutine(PlaySoundCoroutine(position, name, volume, pitch, spacialBlend));
    }

    IEnumerator PlaySoundCoroutine(Transform position, string name, float volume, float pitch, float spacialBlend)
    {
        // gets sound data
        SoundData soundData = soundDatas.Find(data => data.name == name);
        SoundObjectData objectData = GetSoundObject(position, false, false);

        // gets source
        int sourceIdx = GetAudioSource(objectData);
        AudioSource source = InitializeAudioSource(false, soundData, objectData, sourceIdx, volume, pitch, spacialBlend);

        source.loop = false;
        source.Play();

        yield return new WaitForSecondsRealtime(soundData.sound.length);

        // mark source as available after sound is played
        objectData.availableSources.Add(sourceIdx);
    }

    public void StartLoopingSound(Transform position, string name, float volume = -1, float pitch = -4, float spacialBlend = -1)
    {
        if (!loopingSounds.ContainsKey(name))
        {
            // gets sound data
            SoundData soundData = soundDatas.Find(data => data.name == name);
            SoundObjectData objectData = GetSoundObject(position, false, false);

            // gets source
            int sourceIdx = GetAudioSource(objectData);
            AudioSource source = InitializeAudioSource(true, soundData, objectData, sourceIdx, volume, pitch, spacialBlend);

            source.loop = true;
            source.Play();

            loopingSounds.Add(name, new LoopingSoundData(objectData, sourceIdx));
        }
    }

    public void SetLoopingSoundVolume(string name, float volume)
    {
        if (loopingSounds.ContainsKey(name))
        {
            // gets source and data
            LoopingSoundData loopingSoundData = loopingSounds[name];
            AudioSource source = loopingSoundData.soundObjectData.audioSources[loopingSoundData.sourceIdx];

            if (source != null)
            {
                // sets volume
                source.volume = Mathf.Clamp01(volume);
            }
        }
    }

    public void StopLoopingSound(string name)
    {
        musicSource.Stop();
        if (loopingSounds.ContainsKey(name))
        {
            // gets source and data
            LoopingSoundData loopingSoundData = loopingSounds[name];
            AudioSource source = loopingSoundData.soundObjectData.audioSources[loopingSoundData.sourceIdx];

            if (source != null)
            {
                // stops
                source.Stop();
                loopingSoundData.soundObjectData.availableSources.Add(loopingSoundData.sourceIdx);
                loopingSounds.Remove(name);
            }
        }
    }

    public void StartMusic(MonoBehaviour mono, Transform position)
    {
        mono.StartCoroutine(PlayMusicCoroutine(mono, position));
    }

    int lastMusicPlayed = -1;

    IEnumerator PlayMusicCoroutine(MonoBehaviour mono, Transform position)
    {
        // gets sound data
        int musicIdx = 0;
        if (musicIdx == lastMusicPlayed)
            musicIdx = (musicIdx + 1) % musicDatas.Count;
        SoundData soundData = musicDatas[musicIdx];
        SoundObjectData objectData = GetSoundObject(position, true, true);

        // gets source
        int sourceIdx = GetAudioSource(objectData);
        musicSource = InitializeAudioSource(false, soundData, objectData, sourceIdx, musicVolume, 1, 0);

        musicSource.loop = true;
        musicSource.Play();
        lastMusicPlayed = musicIdx;
        yield return new WaitForSecondsRealtime(soundData.sound.length);

        // mark source as available after sound is played
        //if (position.gameObject)
        //{
        //mono.StartCoroutine(PlayMusicCoroutine(mono, position, musicIdx));
        //}
    }

    public void UpdateMusicVolume(float value)
    {
        musicVolume = value;
        if (musicSource)
            musicSource.volume = value;
    }
}

struct LoopingSoundData
{
    public LoopingSoundData(SoundObjectData sod, int si)
    {
        soundObjectData = sod;
        sourceIdx = si;
    }

    public SoundObjectData soundObjectData;
    public int sourceIdx;
}

[System.Serializable]
public struct SoundData
{
    public string name;
    public AudioClip sound;
    [Range(0, 1)]
    public float volume;
    [Range(-3, 3)]
    public float pitch;
    [Range(0, 1)]
    public float spacialBlend;
}

[System.Serializable]
public struct SoundObjectData
{
    public SoundObjectData(GameObject obj, List<AudioSource> sources, List<int> available)
    {
        gameObject = obj;
        audioSources = sources;
        availableSources = available;
    }

    public GameObject gameObject;
    public List<AudioSource> audioSources;
    public List<int> availableSources;
}
