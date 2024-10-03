using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new PotionData", menuName = "PotionData")]
public class PotionData : ScriptableObject
{
    public Material PotionColorMaterial;
    public GameObject PotionPrefab;
    public E_Potion PotionType;

}
