using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public List<DataCharacterSpawner> CharacterAIData;
    public List<DataCharacterSpawner> CharacterPlayerData;
    
    public GameObject CharacterPrefab;
    public GameObject CharacterAIPrefab;
    public GameObject ArrowsPrefab;

    
    [field: SerializeField] public Transform BoardCamera { get; private set; }
    [field: SerializeField] public  int MaxDistanceEnemiesSpawn { get;  set; }
    [field: SerializeField] public  int MaxCharacterPlayerCanBePlace { get; private set; }
    [field: SerializeField] public float CameraSpeed { get; private set; }

    public List<Character> CharacterList = new List<Character>();
    public Tile TileSelected { get;  set; } = null;
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
    public bool PossibleTileIsFinished { get; set;}
    public int IndexOccupiedTiles{ get; set; }
    public Tile[] OccupiedTiles{ get; private set; }

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
    
    private Direction _direction;
    private Vector3 _enemiesDirection;
    
    private int _characterCount;
    private TilesManager _tileManager;
    
    private const int MAX_OCCUPIED_TILES = 15; 
    
    public Action SelectCharacter;
    public Action RemoveUICharacter;
    public Action DeactivateUIButtonCharacter;
    public Action ActivateUIButtonCharacter;
    public Action DesableMoveCharacterUIButtons;
    public Action DesableAttackCharacterUIButtons;
    
    private enum Direction
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
        StateChooseCharacter = new StateChooseCharacter(this,MaxCharacterPlayerCanBePlace, CharacterPlayerData[0].DataSpawn[0]);
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
        ArrowsDirection = Instantiate(ArrowsPrefab, Vector3.zero, Quaternion.identity);
        ArrowsDirection.SetActive(false);
        _enemiesDirection = SetEnemiesDirection();
        yield return _tileManager.SetBoardTiles();
        
        foreach (var characterSpawner in CharacterAIData)
        {
            foreach (var character in characterSpawner.DataSpawn)
            {
                yield return new WaitForSeconds(1);
                if (character.Team == Character.Team.Team2)
                {
                    SpawnAICharacter(
                        _tileManager.TileManagerData.Row - (_tileManager.TileManagerData.Row / MaxDistanceEnemiesSpawn),
                        _tileManager.TileManagerData.Row, character.Ability1, Character.Team.Team2, _enemiesDirection);
                }
                else
                {
                    SpawnAICharacter(
                        0 , _tileManager.TileManagerData.Row / MaxDistanceEnemiesSpawn, character.Ability1, Character.Team.Team1, Vector3.zero);
                }   
            }
        }
            
        
        StartCoroutine(MoveCamera(_tileManager.GetTile(_tileManager.TileManagerData.Column / 2, _tileManager.TileManagerData.Row / 2).GetCameraTransform((int)_direction,false))) ;
        
        if (MaxCharacterPlayerCanBePlace == 0)
        {
            NextCharacterTurn();
        }
    }

    private void SpawnAICharacter(int minCoordY, int maxCoordY, DataCharacterSpawner.CharactersAbility1 characterAbility1, Character.Team team, Vector3 direction)
    {
        bool isCharacterIsSpawned = false;
        while (!isCharacterIsSpawned)
        {
            int coordX = Random.Range(0, _tileManager.TileManagerData.Column);
            int coordY = Random.Range(minCoordY, maxCoordY);
        
            if (SpawnCharacter(CharacterAIPrefab, _tileManager.GetTile(coordX, coordY), direction,
                    team, characterAbility1))
            {
                isCharacterIsSpawned = true;
            }
        }
    }
    
    public bool SpawnCharacter(GameObject characterPrefab, Tile tile, Vector3 rotation, Character.Team team, DataCharacterSpawner.CharactersAbility1 ability1)
    {
        if (tile.IsOccupied) {return false;}
        GameObject character = Instantiate(characterPrefab, tile.Position, Quaternion.identity);
        character.transform.Rotate(rotation);
        _characterCount++;
        character.name = "Character" + _characterCount;
        Character characterReverence = character.GetComponent<Character>();
        characterReverence.CurrentTile = tile;
        characterReverence.CurrentTeam = team;
        switch (ability1)
        {
            case DataCharacterSpawner.CharactersAbility1.None:
                break;
            case DataCharacterSpawner.CharactersAbility1.AounterAttack:
                new CounterAbility(characterReverence, this);
                break;
        }
        CharacterList.Add(characterReverence);
        tile.SetCharacter(characterReverence);
        return true;
    }

    public void SelectTile(GameObject gameObjectTile)
    {
        Tile tile = _tileManager.GetTile(gameObjectTile);
        SelectTile(tile);
    }
    
    public void SelectTile(Tile tile)
    {
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
        
        StartCoroutine(MoveCamera(tile.GetCameraTransform((int)_direction, IsCameraNear)));
    }

    //Select with color possible move tile
    public void ShowPossibleMove(Tile tile)
    {
        StateAttackCharacter.ResetAttackData();
        if (ArrowsDirection.activeSelf)
        {
            ArrowsDirection.SetActive(false);
        }
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
        if (ArrowsDirection.activeSelf)
        {
            ArrowsDirection.SetActive(false);
        }
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

        CurrentCharacterTurn = CheckCharacterTime();
        while (CurrentCharacterTurn == null)
        {
            ReduceCharacterTimeRemaining();
            CurrentCharacterTurn = CheckCharacterTime();
        }
        
        CameraIsMoving = true;
        
        if (CurrentCharacter)
        {
            CurrentCharacter.RemoveUIPopUpCharacterInfo(false);
        }
        
        //Reset Move/Attack character
        CurrentCharacterTurn.ResetCharacterTurn();

        CurrentState = StateNavigation;
        SelectTile(CurrentCharacterTurn.CurrentTile);
    }

    //Spawn Arrows for chose a Direction (end of character turn)
    public IEnumerator EndOfCharacterTurn()
    {
        yield return new WaitUntil(() => !Wait);

        if (CurrentCharacter)
        {
            ArrowsDirection.SetActive(true);
            ArrowsDirection.transform.position = TileSelected.Position;
            ShowPossibleTileDirection(TileSelected);
        }
        else
        {
            NextCharacterTurn();
        }
    }

    //Lerp Camera location to a new destination
    private IEnumerator MoveCamera(Transform destination)
    {
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
}
