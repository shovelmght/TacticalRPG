using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using Random = UnityEngine.Random;

public class CharacterAI : Character
{
    private const float TIME_PATHFINDING = 0.75f;
    private const float TIME_MOVECHARACTER = 0.75f;
    private const float TIME_ATTACKCHARACTER = 2.25f;

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
        if (_IsPoisoned > 0)
        {
            StartCoroutine(SetPoisonDamage());

        }
        HaveMoved = false;
        HaveAttacked = false;
        _gameManager.CurrentCharacter = this;
        _gameManager.RemoveUICharacter();
        StartCoroutine( EnemyTurn());
        TurnTimeRemaining = START_TIME_TURN;
    }

    private bool _TimePathfindingIsFinish;
    private Coroutine _TimePathfindingCoroutine;

    private IEnumerator TimePathFinding()
    {
        _TimePathfindingIsFinish = false;
        yield return new WaitForSeconds(TIME_PATHFINDING);
        _TimePathfindingIsFinish = true;
    }

    private void ShowNormalPossibleAttack()
    {
        if (_WaterAttack != null)
        {
            if (CurrentTile.IsWater)
            {
                if (_gameManager._tileManager.TileManagerData._IsLava)
                {
                    _gameManager.ShowPossibleAttack(CurrentTile, false, _FireAttack);
                }
                else if (_gameManager._tileManager.TileManagerData._IsPoison)
                {
                    _gameManager.ShowPossibleAttack(CurrentTile, false, _PoisonAttack);
                }
                else
                {
                    _gameManager.ShowPossibleAttack(CurrentTile, false, _WaterAttack);
                }
            }
            else
            {
                _gameManager.ShowPossibleAttack(CurrentTile, false, _Attack);
            }
        }
        else
        {
            _gameManager.ShowPossibleAttack(CurrentTile, false, _Attack);
        }
    }
    
    private void SetNormalAttackState()
    {
        if (_WaterAttack != null)
        {
            if (CurrentTile.IsWater)
            {
                if (_gameManager._tileManager.TileManagerData._IsLava)
                {
                    _gameManager.StateAttackCharacter._Attack =  _FireAttack;
                }
                else if (_gameManager._tileManager.TileManagerData._IsPoison)
                {
                    _gameManager.StateAttackCharacter._Attack =  _PoisonAttack;
                }
                else
                {
                    _gameManager.StateAttackCharacter._Attack =  _WaterAttack;
                }
            }
            else
            {
                _gameManager.StateAttackCharacter._Attack =  _Attack;
            }
        }
        else
        {
            _gameManager.StateAttackCharacter._Attack =  _Attack;
        }
    }
 // Enemy AI
 private IEnumerator EnemyTurn()
 {
     yield return new WaitForSeconds(.4f);
     if (CurrentHealth <= 0)
     {
         yield break;
     }

 //If he can already Attack his enemy
        yield return new WaitUntil(() => !_gameManager.CameraIsMoving);
        Debug.Log("CharacterAI :: Before ShowPossibleAttack1 :: Character = " + gameObject.name);

        if (!_IsSelfDestruct)
        {
            ShowNormalPossibleAttack();
            _TimePathfindingCoroutine = StartCoroutine(TimePathFinding());
            yield return new WaitUntil(() => _gameManager.PossibleAttackTileIsFinished || _TimePathfindingIsFinish);
            if (_TimePathfindingCoroutine != null)
            {
                StopCoroutine(_TimePathfindingCoroutine);
            }
        }

        Tile enemyTile = CheckIfOccupiedTileAreEnemy();
        bool isSkillAttack = false;

        if (enemyTile == null)
        {
            Debug.Log("CharacterAI :: After ShowPossibleAttack1 enemyTile == null :: Character = " + gameObject.name);
            isSkillAttack = true;
            if (_SkillAttack != null && !_SkillAttack.IsSpawnSkill)
            {
                _gameManager.ShowPossibleAttack(CurrentTile, false, _SkillAttack);
                _TimePathfindingCoroutine = StartCoroutine(TimePathFinding());
                yield return new WaitUntil(() => _gameManager.PossibleAttackTileIsFinished ||_TimePathfindingIsFinish );
                if (_TimePathfindingCoroutine != null)
                {
                    StopCoroutine(_TimePathfindingCoroutine);
                }
                enemyTile = CheckIfOccupiedTileAreEnemy();
            
                if (enemyTile != null && _SkillAttack.IsDashAttack)
                {
                    Debug.Log("CharacterAI :: CurrentTile.CoordX = " + CurrentTile.CoordX + "  CurrentTile.CoordY = " + CurrentTile.CoordY + "  enemyTile.CoordX = " + enemyTile.CoordX + "  enemyTile.CoordY = " + enemyTile.CoordY);
                    enemyTile = FindTileBehind(CurrentTile, enemyTile);
                    if (enemyTile != null)
                    {
                        Debug.Log("CharacterAI :: BehindTile.CoordX = " + enemyTile.CoordX + "  BehindTile.CoordY = " + enemyTile.CoordY);
                    }
                } 
            }
        }

        if (enemyTile != null )
        {
            Debug.Log("CharacterAI :: After ShowPossibleAttack1 enemyTile != null :: Character = " + gameObject.name);
            
            if (isSkillAttack)
            {
                _gameManager.StateAttackCharacter._Attack = _SkillAttack;
            }
            else
            {
                SetNormalAttackState();
            }

            yield return new WaitForSeconds(0.5f);
            
            Debug.Log("CharacterAI :: After ShowPossibleAttack1 and Set current attack enemyTile != null :: Character = " + gameObject.name + " attack = " + _gameManager.StateAttackCharacter._Attack);
            
            if (!_gameManager.StateAttackCharacter._Attack.IsDashAttack)
            {
                _gameManager.SelectTile(enemyTile);
            }
            
            _gameManager.SelectTile(enemyTile);

            HaveAttacked = false;
            _gameManager.Wait = true;
            yield return new WaitForSeconds(TIME_ATTACKCHARACTER);
            yield return new WaitUntil(() => HaveAttacked);
            yield return new WaitUntil(() => !_gameManager.Wait);

            if (CurrentHealth <= 0)
            {
                _gameManager.NextCharacterTurn();
                yield break;
            }

            Debug.Log("CharacterAI :: Before ShowPossibleMove1 :: Character = " + gameObject.name);

            if (CanMove)
            {
                _gameManager.ShowPossibleMove(CurrentTile);
                _TimePathfindingCoroutine = StartCoroutine(TimePathFinding());
                yield return new WaitUntil(() => _gameManager.PossibleMoveTileIsFinished || _TimePathfindingIsFinish);
                if (_TimePathfindingCoroutine != null)
                {
                    StopCoroutine(_TimePathfindingCoroutine);
                }
                Debug.Log("CharacterAI :: Before GetNearestTile1 GetSelectedTileLenght =  " + _tileManager.GetSelectedTileLenght() + " :: Character = " + gameObject.name);
                if (_tileManager.GetSelectedTileLenght() <= 1)
                {
                    Tile tile = null;
                    while (tile == null)
                    {
                        StartCoroutine(_gameManager.ShowPossibleTileDirectionEndOfCharacterTurn(0));
                        _TimePathfindingCoroutine = StartCoroutine(TimePathFinding());
                        yield return new WaitUntil(() => _gameManager.PossibleEndTurnDirectionTileIsFinished || _TimePathfindingIsFinish);
                        if (_TimePathfindingCoroutine != null)
                        {
                            StopCoroutine(_TimePathfindingCoroutine);
                        }
                        tile = GetNearestTile();

                    }
                    
                    _gameManager.SelectTile(TryFoundBetterTile(tile));
                    yield break;
                }
                
           
         
                _gameManager.SelectTile(_tileManager.GetSelectedTile(Random.Range(1,_tileManager.GetSelectedTileLenght())));
                _gameManager.Wait = true;
                HaveMoved = false;
                yield return new WaitForSeconds(TIME_MOVECHARACTER);
                yield return new WaitUntil(() => HaveMoved);
                yield return new WaitUntil(() => !_gameManager.Wait);
                Debug.Log("CharacterAI :: Before GetNearestTile1 :: Character = " + gameObject.name);
                Tile NearestTile = null;
                while (NearestTile == null)
                {
                    StartCoroutine(_gameManager.ShowPossibleTileDirectionEndOfCharacterTurn(0));
                    _TimePathfindingCoroutine = StartCoroutine(TimePathFinding());
                    yield return new WaitUntil(() => _gameManager.PossibleEndTurnDirectionTileIsFinished || _TimePathfindingIsFinish);
                    if (_TimePathfindingCoroutine != null)
                    {
                        StopCoroutine(_TimePathfindingCoroutine);
                    }
                    NearestTile = GetNearestTile();

                }
                _gameManager.SelectTile(TryFoundBetterTile(NearestTile));
                yield break;
            }
            else
            {
                Debug.Log("CharacterAI :: EndOfCharacterTurn :: Character = " + gameObject.name);
                Tile tile = null;
                while (tile == null)
                {
                    StartCoroutine(_gameManager.ShowPossibleTileDirectionEndOfCharacterTurn(0));
                    _TimePathfindingCoroutine = StartCoroutine(TimePathFinding());
                    yield return new WaitUntil(() => _gameManager.PossibleEndTurnDirectionTileIsFinished || _TimePathfindingIsFinish);
                    if (_TimePathfindingCoroutine != null)
                    {
                        StopCoroutine(_TimePathfindingCoroutine);
                    }
                    tile = GetNearestTile();

                }
                _gameManager.SelectTile(TryFoundBetterTile(tile));
                yield break;
            }

        }
        else
        {
            if (!CanMove)
            {
                Debug.Log("CharacterAI :: !CanMove :: Character = " + gameObject.name);
                Tile tile = null;
                while (tile == null)
                {
                    StartCoroutine(_gameManager.ShowPossibleTileDirectionEndOfCharacterTurn(0));
                    _TimePathfindingCoroutine = StartCoroutine(TimePathFinding());
                    yield return new WaitUntil(() => _gameManager.PossibleEndTurnDirectionTileIsFinished || _TimePathfindingIsFinish);
                    if (_TimePathfindingCoroutine != null)
                    {
                        StopCoroutine(_TimePathfindingCoroutine);
                    }
                    tile = GetNearestTile();

                }

                _gameManager.SelectTile(TryFoundBetterTile(tile));
                yield break;
            }

            //Moves towards his enemy
            Debug.Log("CharacterAI :: Before ShowPossibleMove2 :: Character = " + gameObject.name);
            _gameManager.ShowPossibleMove(CurrentTile);
            _TimePathfindingCoroutine = StartCoroutine(TimePathFinding());
            yield return new WaitUntil(() => _gameManager.PossibleMoveTileIsFinished || _TimePathfindingIsFinish);
            if (_TimePathfindingCoroutine != null)
            {
                StopCoroutine(_TimePathfindingCoroutine);
            }

            if (_tileManager.GetSelectedTileLenght() <= 1)
            {
                Tile tile = null;
                while (tile == null)
                {
                    StartCoroutine(_gameManager.ShowPossibleTileDirectionEndOfCharacterTurn(0));
                    _TimePathfindingCoroutine = StartCoroutine(TimePathFinding());
                    yield return new WaitUntil(() => _gameManager.PossibleEndTurnDirectionTileIsFinished || _TimePathfindingIsFinish);
                    if (_TimePathfindingCoroutine != null)
                    {
                        StopCoroutine(_TimePathfindingCoroutine);
                    }
                    tile = GetNearestTile();

                }
                _gameManager.SelectTile(TryFoundBetterTile(tile));
                yield break;
            }
            Debug.Log("CharacterAI :: Before GetNearestTile1 GetSelectedTileLenght =  " + _tileManager.GetSelectedTileLenght() + ":: Character = " + gameObject.name);
            //If his enemy is a on moveTile
            enemyTile = FindBestPossibleMoveTile();
            
            if (enemyTile != null)
            {
                Debug.Log("CharacterAI :: FindBestPossibleMoveTile ::  Character = " + gameObject.name + " bestMatchTile != null");
                _gameManager.CurrentState = _gameManager.StateMoveCharacter;
                _gameManager.SelectTile(enemyTile);
                HaveMoved = false;
                _gameManager.Wait = true;
                yield return new WaitForSeconds(TIME_MOVECHARACTER);
                yield return new WaitUntil(() => HaveMoved);
                yield return new WaitUntil(() => !_gameManager.Wait);
                Debug.Log("CharacterAI :: Before ShowPossibleAttack2 :: Character = " + gameObject.name);
                ShowNormalPossibleAttack();
                _TimePathfindingCoroutine = StartCoroutine(TimePathFinding());
                yield return new WaitUntil(() => _gameManager.PossibleAttackTileIsFinished || _TimePathfindingIsFinish);
                if (_TimePathfindingCoroutine != null)
                {
                    StopCoroutine(_TimePathfindingCoroutine);
                }
                enemyTile = CheckIfOccupiedTileAreEnemy();
                
                isSkillAttack = false;

                if (enemyTile == null)
                {
                    isSkillAttack = true;

                    if (_SkillAttack != null && !_SkillAttack.IsSpawnSkill)
                    {
                        _gameManager.ShowPossibleAttack(CurrentTile, false, _SkillAttack);
                        _TimePathfindingCoroutine = StartCoroutine(TimePathFinding());
                        yield return new WaitUntil(() => _gameManager.PossibleAttackTileIsFinished || _TimePathfindingIsFinish);
                        if (_TimePathfindingCoroutine != null)
                        {
                            StopCoroutine(_TimePathfindingCoroutine);
                        }
                        enemyTile = CheckIfOccupiedTileAreEnemy();
                    
                        if (enemyTile != null && _SkillAttack.IsDashAttack)
                        {
                            Debug.Log("CharacterAI ::CurrentTile.CoordX = " + CurrentTile.CoordX + "  CurrentTile.CoordY = " + CurrentTile.CoordY + "  enemyTile.CoordX = " + enemyTile.CoordX + "  enemyTile.CoordY = " + enemyTile.CoordY);
                            enemyTile = FindTileBehind(CurrentTile, enemyTile);
                            if (enemyTile != null)
                            {
                                Debug.Log("CharacterAI ::BehindTile.CoordX = " + enemyTile.CoordX + "  BehindTile.CoordY = " + enemyTile.CoordY);
                            }
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
                        SetNormalAttackState();
                    }

                    yield return new WaitForSeconds(0.5f);
                    
                    if (!_gameManager.StateAttackCharacter._Attack.IsDashAttack)
                    {
                        _gameManager.SelectTile(enemyTile);
                    }
                    
                    _gameManager.SelectTile(enemyTile);
               
                    Debug.Log("CharacterAI :: after Attack :: Character = " + gameObject.name + " attack = " + _gameManager.StateAttackCharacter._Attack.name);
                    _gameManager.Wait = true;
                    HaveAttacked = false;
                    yield return new WaitForSeconds(TIME_ATTACKCHARACTER);
                    yield return new WaitUntil(() => !_gameManager.Wait);
                    yield return new WaitUntil(() => HaveAttacked);
                    
                    if (CurrentHealth <= 0)
                    {
                        _gameManager.NextCharacterTurn();
                        yield break;
                    }
                    
                    Tile tile = null;
                    while (tile == null)
                    {
                        StartCoroutine(_gameManager.ShowPossibleTileDirectionEndOfCharacterTurn(0));
                        _TimePathfindingCoroutine = StartCoroutine(TimePathFinding());
                        yield return new WaitUntil(() => _gameManager.PossibleEndTurnDirectionTileIsFinished || _TimePathfindingIsFinish);
                        if (_TimePathfindingCoroutine != null)
                        {
                            StopCoroutine(_TimePathfindingCoroutine);
                        }
                        tile = GetNearestTile();

                    }
                    _gameManager.SelectTile(TryFoundBetterTile(tile));
                    yield break;
                }
            }
            else
            {
                Debug.Log("CharacterAI :: FindBestPossibleMoveTile :: Character = " + gameObject.name + " bestMatchTile == null");
                //Select nearest possible MoveTile to reach his enemy 
                _gameManager.SelectTile(GetNearestTile());
                HaveMoved = false;
                _gameManager.Wait = true;
                yield return new WaitForSeconds(TIME_MOVECHARACTER);
                yield return new WaitUntil(() => HaveMoved);
                yield return new WaitUntil(() => !_gameManager.Wait);
                Debug.Log("CharacterAI :: Before ShowPossibleAttack3 :: Character = " + gameObject.name);
                ShowNormalPossibleAttack();
                _TimePathfindingCoroutine = StartCoroutine(TimePathFinding());
                yield return new WaitUntil(() => _gameManager.PossibleAttackTileIsFinished || _TimePathfindingIsFinish);
                if (_TimePathfindingCoroutine != null)
                {
                    StopCoroutine(_TimePathfindingCoroutine);
                }
                enemyTile = CheckIfOccupiedTileAreEnemy();
                
                isSkillAttack = false;

                if (enemyTile == null)
                {
                     Debug.Log("CharacterAI :: Before ShowPossibleAttack4 :: Character = " + gameObject.name);
                    isSkillAttack = true;
                    if (_SkillAttack != null && !_SkillAttack.IsSpawnSkill)
                    {
                        _gameManager.ShowPossibleAttack(CurrentTile, false, _SkillAttack);
                        _TimePathfindingCoroutine = StartCoroutine(TimePathFinding());
                        yield return new WaitUntil(() => _gameManager.PossibleAttackTileIsFinished || _TimePathfindingIsFinish);
                        if (_TimePathfindingCoroutine != null)
                        {
                            StopCoroutine(_TimePathfindingCoroutine);
                        }
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
                }

                if (enemyTile != null)
                {
                    if (isSkillAttack)
                    {
                        _gameManager.StateAttackCharacter._Attack = _SkillAttack;
                    }
                    else
                    {
                        SetNormalAttackState();
                    }

                    Debug.Log("CharacterAI :: after Attack :: Character = " + gameObject.name + " attack = " + _gameManager.StateAttackCharacter._Attack.name);
                    yield return new WaitForSeconds(0.5f);
                    if (!_gameManager.StateAttackCharacter._Attack.IsDashAttack)
                    {
                        _gameManager.SelectTile(enemyTile);
                    }
                    
                    _gameManager.SelectTile(enemyTile);
                    _gameManager.Wait = true;
                    HaveAttacked = false;
                    yield return new WaitForSeconds(TIME_ATTACKCHARACTER);
                    yield return new WaitUntil(() => !_gameManager.Wait);
                    yield return new WaitUntil(() => HaveAttacked);
                    if (CurrentHealth <= 0)
                    {
                        _gameManager.NextCharacterTurn();
                        yield break;
                    }
                }

                if (_SkillAttack != null && _SkillAttack.IsSpawnSkill && enemyTile == null)
                {
                    Debug.Log("CharacterAI :: Before ShowPossible SpawnSkill :: Character = " + gameObject.name);
                    _gameManager.ShowPossibleAttack(CurrentTile, false, _SkillAttack);
                    _TimePathfindingCoroutine = StartCoroutine(TimePathFinding());
                    yield return new WaitUntil(() => _gameManager.PossibleAttackTileIsFinished || _TimePathfindingIsFinish);
                    if (_TimePathfindingCoroutine != null)
                    {
                        StopCoroutine(_TimePathfindingCoroutine);
                    }
                    _gameManager.StateAttackCharacter._Attack = _SkillAttack;
                    List<Tile> possibleSpawnTile = new List<Tile>();
                    
                    for (int i = 0; i < CurrentTile.SideTiles.Length; i++)
                    {
                        if (CurrentTile.SideTiles[i] != null && !CurrentTile.SideTiles[i].IsOccupied && !CurrentTile.SideTiles[i].IsPotionTile)
                        {
                            possibleSpawnTile.Add(CurrentTile.SideTiles[i]);
                        }
                    }

                    if (possibleSpawnTile.Count > 0)
                    {
                        int randomInt = Random.Range(0, possibleSpawnTile.Count);
                        _gameManager.SelectTile(possibleSpawnTile[randomInt]);
                    }

                    yield return new WaitForSeconds(4);

                }
                

                Tile tile = null;
                while (tile == null)
                {
                    StartCoroutine(_gameManager.ShowPossibleTileDirectionEndOfCharacterTurn(0));
                    _TimePathfindingCoroutine = StartCoroutine(TimePathFinding());
                    yield return new WaitUntil(() => _gameManager.PossibleEndTurnDirectionTileIsFinished || _TimePathfindingIsFinish);
                    if (_TimePathfindingCoroutine != null)
                    {
                        StopCoroutine(_TimePathfindingCoroutine);
                    }
                    tile = GetNearestTile();

                }
                _gameManager.SelectTile(TryFoundBetterTile(tile));
                yield break;
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
        
        Debug.Log("CharacterAI :: GetNearestTile _tileManager.GetSelectedTileLenght() = " + _tileManager.GetSelectedTileLenght());
        
        for (int i = _tileManager.GetSelectedTileLenght() - 1; i > 0; i--)
        {
            int distance = GetPlayerDistance(_tileManager.GetSelectedTile(i), CurrentTeam);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestTile = _tileManager.GetSelectedTile(i);
            }
        }

        if (nearestTile == null)
        {
            Debug.Log("CharacterAI :: GetNearestTile nearestTile == null");
        }
        
        return nearestTile;
    }
    
    //Get nearest distance between Tile and all player
    private int GetPlayerDistance(Tile tile, Team team)
    {
        int minDistance = 1000;
        
        for (int i = 0; i < _gameManager.CharacterList.Count; i++)
        {
            if (_gameManager.CharacterList[i].CurrentTeam != team && !_gameManager.CharacterList[i]._IsDead)
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
            if (_gameManager.OccupiedTiles[i].CharacterReference == null || _gameManager.OccupiedTiles[i].CharacterReference != null && _gameManager.OccupiedTiles[i].CharacterReference._IsDead) { continue;}
            if (_gameManager.OccupiedTiles[i].CharacterReference.CurrentTeam != CurrentTeam)
            {
                return _gameManager.OccupiedTiles[i];
            }
        }
        return null;
    }

    private Tile _BestPossibleMoveTile;
    private bool _FindBestPossibleMoveTileIsFinish;

    private Tile FindBestPossibleMoveTile()
    {
        _FindBestPossibleMoveTileIsFinish = false;
        _BestPossibleMoveTile = null;
        Tile bestMatchTile = null;
        GetAttackDirection.AttackDirection attackDirection = GetAttackDirection.AttackDirection.None;

        for (int i = 0; i < _tileManager.GetSelectedTileLenght(); i++)
        {
            _gameManager.IndexOccupiedTiles = 0;
            _gameManager.PossibleAttackTileIsFinished = false;
            _tileManager.GetAICheckAttackTiles(_Attack.AttackLenght,_tileManager.GetSelectedTile(i));
      
            for (int j = 0; j < _gameManager.IndexOccupiedTiles; j++)
            {
                if (_gameManager.OccupiedTiles[j] == null || _gameManager.OccupiedTiles[j].CharacterReference == null || _gameManager.OccupiedTiles[j].CharacterReference != null &&_gameManager.OccupiedTiles[j].CharacterReference._IsDead) { continue;}

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

    private Tile TryFoundBetterTile(Tile tile)
    {
        if (tile.IsOccupied && tile.CharacterReference == null)
        {
            foreach (var sideTile in CurrentTile.SideTiles)
            {
                if (sideTile != null && sideTile.IsOccupied && sideTile.CharacterReference != null)
                {
                    return sideTile;

                }
            }
            
            foreach (var sideTile in CurrentTile.SideTiles)
            {
                if (sideTile != null && !sideTile.IsOccupied)
                {
                    return tile;
                }
            }
        }

        return tile;
    }
}
