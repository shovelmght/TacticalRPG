
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
            tile.CharacterReference =  GameManager.Instance.CurrentCharacter;
            _gameManager.TileSelected.UnSetCharacter();
            tile.SetCharacter(GameManager.Instance.CurrentCharacter);
            _gameManager.CurrentCharacter.SetMovementCharacter(tile);
            _tilesManager.AddSelectedTile(tile);
            tile.SetTopMaterial(_tilesManager.MoveTileMaterial);
            _gameManager.CurrentState =  _gameManager.StateNavigation;
            _gameManager.NeedResetTiles = false;
            _gameManager.TileSelected = tile;
            _gameManager.CurrentCharacter = tile.CharacterReference;
            _gameManager.DesableMoveCharacterUIButtons();
        }
    }
}
