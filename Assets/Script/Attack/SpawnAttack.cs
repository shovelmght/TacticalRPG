using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new SpawnAttack", menuName = "SpawnAttack")]
public class SpawnAttack : Attack
{
    protected static readonly int Attack = Animator.StringToHash("Attack");
    [SerializeField] private GameObject SpawnPrefab;
    
    public override void DoAttack(Character character, Tile tile, bool isAcounterAttack, GetAttackDirection.AttackDirection attackDirection)
    {
        character.StartCoroutine(character.RotateTo(tile.Position));
        

        if (!tile.CharacterReference)
        {
            character.CharacterAnimator.SetTrigger(AttackAnimationName);
            tile.IsOccupied = true;
            character.StartCoroutine(character.SpawnAttack(PreSfx,SfxAtSpawn, SpawnPrefab, tile));
        }
        
    }
}
