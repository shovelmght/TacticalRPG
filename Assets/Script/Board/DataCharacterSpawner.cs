using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DataCharacterSpawner", menuName = "ScriptableObjects/DataCharacterSpawner")]
public class DataCharacterSpawner : ScriptableObject
{   
    [System.Serializable]
    public class DataSpawner
    {
        public CharactersPrefab CharactersPrefab;
        public Character.Team Team;
        public CharacterSkillAttack SkillAttack;
        public CharacterAbility Ability1;
        public CharacterAbility Ability2;
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
        Turret = 10,
        SelfDestructRobot = 11,
        SelfDestructRobotAI = 12,
        DevilBoss = 13,
        DevilBossAI = 14
    }
    
    public enum CharacterAbility
    {
        None = 0,
        CounterAttack = 1,
        ImmuneToFire = 2,
        ImmuneToPoison = 3,
        InFire = 4,
        SpeedBoost = 5
    }
    
    public enum CharacterSkillAttack
    {
        None = 0,
        Dash = 1,
        Explosion = 2,
        FireArea = 3,
        ProjectileNeedle = 4,
        ProjectileFire = 5,
        ProjectileFireDevilBoss = 6,
        ProjectileMagic = 7,
        ProjectilePoison = 8,
        ProjectileWater = 9,
        SpawnSelfDestructRobot = 10,
        SpawnTree = 11,
        SpawnTurret = 12,
        TurretAttack = 13,
        SpawnWaterTile = 14,
        RemoveWater = 15
    }

    public CharacterAbility GetCharacterAbility(int index)
    {
        switch (index)
        {
            case 0:
                return CharacterAbility.None;
            case 1:
                return CharacterAbility.CounterAttack;
            case 2:
                return CharacterAbility.ImmuneToFire;
            case 3:
                return CharacterAbility.ImmuneToPoison;
            case 4:
                return CharacterAbility.InFire;
        }

        return CharacterAbility.None;
    }
    
    public CharacterSkillAttack GetCharacterSkillAttack(int index)
    {
        switch (index)
        {
            case 0:
                return CharacterSkillAttack.None;
            case 1:
                return CharacterSkillAttack.Dash;
            case 2:
                return CharacterSkillAttack.Explosion;
            case 3:
                return CharacterSkillAttack.FireArea;
            case 4:
                return CharacterSkillAttack.ProjectileNeedle;
            case 5:
                return CharacterSkillAttack.ProjectileFire;
            case 6:
                return CharacterSkillAttack.ProjectileFireDevilBoss;
            case 7:
                return CharacterSkillAttack.ProjectileMagic;
            case 8:
                return CharacterSkillAttack.ProjectilePoison;
            case 9:
                return CharacterSkillAttack.ProjectileWater;
            case 10:
                return CharacterSkillAttack.SpawnSelfDestructRobot;
            case 11:
                return CharacterSkillAttack.SpawnTree;
            case 12:
                return CharacterSkillAttack.SpawnTurret;
            case 13:
                return CharacterSkillAttack.TurretAttack;
            case 14:
                return CharacterSkillAttack.SpawnWaterTile;
            case 15:
                return CharacterSkillAttack.RemoveWater;
        }
        
        return CharacterSkillAttack.None;
    }

    public List<DataSpawner>  DataSpawn;
    public int TeamColor;

}
