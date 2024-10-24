using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract  class Attack : ScriptableObject
{
    public int Power = 10;
    public int AttackLenght = 1;
    public string AttackName = "Attack";
    public string AttackAnimationName = "Attack";
    public bool IsProjectile;
    public bool IsLineAttack;
    public bool IsDashAttack;
    public bool IsWaterAttack;
    public bool IsSpawnSkill;
    public bool IsReapeatableAttack;
    public bool SoftBattleZoom;
    public bool _IsPoison;
    public bool _IsFire;
    public bool _IsCrossAttack;
    public AudioManager.SfxClass PreSfx;
    public AudioManager.SfxClass SfxAtSpawn;
    public AudioManager.SfxClass ImpactSfx;
    public AudioManager.SfxClass NoTargetSfx;

    public abstract void  DoAttack(Character character, Tile tile,bool isAcounterAttack, GetAttackDirection.AttackDirection attackDirection);
}
