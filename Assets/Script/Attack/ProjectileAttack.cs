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
    
    public override void DoAttack(Character character, Tile tile, bool isAcounterAttack, GetAttackDirection.AttackDirection attackDirection)
    {
        character.StartCoroutine(character.RotateTo(tile.Position));
        
        
        if (tile.CharacterReference)
        {
            character._attackTarget = tile.CharacterReference;
            character._attackTarget._attackDirection = attackDirection;
        }

        character.CharacterAnimator.SetTrigger(Attack);
        character._isCounterAttack = isAcounterAttack;
        if (character == null)
        {
            Debug.LogError("character == null");
        }
        
        if (tile.CharacterReference == null)
        {
            Debug.LogError("tile.CharacterReference == null");  //   this one
        }
        
        if (tile.CharacterReference.gameObject == null)
        {
            Debug.LogError("tile.CharacterReference.gameObject == null");
        }
        
        if (tile.CharacterReference.gameObject.transform == null)
        {
            Debug.LogError("tile.CharacterReference.gameObject.transform == null");
        }
        
        if (_ProjectilePrefab == null)
        {
            Debug.LogError("_ProjectilePrefab == null");
        }
        character.StartCoroutine(character.ThrowProjectile((tile.CharacterReference.gameObject.transform.position + new Vector3(0,_HeightGabToAdd,0)), _SpawnProjectileDelay, _ProjectilePrefab));
    }
}
