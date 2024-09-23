using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorActivator : MonoBehaviour
{
    [SerializeField] private GameObject[] _GameObjectsToActivate;
    [SerializeField] private float _LerpScaleSpeed = 0.2f;

    private Vector3 _StartScale;

    private void Start()
    {
        _StartScale = _GameObjectsToActivate[0].transform.localScale;
    }

    public void ActivateGameObjects()
    {
       
        foreach (var gameObjectToActivate in _GameObjectsToActivate)
        {
            gameObjectToActivate.transform.localScale = _StartScale;
            gameObjectToActivate.SetActive(true);
        }

        StartCoroutine(LerpScaleAndActivateGameObjects());
    }

    private IEnumerator LerpScaleAndActivateGameObjects()
    {
        // Set all objects to the initial small scale
        foreach (var gameObjectToActivate in _GameObjectsToActivate)
        {
            gameObjectToActivate.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            gameObjectToActivate.SetActive(true); // Ensure the object is active before scaling
        }

        bool allScaledUp = false;

        while (!allScaledUp)
        {
            allScaledUp = true; // Assume all are scaled up

            foreach (var gameObjectToActivate in _GameObjectsToActivate)
            {
                Vector3 currentScale = gameObjectToActivate.transform.localScale;

                // If the object's scale is below the target scale, continue scaling it up
                if (currentScale.x < _StartScale.x)
                {
                    allScaledUp = false; // At least one object is still below the target scale
                    gameObjectToActivate.transform.localScale = Vector3.Min(
                        currentScale + new Vector3(_LerpScaleSpeed, _LerpScaleSpeed, _LerpScaleSpeed),
                        new Vector3(_StartScale.x, _StartScale.x, _StartScale.x) // Ensure it doesn't exceed the target scale
                    );
                }
            }

            // Wait for a short time before the next scale step
            yield return new WaitForSeconds(0.01f);
        }

        // All objects are scaled up to the target size and active
    }
    
    public void DeactivateGameObjects()
    {
        StartCoroutine(LerpScaleAndDeactivateGameObjects());
    }

    private IEnumerator LerpScaleAndDeactivateGameObjects()
    {
        bool allScaledDown = false;

        while (!allScaledDown)
        {
            allScaledDown = true; // Assume all are scaled down

            foreach (var gameObjectToActivate in _GameObjectsToActivate)
            {
                Vector3 currentScale = gameObjectToActivate.transform.localScale;

                // If the object's scale is above the threshold, continue scaling it down
                if (currentScale.x > 0.2f)
                {
                    allScaledDown = false; // At least one object is still above the threshold
                    gameObjectToActivate.transform.localScale = Vector3.Max(
                        currentScale - new Vector3(_LerpScaleSpeed, _LerpScaleSpeed, _LerpScaleSpeed),
                        new Vector3(0.2f, 0.2f, 0.2f) // Prevent scaling below the threshold
                    );
                }
            }

            // Wait for a short time before the next scale step
            yield return new WaitForSeconds(0.01f);
        }

        // After scaling is done, deactivate all GameObjects
        foreach (var gameObjectToActivate in _GameObjectsToActivate)
        {
            gameObjectToActivate.SetActive(false);
        }
    }
}
