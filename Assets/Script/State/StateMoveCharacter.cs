
public class StateMoveCharacter : State
{
    public StateMoveCharacter(GameManager gameManager)
    {
        _gameManager = gameManager;
        _tilesManager = TilesManager.Instance;
    }
    public override void SelectTile(Tile tile)
    {
        if(tile == _gameManager.TileSelected) return;
        if (tile.CanInteract)
        {
            if (_gameManager._IsMapScene && tile.CharacterReference != null)
            {
                _gameManager.StartCoroutine(_gameManager.MapSceneToBattleScene(tile.CharacterReference.gameObject.gameObject.name));
            }
            
            tile.CharacterReference =  GameManager.Instance.CurrentCharacter;
            _gameManager.TileSelected.UnSetCharacter();
            tile.SetCharacter(GameManager.Instance.CurrentCharacter);
            _gameManager.CurrentCharacter.SetMovementCharacter(tile);
            if (_gameManager._IsMapScene)
            {
                tile.MapTilesManager.AddSelectedTile(tile);
            }
            else
            {
                _tilesManager.AddSelectedTile(tile);
            }

            if (_gameManager._IsMapScene && _gameManager.IsController)
            {
                
            }
            else
            {
                tile.SetTopMaterial(_tilesManager.MoveTileMaterial); 
            }
            
            _gameManager.CurrentState =  _gameManager.StateNavigation;
            _gameManager.NeedResetTiles = false;
            _gameManager.TileSelected = tile;
            _gameManager.CurrentCharacter = tile.CharacterReference;
            if (_gameManager.DesableMoveCharacterUIButtons != null)
            {
                _gameManager.DesableMoveCharacterUIButtons();
            }
        }
    }
}
