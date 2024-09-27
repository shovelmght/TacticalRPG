
using UnityEngine;


[CreateAssetMenu(fileName = "CharacterMaterial", menuName = "ScriptableObjects/CharacterMaterial")]
public class CharacterMaterial : ScriptableObject
{
    [field: SerializeField] public Material[] AllPossibleMaterials { get; private set; }
}
