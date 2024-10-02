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
        SquireAI = 1,
        Dragon = 2,
        DragonAI = 3,
        Wizard = 4,
        WizardAI = 5,
        MotherNature = 6,
        MotherNatureAI = 7,
        Robot = 8,
        RobotAI = 9,
        Turret = 10
    }
    
    public enum CharactersAbility1
    {
        None = 0,
        AounterAttack = 1,
    }

    public List<DataSpawner>  DataSpawn;

}
