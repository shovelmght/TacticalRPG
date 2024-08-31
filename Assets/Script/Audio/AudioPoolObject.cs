using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPoolObject : MonoBehaviour
{
    [SerializeField] private AudioSource _AudioSource;
    private void Update()
    {
        if (_AudioSource != null && _AudioSource.isPlaying == false)
        {
            gameObject.SetActive(false);
        }
    }
}
