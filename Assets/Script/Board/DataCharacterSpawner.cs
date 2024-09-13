using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataCharacterSpawner", menuName = "ScriptableObjects/DataCharacterSpawner")]
public class DataCharacterSpawner : ScriptableObject
{   
    [System.Serializable]
    public struct DataSpawner
    {
        public CharactersPrefab CharactersPrefab;
        public Character.Team Team;
        public CharactersAbility1 Ability1;
        public string Name;
        public string Class;
    }
    
    public enum CharactersPrefab
    {
        Squire = 0,
        Dragon = 1,        
        SquireAI = 2,
        DragonAI = 3
    }
    
    public enum CharactersAbility1
    {
        None = 0,
        AounterAttack = 1,
        CharacterAI = 2
    }

    public List<DataSpawner>  DataSpawn;

}
