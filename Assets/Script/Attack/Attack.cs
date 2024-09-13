using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract  class Attack : ScriptableObject
{
    public float Power = 10;
    public int AttackLenght = 1;

    public abstract void  DoAttack(Character character, Tile tile,bool isAcounterAttack, GetAttackDirection.AttackDirection attackDirection);
}
