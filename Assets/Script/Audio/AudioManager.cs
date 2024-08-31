using System;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private GameObject _PrefabAudioSource;
    [SerializeField] private int _AudioPoolLength;
    private int _IndexAudioPool;
    private List<AudioPoolObject> _AudioPool;
    private float _MainVolume = 0.5f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        _AudioPool = new List<AudioPoolObject>(_AudioPoolLength);

        for (int i = 0; i < _AudioPoolLength; i++)
        {
            GameObject go = Instantiate(_PrefabAudioSource, transform);
            AudioPoolObject audioPoolObject = new AudioPoolObject(go, go.GetComponent<AudioSource>());
            _AudioPool.Add(audioPoolObject);
            go.SetActive(false); // Optionally, deactivate the objects initially
        }
    }

    public void SpawnSound(AudioClip audioClip, float pitch, float volume)
    {

        _IndexAudioPool = (_IndexAudioPool + 1) % _AudioPool.Count;
        AudioPoolObject poolObject = _AudioPool[_IndexAudioPool];

        // Set up audio properties
        poolObject._GameObject.SetActive(true);
        poolObject._AudioSource.clip = audioClip;
        poolObject._AudioSource.pitch = pitch;
        poolObject._AudioSource.volume = _MainVolume * volume;
        poolObject._AudioSource.Play();


        StartCoroutine(DeactivateAfterPlay(poolObject));
    }

    private IEnumerator<WaitForSeconds> DeactivateAfterPlay(AudioPoolObject poolObject)
    {
        yield return new WaitForSeconds(poolObject._AudioSource.clip.length / poolObject._AudioSource.pitch);
        poolObject._GameObject.SetActive(false);
    }

    [Serializable]
    public class AudioPoolObject
    {
        public GameObject _GameObject;
        public AudioSource _AudioSource;

        public AudioPoolObject(GameObject gameObject, AudioSource audioSource)
        {
            _GameObject = gameObject;
            _AudioSource = audioSource;
        }
    }
}