
using UnityEngine;

public class StateTurnCharacter : State
{
    public StateTurnCharacter(GameManager boardManager)
    {
        _gameManager = boardManager;
        _tilesManager = TilesManager.Instance;
    }

    public override void SelectTile(Tile tile)
    {
        if (tile == _gameManager.TileSelected || !tile.CanInteract)
        {
            _gameManager.NeedResetTiles = false;
        }
        else
        {
            _gameManager.TilePreSelected = tile;
            _gameManager.CurrentCharacter.TurnCharacter(tile);
            //_gameManager.ArrowsDirection.SetActive(false);
            _tilesManager.DeselectTiles();
            _gameManager.NeedResetTiles = false;
            _gameManager.NextCharacterTurn();
        }
    }
}
