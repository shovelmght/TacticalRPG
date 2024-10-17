using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "new AreaCrossAttack", menuName = "AreaCrossAttack")]
public class AreaCrossAttack : Attack
{
    
    [SerializeField] private GameObject _ParticleEffectPrefab;
    [SerializeField] private bool _RemoveWater;

    public override void DoAttack(Character character, Tile tile, bool isAcounterAttack, GetAttackDirection.AttackDirection attackDirection)
    {
        character.StartCoroutine(ElementalAttack(tile, character));
    }

    private IEnumerator ElementalAttack(Tile tile, Character character)
    {
        TilesManager.Instance.DeselectTiles();
        
        if (_RemoveWater)
        {
            foreach (var sideTiles in tile.SideTiles)
            {
                if (sideTiles != null && sideTiles.IsWater && !sideTiles.IsOccupied && _ParticleEffectPrefab != null)
                {
                    Instantiate(_ParticleEffectPrefab, sideTiles.Position, Quaternion.identity);
                }
            }
            
            if (tile.IsWater && !tile.IsOccupied)
            {
                if (_ParticleEffectPrefab != null)
                {
                    Instantiate(_ParticleEffectPrefab, tile.Position, Quaternion.identity);
                }
            }
        }
        else
        {
            foreach (var sideTiles in tile.SideTiles)
            {
                if (sideTiles != null && _ParticleEffectPrefab != null && !sideTiles.IsOccupied && !sideTiles.IsWater && !sideTiles.IsBridge)
                {
                    Instantiate(_ParticleEffectPrefab, sideTiles.Position, Quaternion.identity);
                }
            }
            
            if (!tile.IsWater && !tile.IsOccupied  && !tile.IsBridge)
            {
                if (_ParticleEffectPrefab != null)
                {
                    Instantiate(_ParticleEffectPrefab, tile.Position, Quaternion.identity);
                }
            }
        }
                
        AudioManager._Instance.SpawnSound(PreSfx);
        character.CharacterAnimator.SetTrigger(AttackAnimationName);
        yield return new WaitForSeconds(0.5f);

        AudioManager._Instance.SpawnSound(SfxAtSpawn);

        yield return new WaitForSeconds(0.5f);

        if (_RemoveWater)
        {
            Material material = null;
            foreach (var allTile in TilesManager.Instance.GetAllTiles())
            {
                if (allTile.StartMaterial != TilesManager.Instance.TileManagerData.WaterTileMaterial)
                {
                    material = allTile.StartMaterial;
                    break;
                }
            }
            foreach (var sideTiles in tile.SideTiles)
            {
                if (sideTiles != null && sideTiles.IsWater && !sideTiles.IsOccupied)
                {
                    sideTiles.IsWater = false;
                    sideTiles.SetTopMaterial(material);
                    sideTiles.StartMaterial = material;
                    sideTiles.Height++;
                    Vector3 newTileLocation = new Vector3(0, 0.5f, 0);
                    sideTiles.CurrentGameObject.transform.position += newTileLocation;
                    sideTiles.Position += (newTileLocation + new Vector3(0, 0.5f, 0));
                }
            }
            
            if (tile.IsWater && !tile.IsOccupied)
            {
                tile.IsWater = false;
                tile.SetTopMaterial(material);
                tile.StartMaterial = material;
                tile.Height--;
                Vector3 newTileLocation = new Vector3(0, 0.5f, 0);
                tile.CurrentGameObject.transform.position += newTileLocation;
                tile.Position += (newTileLocation + new Vector3(0, 0.5f, 0));
            }
        }
        else
        {
            foreach (var sideTiles in tile.SideTiles)
            {
                if (sideTiles != null && !sideTiles.IsWater && !sideTiles.IsOccupied && !sideTiles.IsBridge)
                {
                    sideTiles.IsWater = true;
                    sideTiles.SetTopMaterial(TilesManager.Instance.TileManagerData.WaterTileMaterial);
                    sideTiles.StartMaterial = TilesManager.Instance.TileManagerData.WaterTileMaterial;
                    sideTiles.Height++;
                    Vector3 newTileLocation = new Vector3(0, -0.5f, 0);
                    sideTiles.CurrentGameObject.transform.position += newTileLocation;
                    sideTiles.Position += (newTileLocation + new Vector3(0, -0.5f, 0));
                }
            }
            
            if (!tile.IsWater && !tile.IsOccupied && !tile.IsBridge)
            {
                tile.IsWater = true;
                tile.SetTopMaterial(TilesManager.Instance.TileManagerData.WaterTileMaterial);
                tile.StartMaterial = TilesManager.Instance.TileManagerData.WaterTileMaterial;
                tile.Height--;
                Vector3 newTileLocation = new Vector3(0, -0.5f, 0);
                tile.CurrentGameObject.transform.position += newTileLocation;
                tile.Position += (newTileLocation + new Vector3(0, -0.5f, 0));
            }
        }
        
        yield return new WaitForSeconds(1.25f);
        character.HaveAttacked = true;
        GameManager.Instance.Wait = false;

        if (!character.IsAI && character.HaveMoved)
        {
            GameManager.Instance.SelectTile(GameManager.Instance.CurrentCharacterTurn.CurrentTile);
            GameManager.Instance.StartCoroutine( GameManager.Instance.ShowPossibleTileDirectionEndOfCharacterTurn(0.75f));
            InputManager.Instance._TempSelectTileMaterial = null;
        }
        
        else if (!character.IsAI && GameManager.Instance.IsController)
        {
            
            GameManager.Instance.SelectTile(GameManager.Instance.CurrentCharacterTurn.CurrentTile);
            GameManager.Instance.SelectCharacter?.Invoke();
            InputManager.Instance._TempSelectTileMaterial = null;
        }
    }
}
