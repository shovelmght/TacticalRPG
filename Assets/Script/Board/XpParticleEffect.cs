using System.Collections;
using UnityEngine;

public class XpParticleEffect : MonoBehaviour
{
    [SerializeField] private Light _Light;
    [SerializeField] private Transform[] RandomSpherePositions;
    [SerializeField] private GameObject Visual;
    private const float DURATION = 2;
    private const float SpeedFadeLight = 2;
    
     
    public IEnumerator StartXpParticleEffect(Character character, int xPToGive)
    {
        StartCoroutine(FadeLight(character, xPToGive));
        float startTime = Time.time;
        Vector3 startPosition = transform.position;
        Vector3 randomPosition = RandomSpherePositions[Random.Range(0 ,RandomSpherePositions.Length - 1)].position;

        yield return new WaitForSeconds(0.75f);
        
        Visual.SetActive(true);
        
        while (Time.time < startTime + DURATION)
        {
            float t = (Time.time - startTime) / DURATION;
            transform.position = Vector3.Lerp(startPosition, randomPosition, t);
            yield return null; // Wait for the next frame
        }
        
        startTime = Time.time;
        startPosition = transform.position;
        
        while (Time.time < startTime + DURATION * 10)
        {
            float t = (Time.time - startTime) / DURATION * 10;
           transform.position = Vector3.Lerp(startPosition, character.transform.position +  new Vector3(0,1,0), t);
            yield return null; // Wait for the next frame
        }
        
        
        
        Destroy(this.gameObject);
    }
    
    public IEnumerator FadeLight(Character character, int xPToGive)
    {
        
        while (_Light.intensity > 0.01f)
        {
            Debug.Log("_Light.intensity = " + _Light.intensity);
            _Light.intensity -= SpeedFadeLight * Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        
        character.SetXpEarned?.Invoke(xPToGive);
    }
    
}
