using System.Collections.Generic;
using UnityEngine;

public  class StateChooseCharacter : State
{
   private int _numberOfCharactersToInstantiate;


   public StateChooseCharacter(GameManager gameManager, int numberOfCharactersToInstantiate)
   {
      _gameManager = gameManager;
      _tilesManager = TilesManager.Instance;
      _numberOfCharactersToInstantiate = numberOfCharactersToInstantiate;
   }
   public override void SelectTile(Tile tile)
   {
      _gameManager.NeedResetTiles = false;
         if (tile.IsOccupied || tile.CharacterReference != null || !tile.IsValidSpawnTile) { return;}
         _numberOfCharactersToInstantiate--;
         _gameManager.TileSelected = tile;
         _tilesManager.AddSelectedTile(tile);
         tile.SetTopMaterial(_tilesManager.MoveTileMaterial);
         _gameManager.SpawnCharacter(tile, Vector3.zero, _gameManager._PlayerCharacterSpawnerList.SpawnCurrentCharacterSelected());
         _gameManager._PlayerCharacterSpawnerList.NextCharacter();
         AudioManager._Instance.SpawnSound( AudioManager._Instance._SpawnCharacter);
         
         if (_numberOfCharactersToInstantiate <= 0 || !_gameManager._PlayerCharacterSpawnerList.HasRemainingCharacters())
         {
            _tilesManager.DeselectTiles();
            _tilesManager.UnselectValidSpawnTiles();
            _gameManager.CurrentState = _gameManager.StateNavigation;
            _gameManager.NextCharacterTurn();
            _gameManager._PlayerCharacterSpawnerList.gameObject.SetActive(false);
         }
   }
}
