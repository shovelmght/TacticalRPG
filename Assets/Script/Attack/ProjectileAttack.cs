using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "new ProjectileAttack", menuName = "ProjectileAttack")]
public class ProjectileAttack : Attack
{
    protected static readonly int Attack = Animator.StringToHash("Attack");
    [SerializeField] private GameObject _ProjectilePrefab;
    [SerializeField] private float _SpawnProjectileDelay = 1.0f;
    [SerializeField] private float _HeightGabToAdd = 10.0f;
    [SerializeField] private float _MovementSpeed = 10.0f;
    
    public override void DoAttack(Character character, Tile tile, bool isAcounterAttack, GetAttackDirection.AttackDirection attackDirection)
    {
        character.StartCoroutine(character.RotateTo(tile.Position));
        
        if (tile.CharacterReference)
        {
            character._attackTarget = tile.CharacterReference;
            character._attackTarget._attackDirection = attackDirection;
        }

        if (PreSfx != null)
        {
            AudioManager._Instance.SpawnSound(PreSfx);
        }
        character.CharacterAnimator.SetTrigger(AttackAnimationName);
        character._isCounterAttack = isAcounterAttack;

        if (tile.CharacterReference == null)
        {
            character.StartCoroutine(character.ThrowProjectile((tile.Position + new Vector3(0,_HeightGabToAdd,0)), _SpawnProjectileDelay, _ProjectilePrefab, _MovementSpeed, SfxAtSpawn));
            return;
        }
        
        tile.CharacterReference._IncomingAttacker = character;
        character.StartCoroutine(character.ThrowProjectile((tile.CharacterReference.gameObject.transform.position + new Vector3(0,_HeightGabToAdd,0)), _SpawnProjectileDelay, _ProjectilePrefab, _MovementSpeed, SfxAtSpawn));
    }
}
