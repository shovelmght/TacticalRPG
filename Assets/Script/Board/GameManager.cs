using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource _TempVcamCinemachineImpulseSource;
    [SerializeField] private CinemachineImpulseSource _CinemachineImpulseSource;
    [SerializeField] private CinemachineImpulseSource _ZoomCinemachineImpulseSource;
    [SerializeField] private Transform TempBoardVCam;
    [SerializeField] private Transform _CurrentCamera;
    [SerializeField] private CinemachineBrain _CinemachineBrain;
    [SerializeField] private float _CinemachineBlendTimeZoomBattle = 0.5f;
    [SerializeField] private float _CinemachineBlendTimeMovementBattle = 2;
    [SerializeField] private bool _RandomMap = true;
    [SerializeField] private Animator _ZommEffectAnimator;
    [SerializeField] private GameObject _WinLooseAnimator;
    [SerializeField] private GameObject _LavaWaterPlane;
    [SerializeField] private TMP_Text _WinLooseText;
    [SerializeField] private List<Transform> AllTransitionElement;
    [SerializeField] private List<GameObject> AllTransitionElementToDeactivate;
    [SerializeField] private List<GameObject> AllTransitionElementToDeactivate2;
    [SerializeField] private CharacterMaterial _AllPossibleCharacterMaterials;
    [SerializeField] private GameObject _CameraRotation;
    public List<DataCharacterSpawner> CharacterAIData;
    public DataCharacterSpawner _MapCharacterData;
    public CharacterSelectable _PlayerCharacterSpawnerList;
    public GameObject SquirePrefab;
    public GameObject SquireAIPrefab;
    public GameObject WizardPrefab;
    public GameObject WizardAIPrefab;
    public GameObject DragonPrefab;
    public GameObject DragonAIPrefab;
    public GameObject MotherNaturePrefab;
    public GameObject MotherNatureAIPrefab;
    public GameObject RobotPrefab;
    public GameObject RobotAIPrefab;
    public GameObject TurretPrefab;
    public GameObject ArrowsPrefab;
    public GameObject CameraButton;
    [field: SerializeField] public Transform BoardCamera { get; private set; }
    [field: SerializeField] public int MaxDistanceEnemiesSpawn { get; set; }
    [field: SerializeField] public int MaxCharacterPlayerCanBePlace { get; private set; }
    [field: SerializeField] public float CameraSpeed { get; private set; }

    public bool _IsStartScene;
    public bool _IsMapScene;

    public List<Character> CharacterList = new List<Character>();
    public Tile TileSelected { get; set; } = null;
    public Tile TilePreSelected { get; set; } = null;
    public Tile LastSpawnTile { get; set; } = null;
    public GameObject ArrowsDirection { get; set; }
    public State CurrentState { get; set; }
    public StateChooseCharacter StateChooseCharacter { get; private set; }
    public StateMoveCharacter StateMoveCharacter { get; set; }
    public StateAttackCharacter StateAttackCharacter { get; set; }
    public StateTurnCharacter StateTurnCharacter { get; set; }
    public StateNavigation StateNavigation { get; set; }
    public bool NeedResetTiles { get; set; }

    public bool _GameIsFinish { get; private set; }
    public bool CameraIsMoving { get; private set; }
    public bool IsCameraNear { get; set; }
    public bool MenuIsOpen { get; set; } = false;
    public bool IsController { get; set; } = false;
    public bool IsAIChatacterTurn { get; set; } = false;

    public bool IsCharactersAttacking { get; set; }
    public int IndexOccupiedTiles { get; set; }
    public bool RepeatableAttackInputIsPress { get; set; }
    public Tile[] OccupiedTiles { get; private set; }

    /*private void Update()
    {
        foreach (var occupiedTile in OccupiedTiles)
        {
            if(occupiedTile != null && occupiedTile.CharacterReference != null)
            {
                Debug.Log(occupiedTile.CharacterReference.gameObject.name);
            }
        }
    }*/

    public Character CurrentCharacter { get; set; }
    public Character CurrentCharacterTurn { get; private set; }

    public bool PossibleMapMoveTileIsFinished;
    public bool PossibleMoveTileIsFinished;
    public bool PossibleAttackTileIsFinished;
    public bool PossibleEndTurnDirectionTileIsFinished;

    public bool Wait
    {
        get => _wait;
        set
        {
            if (value)
            {
                if (DeactivateUIButtonCharacter != null)
                {
                    DeactivateUIButtonCharacter();
                }

            }
            else
            {
                if (ActivateUIButtonCharacter != null)
                {
                    ActivateUIButtonCharacter();
                }
            }

            _wait = value;
        }
    }

    private bool _wait;

    [SerializeField] private Direction _Enemiesdirection;

    public Direction _direction;
    private int _SpawnMobItteration;
    private int _characterCount;
    private bool _DoOnceStartBattle;
    private Vector3 _ScaleLerpSpeed = new Vector3(0.05f, 0.05f, 0.05f);

    public TilesManager _tileManager;
    public MapTilesManager _MapTilesManager_Lava;
    public MapTilesManager _MapTilesManager_Grass;
    public MapTilesManager _MapTilesManager_Snow;
    public MapTilesManager _MapTilesManager_Desert;
    public MapTilesManager _MapTilesManager_Poison;
    public MapTilesManager _MapTilesManager_Corner1;
    public MapTilesManager _MapTilesManager_Corner2;
    public MapTilesManager _MapTilesManager_Corner3;
    public MapTilesManager _MapTilesManager_Corner4;

    private const int MAX_OCCUPIED_TILES = 30;

    public Action SelectCharacter;
    public Action RemoveUICharacter;
    public Action DeactivateUIButtonCharacter;
    public Action ActivateUIButtonCharacter;
    public Action DesableMoveCharacterUIButtons;
    public Action DesableAttackCharacterUIButtons;
    public Action SetInteractableWaitButton;


    public enum Direction
    {
        None = 0,
        South = 1,
        Est = 2,
        North = 3,
        West = 4
    }

    public static GameManager Instance { get; private set; }

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

        StateTurnCharacter = new StateTurnCharacter(this);
        StateChooseCharacter = new StateChooseCharacter(this, MaxCharacterPlayerCanBePlace);
        StateAttackCharacter = new StateAttackCharacter(this);
        StateMoveCharacter = new StateMoveCharacter(this);
        StateNavigation = new StateNavigation(this);
        CurrentState = StateChooseCharacter;
    }

    void Start()
    {
        Time.timeScale = 1;
        StartCoroutine(InitializeGame());
    }


    private void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            if (_CameraRotation.activeInHierarchy)
            {
                _CameraRotation.SetActive(false);
            }
            else
            {
                _CameraRotation.SetActive(true);
            }
           
        }
    }

    private IEnumerator InitializeGame()
    {
        _tileManager = TilesManager.Instance;
        OccupiedTiles = new Tile[MAX_OCCUPIED_TILES];
        _direction = Direction.South;

        if (_IsStartScene)
        {
            yield return _tileManager.SetBoardTiles();
            yield break;
        }

        if (_IsMapScene)
        {
            StartCoroutine(_MapTilesManager_Lava.SetBoardTiles());
            StartCoroutine(_MapTilesManager_Grass.SetBoardTiles());
            StartCoroutine(_MapTilesManager_Desert.SetBoardTiles());
            StartCoroutine(_MapTilesManager_Snow.SetBoardTiles());
            StartCoroutine(_MapTilesManager_Poison.SetBoardTiles());
            StartCoroutine(_MapTilesManager_Corner1.SetBoardTiles());
            StartCoroutine(_MapTilesManager_Corner2.SetBoardTiles());
            StartCoroutine(_MapTilesManager_Corner3.SetBoardTiles());
            yield return _MapTilesManager_Corner4.SetBoardTiles();
            _LavaWaterPlane.SetActive(true);
            yield return new WaitForSeconds(3);

            Tile spawnTile = null;
            MapTilesManager mapTilesManager = null;
            int tileCoordX = FBPP.GetInt("PositionTileCoordX");
            int tileCoordY = FBPP.GetInt("PositionTileCoordY");
            int environment = FBPP.GetInt("Environment");

            if (environment == (int)_MapTilesManager_Lava._Environement)
            {
                mapTilesManager = _MapTilesManager_Lava;
            }
            
            if (environment == (int)_MapTilesManager_Grass._Environement)
            {
                mapTilesManager = _MapTilesManager_Lava;
            }
            
            if (environment == (int)_MapTilesManager_Desert._Environement)
            {
                mapTilesManager = _MapTilesManager_Lava;
            }
            
            if (environment == (int)_MapTilesManager_Snow._Environement)
            {
                mapTilesManager = _MapTilesManager_Lava;
            }
            
            if (environment == (int)_MapTilesManager_Poison._Environement)
            {
                mapTilesManager = _MapTilesManager_Lava;
            }
            
            if (environment == (int)_MapTilesManager_Corner1._Environement)
            {
                mapTilesManager = _MapTilesManager_Lava;
            }
            
            if (environment == (int)_MapTilesManager_Corner2._Environement)
            {
                mapTilesManager = _MapTilesManager_Lava;
            }
            
            if (environment == (int)_MapTilesManager_Corner3._Environement)
            {
                mapTilesManager = _MapTilesManager_Lava;
            }
            
            if (environment == (int)_MapTilesManager_Corner4._Environement)
            {
                mapTilesManager = _MapTilesManager_Lava;
            }

            if (mapTilesManager != null)
            {
                spawnTile = mapTilesManager.GetTile(tileCoordX, tileCoordY);
            }
            
            if (spawnTile == null)
            {
                SpawnMapCharacter(_MapTilesManager_Lava.GetTile(5, 5), Vector3.zero, _MapCharacterData.DataSpawn[0]);
            }
            else
            {
                SpawnMapCharacter(spawnTile, Vector3.zero, _MapCharacterData.DataSpawn[0]);
            }
            
            CameraButton.SetActive(true);
            ShowPossibleMapMove(TileSelected);
            TilePreSelected = CurrentCharacter.CurrentTile;
            yield break;
        }

        if (_UnitTest || _RandomMap)
        {
            _tileManager.SetRandomTileManagerData();
        }
        else
        {
            int environment = FBPP.GetInt("Environment");

            _tileManager.SetEnvironmentTileManagerData(environment);
        }

        yield return _tileManager.SetBoardTiles();

        if (_UnitTest)
        {
            DataCharacterSpawner dataCharacterSpawner = CharacterAIDataTeam2UnitTest[Random.Range(0, CharacterAIDataTeam2UnitTest.Count - 1)];

            foreach (var character in dataCharacterSpawner.DataSpawn)
            {
                yield return new WaitForSeconds(1);
                SpawnAICharacter(_tileManager.TileManagerData.Row - (_tileManager.TileManagerData.Row / MaxDistanceEnemiesSpawn),
                    _tileManager.TileManagerData.Row, character, SetEnemiesDirection(Direction.Est));
            }

            dataCharacterSpawner = CharacterAIDataTeam1UnitTest[Random.Range(0, CharacterAIDataTeam1UnitTest.Count - 1)];

            foreach (var character in dataCharacterSpawner.DataSpawn)
            {
                yield return new WaitForSeconds(1);
                SpawnAICharacter(0, MaxDistanceEnemiesSpawn, character, SetEnemiesDirection(Direction.West));
            }
        }
        else
        {
            foreach (var characterSpawner in CharacterAIData)
            {
                foreach (var character in characterSpawner.DataSpawn)
                {
                    yield return new WaitForSeconds(1);
                    SpawnAICharacter(_tileManager.TileManagerData.Row - (_tileManager.TileManagerData.Row / MaxDistanceEnemiesSpawn),
                        _tileManager.TileManagerData.Row, character, SetEnemiesDirection(Direction.Est));
                }
            }
        }

        Tile tile = _tileManager.GetTile(_tileManager.TileManagerData.Column / 2, _tileManager.TileManagerData.Row / 2);
        TilePreSelected = tile;
        StartCoroutine(MoveCamera(tile.GetCameraTransform((int)_direction, false)));

        if (_UnitTest)
        {
            NextCharacterTurn();
        }
        else
        {
            _tileManager.SetValidSpawnTiles();
            _PlayerCharacterSpawnerList.gameObject.SetActive(true);
        }
        
        CameraButton.SetActive(true);
    }

    private void SpawnAICharacter(int minCoordY, int maxCoordY, DataCharacterSpawner.DataSpawner dataCharacterSpawner, Vector3 direction)
    {
        bool isCharacterIsSpawned = false;
        while (!isCharacterIsSpawned)
        {
            int coordX = Random.Range(0, _tileManager.TileManagerData.Column);
            int coordY = Random.Range(minCoordY, maxCoordY);

            if (SpawnCharacter(_tileManager.GetTile(coordX, coordY), direction, dataCharacterSpawner))
            {
                isCharacterIsSpawned = true;
            }
        }
    }

    public bool SpawnCharacter(Tile tile, Vector3 rotation, DataCharacterSpawner.DataSpawner dataCharacterSpawner)
    {
        if (tile.IsOccupied || tile.IsPotionTile)
        {
            return false;
        }

        Vector3 spawnPosition = tile.Position;
        if (tile.IsWater)
        {
            spawnPosition = tile.Position + new Vector3(0, 0.1f, 0);
        }

        GameObject character = InstantiateCharacter(dataCharacterSpawner.CharactersPrefab, spawnPosition);
        character.transform.Rotate(rotation);
        _characterCount++;
        character.name = dataCharacterSpawner.Name;
        Character characterReference = character.GetComponent<Character>();
        characterReference.CurrentTile = tile;
        foreach (var waterParticleEffect in characterReference._waterParticleEffect)
        {
            waterParticleEffect.SetActive(tile.IsWater);
        }

        characterReference.CurrentTeam = dataCharacterSpawner.Team;
        switch (dataCharacterSpawner.Ability1)
        {
            case DataCharacterSpawner.CharactersAbility1.None:
                break;
            case DataCharacterSpawner.CharactersAbility1.AounterAttack:
                new CounterAbility(characterReference, this);
                break;
        }

        CharacterList.Add(characterReference);
        tile.SetCharacter(characterReference);
        return true;
    }

    public void SpawnMobCharacter(Tile tile, Vector3 rotation, DataCharacterSpawner.CharactersPrefab CharactersPrefab, bool canMove)
    {
        Vector3 spawnPosition = tile.Position;
        if (tile.IsWater)
        {
            spawnPosition = tile.Position + new Vector3(0, 0.1f, 0);
        }


        GameObject character = InstantiateCharacter(CharactersPrefab, spawnPosition);
        Character characterReference = character.GetComponent<Character>();
        CurrentCharacter.DestroyCharacterRelated += characterReference.DestroyCharacter;
        characterReference.CanMove = canMove;
        character.transform.Rotate(rotation);
        _characterCount++;
        _SpawnMobItteration++;
        character.name += _SpawnMobItteration;
        characterReference.CurrentTile = tile;
        characterReference.CurrentTeam = CurrentCharacter.CurrentTeam;

        CharacterList.Add(characterReference);
        tile.SetCharacter(characterReference);
    }

    public void SpawnMapCharacter(Tile tile, Vector3 rotation, DataCharacterSpawner.DataSpawner dataCharacterSpawner)
    {
        if (tile.IsOccupied)
        {
            foreach (var sideTile in tile.SideTiles)
            {
                if (sideTile != null && sideTile.IsOccupied)
                {
                    tile = sideTile;
                }
            }
        }

        Vector3 spawnPosition = tile.Position;
        if (tile.IsWater)
        {
            spawnPosition = tile.Position + new Vector3(0, 0.1f, 0);
        }

        GameObject characterGameObject = InstantiateCharacter(dataCharacterSpawner.CharactersPrefab, spawnPosition);
        characterGameObject.transform.Rotate(rotation);
        characterGameObject.name = dataCharacterSpawner.Name;
        Character characterReference = characterGameObject.GetComponent<Character>();
        characterReference.CurrentTile = tile;
        tile.SetCharacter(characterReference);
        characterGameObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        CurrentCharacter = characterReference;
        CurrentState = StateMoveCharacter;
        TileSelected = tile;
        StartCoroutine(MoveCamera(tile.GetCameraTransform((int)_direction, IsCameraNear)));
        int indexMaterial = FBPP.GetInt("TeamColor");
        characterReference.SetCharacterColor(_AllPossibleCharacterMaterials.AllPossibleMaterials[indexMaterial]);
        AllTransitionElement.Add(characterGameObject.transform);

    }

    public GameObject InstantiateCharacter(DataCharacterSpawner.CharactersPrefab charactersPrefab, Vector3 position)
    {
        if (charactersPrefab == DataCharacterSpawner.CharactersPrefab.Squire)
        {
            return Instantiate(SquirePrefab, position, Quaternion.identity);
        }

        if (charactersPrefab == DataCharacterSpawner.CharactersPrefab.Dragon)
        {
            return Instantiate(DragonPrefab, position, Quaternion.identity);
        }

        if (charactersPrefab == DataCharacterSpawner.CharactersPrefab.SquireAI)
        {
            return Instantiate(SquireAIPrefab, position, Quaternion.identity);
        }

        if (charactersPrefab == DataCharacterSpawner.CharactersPrefab.DragonAI)
        {
            return Instantiate(DragonAIPrefab, position, Quaternion.identity);
        }

        if (charactersPrefab == DataCharacterSpawner.CharactersPrefab.Wizard)
        {
            return Instantiate(WizardPrefab, position, Quaternion.identity);
        }

        if (charactersPrefab == DataCharacterSpawner.CharactersPrefab.WizardAI)
        {
            return Instantiate(WizardAIPrefab, position, Quaternion.identity);
        }

        if (charactersPrefab == DataCharacterSpawner.CharactersPrefab.MotherNature)
        {
            return Instantiate(MotherNaturePrefab, position, Quaternion.identity);
        }

        if (charactersPrefab == DataCharacterSpawner.CharactersPrefab.MotherNatureAI)
        {
            return Instantiate(MotherNatureAIPrefab, position, Quaternion.identity);
        }

        if (charactersPrefab == DataCharacterSpawner.CharactersPrefab.Robot)
        {
            return Instantiate(RobotPrefab, position, Quaternion.identity);
        }

        if (charactersPrefab == DataCharacterSpawner.CharactersPrefab.RobotAI)
        {
            return Instantiate(RobotAIPrefab, position, Quaternion.identity);
        }

        if (charactersPrefab == DataCharacterSpawner.CharactersPrefab.Turret)
        {
            return Instantiate(TurretPrefab, position, Quaternion.identity);
        }

        return null;
    }

    public void SelectTile(GameObject gameObjectTile)
    {
        if (_wait)
        {
            return;
        }

        Tile tile = null;

        if (_IsMapScene)
        {
            tile = CurrentCharacter.CurrentTile.MapTilesManager.GetTile(gameObjectTile);

            if (tile == null && CurrentCharacter.CurrentTile.MapTilesManager._WestMapTilesManager != null)
            {
                tile = CurrentCharacter.CurrentTile.MapTilesManager._WestMapTilesManager.GetTile(gameObjectTile);
            }

            if (tile == null && CurrentCharacter.CurrentTile.MapTilesManager._EstMapTilesManager != null)
            {
                tile = CurrentCharacter.CurrentTile.MapTilesManager._EstMapTilesManager.GetTile(gameObjectTile);
            }

            if (tile == null && CurrentCharacter.CurrentTile.MapTilesManager._NorthMapTilesManager != null)
            {
                tile = CurrentCharacter.CurrentTile.MapTilesManager._NorthMapTilesManager.GetTile(gameObjectTile);
            }

            if (tile == null && CurrentCharacter.CurrentTile.MapTilesManager._SouthMapTilesManager != null)
            {
                tile = CurrentCharacter.CurrentTile.MapTilesManager._SouthMapTilesManager.GetTile(gameObjectTile);
            }


            if (tile == null || tile == CurrentCharacter.CurrentTile)
            {
                return;
            }
        }
        else
        {
            tile = _tileManager.GetTile(gameObjectTile);

            if (AddOccupiedTileOnClick)
            {
                tile.IsOccupied = true;
                StartCoroutine(MoveCamera(tile.GetCameraTransform((int)_direction, IsCameraNear)));
                return;
            }

        }

        SelectTile(tile);
    }

    public void SelectTile(Tile tile)
    {
        if (_wait || _IsStartScene)
        {
            return;
        }

        if (tile == null)
        {
            Debug.Log("GameManager :: SelectTile tile == null");
        }
        else
        {
            Debug.Log("GameManager :: SelectTile  tile = " + tile.CoordX + " " + tile.CoordY + "  CurrentState = " + CurrentState);
        }

        StartCoroutine(MoveCamera(tile.GetCameraTransform((int)_direction, IsCameraNear)));
        TilePreSelected = tile;
        NeedResetTiles = true;
        CurrentState.SelectTile(tile);

        if (NeedResetTiles)
        {
            if (CurrentCharacter)
            {
                CurrentCharacter.RemoveUIPopUpCharacterInfo(false);
            }

            if (_IsMapScene)
            {
                bool isMoveTile = false;
                for (int i = 0; i < TileSelected.MapTilesManager.GetSelectedTileLenght(); i++)
                {
                    if (TileSelected.MapTilesManager.GetSelectedTile(i) == tile)
                    {
                        isMoveTile = true;
                    }
                }

                if (!isMoveTile)
                {
                    return;
                }

                TileSelected.MapTilesManager.DeselectTiles();

                if (TileSelected.MapTilesManager._EstMapTilesManager != null)
                {
                    TileSelected.MapTilesManager._EstMapTilesManager.DeselectTiles();
                }

                if (TileSelected.MapTilesManager._WestMapTilesManager != null)
                {
                    TileSelected.MapTilesManager._WestMapTilesManager.DeselectTiles();
                }

                if (TileSelected.MapTilesManager._SouthMapTilesManager != null)
                {
                    TileSelected.MapTilesManager._SouthMapTilesManager.DeselectTiles();
                }

                if (TileSelected.MapTilesManager._NorthMapTilesManager != null)
                {
                    TileSelected.MapTilesManager._NorthMapTilesManager.DeselectTiles();
                }

                TileSelected.MapTilesManager.AddSelectedTile(tile);

                tile.SetTopMaterial(TileSelected.MapTilesManager.MoveTileMaterial);
            }
            else
            {
                _tileManager.DeselectTiles();
                _tileManager.AddSelectedTile(tile);

                tile.SetTopMaterial(_tileManager.MoveTileMaterial);
            }

            if (RemoveUICharacter != null)
            {
                RemoveUICharacter();
            }

            CurrentState = StateNavigation;
            TileSelected = tile;
            StateAttackCharacter.ResetAttackData();
        }
    }

    public void SelectTileController(Tile tile)
    {
        Debug.Log("SelectTileController tile = " + tile.CoordX + "  " + tile.CoordY);
        if (_wait)
        {
            return;
        }

        StartCoroutine(MoveCamera(tile.GetCameraTransform((int)_direction, IsCameraNear)));

        if (_IsMapScene && IsController)
        {

        }
        else
        {
            tile.SetTopMaterial(_tileManager.SelectTileMaterial);
        }

    }

    //Select with color possible move tile
    public void ShowPossibleMove(Tile tile)
    {
        StateAttackCharacter.ResetAttackData();
        /*if (ArrowsDirection.activeSelf)
        {
            ArrowsDirection.SetActive(false);
        }*/
        IndexOccupiedTiles = 0;
        PossibleMoveTileIsFinished = false;
        _tileManager.DeselectTiles();
        Debug.Log("Before GetMoveTiles1");
        _tileManager.BranchPath = 0;
        StartCoroutine(_tileManager.GetMoveTiles(tile.CharacterReference.MovementPoint, null, tile));
        CurrentState = StateMoveCharacter;
    }
    
    public void ShowPossibleMapMove(Tile tile)
    {
        if (_IsMapScene)
        {
            if (_DoOnceStartBattle && Random.Range(0, 6) == 1)
            {
                
                StartCoroutine(MapSceneToBattleScene());
                return;
            }

            _DoOnceStartBattle = true;
        }

        StateAttackCharacter.ResetAttackData();
        /*if (ArrowsDirection.activeSelf)
        {
            ArrowsDirection.SetActive(false);
        }*/
        IndexOccupiedTiles = 0;
        PossibleMapMoveTileIsFinished = false;
        tile.MapTilesManager.DeselectTiles();
        if (TileSelected.MapTilesManager._EstMapTilesManager != null)
        {
            TileSelected.MapTilesManager._EstMapTilesManager.DeselectTiles();
        }

        if (TileSelected.MapTilesManager._WestMapTilesManager != null)
        {
            TileSelected.MapTilesManager._WestMapTilesManager.DeselectTiles();
        }

        if (TileSelected.MapTilesManager._SouthMapTilesManager != null)
        {
            TileSelected.MapTilesManager._SouthMapTilesManager.DeselectTiles();
        }

        if (TileSelected.MapTilesManager._NorthMapTilesManager != null)
        {
            TileSelected.MapTilesManager._NorthMapTilesManager.DeselectTiles();
        }

        Debug.Log("Before GetMoveTiles1");
        tile.MapTilesManager.BranchPath = 0;
        StartCoroutine(tile.MapTilesManager.GetMoveTiles(1, null, tile));
        CurrentState = StateMoveCharacter;
    }

    private IEnumerator MapSceneToBattleScene()
    {
        FBPP.SetInt("PositionTileCoordX", TileSelected.CoordX);
        FBPP.SetInt("PositionTileCoordY", TileSelected.CoordY);
        FBPP.SetInt("Environment", (int)TileSelected.MapTilesManager._Environement);
        FBPP.Save();
        StartCoroutine(ZoomBattleCamera(100f));
        foreach (var elementToDeactivate in AllTransitionElementToDeactivate)
        {
            elementToDeactivate.SetActive(false);
        }
        StartCoroutine(_MapTilesManager_Snow.TransitionToBattleScene());
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(_MapTilesManager_Grass.TransitionToBattleScene());
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(_MapTilesManager_Lava.TransitionToBattleScene());
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(_MapTilesManager_Poison.TransitionToBattleScene());
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(_MapTilesManager_Desert.TransitionToBattleScene());
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(_MapTilesManager_Corner1.TransitionToBattleScene());
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(_MapTilesManager_Corner2.TransitionToBattleScene());
        yield return new WaitForSeconds(0.15f);
        StartCoroutine(_MapTilesManager_Corner3.TransitionToBattleScene());
        yield return new WaitForSeconds(0.15f);
        StartCoroutine(_MapTilesManager_Corner4.TransitionToBattleScene());
        yield return new WaitForSeconds(0.15f);
        StartCoroutine(TransitionToBattleScene());
        
        yield return new WaitForSeconds(2.5f);
        foreach (var element in AllTransitionElementToDeactivate2)
        {
            if (element != null)
            {
                element.gameObject.SetActive(false);
            }
        }
        
        SceneManager.LoadScene("BattleScene");
    }

    private IEnumerator TransitionToBattleScene()
    {
        while (true)
        {
            foreach (var element in AllTransitionElement)
            {
                if (element != null && element.localScale.x > 0)
                {
                    element.localScale -= _ScaleLerpSpeed;
                }
            }
            yield return new WaitForSeconds(0.01f);
        }
    }


