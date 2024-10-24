using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTurret : MonoBehaviour
{
    [SerializeField] private DataCharacterSpawner.CharactersPrefab _CharactersPrefab;
    [SerializeField] private GameObject _ParticleEffect;
    [SerializeField] private GameObject VisualMesh;
    [SerializeField] private float _LerpScaleSpeed = 0.005f;
    [SerializeField] private bool SpawnMob = true;
    [SerializeField] private bool _CanMove;
    void Start()
    {
        StartCoroutine(AddCharacterTurret());
    }

    private IEnumerator AddCharacterTurret()
    {
        StartCoroutine(LerpScaleAndDeactivateGameObjects());

        yield return new WaitForSeconds(1.25f);
      
        if (SpawnMob)
        {
            GameManager.Instance.SpawnMobCharacter(GameManager.Instance.LastSpawnTile, Vector3.zero, _CharactersPrefab, _CanMove);
            VisualMesh.SetActive(false);
        }
        else
        {
            GameManager.Instance.AllCharacterGo.Add(transform);
        }
        
    }
    
    
    private IEnumerator LerpScaleAndDeactivateGameObjects()
    {
        Debug.Log("LerpScaleAndDeactivateGameObjects bug 1" );
        bool allScaledDown = false;

        while (!allScaledDown)
        {
            Debug.Log("LerpScaleAndDeactivateGameObjects bug 2" );
            allScaledDown = true; // Assume all are scaled down


            if (_ParticleEffect == null)
            {
                yield break;
            }
            
            Vector3 currentScale = _ParticleEffect.transform.localScale;
            // If the object's scale is above the threshold, continue scaling it down
            
                if (currentScale.x > 0.05f)
                {
                    Debug.Log("LerpScaleAndDeactivateGameObjects bug 3" );
                    allScaledDown = false; // At least one object is still above the threshold
                    _ParticleEffect.transform.localScale = Vector3.Max(
                        currentScale - new Vector3(_LerpScaleSpeed, _LerpScaleSpeed, _LerpScaleSpeed),
                        new Vector3( 0.05f,  0.05f,  0.05f) // Prevent scaling below the threshold
                    );
                }
            

            // Wait for a short time before the next scale step
            yield return new WaitForSeconds(0.01f);
        }

        Debug.Log("LerpScaleAndDeactivateGameObjects bug 4" );
        // After scaling is done, deactivate all GameObjects

        yield return new WaitForSeconds(0.1f);
        
        if (_ParticleEffect != null)
        {
            _ParticleEffect.SetActive(false);
        }
        
        yield return new WaitForSeconds(3);
        
        if (SpawnMob)
        {
           Destroy(gameObject);
        }
      
        
    }
    
}
