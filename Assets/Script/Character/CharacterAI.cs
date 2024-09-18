using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class CharacterAI : Character
{
    [SerializeField] private float _TimePathFinding = 1.25f;

    protected override void Start()
    {
        _tileManager = TilesManager.Instance;
        _gameManager = GameManager.Instance;
        CurrentHealth = MaxHealth;
        IsAI = true;
        
        if (_RandomMaterial)
        {
            int randomMaterialIndex = Random.Range(0, _Materials.Length);
            _SkinnedMeshRenderer.material = _Materials[randomMaterialIndex];
        }

    }
    
    public override void ResetCharacterTurn()
    {
        HaveMoved = false;
        HaveAttacked = false;
        _gameManager.CurrentCharacter = this;
        _gameManager.RemoveUICharacter();
        StartCoroutine( EnemyTurn());
        TurnTimeRemaining = START_TIME_TURN;
    }
    
 // Enemy AI
    private IEnumerator EnemyTurn()
    {
        //If he can already Attack his enemy
        yield return new WaitUntil(() => !_gameManager.CameraIsMoving);
        Debug.Log("CharacterAI Before ShowPossibleAttack1");
        _gameManager.ShowPossibleAttack(CurrentTile, false, _Attack);
        yield return new WaitUntil(() => _gameManager.PossibleTileIsFinished);

        Tile enemyTile = CheckIfOccupiedTileAreEnemy();
        
        if (enemyTile != null)
        {
            _gameManager.StateAttackCharacter._Attack = _Attack;
            _gameManager.SelectTile(enemyTile);
            _gameManager.SelectTile(enemyTile);
            
            yield return new WaitUntil(() => HaveAttacked);
            yield return new WaitUntil(() => !_gameManager.Wait);
            if (CurrentHealth <= 0)
            {
                _gameManager.NextCharacterTurn();
                yield break;
            }
            Debug.Log("CharacterAI Before ShowPossibleMove1");
            _gameManager.ShowPossibleMove(CurrentTile);
            yield return new WaitForSeconds(_TimePathFinding);
            _gameManager.SelectTile(_tileManager.GetSelectedTile(Random.Range(1,_tileManager.GetSelectedTileLenght()))); 
            _gameManager.PossibleTileIsFinished = false;
            yield return new WaitUntil(() => _gameManager.PossibleTileIsFinished);
            Debug.Log("CharacterAI Before GetNearestTile1");
            _gameManager.SelectTile(GetNearestTile());
        }
        else
        {
            //Moves towards his enemy
            Debug.Log("CharacterAI Before ShowPossibleMove2");
            _gameManager.ShowPossibleMove(CurrentTile);
            yield return new WaitForSeconds(_TimePathFinding);
            //If his enemy is a on moveTile
            Debug.Log("CharacterAI FindBestPossibleMoveTile");
            enemyTile = FindBestPossibleMoveTile();
            if (enemyTile != null)
            {
                _gameManager.CurrentState = _gameManager.StateMoveCharacter;
                _gameManager.SelectTile(enemyTile);
                yield return new WaitUntil(() => HaveMoved);
                Debug.Log("CharacterAI Before ShowPossibleAttack2");
                _gameManager.ShowPossibleAttack(CurrentTile, false, _Attack);
                yield return new WaitUntil(() => _gameManager.PossibleTileIsFinished);
                enemyTile = CheckIfOccupiedTileAreEnemy();
                if (enemyTile != null)
                {
                    Debug.Log("CharacterAI Before Attack");
                    _gameManager.StateAttackCharacter._Attack = _Attack;
                    _gameManager.SelectTile(enemyTile);
                    _gameManager.SelectTile(enemyTile);
               
                    Debug.Log("CharacterAI after Attack");
                    yield return new WaitUntil(() => !_gameManager.Wait);
                    if (CurrentHealth <= 0)
                    {
                        _gameManager.NextCharacterTurn();
                        yield break;
                    }
                    _gameManager.PossibleTileIsFinished = false;
                    yield return new WaitUntil(() => _gameManager.PossibleTileIsFinished);
                    Debug.Log("CharacterAI before wait");
                    Tile nearestTile = GetNearestTile();
                    if (nearestTile != null)
                    {
                        _gameManager.SelectTile(nearestTile);
                    }
                    else
                    {
                        Debug.LogError("nearestTile IS null");
                    }
                   
                    Debug.Log("CharacterAI after wait");
                }
            }
            else
            {
                //Find nearest possible MoveTile to reach his enemy 
                Debug.Log("CharacterAI Before GetNearestTile2");
                _gameManager.SelectTile(GetNearestTile());
                yield return new WaitUntil(() => HaveMoved);
                Debug.Log("CharacterAI Before ShowPossibleAttack3");
                _gameManager.ShowPossibleAttack(CurrentTile, false, _Attack);
                yield return new WaitUntil(() => _gameManager.PossibleTileIsFinished);
                enemyTile = CheckIfOccupiedTileAreEnemy();
                if (enemyTile != null)
                {
                    _gameManager.StateAttackCharacter._Attack = _Attack;
                    _gameManager.SelectTile(enemyTile);
                    _gameManager.SelectTile(enemyTile);
                    _gameManager.PossibleTileIsFinished = false;
                    yield return new WaitUntil(() => _gameManager.PossibleTileIsFinished);
                    yield return new WaitUntil(() => !_gameManager.Wait);
                    if (CurrentHealth <= 0)
                    {
                        _gameManager.NextCharacterTurn();
                        yield break;
                    }
                }
                StartCoroutine(_gameManager.EndOfCharacterTurn(0));
                _gameManager.PossibleTileIsFinished = false;
                yield return new WaitUntil(() => _gameManager.PossibleTileIsFinished);
                _gameManager.SelectTile(GetNearestTile());
            }
        }
    }
    
    //Find Nearest tile 
    public Tile GetNearestTile()
    {
        int minDistance = 1000;
        Tile nearestTile = null;
        for (int i = _tileManager.GetSelectedTileLenght() - 1; i > 0; i--)
        {
            int distance = GetPlayerDistance(_tileManager.GetSelectedTile(i), CurrentTeam);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestTile = _tileManager.GetSelectedTile(i);
            }
        }

        return nearestTile;
    }
    
    //Get nearest distance between Tile and all player
    private int GetPlayerDistance(Tile tile, Team team)
    {
        int minDistance = 1000;
        
        for (int i = 0; i < _gameManager.CharacterList.Count; i++)
        {
            if (_gameManager.CharacterList[i].CurrentTeam != team)
            {
                int a = _gameManager.CharacterList[i].CurrentTile.CoordX - tile.CoordX;
                a *= a;
                int b = _gameManager.CharacterList[i].CurrentTile.CoordY - tile.CoordY;
                b *= b;
                int c = a + b;
                float distance = Mathf.Sqrt(c);

                if (distance < minDistance)
                {
                    minDistance = (int)distance;
                }
            }
        }
        
        return  minDistance;
    }

    private Tile CheckIfOccupiedTileAreEnemy()
    {
        for (int i = 0; i < _gameManager.IndexOccupiedTiles; i++)
        {
            if (_gameManager.OccupiedTiles[i].CharacterReference == null) { continue;}
            if (_gameManager.OccupiedTiles[i].CharacterReference.CurrentTeam != CurrentTeam)
            {
                return _gameManager.OccupiedTiles[i];
            }
        }
        return null;
    }

    private Tile FindBestPossibleMoveTile()
    {
        Tile bestMatchTile = null;
        GetAttackDirection.AttackDirection attackDirection = GetAttackDirection.AttackDirection.None;

        for (int i = 0; i < _tileManager.GetSelectedTileLenght(); i++)
        {
            _gameManager.ShowPossibleAttack(_tileManager.GetSelectedTile(i), true, _Attack); 
            for (int j = 0; j < _gameManager.IndexOccupiedTiles; j++)
            {
                if (_gameManager.OccupiedTiles[j].CharacterReference == null) { continue;}

                if (_gameManager.OccupiedTiles[j].CharacterReference.CurrentTeam != CurrentTeam)
                {
                    if (bestMatchTile == null)
                    {
                        bestMatchTile = _tileManager.GetSelectedTile(i);
                        attackDirection =
                            GetAttackDirection.SetAttackDirection(bestMatchTile.Position, _gameManager.OccupiedTiles[j].CharacterReference.transform);
                    }
                    else
                    {
                        GetAttackDirection.AttackDirection tempAttackDirection =
                            GetAttackDirection.SetAttackDirection(_tileManager.GetSelectedTile(i).Position, _gameManager.OccupiedTiles[j].CharacterReference.transform);
                        if (tempAttackDirection > attackDirection)
                        {
                            bestMatchTile = _tileManager.GetSelectedTile(i);
                        }
                        
                    }
                }
            }
        }

        return bestMatchTile;
        
    }
}