//Select with color possible attack tile
    public void ShowPossibleAttack(Tile tile, bool isAICheck, Attack attack)
    {
        Debug.Log("GameManager :: ShowPossibleAttack");
        /*if (ArrowsDirection.activeSelf)
        {
            ArrowsDirection.SetActive(false);
        }*/
        IndexOccupiedTiles = 0;
        PossibleAttackTileIsFinished = false;
        if (!isAICheck)
        {
            _tileManager.DeselectTiles();
            CurrentState = StateAttackCharacter;
        }

        if (attack.IsLineAttack)
        {
            StartCoroutine(_tileManager.GetLineAttackTiles(attack.AttackLenght, null, tile, _tileManager.AttackTileMaterial, isAICheck));
        }
        else if (attack.IsDashAttack)
        {
            StartCoroutine(_tileManager.GetDashLineAttackTiles(attack.AttackLenght, null, tile, _tileManager.AttackTileMaterial, isAICheck));
        }
        else
        {
            StartCoroutine(_tileManager.GetAttackTiles(attack.AttackLenght, null, tile, _tileManager.AttackTileMaterial, isAICheck, attack.IsSpawnSkill));
        }
        
    }
    
    //Select with color possible direction (end character turn) tile
    private void ShowPossibleTileDirection(Tile tile)
    {
        _tileManager.DeselectTiles();
        StartCoroutine(_tileManager.GetEndTurnDirectionTiles(1, null, tile, _tileManager.MoveTileMaterial, false, false));
        CurrentState = StateTurnCharacter;
    }
    

    private IEnumerator CheckWinCondition()
    {
        int team = 0;

        bool team1IsAlreadyCount = false;
        bool team2IsAlreadyCount = false;
        bool team3IsAlreadyCount = false;
        
        foreach (var character in CharacterList)
        {
            if (!team1IsAlreadyCount && character.CurrentTeam == Character.Team.Team1)
            {
                team1IsAlreadyCount = true;
                team++;
            }
            else if (!team2IsAlreadyCount && character.CurrentTeam == Character.Team.Team2)
            {
                team2IsAlreadyCount = true;
                team++;
            }
            else if (!team3IsAlreadyCount && character.CurrentTeam == Character.Team.Team3)
            {
                team3IsAlreadyCount = true;
                team++;
            }
        }

        if (team > 1)
        {
            yield break;
        }

        yield return new WaitForSeconds(0.25f);
        AudioManager._Instance.SpawnSound( AudioManager._Instance._GameIsOver);
        yield return new WaitForSeconds(0.25f);
        if (CharacterList[0].CurrentTeam != Character.Team.Team1)
        {
            _WinLooseText.text = "You Lost";
        }
        _WinLooseAnimator.SetActive(true);
        Time.timeScale = 0.2f;
        _GameIsFinish = true;
        
        
        
        yield return new WaitForSeconds(0.3f);
        AudioManager._Instance.SpawnSound( AudioManager._Instance._GameIsOverMoveTxt);
        yield return new WaitForSeconds(0.5f);
        AudioManager._Instance.SpawnSound( AudioManager._Instance._GameIsOverMoveTxt);
        
        yield return new WaitForSeconds(2f);
        if (_UnitTest)
        {
            SceneManager.LoadScene("BattleScene");
        }
        
    }

    //Go to next character turn
    public void NextCharacterTurn()
    {
        StartCoroutine(BeginOfTurn());
    }
    
    // Begin new turn
    private IEnumerator BeginOfTurn()
    {
        yield return new WaitForSeconds(1);
        
        SetInteractableWaitButton?.Invoke();
        CurrentCharacterTurn = CheckCharacterTime();
        while (CurrentCharacterTurn == null)
        {
            ReduceCharacterTimeRemaining();
            CurrentCharacterTurn = CheckCharacterTime();
        }

        //CameraIsMoving = true;
        
        if (CurrentCharacter)
        {
            CurrentCharacter.RemoveUIPopUpCharacterInfo(false);
        }
        
        //Reset Move/Attack character
        if (CurrentCharacterTurn.IsAI)
        {
            IsAIChatacterTurn = true;
            UIBoardGame.Instance.HideMenuUI();
        }
        else
        {
            IsAIChatacterTurn = false;
            UIBoardGame.Instance.UnHideMenuUI();
        }
        CurrentCharacterTurn.ResetCharacterTurn();

        CurrentState = StateNavigation;
        SelectTile(CurrentCharacterTurn.CurrentTile);
    }

    //Spawn Arrows for chose a Direction (end of character turn)
    public IEnumerator ShowPossibleTileDirectionEndOfCharacterTurn(float waitingTime)
    {
        PossibleEndTurnDirectionTileIsFinished = false;
        yield return new WaitUntil(() => !Wait);
        yield return new WaitForSeconds(waitingTime);
        
        if (CurrentCharacter)
        {
            /*ArrowsDirection.SetActive(true);
            ArrowsDirection.transform.position = TileSelected.Position;*/
            ShowPossibleTileDirection(CurrentCharacter.CurrentTile);
            if (IsController)
            {
                RemoveUICharacter?.Invoke();
            }
        }
        else
        {
            NextCharacterTurn();
        }
    }

    public Transform transformtest;
    public Vector3 Vector3test;
    
    [ContextMenu("MoveBoardCamera")]
    public void MoveBoardCamera(Vector3 destination)
    {
        StartCoroutine(MoveCamera(destination));
    }

    [ContextMenu("MoveBoardCameraVector")]
    public void MoveBoardCameraVector()
    {
        StartCoroutine(MoveCamera(Vector3test));
    }

 
    //Lerp Camera location to a new destination
    public IEnumerator MoveCamera(Transform destination)
    {
        if (CameraIsMoving) { yield break; }

        CameraIsMoving = true;
        Vector3 startPosition = BoardCamera.position;
        Quaternion startRotation = BoardCamera.rotation;
        float elapsedTime = 0;
        while (elapsedTime < CameraSpeed)
        {
            BoardCamera.SetPositionAndRotation(
                Vector3.Lerp(startPosition, destination.position, elapsedTime / CameraSpeed),
                Quaternion.Slerp(startRotation, destination.rotation, elapsedTime / CameraSpeed)
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        BoardCamera.SetPositionAndRotation(destination.position, destination.rotation);
        CameraIsMoving = false;
    }

    [SerializeField] private GameObject _ZoomVCam;

    public IEnumerator ZoomBattleCamera(float lifeTime)
    {
        yield return new WaitForSeconds(.75f);
        _ZommEffectAnimator.SetTrigger(Zoom);
        _ZoomVCam.SetActive(true);
        yield return new WaitForSeconds(lifeTime);
        _ZoomVCam.SetActive(false);
    }

    public IEnumerator SetBattleCamera(Character theAttacker , Character theAttacked,  GetAttackDirection.AttackDirection attackDirection, bool isCounter)
    {
        if (theAttacker.IsAI)
        {
            yield return new WaitForSeconds(0.3f);
        }
        TempBoardVCam.gameObject.SetActive(false);
        Vector3 vCamLeftPosition = theAttacker.VCamLeft.transform.position;
        Vector3 vCamRightPosition = theAttacker.VCamRight.transform.position;
        float vCamLeftDistance = Vector3.Distance(_CurrentCamera.position, vCamLeftPosition);
        float vCamRightDistance = Vector3.Distance(_CurrentCamera.position, vCamRightPosition);
        bool vCamLeftIsNear = vCamLeftDistance < vCamRightDistance;
        Debug.Log("SetBattleCamera attackDirection = " + attackDirection + " vCamLeftdistance = " + vCamLeftDistance + "  vCamRightdistance = " + vCamRightDistance);
        _CinemachineBrain.m_DefaultBlend.m_Time = _CinemachineBlendTimeZoomBattle;
        
        if (!isCounter)
        {
            AudioManager._Instance.SpawnSound( AudioManager._Instance._ZoomSFX);
            _ZommEffectAnimator.SetTrigger(Zoom);
        }
        
        if (vCamLeftIsNear)
        {
            theAttacker.VCamLeft.SetActive(true);
            yield return new WaitForSeconds(0.9f);
            _CinemachineBrain.m_DefaultBlend.m_Time = _CinemachineBlendTimeMovementBattle;
            if (attackDirection == GetAttackDirection.AttackDirection.Front)
            {
                if (theAttacked != null)
                {
                    theAttacker.VCamLeft.SetActive(false);
                    TempBoardVCam.transform.SetPositionAndRotation(theAttacked.VCamRight.transform.position,  theAttacked.VCamRight.transform.rotation);
                    TempBoardVCam.gameObject.SetActive(true);
                }
            }
            else if (attackDirection == GetAttackDirection.AttackDirection.Behind)
            {
                if (theAttacked != null)
                {
                    theAttacker.VCamLeft.SetActive(false);
                    TempBoardVCam.transform.SetPositionAndRotation(theAttacked.VCamLeft.transform.position,  theAttacked.VCamLeft.transform.rotation);
                    TempBoardVCam.gameObject.SetActive(true);
                }
            }
            else if (attackDirection == GetAttackDirection.AttackDirection.LeftSide)
            {
                if (theAttacked != null)
                {
                    theAttacker.VCamLeft.SetActive(false);
                    TempBoardVCam.transform.SetPositionAndRotation(theAttacked.VCamBehind.transform.position,  theAttacked.VCamBehind.transform.rotation);
                    TempBoardVCam.gameObject.SetActive(true);
                }
            }
            else
            {
                if (theAttacked != null)
                {
                    theAttacker.VCamLeft.SetActive(false);
                    TempBoardVCam.transform.SetPositionAndRotation(theAttacked.VCamFront.transform.position, theAttacked.VCamFront.transform.rotation);
                    TempBoardVCam.gameObject.SetActive(true);
                }
            }
        }
        else
        {
            theAttacker.VCamRight.SetActive(true);
            yield return new WaitForSeconds(0.9f);
            _CinemachineBrain.m_DefaultBlend.m_Time = _CinemachineBlendTimeMovementBattle;
            if (attackDirection == GetAttackDirection.AttackDirection.Front)
            {
                if (theAttacked != null)
                {
                    theAttacker.VCamRight.SetActive(false);
                    TempBoardVCam.transform.SetPositionAndRotation(theAttacked.VCamLeft.transform.position, theAttacked.VCamLeft.transform.rotation);
                    TempBoardVCam.gameObject.SetActive(true);
                }
                
            }
            else if (attackDirection == GetAttackDirection.AttackDirection.Behind)
            {
                if (theAttacked != null)
                {
                    theAttacker.VCamRight.SetActive(false);
                    TempBoardVCam.transform.SetPositionAndRotation(theAttacked.VCamRight.transform.position, theAttacked.VCamRight.transform.rotation);
                    TempBoardVCam.gameObject.SetActive(true);
                }
            }
            else if (attackDirection == GetAttackDirection.AttackDirection.LeftSide)
            {
                if (theAttacked != null)
                {
                    theAttacker.VCamRight.SetActive(false);
                    TempBoardVCam.transform.SetPositionAndRotation(theAttacked.VCamFront.transform.position, theAttacked.VCamFront.transform.rotation);
                    TempBoardVCam.gameObject.SetActive(true);
                }
            }
            else
            {
                if (theAttacked != null)
                {
                    theAttacker.VCamRight.SetActive(false);
                    TempBoardVCam.transform.SetPositionAndRotation(theAttacked.VCamBehind.transform.position, theAttacked.VCamBehind.transform.rotation);
                    TempBoardVCam.gameObject.SetActive(true);
                }
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => !Wait);
        yield return new WaitForSeconds(0.75f);
        Debug.Log("SetBattleCamera end of waiting ");
        TempBoardVCam.gameObject.SetActive(false);
    }
    
    private IEnumerator MoveCamera(Vector3 destination)
    {
        if (CameraIsMoving) { yield break; }

        CameraIsMoving = true;
        Vector3 startPosition = BoardCamera.position;
        Quaternion startRotation = BoardCamera.rotation;
        float elapsedTime = 0;
        while (elapsedTime < CameraSpeed)
        {
            BoardCamera.SetPositionAndRotation(
                Vector3.Lerp(startPosition, destination, elapsedTime / CameraSpeed),
                Quaternion.Slerp(startRotation, BoardCamera.rotation, elapsedTime / CameraSpeed)
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        BoardCamera.SetPositionAndRotation(destination,BoardCamera.rotation);
        CameraIsMoving = false;
    }

    // Turn left camera 
    public void SetRotationCameraLeft()
    {
        if(TileSelected == null) {return;}
        if ((int)_direction > 1)
        {
            _direction--;
        }
        else
        {
            _direction = Direction.West;
        }
        
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        StartCoroutine(MoveCamera(TileSelected.GetCameraTransform((int)_direction, IsCameraNear)));
    }
    
    // Turn right camera 
    public void SetRotationCameraRight()
    {
        if(TileSelected == null) {return;}
        if ((int)_direction < 4)
        {
            _direction++;
        }
        else
        {
            _direction = Direction.South;
        }
        
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        StartCoroutine(MoveCamera(TileSelected.GetCameraTransform((int)_direction, IsCameraNear)));
    }

    //When a Character die, remove it of the game
    public void RemoveCharacter(Character character)
    {
        if (character == CurrentCharacter)
        {
            CurrentCharacter = null;
        }
        character.CurrentTile.UnSetCharacter();
        CharacterList.Remove(character);

        StartCoroutine(CheckWinCondition());
    }

    private Vector3 SetEnemiesDirection(Direction direction)
    {
        switch (direction)
        {
            case Direction.None:
                break;
            case Direction.South:
                return new Vector3(0, 90, 0);
                break;
            case Direction.Est:
                return new Vector3(0, 180, 0);
                break;
            case Direction.North:
                return new Vector3(0, -90, 0);
                break;
            case Direction.West:
                break;
        }
        return Vector3.zero;
    }
    
    private Character CheckCharacterTime()
    {
        foreach (var character in CharacterList)
        {
            if (character.TurnTimeRemaining < 0)
            {
                return character;
            }
        }

        return null;
    }
    
    private void ReduceCharacterTimeRemaining()
    {
        foreach (var character in CharacterList)
        {
            character.SetRemaininTimeTurn();
        }
    }

    [ContextMenu("StartCinemachineImpulseSource")]
    public void StartCinemachineImpulseSource()
    {
        if (_TempVcamCinemachineImpulseSource.gameObject.activeInHierarchy)
        {
            _TempVcamCinemachineImpulseSource.GenerateImpulse();
        }
        if (_CinemachineImpulseSource.gameObject.activeInHierarchy)
        {
            _CinemachineImpulseSource.GenerateImpulse();
        }
        if (_ZoomCinemachineImpulseSource.gameObject.activeInHierarchy)
        {
            _ZoomCinemachineImpulseSource.GenerateImpulse();
        }
        CurrentCharacter.StartCinemachineImpulseSource();
    }

    private bool _CanressRepeatableAttack = true;

    public IEnumerator PressRepeatableAttackInput()
    {
        if(!_CanressRepeatableAttack){yield break;}
        Debug.Log("RepeatableAttackInputIsPress = true;");
        _CanressRepeatableAttack = false;
        RepeatableAttackInputIsPress = true;
        yield return new WaitForSeconds(0.05f);
        RepeatableAttackInputIsPress = false;
        Debug.Log("RepeatableAttackInputIsPress = false;");
        yield return new WaitForSeconds(0.9f);
        _CanressRepeatableAttack = true;
    }

    //-------------------------------DEBUG---------------------------------
    
    [SerializeField] private bool _UnitTest;
    public bool AddOccupiedTileOnClick;
    public List<TileManagerData> AllTileManagerDataUnitTest;
    public List<DataCharacterSpawner> CharacterAIDataTeam1UnitTest;
    public List<DataCharacterSpawner> CharacterAIDataTeam2UnitTest;
    public GameObject objectToSpawn;
    public int itterationobjectToSpawn;
    private static readonly int Zoom = Animator.StringToHash("Zoom");

    public  void SpawnObject(Vector3 spawnPosition, string name)
    {
        if (objectToSpawn != null)
        {
            // Instantiate the prefab at the specified position and rotation
            GameObject go = Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);
            go.name = name + itterationobjectToSpawn;
            itterationobjectToSpawn++;
        }
    }
    
    [ContextMenu("SetWaitValueFalse")]
    public void SetWaitValueFalse()
    {
        Wait = false;
    }
    
    [ContextMenu("SetWaitValueTrue")]
    public void SetWaitValueTrue()
    {
        Wait = true;
    }
}
