using UnityEngine;

public  class StateChooseCharacter : State
{
   private int _numberOfCharactersToInstantiate;
   private DataCharacterSpawner.DataSpawner _dataSpawner ;
   public StateChooseCharacter(GameManager gameManager, int numberOfCharactersToInstantiate, DataCharacterSpawner.DataSpawner dataSpawner)
   {
      _gameManager = gameManager;
      _tilesManager = TilesManager.Instance;
      _numberOfCharactersToInstantiate = numberOfCharactersToInstantiate;
      _dataSpawner = dataSpawner;
   }
   public override void SelectTile(Tile tile)
   {
      _gameManager.NeedResetTiles = false;
         if (tile.CharacterReference != null) { return;}
         _numberOfCharactersToInstantiate--;
         _tilesManager.DeselectTiles();
         _gameManager.TileSelected = tile;
         _tilesManager.AddSelectedTile(tile);
         tile.SetTopMaterial(_tilesManager.MoveTileMaterial);
         _gameManager.SpawnCharacter(_gameManager.CharacterPrefab, tile, Vector3.zero, Character.Team.Team1,_dataSpawner.Ability1);
         if (_numberOfCharactersToInstantiate <= 0)
         {
            _gameManager.CurrentState = _gameManager.StateNavigation;
            _gameManager.NextCharacterTurn();
         }
   }
}
