using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private GameObject _PrefabAudioSource;
    [SerializeField] private int _AudioPoolLength = 10; // Default value
    private int _IndexAudioPool;
    private List<AudioPoolObject> _AudioPool;
    public float _MainVolume = 0.5f;
    
    public SfxClass _ZoomSFX;
    public SfxClass _SwordHit;
    public SfxClass _SwordSoft;
    public SfxClass _BlockSound;
    public SfxClass _ClickSfx;
    public SfxClass _StartGameGuitSfx;
    public SfxClass _SpawnCharacter;
    public static AudioManager _Instance { get; private set; } // Singleton instance

    private void Awake()
    {
        if (_Instance != null && _Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        
        _Instance = this;

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

    public void SpawnSound(SfxClass Sfx)
    {
        _IndexAudioPool = (_IndexAudioPool + 1) % _AudioPool.Count;
        AudioPoolObject poolObject = _AudioPool[_IndexAudioPool];

        // Set up audio properties
        poolObject._GameObject.SetActive(true);
        poolObject._AudioSource.clip = Sfx._AudioClip;
        float randomPitch = Random.Range(Sfx._MinRandomPitch, Sfx._MaxRandomPitch);
        poolObject._AudioSource.pitch = randomPitch;
        poolObject._AudioSource.volume = _MainVolume * Sfx._RelativeVolume;
        poolObject._AudioSource.Play();

        StartCoroutine(DeactivateAfterPlay(poolObject));
    }

    private IEnumerator DeactivateAfterPlay(AudioPoolObject poolObject)
    {
        if(poolObject == null) yield break;
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
    
    [Serializable]
    public class SfxClass
    {
        public AudioClip _AudioClip;
        public float _RelativeVolume;
        public float _MinRandomPitch;
        public float _MaxRandomPitch;
    }
}