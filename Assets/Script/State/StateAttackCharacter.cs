
using System.Numerics;
using Vector3 = UnityEngine.Vector3;

public class StateAttackCharacter : State
{
    private Tile _tileSelected;
    private Character _characterAttactedSelected;
    public Attack _Attack;
    
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
                UIBoardGame.Instance.ReturnToMenuFromAttack();
                _gameManager.IsCharactersAttacking = true;
                attackDirection = GetAttackDirection.SetAttackDirection(_gameManager.CurrentCharacter.transform.position,
                    _characterAttactedSelected.transform);
                _gameManager.StartCoroutine(_gameManager.SetBattleCamera(_gameManager.CurrentCharacter, tile.CharacterReference, attackDirection, false));
                _gameManager.CurrentCharacter.StartCoroutine(_gameManager.CurrentCharacter.Attack(tile, false, attackDirection, _Attack));
                _gameManager.DesableAttackCharacterUIButtons();
                _characterAttactedSelected = null;
            }
            else
            {
                _characterAttactedSelected = tile.CharacterReference;
                attackDirection = GetAttackDirection.SetAttackDirection(_gameManager.CurrentCharacter.transform.position,
                    _characterAttactedSelected.transform);
                _characterAttactedSelected.ShowCharacterHitSuccess((int)tile.CharacterReference.GetHitSuccess(attackDirection));
                _gameManager.CurrentCharacter.StartCoroutine(_gameManager.CurrentCharacter.RotateTo(tile.Position));
            }
        }
        else
        {
            _gameManager.StartCoroutine(_gameManager.ZoomBattleCamera());
            UIBoardGame.Instance.ReturnToMenuFromAttack();
            _gameManager.IsCharactersAttacking = true;
            _gameManager.CurrentCharacter.StartCoroutine(_gameManager.CurrentCharacter.Attack(tile, false, attackDirection, _Attack));
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
