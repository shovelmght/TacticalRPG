using UnityEngine;

[CreateAssetMenu(fileName = "new SwordAttack", menuName = "SwordAttack")]
public class SwordAttack : Attack
{
    public override void DoAttack(Character character, Tile tile, bool isAcounterAttack, GetAttackDirection.AttackDirection attackDirection)
    {
        
        character.StartCoroutine(character.RotateTo(tile.Position));
        
        if (tile.CharacterReference)
        {
            Debug.Log("SwordAttack tile.CharacterReference");
            tile.CharacterReference._IncomingAttacker = character;
            character._attackTarget = tile.CharacterReference;
            character._attackTarget._attackDirection = attackDirection;
        }
        Debug.Log("SwordAttack !tile.CharacterReference");
        character.CharacterAnimator.SetTrigger(AttackAnimationName);
        character._isCounterAttack = isAcounterAttack;
    }
}
