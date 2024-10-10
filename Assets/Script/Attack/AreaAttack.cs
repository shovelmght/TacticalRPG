using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "new AreaAttack", menuName = "AreaAttack")]
public class AreaAttack : Attack
{
    protected static readonly int Attack = Animator.StringToHash("Attack");
    [SerializeField] private GameObject _ParticleEffectPrefab;
    [SerializeField] private float _SpawnProjectileDelay = 1.0f;
    [SerializeField] private float _HeightGabToAdd = 10.0f;
    [SerializeField] private float _MovementSpeed = 10.0f;

    public override void DoAttack(Character character, Tile tiles, bool isAcounterAttack, GetAttackDirection.AttackDirection attackDirection)
    {

        character.StartCoroutine(ExplosionAttack(character));
        Debug.Log(GameManager.Instance.OccupiedTiles.Length);

        /*if (tile.CharacterReference)
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
        character.StartCoroutine(character.ThrowProjectile((tile.CharacterReference.gameObject.transform.position + new Vector3(0,_HeightGabToAdd,0)), _SpawnProjectileDelay, _ProjectilePrefab, _MovementSpeed, SfxAtSpawn));*/
    }

    private IEnumerator ExplosionAttack(Character character)
    {
        TilesManager.Instance.DeselectTiles();
        AudioManager._Instance.SpawnSound(PreSfx);
        character.CharacterAnimator.SetTrigger(AttackAnimationName);
        yield return new WaitForSeconds(1);
        AudioManager._Instance.SpawnSound(SfxAtSpawn);
        GameObject _ParticleEffectGo =
            Instantiate(_ParticleEffectPrefab, character.transform.position + new Vector3(0, 0.25f, 0), Quaternion.Euler(-90, 0, 0));
        _ParticleEffectGo.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        yield return new WaitForSeconds(0.25f);
        Instantiate(_ParticleEffectPrefab, character.transform.position, Quaternion.Euler(-90, 0, 0));

        for (int i = 0; i < GameManager.Instance.IndexOccupiedTiles; i++)
        {
            Character characterReference = GameManager.Instance.OccupiedTiles[i].CharacterReference;

            if (characterReference == null || characterReference == character)
            {
                continue;
            }

            AudioManager._Instance.SpawnSound(GameManager.Instance.StateAttackCharacter._Attack.ImpactSfx);
            characterReference.IsAttacked(Power * character.Strength, character._isCounterAttack);
        }

        if (character.IsAI)
        {
            GameManager.Instance.NextCharacterTurn();
        }
        character.DestroyCharacter();
       
        
    }



}
