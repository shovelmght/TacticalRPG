
using System.Numerics;
using Vector3 = UnityEngine.Vector3;

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
                Vector3 midCameraPosition = (tile.Position + _gameManager.CurrentCharacter.CurrentTile.Position) / 2;
                midCameraPosition = new Vector3(midCameraPosition.x + 5, 5, midCameraPosition.z);
                _gameManager.MoveBoardCamera(midCameraPosition);
                attackDirection = GetAttackDirection.SetAttackDirection(_gameManager.CurrentCharacter.transform.position,
                    _characterAttactedSelected.transform);
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
