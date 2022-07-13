
public class StateNavigation : State
{
    public StateNavigation(GameManager boardManager)
    {
        _gameManager = boardManager;
        _tilesManager = TilesManager.Instance;
    }
    public override void SelectTile(Tile tile)
    {
        
        if (tile.CharacterReference == null) { return; }
        
        
        if (tile.CharacterReference != _gameManager.CurrentCharacter )
        {
            _gameManager.CurrentCharacter.RemoveUIPopUpCharacterInfo(true);
            tile.CharacterReference.ShowUIPopUpCharacterInfo(false, true);
            _gameManager.RemoveUICharacter();
        }
        else
        {
            tile.CharacterReference.ShowUIPopUpCharacterInfo(false, false);
        }
   
        _tilesManager.DeselectTiles();
        _tilesManager.AddSelectedTile(tile);
        tile.SetTopMaterial(_tilesManager.MoveTileMaterial);
        _gameManager.TileSelected = tile;
        if (tile.CharacterReference == _gameManager.CurrentCharacterTurn)
        { 
            _gameManager.SelectCharacter();
        }
        _gameManager.NeedResetTiles = false;
 
        _gameManager.CurrentCharacter = tile.CharacterReference;
        }

    
}
