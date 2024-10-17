using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Attack[] _SkillAttack; 
    [SerializeField] private CinemachineImpulseSource _TempVcamCinemachineImpulseSource;
    [SerializeField] private CinemachineImpulseSource _CinemachineImpulseSource;
    [SerializeField] private CinemachineImpulseSource _ZoomCinemachineImpulseSource;
    [SerializeField] private Transform TempBoardVCam;
    [SerializeField] private Transform _CurrentCamera;
    [SerializeField] private CinemachineBrain _CinemachineBrain;
    [SerializeField] private GameObject _ZoomVCam;
    [SerializeField] private GameObject _UnZoomVCam;
    [SerializeField] private float _CinemachineBlendTimeZoomBattle = 0.5f;
    [SerializeField] private float _CinemachineBlendTimeMovementBattle = 2;
    [SerializeField] private Animator _ZommEffectAnimator;
    [SerializeField] private Animator _WinLooseAnimator;
    [SerializeField] private GameObject _LavaWaterPlane;
    [SerializeField] private TMP_Text _WinLooseText;
    [SerializeField] private List<Transform> AllTransitionElement;
    [SerializeField] private List<GameObject> AllTransitionElementToDeactivate;
    [SerializeField] private List<GameObject> AllTransitionElementToDeactivate2;
    [SerializeField] private GameObject _CameraRotation;
    [SerializeField] private Animator AddCharacterAnimator;
    [SerializeField] private GameObject AddCharacterWizard;
    [SerializeField] private GameObject AddCharacterRobot;
    [SerializeField] private GameObject AddCharacterFatherNature;
    public CharacterMaterial _AllPossibleCharacterMaterials;
    public List<DataCharacterSpawner> CharacterAIData;
    public DataCharacterSpawner PlayerDataCharacterSpawner;
    public DataCharacterSpawner PlayerTestDataCharacterSpawner;
    public DataCharacterSpawner CharacterDevilBossAIData;
    public DataCharacterSpawner CharacterWizardBossAIData;
    public DataCharacterSpawner CharacterRobotBossAIData;
    public DataCharacterSpawner CharacterFatherNatureBossAIData;
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
    public GameObject SelfDestructRobotPrefab;
    public GameObject SelfDestructRobotAIPrefab;
    public GameObject DevilBossPrefab;
    public GameObject DevilBossAIPrefab;
    public GameObject ArrowsPrefab;
    public GameObject CameraButton;
    [field: SerializeField] public Transform BoardCamera { get; private set; }
    [field: SerializeField] public int MaxDistanceEnemiesSpawn { get; set; }
    [field: SerializeField] public int MaxCharacterPlayerCanBePlace { get; private set; }
    [field: SerializeField] public float CameraSpeed { get; private set; }

    public bool _IsStartScene;
    public bool _IsMapScene;

    public List<Character> CharacterList = new List<Character>();
    public List<Transform> AllCharacterGo = new List<Transform>();
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

    public bool _GameIsFinish { get; set; }
    public bool CameraIsMoving { get; private set; }
    public bool IsCameraNear { get; set; }
    public bool MenuIsOpen { get; set; } = false;
    public bool IsController { get; set; } = false;
    public bool IsAIChatacterTurn { get; set; } = false;

    public bool IsCharactersAttacking { get; set; }
    public int IndexOccupiedTiles { get; set; }
    public bool RepeatableAttackInputIsPress { get; set; }
    
    public bool _IsWizardBoss { get; set; }
    public bool _IsRobotBoss { get; set; }
    public bool _IsFatherNatureBoss { get; set; }
    
    public Tile[] OccupiedTiles { get; private set; }
    

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
    public Direction _direction;
    private int _SpawnMobItteration;
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

    private const int MAX_OCCUPIED_TILES = 60;

    public Action SelectCharacter;
    public Action RemoveUICharacter;
    public Action DeactivateUIButtonCharacter;
    public Action ActivateUIButtonCharacter;
    public Action DesableMoveCharacterUIButtons;
    public Action DesableAttackCharacterUIButtons;
    public Action SetInteractableWaitButton;
    public Action<bool> SetInteractableAttackButton;


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

    private int _IndexTeam2TeamColor;
    

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
        
        
        PlayerDataCharacterSpawner.DataSpawn.Clear();

        DataCharacterSpawner.DataSpawner squireDataSpawner = new DataCharacterSpawner.DataSpawner()
        {
            CharactersPrefab = DataCharacterSpawner.CharactersPrefab.Squire,
            Team = Character.Team.Team1,
            SkillAttack = CharacterDevilBossAIData.GetCharacterSkillAttack(FBPP.GetInt("SquireSkillAttack")),
            Ability1 = CharacterDevilBossAIData.GetCharacterAbility(FBPP.GetInt("SquireAbility1")),
            Ability2 = CharacterDevilBossAIData.GetCharacterAbility(FBPP.GetInt("SquireAbility2")),
            Name = FBPP.GetString("SquireName")
        };
        
        PlayerDataCharacterSpawner.DataSpawn.Add(squireDataSpawner);

        if (FBPP.GetBool("WizardBossIsDead"))
        {
            DataCharacterSpawner.DataSpawner wizardDataSpawner = new DataCharacterSpawner.DataSpawner()
            {
                CharactersPrefab = DataCharacterSpawner.CharactersPrefab.Wizard,
                Team = Character.Team.Team1,
                SkillAttack = CharacterDevilBossAIData.GetCharacterSkillAttack(FBPP.GetInt("WizardSkillAttack")),
                Ability1 = CharacterDevilBossAIData.GetCharacterAbility(FBPP.GetInt("WizardAbility1")),
                Ability2 = CharacterDevilBossAIData.GetCharacterAbility(FBPP.GetInt("WizardAbility2")),
                Name = FBPP.GetString("WizardName")
            };
        
            PlayerDataCharacterSpawner.DataSpawn.Add(wizardDataSpawner);
        }
        
        if (FBPP.GetBool("RobotBossIsDead"))
        {
            DataCharacterSpawner.DataSpawner robotDataSpawner = new DataCharacterSpawner.DataSpawner()
            {
                CharactersPrefab = DataCharacterSpawner.CharactersPrefab.Robot,
                Team = Character.Team.Team1,
                SkillAttack = CharacterDevilBossAIData.GetCharacterSkillAttack(FBPP.GetInt("RobotSkillAttack")),
                Ability1 = CharacterDevilBossAIData.GetCharacterAbility(FBPP.GetInt("RobotAbility1")),
                Ability2 = CharacterDevilBossAIData.GetCharacterAbility(FBPP.GetInt("RobotAbility2")),
                Name = FBPP.GetString("RobotName")
            };
        
            PlayerDataCharacterSpawner.DataSpawn.Add(robotDataSpawner);
        }
        
        if (FBPP.GetBool("FatherNatureBossIsDead"))
        {
            DataCharacterSpawner.DataSpawner fatherNatureDataSpawner = new DataCharacterSpawner.DataSpawner()
            {
                CharactersPrefab = DataCharacterSpawner.CharactersPrefab.MotherNature,
                Team = Character.Team.Team1,
                SkillAttack = CharacterDevilBossAIData.GetCharacterSkillAttack(FBPP.GetInt("FatherNatureSkillAttack")),
                Ability1 = CharacterDevilBossAIData.GetCharacterAbility(FBPP.GetInt("FatherNatureAbility1")),
                Ability2 = CharacterDevilBossAIData.GetCharacterAbility(FBPP.GetInt("FatherNatureAbility2")),
                Name = FBPP.GetString("FatherNatureName")
            };
        
            PlayerDataCharacterSpawner.DataSpawn.Add(fatherNatureDataSpawner);
        }
        

        if (_IsMapScene)
        {
            FBPP.SetBool("IsDevilBoss",  false);
            FBPP.SetBool("IsRobotBoss",  false);
            FBPP.SetBool("IsFatherNatureBoss",  false);
            FBPP.SetBool("IsWizardBoss",  false);
            FBPP.Save();
            int tileCoordX = FBPP.GetInt("PositionTileCoordX");
            int tileCoordY = FBPP.GetInt("PositionTileCoordY");
            int environment = FBPP.GetInt("Environment");
            bool isWaterTile = FBPP.GetBool("isWaterTile");
            
            StartCoroutine(_MapTilesManager_Lava.CameraTransitionToMapScene(BoardCamera));
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
            
            MapTilesManager mapTilesManager = null;
            
            if (environment == (int)_MapTilesManager_Lava._Environement)
            {
                mapTilesManager = _MapTilesManager_Lava;
            }
            
            if (environment == (int)_MapTilesManager_Grass._Environement)
            {
                mapTilesManager = _MapTilesManager_Grass;
            }
            
            if (environment == (int)_MapTilesManager_Desert._Environement)
            {
                mapTilesManager = _MapTilesManager_Desert;
            }
            
            if (environment == (int)_MapTilesManager_Snow._Environement)
            {
                mapTilesManager = _MapTilesManager_Snow;
            }
            
            if (environment == (int)_MapTilesManager_Poison._Environement)
            {
                mapTilesManager = _MapTilesManager_Poison;
            }
            
            if (environment == (int)_MapTilesManager_Corner1._Environement)
            {
                mapTilesManager = _MapTilesManager_Corner1;
            }
            
            if (environment == (int)_MapTilesManager_Corner2._Environement)
            {
                mapTilesManager = _MapTilesManager_Corner2;
            }
            
            if (environment == (int)_MapTilesManager_Corner3._Environement)
            {
                mapTilesManager = _MapTilesManager_Corner3;
            }
            
            if (environment == (int)_MapTilesManager_Corner4._Environement)
            {
                mapTilesManager = _MapTilesManager_Corner4;
            }
            
            Tile spawnTile = null;
            
            if (mapTilesManager != null)
            {
                if (tileCoordX == 99)
                {
                    spawnTile = mapTilesManager.GetTile(_MapTilesManager_Lava.TileManagerData.Row - 2, _MapTilesManager_Lava.TileManagerData.Column / 2);
                }
                else
                {
                    spawnTile = mapTilesManager.GetTile(tileCoordX, tileCoordY);
                }
            }
            
            if (environment == 1 || environment == 6 || environment == 7)
            {
                //TileSelected = spawnTile;
                _direction = Direction.North;
               // StartCoroutine(MoveCamera(TileSelected.GetCameraTransform((int)_direction, IsCameraNear)));
            }
            yield return new WaitForSeconds(2);


            
            if (spawnTile == null)
            {
                SpawnMapCharacter(_MapTilesManager_Lava.GetTile(5, 5), Vector3.zero, _MapCharacterData.DataSpawn[0], false, true);
            }
            else
            {
                SpawnMapCharacter(spawnTile, Vector3.zero, _MapCharacterData.DataSpawn[0], false, true);
            }

 
            CameraButton.SetActive(true);
            ShowPossibleMapMove(TileSelected);
            TilePreSelected = CurrentCharacter.CurrentTile;
            yield break;
        }

        if (_UnitTest || _RandomMap)
        {
            if (!_ForceTileManager)
            {
                _tileManager.SetRandomTileManagerData();
            }
        }
        else if(_ForceTileManager)
        {
            int backgroundTile  = _tileManager.GetApproximateHeight(_tileManager.TileManagerData.BoardTileHeight);
            _tileManager.TileManagerData.NumberGroundBackgroundTilesColumn = backgroundTile;
            _tileManager.TileManagerData.NumberGroundBackgroundTilesRow = backgroundTile;

        }
        else
        {
            bool isWaterTile = FBPP.GetBool("isWaterTile");
            _IsFatherNatureBoss = FBPP.GetBool("IsFatherNatureBoss");
            _IsWizardBoss = FBPP.GetBool("IsWizardBoss");
            _IsRobotBoss = FBPP.GetBool("IsRobotBoss");
            bool isDevilBoss = FBPP.GetBool("IsDevilBoss");
            int environment = FBPP.GetInt("Environment");

            if (isDevilBoss)
            {
                CharacterAIData.Clear();
                CharacterAIData.Add(CharacterDevilBossAIData);
            }
            else if (_IsWizardBoss)
            {
                CharacterAIData.Clear();
                CharacterAIData.Add(CharacterWizardBossAIData);
            }
            if (_IsFatherNatureBoss)
            {
                CharacterAIData.Clear();
                CharacterAIData.Add(CharacterFatherNatureBossAIData);
            }
            if (_IsRobotBoss)
            {
                CharacterAIData.Clear();
                CharacterAIData.Add(CharacterRobotBossAIData);
            }

            
            _tileManager.SetEnvironmentTileManagerData(environment, isWaterTile);
            
        }

        yield return _tileManager.SetBoardTiles();

        if (_UnitTest)
        {
            DataCharacterSpawner dataCharacterSpawner = CharacterAIDataTeam2UnitTest[Random.Range(0, CharacterAIDataTeam2UnitTest.Count - 1)];
            _IndexTeam2TeamColor = dataCharacterSpawner.TeamColor;

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
                _IndexTeam2TeamColor = characterSpawner.TeamColor;
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
        character.name = dataCharacterSpawner.Name;
        Character characterReference = character.GetComponent<Character>();
        characterReference.CharacterType = dataCharacterSpawner.CharactersPrefab;
        characterReference.CurrentTile = tile;
        CharacterList.Add(characterReference);
        tile.SetCharacter(characterReference);
        AllCharacterGo.Add(character.transform);
        characterReference.CurrentTeam = dataCharacterSpawner.Team;
        int indexMaterial = FBPP.GetInt("TeamColor");
        if (characterReference.CurrentTeam == Character.Team.Team1)
        {
            characterReference.SetCharacterColor(_AllPossibleCharacterMaterials.AllPossibleMaterials[indexMaterial]);
        }
        else
        {
            if (_IndexTeam2TeamColor == indexMaterial)
            {
                _IndexTeam2TeamColor++;
            }

            if (_IndexTeam2TeamColor > _AllPossibleCharacterMaterials.AllPossibleMaterials.Length - 1)
            {
                _IndexTeam2TeamColor = 0;
            }
            characterReference.SetCharacterColor(_AllPossibleCharacterMaterials.AllPossibleMaterials[_IndexTeam2TeamColor]);
        }

        characterReference._SkillAttack = _SkillAttack[(int)dataCharacterSpawner.SkillAttack];
        characterReference.SetAbility(dataCharacterSpawner.Ability1);
        characterReference.SetAbility(dataCharacterSpawner.Ability2);
        characterReference.SetAbility(characterReference.BaseAbility1);
        characterReference.SetAbility(characterReference.BaseAbility2);
        
        if (!_IsMapScene)
        {
            characterReference.SetElementEffect(tile.IsWater, true);
        }
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
        CurrentCharacter.ActionDestroyCharacterRelated += characterReference.DestroyCharacter;
        characterReference.CanMove = canMove;
        character.transform.Rotate(rotation);
        _SpawnMobItteration++;
        character.name += _SpawnMobItteration;
        characterReference.CurrentTile = tile;
        characterReference.CurrentTeam = CurrentCharacter.CurrentTeam;

        CharacterList.Add(characterReference);
        tile.SetCharacter(characterReference);
        AllCharacterGo.Add(character.transform);
        if (CurrentCharacter.CurrentTeam == Character.Team.Team1)
        {
            int indexMaterial = FBPP.GetInt("TeamColor");
            characterReference.SetCharacterColor(_AllPossibleCharacterMaterials.AllPossibleMaterials[indexMaterial]);
        }
        else
        {
            characterReference.SetCharacterColor(_AllPossibleCharacterMaterials.AllPossibleMaterials[_IndexTeam2TeamColor]);
        }
        
        characterReference.SetElementEffect(tile.IsWater ,true);
        
    }

    public Character SpawnMapCharacter(Tile tile, Vector3 rotation, DataCharacterSpawner.DataSpawner dataCharacterSpawner, bool isDevilBoss , bool isPlayerCharacter)
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
        
        if (!isDevilBoss)
        {
            characterGameObject.transform.localScale = new Vector3(0.6f, 0.6f, 0.6f);
        }

        int indexMaterial = FBPP.GetInt("TeamColor");
        
        if (isPlayerCharacter)
        {
            CurrentCharacter = characterReference;
            CurrentState = StateMoveCharacter;
            TileSelected = tile;
            StartCoroutine(MoveCamera(tile.GetCameraTransform((int)_direction, IsCameraNear)));
            characterReference.SetCharacterColor(_AllPossibleCharacterMaterials.AllPossibleMaterials[indexMaterial]);
        }
        else
        {
            if (_IndexTeam2TeamColor == indexMaterial)
            {
                _IndexTeam2TeamColor++;
            }

            if (_IndexTeam2TeamColor > _AllPossibleCharacterMaterials.AllPossibleMaterials.Length - 1)
            {
                _IndexTeam2TeamColor = 0;
            }
            characterReference.SetCharacterColor(_AllPossibleCharacterMaterials.AllPossibleMaterials[_IndexTeam2TeamColor]);
        }

        AllTransitionElement.Add(characterGameObject.transform);
        return characterReference;

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
        
        if (charactersPrefab == DataCharacterSpawner.CharactersPrefab.SelfDestructRobot)
        {
            GameObject SelfDestructRobotGo = Instantiate(SelfDestructRobotPrefab, position, Quaternion.identity);
            SelfDestructRobotGo.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            return SelfDestructRobotGo;
        }
        
        if (charactersPrefab == DataCharacterSpawner.CharactersPrefab.SelfDestructRobotAI)
        {
            GameObject SelfDestructRobotAIGo = Instantiate(SelfDestructRobotAIPrefab, position, Quaternion.identity);
            SelfDestructRobotAIGo.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            return SelfDestructRobotAIGo;
        }
        
        if (charactersPrefab == DataCharacterSpawner.CharactersPrefab.DevilBoss)
        {
            GameObject devilBoss = Instantiate(DevilBossPrefab, position, Quaternion.identity);
            devilBoss.transform.localScale = new Vector3(2, 2, 2);
            return devilBoss;
        }
        
        if (charactersPrefab == DataCharacterSpawner.CharactersPrefab.DevilBossAI)
        {
            GameObject devilBossAI = Instantiate(DevilBossAIPrefab, position, Quaternion.identity);
            devilBossAI.transform.localScale = new Vector3(2, 2, 2);
            return devilBossAI;
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
                CurrentCharacter.ActionRemoveUIPopUpCharacterInfo(false);
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
            if (tile.CharacterReference != CurrentCharacter && tile.CharacterReference != null)
            {
                StartCoroutine(MapSceneToBattleScene(""));
                return;
            }
            /*if (_DoOnceStartBattle && Random.Range(0, 4) == 1)
            {
                StartCoroutine(MapSceneToBattleScene());
                return;
            }*/

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

    public IEnumerator MapSceneToBattleScene(string characterType)
    {
        _GameIsFinish = true;
        AudioManager._Instance.SpawnSound( AudioManager._Instance._GameIsOver);
        if (characterType == "14")
        {
            FBPP.SetBool("IsDevilBoss", true);
        }
        else if (characterType == "7")
        {
            FBPP.SetBool("IsFatherNatureBoss", true);
        }
        else if (characterType == "5")
        {
            FBPP.SetBool("IsWizardBoss", true);
        }
        else if (characterType == "9")
        {
            FBPP.SetBool("IsRobotBoss", true);
        }
        
        FBPP.SetBool("IsWaterTile", TileSelected.IsWater);
        FBPP.SetInt("PositionTileCoordX", TileSelected.CoordX);
        FBPP.SetInt("PositionTileCoordY", TileSelected.CoordY);
        FBPP.SetInt("Environment", (int)TileSelected.MapTilesManager._Environement);
        FBPP.Save();
        StartCoroutine(ZoomBattleCamera(100f));
        foreach (var elementToDeactivate in AllTransitionElementToDeactivate)
        {
            elementToDeactivate.SetActive(false);
        }

        if (TileSelected.MapTilesManager != _MapTilesManager_Snow)
        {
            StartCoroutine(_MapTilesManager_Snow.TransitionToBattleScene());
        }
      
        yield return new WaitForSeconds(0.2f);
        
        if (TileSelected.MapTilesManager != _MapTilesManager_Grass)
        {
            StartCoroutine(_MapTilesManager_Grass.TransitionToBattleScene());
        }
    
        AudioManager._Instance.SpawnSound( AudioManager._Instance._GameIsOverMoveTxt);
        yield return new WaitForSeconds(0.2f);
        
        if (TileSelected.MapTilesManager != _MapTilesManager_Lava)
        {
            StartCoroutine(_MapTilesManager_Lava.TransitionToBattleScene());
        }
     
        yield return new WaitForSeconds(0.2f);
        
        if (TileSelected.MapTilesManager != _MapTilesManager_Poison)
        {
            StartCoroutine(_MapTilesManager_Poison.TransitionToBattleScene());
        }
  
        yield return new WaitForSeconds(0.2f);
        
        if (TileSelected.MapTilesManager != _MapTilesManager_Desert)
        {
            StartCoroutine(_MapTilesManager_Desert.TransitionToBattleScene());
        }
      
        if (TileSelected.MapTilesManager != _MapTilesManager_Corner1)
        {
            StartCoroutine(_MapTilesManager_Corner1.TransitionToBattleScene());
        }
       
        yield return new WaitForSeconds(0.2f);
        
        if (TileSelected.MapTilesManager != _MapTilesManager_Corner2)
        {
            StartCoroutine(_MapTilesManager_Corner2.TransitionToBattleScene());
        }
        
        if (TileSelected.MapTilesManager != _MapTilesManager_Corner3)
        {
            StartCoroutine(_MapTilesManager_Corner3.TransitionToBattleScene());
        }
        
        yield return new WaitForSeconds(0.15f);
        
        if (TileSelected.MapTilesManager != _MapTilesManager_Corner4)
        {
            StartCoroutine(_MapTilesManager_Corner4.TransitionToBattleScene());
        }
     
        yield return new WaitForSeconds(0.15f);
        StartCoroutine(TransitionToBattleScene());
        
        if (TileSelected.MapTilesManager == _MapTilesManager_Snow)
        {
            StartCoroutine(_MapTilesManager_Snow.TransitionToBattleScene());
        }
        
        if (TileSelected.MapTilesManager == _MapTilesManager_Grass)
        {
            StartCoroutine(_MapTilesManager_Grass.TransitionToBattleScene());
        }

        if (TileSelected.MapTilesManager != _MapTilesManager_Lava)
        {
            StartCoroutine(_MapTilesManager_Lava.TransitionToBattleScene());
        }

        if (TileSelected.MapTilesManager == _MapTilesManager_Poison)
        {
            StartCoroutine(_MapTilesManager_Poison.TransitionToBattleScene());
        }
          
        if (TileSelected.MapTilesManager == _MapTilesManager_Desert)
        {
            StartCoroutine(_MapTilesManager_Desert.TransitionToBattleScene());
        }
      
        if (TileSelected.MapTilesManager == _MapTilesManager_Corner1)
        {
            StartCoroutine(_MapTilesManager_Corner1.TransitionToBattleScene());
        }

        if (TileSelected.MapTilesManager == _MapTilesManager_Corner2)
        {
            StartCoroutine(_MapTilesManager_Corner2.TransitionToBattleScene());
        }
        
        if (TileSelected.MapTilesManager == _MapTilesManager_Corner3)
        {
            StartCoroutine(_MapTilesManager_Corner3.TransitionToBattleScene());
        }
        
        
        if (TileSelected.MapTilesManager == _MapTilesManager_Corner4)
        {
            StartCoroutine(_MapTilesManager_Corner4.TransitionToBattleScene());
        }
        
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
        if (_GameIsFinish)
        {
            yield break;
        }
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

        StartCoroutine(SetEndGame(false));

    }

    [ContextMenu("SetEndGameTest")]
    public void SetEndGameTest()
    {
        StartCoroutine(SetEndGame(true));
    }

    public IEnumerator SetEndGame(bool forcePlayerWin)
    {
        yield return new WaitForSeconds(0.25f);
        AudioManager._Instance.SpawnSound( AudioManager._Instance._GameIsOver);
        yield return new WaitForSeconds(0.25f);
        bool playerWin = true;
        

        if (CharacterList.Count > 0)
        {
            if (CharacterList[0] == null || CharacterList[0].CurrentTeam != Character.Team.Team1)
            {
                playerWin = false;
                if (!forcePlayerWin)
                {
                    _WinLooseText.text = "You Lost";
                }
                
            }
        }
        else
        {
            playerWin = false;
            if (!forcePlayerWin)
            {
                _WinLooseText.text = "You Lost";
            }
        }

        if (forcePlayerWin)
        {
            playerWin = true;
        }
        
        _WinLooseAnimator.gameObject.SetActive(true);
        Time.timeScale = 0.2f;
        _GameIsFinish = true;
        yield return new WaitForSeconds(0.3f);
        AudioManager._Instance.SpawnSound( AudioManager._Instance._GameIsOverMoveTxt);
        yield return new WaitForSeconds(0.5f);
        AudioManager._Instance.SpawnSound( AudioManager._Instance._GameIsOverMoveTxt);
        
      
        if (_UnitTest)
        {
            SceneManager.LoadScene("BattleScene");
        }
        else
        {
            CameraButton.SetActive(false);

            
            if (playerWin)
            {
                yield return new WaitForSeconds(0.2f);
                RemoveUICharacter?.Invoke();
                if (CurrentCharacterTurn != null)
                {
                    CurrentCharacterTurn.ActionRemoveUIPopUpCharacterInfo(true);
                }
                if (_IsWizardBoss)
                {
                    FBPP.SetBool("WizardBossIsDead", true);
                    _WinLooseText.color = Color.yellow;
                    _WinLooseText.text = "The Wizard Joins Your Ranks";
                    _WinLooseAnimator.SetTrigger("NewCharacter");
                    yield return new WaitForSeconds(0.3f);
                    AudioManager._Instance.SpawnSound( AudioManager._Instance._GameIsOverMoveTxt);
                    yield return new WaitForSeconds(0.5f);
                    MenuIsOpen = true;
                    AudioManager._Instance.SpawnSound( AudioManager._Instance._GameIsOverMoveTxt);
                    Time.timeScale = 1f;
                    yield return new WaitForSeconds(0.2f);
                    AddCharacterAnimator.gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.35f);
                    AddCharacterWizard.SetActive(true);
                    Wait = true;
                    yield return new WaitUntil(() => !Wait);
                    AddCharacterAnimator.SetTrigger("Hide");
                    yield return new WaitForSeconds(0.35f);
                    AddCharacterWizard.SetActive(false);
                }
                else if (_IsFatherNatureBoss)
                {
                    FBPP.SetBool("FatherNatureBossIsDead", true);
                    _WinLooseText.color = Color.yellow;
                    _WinLooseText.text = "This Weird Man Joins You";
                    _WinLooseAnimator.SetTrigger("NewCharacter");
                    yield return new WaitForSeconds(0.3f);
                    AudioManager._Instance.SpawnSound( AudioManager._Instance._GameIsOverMoveTxt);
                    yield return new WaitForSeconds(0.5f);
                    MenuIsOpen = true;
                    AudioManager._Instance.SpawnSound( AudioManager._Instance._GameIsOverMoveTxt);
                    Time.timeScale = 1f;
                    yield return new WaitForSeconds(0.2f);
                    AddCharacterAnimator.gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.35f);
                    AddCharacterFatherNature.SetActive(true);
                    Wait = true;
                    yield return new WaitUntil(() => !Wait);
                    AddCharacterAnimator.SetTrigger("Hide");
                    yield return new WaitForSeconds(0.35f);
                    AddCharacterFatherNature.SetActive(false);
                }
                else if (_IsRobotBoss)
                {
                    FBPP.SetBool("RobotBossIsDead", true);
                    _WinLooseText.color = Color.yellow;
                    _WinLooseText.text = "The Robot Joins Your Ranks";
                    _WinLooseAnimator.SetTrigger("NewCharacter");
                    yield return new WaitForSeconds(0.3f);
                    AudioManager._Instance.SpawnSound( AudioManager._Instance._GameIsOverMoveTxt);
                    yield return new WaitForSeconds(0.5f);
                    MenuIsOpen = true;
                    AudioManager._Instance.SpawnSound( AudioManager._Instance._GameIsOverMoveTxt);
                    Time.timeScale = 1f;
                    yield return new WaitForSeconds(0.2f);
                    AddCharacterAnimator.gameObject.SetActive(true);
                    yield return new WaitForSeconds(0.35f);
                    AddCharacterRobot.SetActive(true);
                    Wait = true;
                    yield return new WaitUntil(() => !Wait);
                    AddCharacterAnimator.SetTrigger("Hide");
                    yield return new WaitForSeconds(0.35f);
                    AddCharacterRobot.SetActive(false);
                }
                else if (FBPP.GetBool("IsDevilBoss"))
                {
                    FBPP.SetBool("DevilBossIsDead", true);
                }
                
                FBPP.Save();
            }
            
            yield return new WaitForSeconds(0.5f);
            Time.timeScale = 1f;
            StartCoroutine(_tileManager.TileTransitionToMapScene());
            foreach (var characterGo in AllCharacterGo)
            {
                if (characterGo != null)
                {
                    characterGo.gameObject.SetActive(false);
                }
            }
            StartCoroutine(_tileManager.CharacterTransitionToMapScene(AllCharacterGo));
            if (CurrentCharacterTurn != null)
            {
                CurrentCharacterTurn.ActionRemoveUIPopUpCharacterInfo(true);
            }
           
            RemoveUICharacter?.Invoke();
            yield return new WaitForSeconds(0.5f);
            foreach (var character in CharacterList)
            {
                if (character != null)
                {
                    character.gameObject.SetActive(false);
                }
            }
            yield return new WaitForSeconds(0.25f);
            TempBoardVCam.gameObject.SetActive(false);
            yield return StartCoroutine(_tileManager.CameraTransitionToMapScene(BoardCamera));
            SceneManager.LoadScene("MapScene");
        }
    }


    private bool _IsChangingCharacterTurn;
    //Go to next character turn
    public void NextCharacterTurn()
    {
      if(_IsChangingCharacterTurn || _GameIsFinish) {return;}

      _IsChangingCharacterTurn = true;
      
        if (CurrentCharacterTurn != null)
        {
            Debug.Log("GameManager :: NextCharacterTurn CurrentCharacterTurn = " + CurrentCharacterTurn.name);
        }
        else
        {
            Debug.Log("GameManager :: NextCharacterTurn CurrentCharacterTurn = NULL");
        }
        StartCoroutine(BeginOfTurn());
    }
    
    // Begin new turn
    private IEnumerator BeginOfTurn()
    {
        SetInteractableAttackButton?.Invoke(true);
        yield return new WaitForSeconds(1);
        if (CurrentCharacterTurn != null)
        {
            Debug.Log("GameManager :: BeginOfTurn Before change character CurrentCharacterTurn = " + CurrentCharacterTurn.name);
        }
        else
        {
            Debug.Log("GameManager :: BeginOfTurn  Before change character CurrentCharacterTurn = NULL");
        }
        
        SetInteractableWaitButton?.Invoke();
        CurrentCharacterTurn = CheckCharacterTime();
        int itteration = 0;
        while (CurrentCharacterTurn == null || CurrentCharacterTurn.CurrentHealth <= 0)
        {
            itteration++;
            ReduceCharacterTimeRemaining();
            CurrentCharacterTurn = CheckCharacterTime();

            if (_GameIsFinish || itteration > 100)
            {
                Debug.Log("GameManager :: BeginOfTurn  _GameIsFinish || itteration > 100    yield break itteration = " + itteration);
                yield break;
            }
        }

        if (CurrentCharacterTurn != null)
        {
            Debug.Log("GameManager :: BeginOfTurn after change character CurrentCharacterTurn = " + CurrentCharacterTurn.name);
        }
        else
        {
            Debug.Log("GameManager :: BeginOfTurn after change character CurrentCharacterTurn = NULL");
        }
        //CameraIsMoving = true;
        
        if (CurrentCharacter)
        {
            CurrentCharacter.ActionRemoveUIPopUpCharacterInfo(false);
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
        _IsChangingCharacterTurn = false;
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

    public IEnumerator ZoomBattleCamera(float lifeTime)
    {
        yield return new WaitForSeconds(.75f);
        _ZommEffectAnimator.SetTrigger(Zoom);
        _ZoomVCam.SetActive(true);
        yield return new WaitForSeconds(lifeTime);
        _ZoomVCam.SetActive(false);
    }
    
    public IEnumerator UnZoomBattleCamera(float lifeTime)
    {
        yield return new WaitForSeconds(.75f);
        _ZommEffectAnimator.SetTrigger(Zoom);
        _UnZoomVCam.SetActive(true);
 
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
        Debug.Log("SetBattleCamera attackDirection = " + attackDirection + "  vCamLeftdistance = " + vCamLeftDistance + "  vCamRightdistance = " + vCamRightDistance + "  _Attack = "+ StateAttackCharacter._Attack);
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

        if (!_GameIsFinish)
        {
            TempBoardVCam.gameObject.SetActive(false);
        }
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
        if(_GameIsFinish) { return;}
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
            if (character != null)
            {
                if (character.TurnTimeRemaining < 0)
                {
                    return character;
                }
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

        if (CurrentCharacter != null)
        {
            CurrentCharacter.StartCinemachineImpulseSource();
        }
        
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
    
    [SerializeField] private bool _RandomMap = true;
    [SerializeField] private bool _ForceTileManager = true;
    [SerializeField] private bool _UnitTest;
    public bool AddOccupiedTileOnClick;
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
}
