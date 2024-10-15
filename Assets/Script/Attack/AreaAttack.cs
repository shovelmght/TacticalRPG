using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "new AreaAttack", menuName = "AreaAttack")]
public class AreaAttack : Attack
{
    protected static readonly int Attack = Animator.StringToHash("Attack");
    [SerializeField] private GameObject _ParticleEffectPrefab;
    [SerializeField] private GameObject _TileParticleEffectPrefab;
    [SerializeField] private bool _SelfDestroyAfterAttack;
    


    public override void DoAttack(Character character, Tile tiles, bool isAcounterAttack, GetAttackDirection.AttackDirection attackDirection)
    {

        character.StartCoroutine(ExplosionAttack(character));
        Debug.Log(GameManager.Instance.OccupiedTiles.Length);
    }

    private IEnumerator ExplosionAttack(Character character)
    {
        if (_TileParticleEffectPrefab != null)
        {
            for (int i = 0; i < TilesManager.Instance.GetSelectedTileLenght(); i++)
            {
                Instantiate(_TileParticleEffectPrefab, TilesManager.Instance.GetSelectedTile(i).Position, Quaternion.identity);
            }
        }
        TilesManager.Instance.DeselectTiles();
        AudioManager._Instance.SpawnSound(PreSfx);
        character.CharacterAnimator.SetTrigger(AttackAnimationName);
        yield return new WaitForSeconds(0.5f);
        if (_SelfDestroyAfterAttack)
        {
            character.transform.GetChild(0).gameObject.SetActive(false);
        }
        AudioManager._Instance.SpawnSound(SfxAtSpawn);
        GameObject _ParticleEffectGo =
            Instantiate(_ParticleEffectPrefab, character.transform.position + new Vector3(0, 0.25f, 0), Quaternion.Euler(-90, 0, 0));
        _ParticleEffectGo.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        yield return new WaitForSeconds(0.5f);

        Instantiate(_ParticleEffectPrefab, character.transform.position, Quaternion.Euler(-90, 0, 0));
        yield return new WaitForSeconds(0.25f);
  

        bool characterAttackedHaveCounterAbility = false;
        for (int i = 0; i < GameManager.Instance.IndexOccupiedTiles; i++)
        {
            Character characterReference = GameManager.Instance.OccupiedTiles[i].CharacterReference;

            if (characterReference == null || characterReference == character)
            {
                continue;
            }

            AudioManager._Instance.SpawnSound(GameManager.Instance.StateAttackCharacter._Attack.ImpactSfx);
            characterReference.IsAttacked(Power * character.Strength, character._isCounterAttack, _IsFire);
            
            if (characterReference.HaveCounterAbility)
            {
                characterAttackedHaveCounterAbility = true;
            }
        }

        if (_SelfDestroyAfterAttack)
        {
          
            if (character.IsAI)
            {
                if (GameManager.Instance.CurrentCharacter != null && GameManager.Instance.CurrentCharacterTurn == character)
                {
                    GameManager.Instance.NextCharacterTurn();
                }
            
            }
            character.DestroyCharacter();
        }
        else
        {
            yield return new WaitForSeconds(3);
            character.HaveAttacked = true;
            
            if (!characterAttackedHaveCounterAbility)
            {
                GameManager.Instance.Wait = false;
            }
        }
    }
}
