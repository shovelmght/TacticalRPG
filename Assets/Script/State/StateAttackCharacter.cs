
public class StateAttackCharacter : State
{
    private Tile _tileSelected;
    private Character _characterAttactedSelected;
    
    public StateAttackCharacter(GameManager boardManager)
    {
        _gameManager = boardManager;
        _tilesManager = TilesManager.Instance;
    }
    public override void SelectTile(Tile tile)
    {
        if (!tile.CanInteract || tile == _gameManager.TileSelected) {return; }
        _gameManager.NeedResetTiles = false;

        GetAttackDirection.AttackDirection attackDirection = GetAttackDirection.AttackDirection.None;
        if (tile.CharacterReference)
        {
            if (tile.CharacterReference == _characterAttactedSelected)
            {
                _gameManager.CurrentCharacter.Attack(tile, false, attackDirection);
                _gameManager.DesableAttackCharacterUIButtons();
                _characterAttactedSelected = null;
            }
            else
            {
                _characterAttactedSelected = tile.CharacterReference;
                attackDirection = GetAttackDirection.SetAttackDirection(_gameManager.CurrentCharacter.transform.position,
                    _characterAttactedSelected.transform);
                _characterAttactedSelected.ShowCharacterHitSuccess((int)tile.CharacterReference.GetHitSuccess(attackDirection));
            }

        }
        else
        {
            _gameManager.CurrentCharacter.Attack(tile, false, attackDirection);
        }
    }

    public void ResetAttackData()
    {
        if (_characterAttactedSelected != null)
        {
            _characterAttactedSelected.RemoveUIPopUpCharacterInfo(false);
            _characterAttactedSelected = null;
        }
    }
}
