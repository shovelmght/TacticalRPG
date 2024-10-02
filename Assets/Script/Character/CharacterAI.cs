using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
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
        
        _gameManager.ShowPossibleAttack(CurrentTile, false, CurrentTile.IsWater ? _WaterAttack : _Attack);

        yield return new WaitUntil(() => _gameManager.PossibleTileIsFinished);

        Tile enemyTile = CheckIfOccupiedTileAreEnemy();
        bool isSkillAttack = false;

        if (enemyTile == null)
        {
            isSkillAttack = true;
            _gameManager.ShowPossibleAttack(CurrentTile, false, _SkillAttack);
            yield return new WaitUntil(() => _gameManager.PossibleTileIsFinished);
            enemyTile = CheckIfOccupiedTileAreEnemy();
            
            if (enemyTile != null && _SkillAttack.IsDashAttack)
            {
                Debug.Log("CurrentTile.CoordX = " + CurrentTile.CoordX + "  CurrentTile.CoordY = " + CurrentTile.CoordY + "  enemyTile.CoordX = " + enemyTile.CoordX + "  enemyTile.CoordY = " + enemyTile.CoordY);
                enemyTile = FindTileBehind(CurrentTile, enemyTile);
                if (enemyTile != null)
                {
                    Debug.Log("BehindTile.CoordX = " + enemyTile.CoordX + "  BehindTile.CoordY = " + enemyTile.CoordY);
                }
            }
        }
        
        if (enemyTile != null)
        {
            if (isSkillAttack)
            {
                _gameManager.StateAttackCharacter._Attack = _SkillAttack;
            }
            else
            {
                _gameManager.StateAttackCharacter._Attack = CurrentTile.IsWater ? _WaterAttack : _Attack;
            }
            _gameManager.IsAIChatacterTurn = false;

            yield return new WaitForSeconds(0.5f);
            
            if (!_gameManager.StateAttackCharacter._Attack.IsDashAttack)
            {
                _gameManager.SelectTile(enemyTile);
            }
            
            _gameManager.SelectTile(enemyTile);
            
            yield return new WaitUntil(() => HaveAttacked);
            yield return new WaitUntil(() => !_gameManager.Wait);
            if (CurrentHealth <= 0)
            {
                _gameManager.NextCharacterTurn();
                yield break;
            }
            Debug.Log("CharacterAI Before ShowPossibleMove1");


            if (CanMove)
            {
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
                StartCoroutine(_gameManager.EndOfCharacterTurn(0));
                _gameManager.PossibleTileIsFinished = false;
                yield return new WaitUntil(() => _gameManager.PossibleTileIsFinished);
                _gameManager.SelectTile(GetNearestTile());
            }

        }
        else
        {
            if (!CanMove)
            {
                StartCoroutine(_gameManager.EndOfCharacterTurn(0));
                _gameManager.PossibleTileIsFinished = false;
                yield return new WaitUntil(() => _gameManager.PossibleTileIsFinished);
                _gameManager.SelectTile(GetNearestTile());
                yield break;
            }

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

                _gameManager.ShowPossibleAttack(CurrentTile, false, CurrentTile.IsWater ? _WaterAttack : _Attack);

                yield return new WaitUntil(() => _gameManager.PossibleTileIsFinished);
                enemyTile = CheckIfOccupiedTileAreEnemy();
                
                isSkillAttack = false;

                if (enemyTile == null)
                {
                    isSkillAttack = true;
                    _gameManager.ShowPossibleAttack(CurrentTile, false, _SkillAttack);
                    yield return new WaitUntil(() => _gameManager.PossibleTileIsFinished);
                    enemyTile = CheckIfOccupiedTileAreEnemy();
                    
                    if (enemyTile != null && _SkillAttack.IsDashAttack)
                    {
                        Debug.Log("CurrentTile.CoordX = " + CurrentTile.CoordX + "  CurrentTile.CoordY = " + CurrentTile.CoordY + "  enemyTile.CoordX = " + enemyTile.CoordX + "  enemyTile.CoordY = " + enemyTile.CoordY);
                        enemyTile = FindTileBehind(CurrentTile, enemyTile);
                        if (enemyTile != null)
                        {
                            Debug.Log("BehindTile.CoordX = " + enemyTile.CoordX + "  BehindTile.CoordY = " + enemyTile.CoordY);
                        }
                    }
                }
        
                if (enemyTile != null)
                {
                    if (isSkillAttack)
                    {
                        _gameManager.StateAttackCharacter._Attack = _SkillAttack;
                    }
                    else
                    {
                        _gameManager.StateAttackCharacter._Attack = CurrentTile.IsWater ? _WaterAttack : _Attack;
                    }
                    _gameManager.IsAIChatacterTurn = false;

                    yield return new WaitForSeconds(0.5f);
                    
                    if (!_gameManager.StateAttackCharacter._Attack.IsDashAttack)
                    {
                        _gameManager.SelectTile(enemyTile);
                    }
                    
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
                
                _gameManager.ShowPossibleAttack(CurrentTile, false, CurrentTile.IsWater ? _WaterAttack : _Attack);
           
                yield return new WaitUntil(() => _gameManager.PossibleTileIsFinished);
                enemyTile = CheckIfOccupiedTileAreEnemy();
                
                isSkillAttack = false;

                if (enemyTile == null)
                {
                     Debug.Log("CharacterAI Before ShowPossibleAttack4");
                    isSkillAttack = true;
                    _gameManager.ShowPossibleAttack(CurrentTile, false, _SkillAttack);
                    yield return new WaitUntil(() => _gameManager.PossibleTileIsFinished);
                    enemyTile = CheckIfOccupiedTileAreEnemy();

                    if (enemyTile != null && _SkillAttack.IsDashAttack)
                    {
                        Debug.Log("CurrentTile.CoordX = " + CurrentTile.CoordX + "  CurrentTile.CoordY = " + CurrentTile.CoordY + "  enemyTile.CoordX = " + enemyTile.CoordX + "  enemyTile.CoordY = " + enemyTile.CoordY);
                        enemyTile = FindTileBehind(CurrentTile, enemyTile);
                        if (enemyTile != null)
                        {
                            Debug.Log("BehindTile.CoordX = " + enemyTile.CoordX + "  BehindTile.CoordY = " + enemyTile.CoordY);
                        }
                    }
                }

                bool isSkillSpawnAttack = _SkillAttack.IsSpawnSkill && isSkillAttack;
        
                if (enemyTile != null && !isSkillSpawnAttack)
                {
                    if (isSkillAttack)
                    {
                        _gameManager.StateAttackCharacter._Attack = _SkillAttack;
                    }
                    else
                    {
                        _gameManager.StateAttackCharacter._Attack = CurrentTile.IsWater ? _WaterAttack : _Attack;
                    }
                    _gameManager.IsAIChatacterTurn = false;

                    yield return new WaitForSeconds(0.5f);
                    if (!_gameManager.StateAttackCharacter._Attack.IsDashAttack)
                    {
                        _gameManager.SelectTile(enemyTile);
                    }
                    
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

                if (_SkillAttack.IsSpawnSkill && enemyTile == null)
                {
                    Debug.Log("CharacterAI Before ShowPossible SpawnSkill");
                    _gameManager.ShowPossibleAttack(CurrentTile, false, _SkillAttack);
                    yield return new WaitUntil(() => _gameManager.PossibleTileIsFinished);
                    _gameManager.StateAttackCharacter._Attack = _SkillAttack;
                    List<Tile> possibleSpawnTile = new List<Tile>();
                    
                    for (int i = 0; i < CurrentTile.SideTiles.Length; i++)
                    {
                        if (CurrentTile.SideTiles[i] != null && !CurrentTile.SideTiles[i].IsOccupied)
                        {
                            possibleSpawnTile.Add(CurrentTile.SideTiles[i]);
                        }
                    }

                    if (possibleSpawnTile.Count > 0)
                    {
                        int randomInt = Random.Range(0, possibleSpawnTile.Count);
                        _gameManager.SelectTile(possibleSpawnTile[randomInt]);
                    }
                    else
                    {
                        _gameManager.SelectTile(GetNearestTile());
                    }

                    yield return new WaitForSeconds(4);

                }
                

                StartCoroutine(_gameManager.EndOfCharacterTurn(0));
                _gameManager.PossibleTileIsFinished = false;
                yield return new WaitUntil(() => _gameManager.PossibleTileIsFinished);
                _gameManager.SelectTile(GetNearestTile());
            }
        }
    }
    
    
    // Function to find the tile behind the enemy
    public Tile FindTileBehind(Tile currentTile, Tile enemyTile)
    {
        int directionX = currentTile.CoordX - enemyTile.CoordX;
        int directionY = currentTile.CoordY - enemyTile.CoordY;

        // Normalize the direction to -1, 0, or 1 (so we move only by one tile in each direction)
        directionX = directionX != 0 ? (directionX / Mathf.Abs(directionX)) : 0;
        directionY = directionY != 0 ? (directionY / Mathf.Abs(directionY)) : 0;

        // Apply the opposite direction to the enemy tile to find the tile behind
        int behindTileX = enemyTile.CoordX - directionX;
        int behindTileY = enemyTile.CoordY - directionY;

        if (behindTileX < 0 || behindTileY < 0 || behindTileX > _tileManager.TileManagerData.Column - 1 ||
            behindTileY > _tileManager.TileManagerData.Row - 1)
        {
            return null;
        }

        Tile tileBehind = _tileManager.GetTile(behindTileX, behindTileY);

        
        for (int i = 0; i < _tileManager.GetSelectedTileLenght(); i++)
        {
            if (!_tileManager.GetSelectedTile(i).IsOccupied && _tileManager.GetSelectedTile(i) == tileBehind)
            {
                return tileBehind;
            }
        }
        return null;
        
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
