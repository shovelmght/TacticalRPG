using System.Collections.Generic;
using UnityEngine;

public  class StateChooseCharacter : State
{
   private int _numberOfCharactersToInstantiate;
   private List<DataCharacterSpawner.DataSpawner> _dataSpawner;

   public StateChooseCharacter(GameManager gameManager, int numberOfCharactersToInstantiate,  List<DataCharacterSpawner.DataSpawner> dataSpawner)
   {
      _gameManager = gameManager;
      _tilesManager = TilesManager.Instance;
      _numberOfCharactersToInstantiate = numberOfCharactersToInstantiate;
      _dataSpawner = dataSpawner;
   }
   public override void SelectTile(Tile tile)
   {
      _gameManager.NeedResetTiles = false;
         if (tile.IsOccupied || tile.CharacterReference != null || !tile.IsValidSpawnTile) { return;}
         _numberOfCharactersToInstantiate--;
         _gameManager.TileSelected = tile;
         _tilesManager.AddSelectedTile(tile);
         tile.SetTopMaterial(_tilesManager.MoveTileMaterial);
         _gameManager.SpawnCharacter(tile, Vector3.zero, _gameManager._CurrentCharacterDataSpawner);
         AudioManager._Instance.SpawnSound( AudioManager._Instance._SpawnCharacter);
         if (_numberOfCharactersToInstantiate <= 0)
         {
            _tilesManager.DeselectTiles();
            _tilesManager.UnselectValidSpawnTiles();
            _gameManager.CurrentState = _gameManager.StateNavigation;
            _gameManager.NextCharacterTurn();
         }
   }
}
