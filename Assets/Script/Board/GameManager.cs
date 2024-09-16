using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource _TempVcamCinemachineImpulseSource;
    [SerializeField] private CinemachineImpulseSource _CinemachineImpulseSource;
    [SerializeField] private Transform TempBoardVCam;
    [SerializeField] private Transform _CurrentCamera;
    [SerializeField] private CinemachineBrain _CinemachineBrain;
    [SerializeField] private float _CinemachineBlendTimeZoomBattle = 0.5f;
    [SerializeField] private float _CinemachineBlendTimeMovementBattle = 2;
    [SerializeField] private Animator _ZommEffectAnimator;
    public List<DataCharacterSpawner> CharacterAIData;
    public List<DataCharacterSpawner.DataSpawner> CharacterPlayerData;
    public DataCharacterSpawner.DataSpawner _CurrentCharacterDataSpawner;
    public CharacterSelectable _PlayerCharacterSpawnerList;
    public GameObject CharacterPrefab;
    public GameObject CharacterAIPrefab;
    public GameObject SquireAIPrefab;
    public GameObject SquirePrefab;
    public GameObject DragonPrefab;
    public GameObject DragonAIPrefab;
    public GameObject ArrowsPrefab;
    [field: SerializeField] public Transform BoardCamera { get; private set; }
    [field: SerializeField] public  int MaxDistanceEnemiesSpawn { get;  set; }
    [field: SerializeField] public  int MaxCharacterPlayerCanBePlace { get; private set; }
    [field: SerializeField] public float CameraSpeed { get; private set; }

    public List<Character> CharacterList = new List<Character>();
    public Tile TileSelected { get;  set; } = null;
    public Tile TilePreSelected { get;  set; } = null;
    public  GameObject ArrowsDirection { get;  set; }
    public  State CurrentState { get;  set; }
    public  StateChooseCharacter StateChooseCharacter { get; private set; }
    public  StateMoveCharacter StateMoveCharacter { get;  set; }
    public  StateAttackCharacter StateAttackCharacter { get;  set; }
    public  StateTurnCharacter StateTurnCharacter { get;  set; }
    public  StateNavigation StateNavigation { get;  set; }
    public bool NeedResetTiles { get; set; }
    public bool CameraIsMoving { get; private set; }
    public bool IsCameraNear { get; set ; }
    public bool MenuIsOpen { get; set; } = false;
    public bool IsController { get; set; } = false;
    public bool IsAIChatacterTurn { get; set; } = false;
    public bool PossibleTileIsFinished { get; set;}
    public bool IsCharactersAttacking { get; set; }
    public int IndexOccupiedTiles{ get; set; }
    public Tile[] OccupiedTiles{ get; private set; }

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
    public bool Wait
    {
        get => _wait;
        set
        {
            if (value)
            {
                DeactivateUIButtonCharacter();
            }
            else
            {
                ActivateUIButtonCharacter();
            }
            _wait = value;
        }
    }
    private bool _wait;

    [SerializeField]
    private Direction _Enemiesdirection;
    
    public Direction _direction;
    private Vector3 _enemiesDirection;
    
    private int _characterCount;
    public TilesManager _tileManager;
    
    private const int MAX_OCCUPIED_TILES = 15; 
    
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
        West =4
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
        StateChooseCharacter = new StateChooseCharacter(this,MaxCharacterPlayerCanBePlace, CharacterPlayerData);
        StateAttackCharacter = new StateAttackCharacter(this);
        StateMoveCharacter = new StateMoveCharacter(this);
        StateNavigation = new StateNavigation(this);
        CurrentState = StateChooseCharacter;
    }

    void Start()
    {
        StartCoroutine(InitializeGame());
    }

    private IEnumerator InitializeGame()
    {
        _tileManager = TilesManager.Instance;
        OccupiedTiles = new Tile[MAX_OCCUPIED_TILES];
        _direction = Direction.South;
        //ArrowsDirection = Instantiate(ArrowsPrefab, Vector3.zero, Quaternion.identity);
        //ArrowsDirection.SetActive(false);
        _enemiesDirection = SetEnemiesDirection();
        yield return _tileManager.SetBoardTiles();
        
        foreach (var characterSpawner in CharacterAIData)
        {
            foreach (var character in characterSpawner.DataSpawn)
            {
                yield return new WaitForSeconds(1);
                SpawnAICharacter(_tileManager.TileManagerData.Row - (_tileManager.TileManagerData.Row / MaxDistanceEnemiesSpawn), _tileManager.TileManagerData.Row, character);
            }
        }

        Tile tile = _tileManager.GetTile(_tileManager.TileManagerData.Column / 2, _tileManager.TileManagerData.Row / 2);
        TilePreSelected = tile;
        StartCoroutine(MoveCamera(tile.GetCameraTransform((int)_direction,false))) ;
        
        if (MaxCharacterPlayerCanBePlace == 0)
        {
            NextCharacterTurn();
        }
        else
        {
           _tileManager.SetValidSpawnTiles();
        }
        
        _PlayerCharacterSpawnerList.gameObject.SetActive(true);
    }

    public void SetCurrentCharacterDataSpawner(DataCharacterSpawner.DataSpawner characterDataSpawner)
    {
        _CurrentCharacterDataSpawner = characterDataSpawner;
    }

    private void SpawnAICharacter(int minCoordY, int maxCoordY, DataCharacterSpawner.DataSpawner dataCharacterSpawner)
    {
        bool isCharacterIsSpawned = false;
        while (!isCharacterIsSpawned)
        {
            int coordX = Random.Range(0, _tileManager.TileManagerData.Column);
            int coordY = Random.Range(minCoordY, maxCoordY);
        
            if (SpawnCharacter( _tileManager.GetTile(coordX, coordY), _enemiesDirection, dataCharacterSpawner))
            {
                isCharacterIsSpawned = true;
            }
        }
    }

    public bool SpawnCharacter(Tile tile, Vector3 rotation,  DataCharacterSpawner.DataSpawner dataCharacterSpawner)
    {
        if (tile.IsOccupied) {return false;}

        GameObject character = InstantiateCharacter(dataCharacterSpawner.CharactersPrefab, tile.Position);
        character.transform.Rotate(rotation);
        _characterCount++;
        character.name = "Character" + _characterCount;
        Character characterReference = character.GetComponent<Character>();
        characterReference.CurrentTile = tile;
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

        return null;
    }

    public void SelectTile(GameObject gameObjectTile)
    {
        if (_wait) {return;}
        Tile tile = _tileManager.GetTile(gameObjectTile);
        SelectTile(tile);
    }

    public void SelectTile(Tile tile)
    {
        if (_wait) {return;}
        
        StartCoroutine(MoveCamera(tile.GetCameraTransform((int)_direction, IsCameraNear)));
        TilePreSelected = tile;
        NeedResetTiles = true;
        CurrentState.SelectTile(tile);
        
        if(NeedResetTiles)
        {
            if (CurrentCharacter)
            { 
                CurrentCharacter.RemoveUIPopUpCharacterInfo(false);
            }
            _tileManager.DeselectTiles();
            _tileManager.AddSelectedTile(tile);
            
            tile.SetTopMaterial(_tileManager.MoveTileMaterial);
            RemoveUICharacter();
            CurrentState = StateNavigation;
            TileSelected = tile;
            StateAttackCharacter.ResetAttackData();
        }
    }

    public void SelectTileController(Tile tile)
    {
        Debug.Log("SelectTileController tile = " +tile.CoordX + "  " +tile.CoordY);
        if (_wait)
        {
            return;
        }

        StartCoroutine(MoveCamera(tile.GetCameraTransform((int)_direction, IsCameraNear)));
        tile.SetTopMaterial(_tileManager.SelectTileMaterial);
    }

    /*public void SelectTileController(Tile tile)
    {
        if (_wait) {return;}
        StartCoroutine(MoveCamera(tile.GetCameraTransform((int)_direction, IsCameraNear)));


        NeedResetTiles = true;
        CurrentState.SelectTile(tile);
         
        if(NeedResetTiles)
        {
            if (CurrentCharacter)
            { 
                CurrentCharacter.RemoveUIPopUpCharacterInfo(false);
            }
            _tileManager.DeselectTiles();
            _tileManager.AddSelectedTile(tile);
            
            tile.SetTopMaterial(_tileManager.MoveTileMaterial);
            RemoveUICharacter();
            CurrentState = StateNavigation;
            TileSelected = tile;
            StateAttackCharacter.ResetAttackData();
        }
    }*/

    //Select with color possible move tile
    public void ShowPossibleMove(Tile tile)
    {
        StateAttackCharacter.ResetAttackData();
        /*if (ArrowsDirection.activeSelf)
        {
            ArrowsDirection.SetActive(false);
        }*/
        IndexOccupiedTiles = 0;
        PossibleTileIsFinished = false;
        _tileManager.DeselectTiles();
        Debug.Log("Before GetMoveTiles1");
        _tileManager.BranchPath = 0;
        StartCoroutine(_tileManager.GetMoveTiles(CurrentCharacter.MovementPoint, null, tile));
        CurrentState = StateMoveCharacter;
    }
    
    //Select with color possible attack tile
    public void ShowPossibleAttack(Tile tile, bool isAICheck)
    {
        /*if (ArrowsDirection.activeSelf)
        {
            ArrowsDirection.SetActive(false);
        }*/
        IndexOccupiedTiles = 0;
        PossibleTileIsFinished = false;
        if (!isAICheck)
        {
            _tileManager.DeselectTiles();
            CurrentState = StateAttackCharacter;
        }
        
        StartCoroutine(_tileManager.GetAttackTiles(CurrentCharacter.AttackLenght, null, tile, _tileManager.AttackTileMaterial, isAICheck));
       
    }

    //Select with color possible direction (end character turn) tile
    private void ShowPossibleTileDirection(Tile tile)
    {
        _tileManager.DeselectTiles();
        StartCoroutine(_tileManager.GetAttackTiles(1, null, tile, _tileManager.MoveTileMaterial, false));
        CurrentState = StateTurnCharacter;
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
    public IEnumerator EndOfCharacterTurn(float waitingTime)
    {
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

    public IEnumerator SetBattleCamera(Character theAttacker , Character theAttacked,  GetAttackDirection.AttackDirection attackDirection, bool isCounter)
    {
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
        
        StartCoroutine(MoveCamera(TileSelected.GetCameraTransform((int)_direction, IsCameraNear)));
    }

    //When a Character die, remove it of the game
    public void RemoveCharacter(Character character)
    {
        if (character == CurrentCharacter)
        {
            CurrentCharacter = null;
        }
        character.CurrentTile.IsOccupied = false;
        CharacterList.Remove(character);
    }

    private Vector3 SetEnemiesDirection()
    {
        switch (_Enemiesdirection)
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
        _TempVcamCinemachineImpulseSource.GenerateImpulse();
        _CinemachineImpulseSource.GenerateImpulse();
        CurrentCharacter.StartCinemachineImpulseSource();
    }

    //-------------------------------DEBUG---------------------------------
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
}
