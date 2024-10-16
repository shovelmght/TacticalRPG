

using UnityEngine;

public class CounterAbility
{

    private Character _character;
    private GameManager _gameManager;

    public CounterAbility(Character character, GameManager gameManager)
    {
        _character = character;
        _gameManager = gameManager;
        _character.HaveCounterAbility = true;
        character.ActionCounterAttack += Counter;
    }

    private void Counter()
    {
        GetAttackDirection.AttackDirection attackDirection =  GetAttackDirection.SetAttackDirection(_character.transform.position, _gameManager.CurrentCharacter.transform);
        _gameManager.StateAttackCharacter._Attack = _character._Attack;
        _gameManager.StartCoroutine(_gameManager.SetBattleCamera(_character, _gameManager.CurrentCharacter, attackDirection, true));
        _character.StartCoroutine(_character.Attack( _gameManager.CurrentCharacterTurn.CurrentTile, true, attackDirection, _character._Attack));
    }
}
