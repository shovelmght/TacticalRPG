
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
        Debug.Log("Bug Controller wait StateTurnCharacter SelectTile 11 tile = " + tile.CoordX +" " + tile.CoordY );
        if (tile == _gameManager.TileSelected || !tile.CanInteract)
        {
            Debug.Log("Bug Controller wait StateTurnCharacter SelectTile 22 tile = " + tile.CoordX +" " + tile.CoordY );
            _gameManager.NeedResetTiles = false;
        }
        else
        {
            Debug.Log("Bug Controller wait StateTurnCharacter SelectTile 33 tile = " + tile.CoordX +" " + tile.CoordY );
            _gameManager.TilePreSelected = tile;
            _gameManager.CurrentCharacter.TurnCharacter(tile);
            //_gameManager.ArrowsDirection.SetActive(false);
            _tilesManager.DeselectTiles();
            _gameManager.NeedResetTiles = false;
            _gameManager.NextCharacterTurn();
        }
    }
}
