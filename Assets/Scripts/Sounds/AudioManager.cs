using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : SingletonMonobehaviour<AudioManager>
{
    [SerializeField] private GameObject soundPrefab = null;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambientSoundAudioSource = null;
    [SerializeField] private AudioSource gameMusicAudioSource = null;

    [Header("Audio Mixers")]
    [SerializeField] private AudioMixer gameAudioMixer = null;

    [Header("Audio Snapshots")]
    [SerializeField] private AudioMixerSnapshot gameMusicSnapshot = null;
    [SerializeField] private AudioMixerSnapshot gameAmbientSnapshot = null;

    [Header("Other")]
    [SerializeField] private SO_SoundList so_SoundList = null;

    [SerializeField] private SO_SceneSoundList so_SceneSoundList = null;
    [SerializeField] private float defaultSceneMusicPlayTimeSeconds = 120f;
    [SerializeField] private float sceneMusicStartMinSecs = 5f;
    [SerializeField] private float sceneMusicStartMaxSecs = 10f;
    [SerializeField] private float musicTrasitionSecs = 8f;

    private Dictionary<SoundName, SoundItem> soundDictionary;
    private Dictionary<SceneName, SceneSoundItem> sceneSoundDictionary;

    private Coroutine playSceneSoundsCoroutine;

    protected override void Awake()
    {
        base.Awake();

        soundDictionary = new Dictionary<SoundName, SoundItem>();

        foreach (SoundItem soundItem in so_SoundList.soundDetails)
        {
            soundDictionary.Add(soundItem.soundName, soundItem);
        }
        //初始化字典
        sceneSoundDictionary = new Dictionary<SceneName, SceneSoundItem>();

        foreach (SceneSoundItem sceneSoundItem in so_SceneSoundList.sceneSoundsDetails)
        {
            sceneSoundDictionary.Add(sceneSoundItem.sceneName, sceneSoundItem);
        }
    }

    private void OnEnable()
    {
        EventHandler.AfterSceneLoadEvent += PlaySceneSounds;

    }
    private void OnDisable()
    {
        EventHandler.AfterSceneLoadEvent -= PlaySceneSounds;
    }

    private void PlaySceneSounds()
    {
        SoundItem musicSoundItem = null;
        SoundItem ambientSoundItem = null;

        float musicPlayTime = defaultSceneMusicPlayTimeSeconds;
        //获取环境和氛围音乐并播放
        if (Enum.TryParse<SceneName>(SceneManager.GetActiveScene().name, true, out SceneName currentSceneName))
        {
            if (sceneSoundDictionary.TryGetValue(currentSceneName, out SceneSoundItem sceneSoundItem))
            {
                soundDictionary.TryGetValue(sceneSoundItem.musicForScene, out musicSoundItem);
                soundDictionary.TryGetValue(sceneSoundItem.ambienSoundForScene, out ambientSoundItem);
            }
            else
            {
                return;
            }

            if (playSceneSoundsCoroutine != null)
            {
                StopCoroutine(playSceneSoundsCoroutine);
            }
            playSceneSoundsCoroutine = StartCoroutine(PlaySceneSoundsCoroutine(musicPlayTime, musicSoundItem, ambientSoundItem));
        }
    }

    private IEnumerator PlaySceneSoundsCoroutine(float musicPlayTime, SoundItem musicSoundItem, SoundItem ambientSoundItem)
    {
        //播放顺序：氛围音乐-环境音乐
        if (musicSoundItem != null && ambientSoundItem != null)
        {
            PlayAmbientSoundClip(ambientSoundItem, 0f);

            yield return new WaitForSeconds(UnityEngine.Random.Range(sceneMusicStartMinSecs, sceneMusicStartMaxSecs));

            PlayMusicSoundClip(musicSoundItem, musicTrasitionSecs);

            yield return new WaitForSeconds(musicPlayTime);

            PlayAmbientSoundClip(ambientSoundItem, musicTrasitionSecs);
        }
    }

    private void PlayAmbientSoundClip(SoundItem ambientSoundItem, float transitionTime)
    {
        //设置音量
        gameAudioMixer.SetFloat("AmbientVolume", ConvertSoundVolume(ambientSoundItem.soundVolume));

        ambientSoundAudioSource.clip = ambientSoundItem.soundClip;
        ambientSoundAudioSource.Play();

        gameAmbientSnapshot.TransitionTo(transitionTime);
    }

    //转换函数
    private float ConvertSoundVolume(float dec)
    {
        return Mathf.Log10(Mathf.Clamp(dec, 0.0001f, 1f)) * 20f;
    }

    private void PlayMusicSoundClip(SoundItem musicSoundItem, float transitionTime)
    {
        //设置音量
        gameAudioMixer.SetFloat("MusicVolume", ConvertSoundVolume(musicSoundItem.soundVolume));

        gameMusicAudioSource.clip = musicSoundItem.soundClip;
        gameMusicAudioSource.Play();

        gameMusicSnapshot.TransitionTo(transitionTime);
    }

    public void PlaySound(SoundName soundName)
    {
        if (soundDictionary.TryGetValue(soundName, out SoundItem soundItem) && soundPrefab != null)
        {
            GameObject soundGameObject = PoolManager.Instance.ReuseObject(soundPrefab, Vector3.zero, Quaternion.identity);

            Sound sound = soundGameObject.GetComponent<Sound>();

            sound.SetSound(soundItem);
            soundGameObject.SetActive(true);
            StartCoroutine(DisableSound(soundGameObject, soundItem.soundClip.length));
        }
    }

    private IEnumerator DisableSound(GameObject soundGameObject, float soundDuration)
    {
        yield return new WaitForSeconds(soundDuration);
        soundGameObject.SetActive(false);
    }
}
