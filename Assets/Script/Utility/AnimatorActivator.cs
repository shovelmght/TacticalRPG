using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorActivator : MonoBehaviour
{
    [SerializeField] private GameObject[] _GameObjectsToActivate;


    public void ActivateGameObjects()
    {
        foreach (var gameObjectToActivate in _GameObjectsToActivate)
        {
            gameObjectToActivate.SetActive(true);
        }
    }
    
    public void DeactivateGameObjects()
    {
        foreach (var gameObjectToActivate in _GameObjectsToActivate)
        {
            gameObjectToActivate.SetActive(false);
        }
    }
}
