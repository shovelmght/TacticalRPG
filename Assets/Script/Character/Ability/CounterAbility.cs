

public class CounterAbility
{

    private Character _character;
    private GameManager _gameManager;

    public CounterAbility(Character character, GameManager gameManager)
    {
        _character = character;
        _gameManager = gameManager;
        _character.HaveCounterAbility = true;
        character.CounterAttack += Counter;
    }

    private void Counter()
    {
        _character.Attack( _gameManager.CurrentCharacterTurn.CurrentTile, true, GetAttackDirection.AttackDirection.Font);
    }
}
