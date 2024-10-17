using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAIMapCharacter : MonoBehaviour
{
    [SerializeField] private MapTilesManager _CurrentMapTilesManager;
    [SerializeField] private  DataCharacterSpawner _CharacterDevilBossAIData;
    [SerializeField] private  GameManager _GameManager;
    private Character _Character;
    private static readonly int Move = Animator.StringToHash("Move");

    void Start()
    {
        if (_CharacterDevilBossAIData.DataSpawn[0].CharactersPrefab == DataCharacterSpawner.CharactersPrefab.WizardAI &&
            FBPP.GetBool("WizardBossIsDead"))
        {
            return;
        }

        if (_CharacterDevilBossAIData.DataSpawn[0].CharactersPrefab == DataCharacterSpawner.CharactersPrefab.RobotAI &&
            FBPP.GetBool("RobotBossIsDead"))
        {
            return;
        }

        if (_CharacterDevilBossAIData.DataSpawn[0].CharactersPrefab == DataCharacterSpawner.CharactersPrefab.MotherNatureAI &&
            FBPP.GetBool("FatherNatureBossIsDead"))
        {
            return;
        }

        if (_CharacterDevilBossAIData.DataSpawn[0].CharactersPrefab == DataCharacterSpawner.CharactersPrefab.DevilBossAI &&
            FBPP.GetBool("DevilBossIsDead"))
        {
            return;
        }
        
        StartCoroutine(Initialize());
    }
    
    private IEnumerator Initialize()
    {
        yield return new WaitForSeconds(5);
        Tile spawnTile = null;
        if (_CurrentMapTilesManager._Environement == Environement.Stone)
        {
            spawnTile = _CurrentMapTilesManager.GetTile(_CurrentMapTilesManager.TileManagerData.Row / 2, _CurrentMapTilesManager.TileManagerData.Column / 2);
        }
        else if (_CurrentMapTilesManager._Environement == Environement.Grass)
        {
            spawnTile = _CurrentMapTilesManager.GetTile(_CurrentMapTilesManager.TileManagerData.Row - 1, _CurrentMapTilesManager.TileManagerData.Column / 2);
        }
        else if (_CurrentMapTilesManager._Environement == Environement.Corner2)
        {
            spawnTile = _CurrentMapTilesManager.GetTile(_CurrentMapTilesManager.TileManagerData.Row - 1, 0);
        }
        else if (_CurrentMapTilesManager._Environement == Environement.Corner3)
        {
            spawnTile = _CurrentMapTilesManager.GetTile(_CurrentMapTilesManager.TileManagerData.Row - 1, _CurrentMapTilesManager.TileManagerData.Column - 1);
        }
        
        _Character = _GameManager.SpawnMapCharacter(spawnTile, Vector3.zero, _CharacterDevilBossAIData.DataSpawn[0], true, false);
        StartCoroutine(MoveToNextTile());


    }

    private IEnumerator MoveToNextTile()
    {
        yield return new WaitForSeconds(5);
        Tile spawnTile = null;
        
        while (spawnTile == null)
        {
            spawnTile = _Character.CurrentTile.SideTiles[Random.Range(0, 4)];
        }

        if (!_GameManager._GameIsFinish)
        {
            StartCoroutine(MoveCharacter(spawnTile));
            StartCoroutine(MoveToNextTile());
        }
    }
    
    private IEnumerator MoveCharacter(Tile tile)
    {
        _Character.CharacterAnimator.SetBool(Move, true);
        StartCoroutine(_Character.RotateTo(tile.Position));
        Vector3 spawnPosition = tile.Position;
        
        if (tile.IsWater)
        {
            spawnPosition = tile.Position + new Vector3(0, 0.3f, 0);
        }
        else
        {
            spawnPosition = tile.Position + new Vector3(0, 0.2f, 0);
        }

        yield return _Character.MoveTo(spawnPosition, _Character.MovingSpeed);
        
        if (tile.CharacterReference != null)
        {
            StartCoroutine(_GameManager.MapSceneToBattleScene(_Character.gameObject.name));
        }
        
        _Character.CharacterAnimator.SetBool(Move, false);
        _Character.CurrentTile.CharacterReference = null;
        _Character.CurrentTile = tile;
        tile.CharacterReference = _Character;
    }
}

