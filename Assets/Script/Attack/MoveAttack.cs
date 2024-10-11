using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new MoveAttack", menuName = "MoveAttack")]
public class MoveAttack : Attack
{
    protected static readonly int Attack = Animator.StringToHash("Attack");
    [SerializeField] private GameObject _ParticleEffectPrefab;
    
    public override void DoAttack(Character character, Tile tile, bool isAcounterAttack, GetAttackDirection.AttackDirection attackDirection)
    {
        character.StartCoroutine(character.RotateTo(tile.Position));
        
        if (tile.CharacterReference)
        {
            tile.CharacterReference._IncomingAttacker = character;
            character._attackTarget = tile.CharacterReference;
            character._attackTarget._attackDirection = attackDirection;
        }

        character.CharacterAnimator.SetTrigger(AttackAnimationName);
        character._isCounterAttack = isAcounterAttack;

        character.StartCoroutine(character.MoveAttack(PreSfx, NoTargetSfx,ImpactSfx,_ParticleEffectPrefab, tile));
    }
}
