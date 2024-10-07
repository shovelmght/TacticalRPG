using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TilesManager: MonoBehaviour
{
    [field: SerializeField] public TileManagerData TileManagerData{ get; set; }
    [field: SerializeField] public TileManagerData[] RandomTileManagerData{ get; private set; }
    
    [field: SerializeField] public TileManagerData[] EnvironmentStoneTileManagerData{ get; private set; }
    
    [field: SerializeField] public TileManagerData[] EnvironmentGrassTileManagerData{ get; private set; }
    
    [field: SerializeField] public TileManagerData[] EnvironmentDesertTileManagerData{ get; private set; }
    
    [field: SerializeField] public TileManagerData[] EnvironmentPoisonTileManagerData{ get; private set; }
    
    [field: SerializeField] public TileManagerData[] EnvironmentSnowTileManagerData{ get; private set; }
    
    [field: SerializeField] public TileManagerData[] EnvironmentCorner1TileManagerData{ get; private set; }
    
    [field: SerializeField] public TileManagerData[] EnvironmentCorner2TileManagerData{ get; private set; }
    
    [field: SerializeField] public TileManagerData[] EnvironmentCorner3TileManagerData{ get; private set; }
    
    [field: SerializeField] public TileManagerData[] EnvironmentCorner4TileManagerData{ get; private set; }
    

    [field: SerializeField] public Material MoveTileMaterial { get; private set; }
    [field: SerializeField] public Material AttackTileMaterial { get; private set; }
    [field: SerializeField] public Material PathTileMaterial { get; private set; }
    [field: SerializeField] public Material SelectTileMaterial { get; private set; }
    [field: SerializeField] public Material WoodTileMaterial { get; private set; }
    public int BranchPath { get;  set; } = 0;
    
    [SerializeField] private float _timePathFinding;
    [SerializeField] private TileData _tileData;
    [SerializeField] private float _buildingTime;
    [SerializeField] private float _waterSpeed;
    
    private Tile[,] _boardTiles;
    private Tile[] _tiles;
    private int _index = 0;
    private GameManager _gameManager;
    private Vector2 _offsetMaterialWater = new Vector2(0, 0);
    private const float  OFFSET_WATERFALL_SPEED = 6.2f ;
    private const float  OFFSET_WATERFALL_TILE = 0.2f ;
    private const float  COEF_POS_MULTIP_WATERFALL_TILE = 2;
    private const float  COEF_SCALE_MULTIP_WATERFALL_TILE = 5;
    private const int  GAP_BORDER_WATERFALL = 5;
    private List<Tile> _waterTilesList = new List<Tile>();
    private const float HEIGHT_GAP = 0.5f;
    
    private enum CoordXYZ
    {
        None = 0,
        CoordX = 1,
        CoordY = 2,
        CoordZ = 3
    }

    private enum _bridgeDirection
    {
        None = 0,
        FourSide = 1,
        NorthSouth = 2,
        EstWest = 3
    }

    private _bridgeDirection _currentBridgeDirection = _bridgeDirection.FourSide;


    public static TilesManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        _offsetMaterialWater.y += _waterSpeed * Time.deltaTime;

        TileManagerData.WaterTileMaterial.mainTextureOffset = _offsetMaterialWater;
       TileManagerData.WaterfallTileMaterial.mainTextureOffset = _offsetMaterialWater * OFFSET_WATERFALL_SPEED;
    }

    public IEnumerator SetBoardTiles()
    {
        AudioManager._Instance.SpawnSound( AudioManager._Instance._StartGameGuitSfx);
        _gameManager = GameManager.Instance;
        _tiles = new Tile[TileManagerData.Column * TileManagerData.Row];
        _boardTiles = new Tile[TileManagerData.Column,TileManagerData.Row];
        
        int count = 0;
        for (int i = 0; i < TileManagerData.Column; i++)
        {
            for (int j = 0; j < TileManagerData.Row; j++)
            {
                yield return new WaitForSeconds(_buildingTime);
                float height = 0;
                if (Random.Range(0f, 100.0f) <= TileManagerData.HeightChance)
                {
                    height = HEIGHT_GAP;
                }
                Vector3 tilePosition = new Vector3(TileManagerData.TileGap * i, height, TileManagerData.TileGap * j);
                GameObject tileGameObject =
                    Instantiate(TileManagerData.TilePrefab, tilePosition, Quaternion.identity);
                tileGameObject.transform.parent = transform;
                count++;
                tileGameObject.name = "Cube" + count;
                
                _boardTiles[i, j] = new Tile(_tileData, tileGameObject, i, j, tileGameObject.transform.position, height, null);
            }
        }

        foreach (var tile in _boardTiles)
        {
            tile.SetSideTiles();
            tile.SetDiagonalSideTiles();
        }

        yield return SetTileHeight();
        yield return SetWaterTile();
        yield return SpawnDecors();
        yield return SpawnSmallRocks(); 
        yield return MakeBridges();
        if (!_gameManager._IsMapScene)
        {
            yield return SpawnPotions();
            yield return SetBackGroundTile();
        }
    }

    public void AddSelectedTile(Tile tiles)
    {
        _tiles[_index] = tiles;
        _index++;
    }

    public Tile GetSelectedTile(int index)
    {
        return _tiles[index];
    }
    
    public int GetSelectedTileLenght ()
    {
        return _index;
    }

    public void DeselectTiles()
    {
        for (int i = 0; i < _index; i++)
        {
            _tiles[i].ResetSetTopMaterial();
            _tiles[i].CanInteract = false;
            _tiles[i].ClearPreviousMoveTile();
        }

        _index = 0;
    }

    private bool _IsGetMoveTilesFirstPass;
    
    public IEnumerator GetMoveTiles(int numberOfTimes, Tile previousTile, Tile currentTile)
    {
        int characterIsStuck = 0;
        if (_IsGetMoveTilesFirstPass)
        {
            _IsGetMoveTilesFirstPass = false;
            foreach (var sideTile in currentTile.SideTiles)
            {
                if (sideTile == null)
                {
                    characterIsStuck++;
                    continue;
                }
                
                if (sideTile.IsOccupied)
                {
                    characterIsStuck++;
                }
            }
        }
        
        if (characterIsStuck >= 4)
        {
            Debug.Log("TileManager :: GetMoveTiles characterIsStuck");
            _gameManager.PossibleMoveTileIsFinished = true;
            _IsGetMoveTilesFirstPass = true;
        }

        if (_gameManager._GameIsFinish) { yield break; }
        //Debug.Log("0");
        if (previousTile != null)
        {
            //Debug.Log("1");
            for (int i = 0; i < previousTile.GetPreviousMoveTileLenght(); i++)
            {
                currentTile.AddPreviousMoveTile(previousTile.PreviousMoveTilesList[i]);
            }
            //Debug.Log("2");
            currentTile.AddPreviousMoveTile(previousTile);
            if(currentTile.IsOccupied)
            {
                //Debug.Log("3");
                bool tileIsAlreadyInList = false;
                
                for (int i = 0; i <  _gameManager.IndexOccupiedTiles; i++)
                {
                    if (_gameManager.OccupiedTiles[i] == currentTile)
                    {
                        //Debug.Log("4");
                        tileIsAlreadyInList = true;
                    }
                }
                if (!tileIsAlreadyInList)
                {
                    //Debug.Log("5");
                    _gameManager.OccupiedTiles[_gameManager.IndexOccupiedTiles] = currentTile;
                    _gameManager.IndexOccupiedTiles++;
                }
            }
        }

        //Debug.Log("6");
        if (previousTile == null || !currentTile.IsOccupied)
        {
           // Debug.Log("7");
            currentTile.SetTopMaterial(MoveTileMaterial);
           // Debug.Log("8");
            AddSelectedTile(currentTile);
            currentTile.CanInteract = true;
            yield return new WaitForSeconds(_timePathFinding);
     
            if (numberOfTimes > 0)
            {
                //Debug.Log("9");
                foreach (var sideTile in currentTile.SideTiles)
                {
                    if (sideTile is { CanInteract: false })
                    {
                        StartCoroutine(GetMoveTiles(numberOfTimes - 1, currentTile, sideTile)) ;
                        //Debug.Log("++branchPath = " + BranchPath);
                        BranchPath++;
                    }
                }
            }
            else
            {
                Debug.Log("TileManager :: GetMoveTiles (numberOfTimes > 0 PossibleMoveTileIsFinished = true");
                _gameManager.PossibleMoveTileIsFinished = true;
                _IsGetMoveTilesFirstPass = true;
            }
        }
        else
        {
            if (currentTile.IsOccupied)
            {
                //Debug.Log("--branchPath = " + BranchPath);
                BranchPath--;
                //Debug.Log("11");
                if(BranchPath == 0)
                {
                    Debug.Log("TileManager :: GetMoveTiles  BranchPath == 0 PossibleMoveTileIsFinished = true");
                    _gameManager.PossibleMoveTileIsFinished = true;
                    _IsGetMoveTilesFirstPass = true;
                }
            }

        }
    }
    
    public void GetAICheckAttackTiles(int numberOfTimes, Tile currentTile)
    {
        if (_gameManager._GameIsFinish) { return; }
        

                if (currentTile.IsOccupied)
                {
                    _gameManager.OccupiedTiles[ _gameManager.IndexOccupiedTiles] = currentTile;
                    _gameManager.IndexOccupiedTiles++;
                }

                
        int nbrOfContinue = 0;

        if (numberOfTimes > 0)
        {
            foreach (var sidetile in currentTile.SideTiles)
            {
                if (sidetile is { CanInteract: false })
                {
                    GetAICheckAttackTiles(numberOfTimes - 1, sidetile) ;
                }
            }
        }
        else
        {
            Debug.Log("TileManager :: GetAttackTiles PossibleAttackTileIsFinished = true");
            _gameManager.PossibleAttackTileIsFinished = true;
        }
    }
    

    public IEnumerator GetAttackTiles(int numberOfTimes, Tile previousTile, Tile currentTile, Material material, bool isAICheck, bool isSpawnAttack)
    {
        if (_gameManager._GameIsFinish) { yield break; }
        if (isSpawnAttack)
            {
                bool isAoccupiedSpawnTile = currentTile.IsPotionTile || currentTile.IsOccupied;
                if (!isAoccupiedSpawnTile)
                {
                    if (previousTile != null)
                    {
                        for (int i = 0; i < previousTile.GetPreviousMoveTileLenght(); i++)
                        {
                            currentTile.AddPreviousMoveTile(previousTile.PreviousMoveTilesList[i]);
                        }
                        currentTile.AddPreviousMoveTile(previousTile);
                    }

                    if (material != null)
                    {
                        currentTile.SetTopMaterial(material);
                    }
           
                    AddSelectedTile(currentTile);
                    currentTile.CanInteract = true;
                }
                else
                {
                    _gameManager.OccupiedTiles[ _gameManager.IndexOccupiedTiles] = currentTile;
                    _gameManager.IndexOccupiedTiles++;
                }
            }
            else
            {
                if (currentTile.IsOccupied)
                {
                    _gameManager.OccupiedTiles[ _gameManager.IndexOccupiedTiles] = currentTile;
                    _gameManager.IndexOccupiedTiles++;
                }
                if (previousTile != null)
                {
                    for (int i = 0; i < previousTile.GetPreviousMoveTileLenght(); i++)
                    {
                        currentTile.AddPreviousMoveTile(previousTile.PreviousMoveTilesList[i]);
                    }
                    currentTile.AddPreviousMoveTile(previousTile);
                }

                if (material != null)
                {
                    currentTile.SetTopMaterial(material);
                }
           
                AddSelectedTile(currentTile);
                currentTile.CanInteract = true;
            }

        if (!isAICheck)
        {
            yield return new WaitForSeconds(_timePathFinding);
        }
        
        int nbrOfContinue = 0;

        if (numberOfTimes > 0)
        {
            foreach (var sidetile in currentTile.SideTiles)
            {
                if (sidetile is { CanInteract: false })
                {
                    StartCoroutine(GetAttackTiles(numberOfTimes - 1, currentTile, sidetile, material, isAICheck, isSpawnAttack)) ;
                }
            }
        }
        else
        {
            Debug.Log("TileManager :: GetAttackTiles PossibleAttackTileIsFinished = true");
            _gameManager.PossibleAttackTileIsFinished = true;
        }
    }

    public IEnumerator GetLineAttackTiles(int numberOfTimes, Tile tile, Tile currentTile, Material material, bool isAICheck)
    {
        if (currentTile.IsOccupied)
        {
            _gameManager.OccupiedTiles[ _gameManager.IndexOccupiedTiles] = currentTile;
            _gameManager.IndexOccupiedTiles++;
        } 
        else if(material != null)
        {
            currentTile.SetTopMaterial(material);
            AddSelectedTile(currentTile);
            currentTile.CanInteract = true;
        }
        else
        {
            AddSelectedTile(currentTile);
            currentTile.CanInteract = true;
        }

        Tile previousTileEst = null;
        Tile previousTileWest = null;
        Tile previousTileSouth = null;
        Tile previousTileNorth = null;
        for (int j = 1; j < numberOfTimes; j++)
        {
            Tile sideTile = GetTile(currentTile.CoordX + j, currentTile.CoordY);
            if (sideTile != null)
            {
                if (previousTileEst != null)
                {
                    for (int i = 0; i < previousTileEst.GetPreviousMoveTileLenght(); i++)
                    {
                        sideTile.AddPreviousMoveTile(previousTileEst.PreviousMoveTilesList[i]);
                    }
                    sideTile.AddPreviousMoveTile(previousTileEst);
                }
                if(material != null)
                {
                    sideTile.SetTopMaterial(material);
                    AddSelectedTile(sideTile);
                    sideTile.CanInteract = true;
                }
                else
                {
                    AddSelectedTile(sideTile);
                    sideTile.CanInteract = true;
                }
                
                if (sideTile.IsOccupied)
                {
                    _gameManager.OccupiedTiles[ _gameManager.IndexOccupiedTiles] = sideTile;
                    _gameManager.IndexOccupiedTiles++;
                } 

                previousTileEst = sideTile;
            }
                    
            sideTile = GetTile(currentTile.CoordX - j, currentTile.CoordY);
                    
            if (sideTile != null)
            {
                if (previousTileWest != null)
                {
                    for (int i = 0; i < previousTileWest.GetPreviousMoveTileLenght(); i++)
                    {
                        sideTile.AddPreviousMoveTile(previousTileWest.PreviousMoveTilesList[i]);
                    }
                    sideTile.AddPreviousMoveTile(previousTileWest);
                }
                 if (material != null)
                {
                    sideTile.SetTopMaterial(material);
                    AddSelectedTile(sideTile);
                    sideTile.CanInteract = true;
                }
                else
                {
                    AddSelectedTile(sideTile);
                    sideTile.CanInteract = true;
                }
                
                 if (sideTile.IsOccupied)
                 {
                     _gameManager.OccupiedTiles[ _gameManager.IndexOccupiedTiles] = sideTile;
                     _gameManager.IndexOccupiedTiles++;
                 } 
                 
                previousTileWest = sideTile;
            }
                    
            sideTile = GetTile(currentTile.CoordX, currentTile.CoordY + j);
                    
            if (sideTile != null)
            {
                
                if (previousTileNorth != null)
                {
                    for (int i = 0; i < previousTileNorth.GetPreviousMoveTileLenght(); i++)
                    {
                        sideTile.AddPreviousMoveTile(previousTileNorth.PreviousMoveTilesList[i]);
                    }
                    sideTile.AddPreviousMoveTile(previousTileNorth);
                }
                
                if (material != null)
                {
                    sideTile.SetTopMaterial(material);
                    AddSelectedTile(sideTile);
                    sideTile.CanInteract = true;
                }
                else
                {
                    AddSelectedTile(sideTile);
                    sideTile.CanInteract = true;
                }
                
                if (sideTile.IsOccupied)
                {
                    _gameManager.OccupiedTiles[ _gameManager.IndexOccupiedTiles] = sideTile;
                    _gameManager.IndexOccupiedTiles++;
                } 
                
                previousTileNorth = sideTile;
            }
                    
            sideTile = GetTile(currentTile.CoordX, currentTile.CoordY - j);
                    
            if (sideTile != null)
            {
                if (previousTileSouth != null)
                {
                    for (int i = 0; i < previousTileSouth.GetPreviousMoveTileLenght(); i++)
                    {
                        sideTile.AddPreviousMoveTile(previousTileSouth.PreviousMoveTilesList[i]);
                    }
                    sideTile.AddPreviousMoveTile(previousTileSouth);
                }
                
                if (material != null)
                {
                    sideTile.SetTopMaterial(material);
                    AddSelectedTile(sideTile);
                    sideTile.CanInteract = true;
                }
                else
                {
                    AddSelectedTile(sideTile);
                    sideTile.CanInteract = true;
                }
                
                if (sideTile.IsOccupied)
                {
                    _gameManager.OccupiedTiles[ _gameManager.IndexOccupiedTiles] = sideTile;
                    _gameManager.IndexOccupiedTiles++;
                } 
                
                previousTileSouth = sideTile;

                if (!isAICheck)
                {
                    yield return new WaitForSeconds(_timePathFinding);
                }
               
            }
        }
 
        Debug.Log("TileManager :: GetAttackTiles (Line) PossibleAttackTileIsFinished = true");
        _gameManager.PossibleAttackTileIsFinished = true;
        InputManager.Instance._TempSelectTileMaterial = _gameManager.TilePreSelected.StartMaterial;

    }
    
     public IEnumerator GetDashLineAttackTiles(int numberOfTimes, Tile tile, Tile currentTile, Material material, bool isAICheck)
    {
        if (currentTile.IsOccupied)
        {
            _gameManager.OccupiedTiles[ _gameManager.IndexOccupiedTiles] = currentTile;
            _gameManager.IndexOccupiedTiles++;
        } 
        else if(material != null)
        {
            currentTile.SetTopMaterial(material);
            AddSelectedTile(currentTile);
            currentTile.CanInteract = true;
        }
        else
        {
            AddSelectedTile(currentTile);
            currentTile.CanInteract = true;
        }

        Tile previousTileEst = null;
        Tile previousTileWest = null;
        Tile previousTileSouth = null;
        Tile previousTileNorth = null;
        for (int j = 1; j < numberOfTimes; j++)
        {
            Tile sideTile = GetTile(currentTile.CoordX + j, currentTile.CoordY);
            if (sideTile != null)
            {
                if (previousTileEst != null)
                {
                    for (int i = 0; i < previousTileEst.GetPreviousMoveTileLenght(); i++)
                    {
                        sideTile.AddPreviousMoveTile(previousTileEst.PreviousMoveTilesList[i]);
                    }
                    sideTile.AddPreviousMoveTile(previousTileEst);
                }
                if (sideTile.IsOccupied)
                {
                    _gameManager.OccupiedTiles[ _gameManager.IndexOccupiedTiles] = sideTile;
                    _gameManager.IndexOccupiedTiles++;
                } 
                else if(material != null)
                {
                    sideTile.SetTopMaterial(material);
                    AddSelectedTile(sideTile);
                    sideTile.CanInteract = true;
                }
                else
                {
                    AddSelectedTile(sideTile);
                    sideTile.CanInteract = true;
                }

                previousTileEst = sideTile;
            }
                    
            sideTile = GetTile(currentTile.CoordX - j, currentTile.CoordY);
                    
            if (sideTile != null)
            {
                if (previousTileWest != null)
                {
                    for (int i = 0; i < previousTileWest.GetPreviousMoveTileLenght(); i++)
                    {
                        sideTile.AddPreviousMoveTile(previousTileWest.PreviousMoveTilesList[i]);
                    }
                    sideTile.AddPreviousMoveTile(previousTileWest);
                }
                if (sideTile.IsOccupied)
                {
                    _gameManager.OccupiedTiles[ _gameManager.IndexOccupiedTiles] = sideTile;
                    _gameManager.IndexOccupiedTiles++;
                } 
                else if (material != null)
                {
                    sideTile.SetTopMaterial(material);
                    AddSelectedTile(sideTile);
                    sideTile.CanInteract = true;
                }
                else
                {
                    AddSelectedTile(sideTile);
                    sideTile.CanInteract = true;
                }
                
                previousTileWest = sideTile;
            }
                    
            sideTile = GetTile(currentTile.CoordX, currentTile.CoordY + j);
                    
            if (sideTile != null)
            {
                
                if (previousTileNorth != null)
                {
                    for (int i = 0; i < previousTileNorth.GetPreviousMoveTileLenght(); i++)
                    {
                        sideTile.AddPreviousMoveTile(previousTileNorth.PreviousMoveTilesList[i]);
                    }
                    sideTile.AddPreviousMoveTile(previousTileNorth);
                }
                
                if (sideTile.IsOccupied)
                {
                    _gameManager.OccupiedTiles[ _gameManager.IndexOccupiedTiles] = sideTile;
                    _gameManager.IndexOccupiedTiles++;
                } 
                else if (material != null)
                {
                    sideTile.SetTopMaterial(material);
                    AddSelectedTile(sideTile);
                    sideTile.CanInteract = true;
                }
                else
                {
                    AddSelectedTile(sideTile);
                    sideTile.CanInteract = true;
                }
                
                previousTileNorth = sideTile;
            }
                    
            sideTile = GetTile(currentTile.CoordX, currentTile.CoordY - j);
                    
            if (sideTile != null)
            {
                if (previousTileSouth != null)
                {
                    for (int i = 0; i < previousTileSouth.GetPreviousMoveTileLenght(); i++)
                    {
                        sideTile.AddPreviousMoveTile(previousTileSouth.PreviousMoveTilesList[i]);
                    }
                    sideTile.AddPreviousMoveTile(previousTileSouth);
                }
                
                if (sideTile.IsOccupied)
                {
                    _gameManager.OccupiedTiles[ _gameManager.IndexOccupiedTiles] = sideTile; 
                    _gameManager.IndexOccupiedTiles++;
                }
                else if (material != null)
                {
                    sideTile.SetTopMaterial(material);
                    AddSelectedTile(sideTile);
                    sideTile.CanInteract = true;
                }
                else
                {
                    AddSelectedTile(sideTile);
                    sideTile.CanInteract = true;
                }
                
                previousTileSouth = sideTile;

                if (!isAICheck)
                {
                    yield return new WaitForSeconds(_timePathFinding);
                }
            }
        }
 
        Debug.Log("TileManager :: GetAttackTiles (DashLine) PossibleAttackTileIsFinished = true");
        _gameManager.PossibleAttackTileIsFinished = true;
        InputManager.Instance._TempSelectTileMaterial = _gameManager.TilePreSelected.StartMaterial;

    }
     
     public IEnumerator GetEndTurnDirectionTiles(int numberOfTimes, Tile previousTile, Tile currentTile, Material material, bool isAICheck, bool isSpawnAttack)
    {
        if (_gameManager._GameIsFinish) { yield break; }
        
            if (isSpawnAttack)
            {
                bool isAoccupiedSpawnTile = currentTile.IsPotionTile || currentTile.IsOccupied;
                if (!isAoccupiedSpawnTile)
                {
                    if (previousTile != null)
                    {
                        for (int i = 0; i < previousTile.GetPreviousMoveTileLenght(); i++)
                        {
                            currentTile.AddPreviousMoveTile(previousTile.PreviousMoveTilesList[i]);
                        }
                        currentTile.AddPreviousMoveTile(previousTile);
                    }

                    if (material != null)
                    {
                        currentTile.SetTopMaterial(material);
                    }
           
                    AddSelectedTile(currentTile);
                    currentTile.CanInteract = true;
                }
                else
                {
                    _gameManager.OccupiedTiles[ _gameManager.IndexOccupiedTiles] = currentTile;
                    _gameManager.IndexOccupiedTiles++;
                }
            }
            else
            {
                if (currentTile.IsOccupied)
                {
                    _gameManager.OccupiedTiles[ _gameManager.IndexOccupiedTiles] = currentTile;
                    _gameManager.IndexOccupiedTiles++;
                }
                if (previousTile != null)
                {
                    for (int i = 0; i < previousTile.GetPreviousMoveTileLenght(); i++)
                    {
                        currentTile.AddPreviousMoveTile(previousTile.PreviousMoveTilesList[i]);
                    }
                    currentTile.AddPreviousMoveTile(previousTile);
                }

                if (material != null)
                {
                    currentTile.SetTopMaterial(material);
                }
           
                AddSelectedTile(currentTile);
                currentTile.CanInteract = true;
            }

            if (!isAICheck)
            {
                yield return new WaitForSeconds(_timePathFinding);
            }
            

        int nbrOfContinue = 0;

        if (numberOfTimes > 0)
        {
            foreach (var sidetile in currentTile.SideTiles)
            {
                if (sidetile is { CanInteract: false })
                {
                    StartCoroutine(GetEndTurnDirectionTiles(numberOfTimes - 1, currentTile, sidetile, material, isAICheck, isSpawnAttack)) ;
                }

            }
        }
        else
        {
            Debug.Log("TileManager :: GetEndTurnDirectionTiles  numberOfTimes > 0 PossibleEndTurnDirectionTileIsFinished = true");
            _gameManager.PossibleEndTurnDirectionTileIsFinished = true;
        }
    }
    
    public Tile GetTile(GameObject gameObjectTile)
    {
        return _boardTiles.Cast<Tile>().FirstOrDefault(mapBlock2D => gameObjectTile == mapBlock2D.CurrentGameObject);
    }

    public Tile GetTile(int coordX, int coordY)
    {
        if (coordX > TileManagerData.Column - 1  || coordY > TileManagerData.Row - 1|| coordX < 0 || coordY < 0)
        {
            return null;
        }
        
        return _boardTiles[coordX, coordY];
    }

    public IEnumerator SetTileHeight()
    {
        foreach (var tile in _boardTiles)
        {
            int maxHeight = 0;
            foreach (var sideTile in tile.SideTiles)
            {
                if (sideTile != null && sideTile.Height > maxHeight)
                {
                    if (Random.Range(0f, 100.0f) <= TileManagerData.HeightChance)
                    {
                        yield return new WaitForSeconds(_buildingTime);
                        tile.Height++;
                        Vector3 newLocation =  new Vector3(0, HEIGHT_GAP, 0);
                        tile.CurrentGameObject.transform.position += newLocation;
                        tile.Position += newLocation; 
                        break;
                    }
                }
            }
        }
    }


    private IEnumerator SetWaterTile()
    {
        if (!(TileManagerData.WaterChance > 10)) yield break;
        foreach (var tile in _boardTiles)
        {
            int height = tile.Height;
            foreach (var sideTile in tile.SideTiles)
            {
                if (sideTile != null)
                {
                    height +=sideTile.Height;
                }
            }

            if (height == 0)
            {
                if (Random.Range(0f, 100.0f) <= TileManagerData.WaterChance && !tile.IsOccupied)
                {
                    if (!_waterTilesList.Contains(tile))
                    {
                        _waterTilesList.Add(tile);
                    }
                    tile.IsWater = true;
                    foreach (var sideTile in tile.SideTiles)
                    {
                        if (sideTile != null)
                        {
                            if (!_waterTilesList.Contains(sideTile))
                            {
                                _waterTilesList.Add(sideTile);
                            }
                            sideTile.IsWater = true;
                        }
                    }
                }
            }
        }

        foreach (var tile in _waterTilesList)
        {
            yield return new WaitForSeconds(_buildingTime);
            tile.SetTopMaterial( TileManagerData.WaterTileMaterial);
            tile.StartMaterial =  TileManagerData.WaterTileMaterial;
            tile.Height--;
            Vector3 newTileLocation =  new Vector3(0, -HEIGHT_GAP, 0);
            tile.CurrentGameObject.transform.position += newTileLocation;
            tile.Position += (newTileLocation + new Vector3(0, -HEIGHT_GAP, 0));
        }


    }

    private IEnumerator SetBackGroundTile()
    {
        // Plane normal backgroundTiles
        for (int i = 1; i < TileManagerData.NumberGroundBackgroundTilesColumn; i++)
        {
            bool canSpawnDecor = i > GAP_BORDER_WATERFALL;
            StartCoroutine(SpawnBackgroundTiles(1,  
                TileManagerData.Column / 2 * TileManagerData.TileGap  ,  -i * TileManagerData.TileGap , -HEIGHT_GAP * TileManagerData.BoardTileHeight * TileManagerData.TileGap * 
                TileManagerData.TileGap, CoordXYZ.CoordX, TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileNormalPrefab, false, CardinalPoint.Orientation.North, canSpawnDecor));
            StartCoroutine(SpawnBackgroundTiles(-1,  
                TileManagerData.Column / 2 * TileManagerData.TileGap  ,  -i * TileManagerData.TileGap , -HEIGHT_GAP * TileManagerData.BoardTileHeight * TileManagerData.TileGap* TileManagerData.TileGap
                ,  CoordXYZ.CoordX, TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileNormalPrefab, false, CardinalPoint.Orientation.North, canSpawnDecor));
        }
        for (int i = TileManagerData.Row; i < TileManagerData.Row + TileManagerData.NumberGroundBackgroundTilesColumn; i++)
        {
            bool canSpawnDecor = i > GAP_BORDER_WATERFALL;
            StartCoroutine( SpawnBackgroundTiles(1,  
                TileManagerData.Column / 2 * TileManagerData.TileGap  ,  i  * TileManagerData.TileGap  , -HEIGHT_GAP * TileManagerData.BoardTileHeight * TileManagerData.TileGap, CoordXYZ.CoordX,
                TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileNormalPrefab, false, CardinalPoint.Orientation.North, canSpawnDecor));
            StartCoroutine(SpawnBackgroundTiles(-1,  
                TileManagerData.Column / 2 * TileManagerData.TileGap ,  i  * TileManagerData.TileGap, -HEIGHT_GAP * TileManagerData.BoardTileHeight * TileManagerData.TileGap,  CoordXYZ.CoordX
                , TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileNormalPrefab, false, CardinalPoint.Orientation.North, canSpawnDecor));
        }

        for (int i = 1; i < TileManagerData.Row + 1; i++)
        {
            bool canSpawnDecor = i > GAP_BORDER_WATERFALL;
            StartCoroutine( SpawnBackgroundTiles(-1,  
                TileManagerData.Column * TileManagerData.TileGap  ,  (TileManagerData.Row - i) * TileManagerData.TileGap, -HEIGHT_GAP * TileManagerData.BoardTileHeight * TileManagerData.TileGap, CoordXYZ.CoordX,
                TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileNormalPrefab, false, CardinalPoint.Orientation.North, canSpawnDecor));
            StartCoroutine(SpawnBackgroundTiles(1,  
                -1 * TileManagerData.TileGap   ,  (TileManagerData.Row - i) * TileManagerData.TileGap , -HEIGHT_GAP * TileManagerData.BoardTileHeight * TileManagerData.TileGap, CoordXYZ.CoordX,
                TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileNormalPrefab, false, CardinalPoint.Orientation.North, canSpawnDecor));
        }
        
        // down ( Y ) BackgroundTiles ( ground and water tile)
        foreach (var tile in _boardTiles)
        {
            if (tile.CoordX == 0 || tile.CoordY == 0 || tile.CoordX == TileManagerData.Column - 1 ||
                tile.CoordY == TileManagerData.Row - 1)
            {
                StartCoroutine( SpawnBackgroundTiles(1,  
                    tile.Position.x   ,  tile.Position.z  ,  tile.Position.y - TileManagerData.TileGap , CoordXYZ.CoordY, TileManagerData.BoardTileHeight / 2, 
                    TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
                if (!tile.IsWater) continue;
                if(tile.CoordX == 0)
                {
                    StartCoroutine( SpawnBackgroundTiles(1,  
                        tile.Position.x -  TileManagerData.TileGap   ,  tile.Position.z  ,  tile.Position.y - TileManagerData.TileGap / 2, CoordXYZ.CoordY,
                        TileManagerData.BoardTileHeight - 2, TileManagerData.EmptyTileMudPrefab, true, CardinalPoint.Orientation.North, false));
                }
                if(tile.CoordY == 0)
                {
                    StartCoroutine( SpawnBackgroundTiles(1,  
                        tile.Position.x  ,  tile.Position.z -  TileManagerData.TileGap ,  tile.Position.y - TileManagerData.TileGap / 2 , CoordXYZ.CoordY, 
                        TileManagerData.BoardTileHeight - 2, TileManagerData.EmptyTileMudPrefab, true, CardinalPoint.Orientation.Est, false));
                }
                
                if(tile.CoordX == TileManagerData.Column - 1)
                {
                    yield return SpawnBackgroundTiles(1,  
                        tile.Position.x  + TileManagerData.TileGap ,  tile.Position.z  ,  tile.Position.y - TileManagerData.TileGap / 2 , CoordXYZ.CoordY,
                        TileManagerData.BoardTileHeight - 2, TileManagerData.EmptyTileMudPrefab, true, CardinalPoint.Orientation.South, false);
                }
                if(tile.CoordY == TileManagerData.Row - 1)
                {
                    StartCoroutine(SpawnBackgroundTiles(1,  
                        tile.Position.x ,  tile.Position.z + TileManagerData.TileGap ,  tile.Position.y - TileManagerData.TileGap / 2 , CoordXYZ.CoordY,
                        TileManagerData.BoardTileHeight - 2, TileManagerData.EmptyTileMudPrefab, true, CardinalPoint.Orientation.West, false));
                }
            }
            else
            {
                StartCoroutine(SpawnBackgroundTiles(1,  
                    tile.Position.x   ,  tile.Position.z  ,  tile.Position.y - TileManagerData.TileGap , CoordXYZ.CoordY, 1, 
                    TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
            }
        }

        if (TileManagerData.WantColumn)
        {
            //Spawn Column
            yield return new WaitForSeconds(_buildingTime);
        StartCoroutine(SpawnBackgroundTiles(-1, 0, -TileManagerData.TileGap, 0, CoordXYZ.CoordY, TileManagerData.NumberGroundBackgroundTilesRow
            , TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false)); ;
        StartCoroutine(SpawnBackgroundTiles(1, 0, -TileManagerData.TileGap, 0, CoordXYZ.CoordY, TileManagerData.NumberGroundBackgroundTilesRow, 
            TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
        StartCoroutine(SpawnBackgroundTiles(-1, -TileManagerData.TileGap, -TileManagerData.TileGap, 0, CoordXYZ.CoordY, 
            TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
        yield return SpawnBackgroundTiles(1, -TileManagerData.TileGap, -TileManagerData.TileGap, 0, CoordXYZ.CoordY ,
            TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false);
        StartCoroutine(SpawnBackgroundTiles(-1, -TileManagerData.TileGap, 0, 0, CoordXYZ.CoordY, TileManagerData.NumberGroundBackgroundTilesRow,
            TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
        StartCoroutine(SpawnBackgroundTiles(1, -TileManagerData.TileGap, 0 , 0, CoordXYZ.CoordY, TileManagerData.NumberGroundBackgroundTilesRow
            , TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
        StartCoroutine(SpawnBackgroundTiles(-1, TileManagerData.Column * TileManagerData.TileGap, 0, 0, CoordXYZ.CoordY, 
            TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
        StartCoroutine(SpawnBackgroundTiles(1, TileManagerData.Column * TileManagerData.TileGap, 0, 0, CoordXYZ.CoordY, 
            TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
        yield return SpawnBackgroundTiles(-1, TileManagerData.Column * TileManagerData.TileGap , -TileManagerData.TileGap, 0, CoordXYZ.CoordY,
            TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false);
        StartCoroutine(SpawnBackgroundTiles(1, TileManagerData.Column * TileManagerData.TileGap , -TileManagerData.TileGap, 0, CoordXYZ.CoordY, 
            TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
        StartCoroutine(SpawnBackgroundTiles(-1, TileManagerData.Column * TileManagerData.TileGap - TileManagerData.TileGap , -TileManagerData.TileGap
            , 0, CoordXYZ.CoordY, TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
        StartCoroutine(SpawnBackgroundTiles(1, TileManagerData.Column * TileManagerData.TileGap - TileManagerData.TileGap , -TileManagerData.TileGap,
            0, CoordXYZ.CoordY, TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
        StartCoroutine(SpawnBackgroundTiles(-1, TileManagerData.Column * TileManagerData.TileGap, TileManagerData.Row * TileManagerData.TileGap, 0, CoordXYZ.CoordY
            , TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
        yield return SpawnBackgroundTiles(1, TileManagerData.Column * TileManagerData.TileGap, TileManagerData.Row * TileManagerData.TileGap, 0, CoordXYZ.CoordY,
            TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false); 
        StartCoroutine(SpawnBackgroundTiles(-1, TileManagerData.Column * TileManagerData.TileGap - TileManagerData.TileGap, TileManagerData.Row * TileManagerData.TileGap, 0,
            CoordXYZ.CoordY, TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
        StartCoroutine(SpawnBackgroundTiles(1, TileManagerData.Column * TileManagerData.TileGap - TileManagerData.TileGap, TileManagerData.Row * TileManagerData.TileGap, 0, CoordXYZ.CoordY,
            TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
        StartCoroutine(SpawnBackgroundTiles(-1, TileManagerData.Column * TileManagerData.TileGap, TileManagerData.Row * TileManagerData.TileGap - TileManagerData.TileGap, 0, CoordXYZ.CoordY,
            TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
        StartCoroutine(SpawnBackgroundTiles(1, TileManagerData.Column * TileManagerData.TileGap, TileManagerData.Row * TileManagerData.TileGap - TileManagerData.TileGap
            , 0, CoordXYZ.CoordY, TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
        yield return SpawnBackgroundTiles(-1, 0, TileManagerData.Row * TileManagerData.TileGap, 0, CoordXYZ.CoordY, TileManagerData.NumberGroundBackgroundTilesRow, 
            TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false);
        StartCoroutine(SpawnBackgroundTiles(1, 0, TileManagerData.Row * TileManagerData.TileGap, 0, CoordXYZ.CoordY, TileManagerData.NumberGroundBackgroundTilesRow, 
            TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
        StartCoroutine(SpawnBackgroundTiles(-1, -TileManagerData.TileGap, TileManagerData.Row * TileManagerData.TileGap - TileManagerData.TileGap, 0, CoordXYZ.CoordY,
            TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
        StartCoroutine(SpawnBackgroundTiles(1, -TileManagerData.TileGap, TileManagerData.Row * TileManagerData.TileGap - TileManagerData.TileGap
            , 0, CoordXYZ.CoordY, TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
        StartCoroutine(SpawnBackgroundTiles(-1, -TileManagerData.TileGap, TileManagerData.Row * TileManagerData.TileGap, 0, CoordXYZ.CoordY
            , TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false));
        yield return SpawnBackgroundTiles(1, -TileManagerData.TileGap, TileManagerData.Row * TileManagerData.TileGap, 0, CoordXYZ.CoordY, 
            TileManagerData.NumberGroundBackgroundTilesRow, TileManagerData.EmptyTileMudPrefab, false, CardinalPoint.Orientation.North, false);
        }
        
    }
    
    private IEnumerator SpawnBackgroundTiles(int negativeModifier , float x, float z, float y, CoordXYZ coord, int numberOfItteration, GameObject tileType, bool isWaterfall, CardinalPoint.Orientation orientation, bool canSpawnDecor)
    {
        for (int i = 0; i < numberOfItteration; i++)
        {
            bool isWaterTile = false;
            bool haveToSpawnDecors = false;

            yield return null;

            Vector3 wallBackgroundTilePosition = new Vector3(x,  y , z);
            switch (coord)
            {
                case CoordXYZ.None:
                    break;
                case CoordXYZ.CoordX:
                    wallBackgroundTilePosition += SetRandomHeightBackgroundTile(i, negativeModifier, coord);
                    isWaterTile = SetRandomWaterTileBackgroundTile();
                    if (canSpawnDecor && i > GAP_BORDER_WATERFALL)
                    {
                        haveToSpawnDecors = SetRandomDecorBackgroundTile();
                    }
                    break;
                case CoordXYZ.CoordY:
                    wallBackgroundTilePosition =new Vector3(x,  y  - TileManagerData.TileGap * i * negativeModifier ,z);
                    break;
                case CoordXYZ.CoordZ:
                    wallBackgroundTilePosition += SetRandomHeightBackgroundTile(i, negativeModifier, coord);
                    isWaterTile = SetRandomWaterTileBackgroundTile();
                    if (canSpawnDecor && i > GAP_BORDER_WATERFALL)
                    {
                        haveToSpawnDecors = SetRandomDecorBackgroundTile();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(coord), coord, null);
            }
      
            GameObject wallBackgroundTileGameObject =
                Instantiate(tileType, wallBackgroundTilePosition, Quaternion.identity);
            wallBackgroundTileGameObject.transform.parent = transform;
            if (isWaterTile || isWaterfall)
            {
                Material material = TileManagerData.WaterTileMaterial;
                if (isWaterfall)
                {
                    material = TileManagerData.WaterfallTileMaterial;
                    wallBackgroundTileGameObject.transform.localEulerAngles = new Vector3(Random.Range(0f, 360f),Random.Range(0f, 360f), Random.Range(0f, 360f));
                    switch (orientation)
                    {
                        case CardinalPoint.Orientation.North:
                            wallBackgroundTileGameObject.transform.position += new Vector3(   OFFSET_WATERFALL_TILE - (float)i / COEF_POS_MULTIP_WATERFALL_TILE,0,0);
                            break;
                        case CardinalPoint.Orientation.South:
                            wallBackgroundTileGameObject.transform.position -= new Vector3(  OFFSET_WATERFALL_TILE - (float)i / COEF_POS_MULTIP_WATERFALL_TILE,0,0);
                            break;
                        case CardinalPoint.Orientation.Est:
                            wallBackgroundTileGameObject.transform.position += new Vector3(  0,0,OFFSET_WATERFALL_TILE - (float)i / COEF_POS_MULTIP_WATERFALL_TILE);
                            break;
                        case CardinalPoint.Orientation.West:
                            wallBackgroundTileGameObject.transform.position -= new Vector3(  0,0,OFFSET_WATERFALL_TILE - (float)i / COEF_POS_MULTIP_WATERFALL_TILE);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null);
                    }

                    wallBackgroundTileGameObject.transform.localScale +=
                        new Vector3((float)i / COEF_SCALE_MULTIP_WATERFALL_TILE, (float)i / COEF_SCALE_MULTIP_WATERFALL_TILE, (float)i / COEF_SCALE_MULTIP_WATERFALL_TILE);
                }
                for (int j = 0; j < wallBackgroundTileGameObject.transform.childCount; j++)
                {
                    MeshRenderer meshRenderer =
                        wallBackgroundTileGameObject.transform.GetChild(j).GetComponent<MeshRenderer>();
      
                    var materials = meshRenderer.materials;
                    if (materials.Length > 1)
                    {
                        materials[0] =  material;
                        materials[1] =  material;
                    }
                    else
                    {
                        materials[0] =  material;
                    }
                    meshRenderer.materials = materials;
                }
            }
            else if (haveToSpawnDecors)
            {
                int randomIndex = Random.Range(0, TileManagerData.DecorsPrefab.Length);
                GameObject decor = Instantiate(TileManagerData.DecorsPrefab[randomIndex], wallBackgroundTileGameObject.transform.position, Quaternion.identity);
                decor.transform.parent = transform;
            }
        }
    }

    private Vector3 SetRandomHeightBackgroundTile(int index, int negativeModifier, CoordXYZ coord)
    {
        float randomFloat = Random.Range(0f, 100.0f);
        if (randomFloat <= TileManagerData.HeightChanceBackgroundTiles)
        {
            if (coord == CoordXYZ.CoordX)
            {
                return new Vector3(-TileManagerData.TileGap * index * negativeModifier, 0, 0);
            }

            return new Vector3(0, 0, -TileManagerData.TileGap * index * negativeModifier);
        }

        if (coord == CoordXYZ.CoordX)
        {
            return  new Vector3(-TileManagerData.TileGap * index * negativeModifier,   HEIGHT_GAP, 0);
        }

        return new Vector3(0, HEIGHT_GAP, -TileManagerData.TileGap * index * negativeModifier);
    }
    
    private bool SetRandomDecorBackgroundTile()
    {
        float randomFloat = Random.Range(0f, 100.0f);
        return randomFloat <= TileManagerData.decorsChanceBackgroundTiles;
    }
    
    private bool SetRandomWaterTileBackgroundTile()
    {
        float randomFloat = Random.Range(0f, 100.0f);
        if (randomFloat <= TileManagerData.WaterChanceBackgroundTiles)
        {
            return true;
        }

        return false;
    }

        private IEnumerator SpawnDecors()
        {
            for (int i = 0; i < TileManagerData.NumberOfDecor; i++)
            {
                Tile tile =  GetTile(Random.Range(0, TileManagerData.Column - 1), Random.Range(0, TileManagerData.Row - 1));
                if (!tile.IsOccupied && !tile.IsWater)
                {
                    yield return new WaitForSeconds(_buildingTime);
                    tile.IsOccupied = true;
                    int randomIndex = Random.Range(0, TileManagerData.DecorsPrefab.Length);
                    Vector3 rotation = new Vector3(0, Random.Range(0f, 360f), 0);
                    GameObject decor = Instantiate(TileManagerData.DecorsPrefab[randomIndex], tile.Position, Quaternion.Euler(rotation));
                    decor.transform.parent = transform;
                }
            }
        }
        
        private IEnumerator SpawnPotions()
        {
            for (int i = 0; i < TileManagerData.NumberOfPotion; i++)
            {
                Tile tile =  GetTile(Random.Range(0, TileManagerData.Column - 1), Random.Range(0, TileManagerData.Row - 1));
                bool canSpawnPotion = tile.IsOccupied && tile.IsPotionTile;
                if (!canSpawnPotion)
                {
                    yield return new WaitForSeconds(_buildingTime);
                    tile.IsPotionTile = true;
                    int randomIndex = Random.Range(0, TileManagerData._PotionData.Length);
                    Vector3 rotation = new Vector3(0, Random.Range(0f, 360f), 0);
                    GameObject potion = Instantiate(TileManagerData._PotionData[randomIndex].PotionPrefab, tile.Position + new Vector3(0,1,0), Quaternion.Euler(rotation));
                    potion.transform.parent = transform;
                    potion.transform.localScale = new Vector3(.4f, .4f, .4f);
                    potion.name = "Potion " + i;
                    tile.SetPotion(potion.GetComponent<Animator>(), TileManagerData._PotionData[randomIndex]);
                    

                }
            }
        }
        
        private IEnumerator SpawnSmallRocks()
        {
            for (int i = 0; i < TileManagerData.NumberOfSmallRock; i++)
            {
                Tile tile =  GetTile(Random.Range(0, TileManagerData.Column - 1), Random.Range(0, TileManagerData.Row - 1));
                if (!tile.IsOccupied || !tile.IsWater)
                {
                    yield return new WaitForSeconds(_buildingTime);
                    int randomIndex = Random.Range(0, TileManagerData.SmallRockPrefab.Length);
                    Vector3 rotation = new Vector3(0, Random.Range(0f, 360f), 0);
                    GameObject decor = Instantiate(TileManagerData.SmallRockPrefab[randomIndex], tile.Position, Quaternion.Euler(rotation));
                    decor.transform.parent = transform;
                }
            }
        }

        private IEnumerator MakeBridges()
        {
            if (_waterTilesList.Count < 8) yield break;
            Tile firstTileBridge = null;
            bool northDirection = false;
            
            foreach (var tile in _waterTilesList)
            {
                if (tile.SideTiles[(int)CardinalPoint.Orientation.North] == null ||
                    tile.SideTiles[(int)CardinalPoint.Orientation.Est] == null ||
                    tile.SideTiles[(int)CardinalPoint.Orientation.West] == null ||
                    tile.SideTiles[(int)CardinalPoint.Orientation.South] == null)
                {
                    continue;
                }

                GetLenghtWater(tile, tile.SideTiles[(int)CardinalPoint.Orientation.North],
                    (int)CardinalPoint.Orientation.North);
                
                GetLenghtWater(tile, tile.SideTiles[(int)CardinalPoint.Orientation.South],
                    (int)CardinalPoint.Orientation.South);
                
                GetLenghtWater(tile, tile.SideTiles[(int)CardinalPoint.Orientation.Est],
                    (int)CardinalPoint.Orientation.Est);
                
                GetLenghtWater(tile, tile.SideTiles[(int)CardinalPoint.Orientation.West],
                    (int)CardinalPoint.Orientation.West);
            }

            if (_waterTilesList.Count > 0)
            {
                firstTileBridge = _waterTilesList[0];
                foreach (var tile in _waterTilesList)
                {
                    if (tile.WaterLenghtNorthSouth + tile.WaterLenghtEstWest > firstTileBridge.WaterLenghtNorthSouth + firstTileBridge.WaterLenghtEstWest)
                    {
                        firstTileBridge = tile;
                    }
                }
            }

            if (Random.Range(0f, 100.0f) <= 50)
            {
                if (firstTileBridge.WaterLenghtNorthSouth > firstTileBridge.WaterLenghtEstWest)
                {
                    _currentBridgeDirection = _bridgeDirection.NorthSouth;
                }
                else
                {
                    _currentBridgeDirection = _bridgeDirection.EstWest;
                }
            }

            switch (_currentBridgeDirection)
            {
                case _bridgeDirection.None:
                    break;
                case _bridgeDirection.FourSide:
                    SetBridgeTile(firstTileBridge, false, TileManagerData.BridgeTile4SidePrefab);
                    firstTileBridge.StartSecondMaterial = firstTileBridge.StartMaterial;
                    yield return SetBridgeTile(firstTileBridge.SideTiles[(int)CardinalPoint.Orientation.North], (int)CardinalPoint.Orientation.North, false);
                    yield return SetBridgeTile(firstTileBridge.SideTiles[(int)CardinalPoint.Orientation.South], (int)CardinalPoint.Orientation.South, false);
                    yield return SetBridgeTile(firstTileBridge.SideTiles[(int)CardinalPoint.Orientation.Est], (int)CardinalPoint.Orientation.Est, true);
                    yield return SetBridgeTile(firstTileBridge.SideTiles[(int)CardinalPoint.Orientation.West], (int)CardinalPoint.Orientation.West, true);
                    break;
                case _bridgeDirection.NorthSouth:
                    yield return SetBridgeTile(firstTileBridge, (int)CardinalPoint.Orientation.North, false);
                    yield return SetBridgeTile(firstTileBridge.SideTiles[(int)CardinalPoint.Orientation.South], (int)CardinalPoint.Orientation.South, false);
                    break;
                case _bridgeDirection.EstWest:
                    yield return SetBridgeTile(firstTileBridge, (int)CardinalPoint.Orientation.Est, true);
                    yield return SetBridgeTile(firstTileBridge.SideTiles[(int)CardinalPoint.Orientation.West], (int)CardinalPoint.Orientation.West, true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        
        private IEnumerator SetBridgeTile(Tile sideTile, int index, bool needTurn)
        {
            yield return new WaitForSeconds(_buildingTime);

            if (sideTile.IsWater)
            {
                SetBridgeTile(sideTile, needTurn, TileManagerData.BridgeTilePrefab);
                if (sideTile.SideTiles[index] != null)
                {
                    {
                        if (sideTile.SideTiles[index].IsWater)
                        {
                            StartCoroutine(SetBridgeTile(sideTile.SideTiles[index], index, needTurn));
                        }
                    }
                }
            }
        }

        private void GetLenghtWater(Tile currentTile, Tile sideTile, int index)
        {
            if (sideTile != null && sideTile.IsWater)
            {
                if (index > 1)
                {
                    currentTile.WaterLenghtEstWest++;
                }
                else
                {
                    currentTile.WaterLenghtNorthSouth++;
                }
                
                GetLenghtWater(currentTile, sideTile.SideTiles[index], index);
            }
        }

        private void SetBridgeTile(Tile tile, bool needTurn, GameObject bridgeTilePrefab)
        {
            Vector3 rotation = new Vector3(0, 0,0);
            if (needTurn)
            {
                rotation = new Vector3(0, 90,0);
            }
            GameObject bridge = Instantiate(bridgeTilePrefab, tile.Position + new Vector3(0,HEIGHT_GAP,0) , Quaternion.Euler(rotation));
            bridge.transform.parent = transform;
            tile.MeshRenderer[0] = bridge.GetComponent<MeshRenderer>();
            tile.MeshRenderer[1] = bridge.GetComponent<MeshRenderer>();
            tile.MeshRenderer[2] = bridge.GetComponent<MeshRenderer>();
            tile.StartMaterial = WoodTileMaterial;
            BoxCollider tileCollider =  tile.CurrentGameObject.GetComponent<BoxCollider>();
            tileCollider.size = new Vector3(tileCollider.size.x, 2.25f, tileCollider.size.z);
            tile.IsWater = false;
            tile.Height++;
            Vector3 newTileLocation =  new Vector3(0, HEIGHT_GAP, 0);
            tile.Position += (newTileLocation + new Vector3(0, HEIGHT_GAP, 0));
        }
        
        public void SetValidSpawnTiles()
        {
            foreach (var tile in _boardTiles)
            {
                if (tile.CoordY == 0 || tile.CoordY == 1 || tile.CoordY == 2)
                {
                    tile.IsValidSpawnTile = true;
                    tile.SetTopMaterial(MoveTileMaterial);
                }
            }
        }
        
        public void UnselectValidSpawnTiles()
        {
            foreach (var tile in _boardTiles)
            {
                if (tile.CoordY == 0 || tile.CoordY == 1 || tile.CoordY == 2)
                {
                    tile.IsValidSpawnTile = true;
                    tile.ResetSetTopMaterial();
                }
            }
        }

        public void SetRandomTileManagerData()
        {
            TileManagerData = RandomTileManagerData[Random.Range(0, RandomTileManagerData.Length - 1)];
        }
        
        public void SetEnvironmentTileManagerData(int environmentIndex)
        {
            if (environmentIndex == 0)
            {
                TileManagerData = EnvironmentStoneTileManagerData[Random.Range(0, EnvironmentStoneTileManagerData.Length - 1)];
                return;
            }
            
            if (environmentIndex == 1)
            {
                TileManagerData = EnvironmentGrassTileManagerData[Random.Range(0, EnvironmentGrassTileManagerData.Length - 1)];
                return;
            }
            
            if (environmentIndex == 2)
            {
                TileManagerData = EnvironmentDesertTileManagerData[Random.Range(0, EnvironmentDesertTileManagerData.Length - 1)];
                return;
            }
            
            if (environmentIndex == 3)
            {
                TileManagerData = EnvironmentSnowTileManagerData[Random.Range(0, EnvironmentSnowTileManagerData.Length - 1)];
                return;
            }
            
            if (environmentIndex == 4)
            {
                TileManagerData = EnvironmentPoisonTileManagerData[Random.Range(0, EnvironmentPoisonTileManagerData.Length - 1)];
                return;
            }
            
            if (environmentIndex == 5)
            {
                TileManagerData = EnvironmentCorner1TileManagerData[Random.Range(0, EnvironmentCorner1TileManagerData.Length - 1)];
                return;
            }
            
            if (environmentIndex == 6)
            {
                TileManagerData = EnvironmentCorner2TileManagerData[Random.Range(0, EnvironmentCorner2TileManagerData.Length - 1)];
                return;
            }
            
            if (environmentIndex == 7)
            {
                TileManagerData = EnvironmentCorner3TileManagerData[Random.Range(0, EnvironmentCorner3TileManagerData.Length - 1)];
                return;
            }
            
            if (environmentIndex == 8)
            {
                TileManagerData = EnvironmentCorner4TileManagerData[Random.Range(0, EnvironmentCorner4TileManagerData.Length - 1)];
                return;
            }
            
        }
}
