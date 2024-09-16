using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    [field: SerializeField] public Camera Cam { get; private set; }
    [field: SerializeField] public LayerMask LayerMask { get; private set; }
    [SerializeField] private Button _NextSelectableCharaterSpawn;
    [SerializeField] private Button _PreviousSelectableCharaterSpawn;
    private float distanceRay = 100;
    private GameManager _gameManager;
    private BattlesTacticInputAction playerInput;
    public Material _TempSelectTileMaterial;
    

    private void Awake()
    {
        playerInput = new BattlesTacticInputAction();
    }

    private void Start()
    {
        _gameManager = GameManager.Instance;
        playerInput.BattleTacticIA.NavigationDown.performed += _ => NavigateControllerTileDown();
        playerInput.BattleTacticIA.NavigationUp.performed += _ => NavigateControllerTileUp();
        playerInput.BattleTacticIA.NavigationRight.performed += _ => NavigateControllerTileRight();
        playerInput.BattleTacticIA.NavigationLeft.performed += _ => NavigateControllerTileLeft();
        playerInput.BattleTacticIA.RotateCameraLeft.performed += _ => RotateCameraLeft();
        playerInput.BattleTacticIA.RotateCameraRight.performed += _ => RotateCameraRight();
        playerInput.BattleTacticIA.Select.performed += _ => SelectTile();
    }
    
    public void OnEnable()
    {
        playerInput.Enable();
    }
    public void OnDisable()
    {
        playerInput.Disable();
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(!EventSystem.current.IsPointerOverGameObject())
            {
                if (Physics.Raycast(ray,out hit, distanceRay, LayerMask))
                {
                    _gameManager.SelectTile(hit.collider.gameObject);
                }
            }
        }
    }
    
    private void NavigateControllerTileDown()
    {
        int tileIndex = 0;
        
        if (_gameManager._direction == GameManager.Direction.North)
        {
            tileIndex = 1;
        }
        else if (_gameManager._direction == GameManager.Direction.Est)
        {
            tileIndex = 2;
        }
        else if (_gameManager._direction == GameManager.Direction.West)
        {
            tileIndex = 3;
        }
        
        _gameManager.IsController = true;
        if(_gameManager.TilePreSelected == null || _gameManager.TilePreSelected.SideTiles[tileIndex] == null || _gameManager.MenuIsOpen) {return;}

        
        if (_TempSelectTileMaterial != null)
        {
            _gameManager.TilePreSelected.SetTopMaterial(_TempSelectTileMaterial);
        }
        
        _TempSelectTileMaterial = _gameManager.TilePreSelected.SideTiles[tileIndex].GetTopMaterial();
        _gameManager.SelectTileController(_gameManager.TilePreSelected.SideTiles[tileIndex]);
        _gameManager.TilePreSelected = _gameManager.TilePreSelected.SideTiles[tileIndex];
        
        Debug.Log("_gameManager._direction = " + _gameManager._direction);
    }
    
    private void NavigateControllerTileUp()
    {
        int tileIndex = 1;
        
        if (_gameManager._direction == GameManager.Direction.North)
        {
            tileIndex = 0;
        }
        else if (_gameManager._direction == GameManager.Direction.Est)
        {
            tileIndex = 3;
        }
        else if (_gameManager._direction == GameManager.Direction.West)
        {
            tileIndex = 2;
        }
        
        _gameManager.IsController = true;
        if(_gameManager.TilePreSelected == null || _gameManager.TilePreSelected.SideTiles[tileIndex] == null || _gameManager.MenuIsOpen) {return;}
      
        if (_TempSelectTileMaterial != null)
        {
            _gameManager.TilePreSelected.SetTopMaterial(_TempSelectTileMaterial);
        }
        
        _TempSelectTileMaterial = _gameManager.TilePreSelected.SideTiles[tileIndex].GetTopMaterial();
        Debug.Log("SelectTileController tile NavigateControllerTileUp  SideTiles[tileIndex]= " +_gameManager.TilePreSelected.SideTiles[tileIndex].CoordX + "  " +_gameManager.TilePreSelected.SideTiles[tileIndex].CoordY + " TilePreSelected = " + _gameManager.TilePreSelected.CoordX + "  " + _gameManager.TilePreSelected.CoordY);
        _gameManager.SelectTileController(_gameManager.TilePreSelected.SideTiles[tileIndex]);
        _gameManager.TilePreSelected = _gameManager.TilePreSelected.SideTiles[tileIndex];

        
        Debug.Log("_gameManager._direction = " + _gameManager._direction);
    }

    private void NavigateControllerTileRight()
    {
        int tileIndex = 2;
        
        if (_gameManager._direction == GameManager.Direction.North)
        {
            tileIndex = 3;
        }
        else if (_gameManager._direction == GameManager.Direction.Est)
        {
            tileIndex = 1;
        }
        else if (_gameManager._direction == GameManager.Direction.West)
        {
            tileIndex = 0;
        }
        
        _gameManager.IsController = true;
        if(_gameManager.TilePreSelected == null || _gameManager.TilePreSelected.SideTiles[tileIndex] == null || _gameManager.MenuIsOpen) {return;}
   
        if (_TempSelectTileMaterial != null)
        {
            _gameManager.TilePreSelected.SetTopMaterial(_TempSelectTileMaterial);
        }
        
        _TempSelectTileMaterial = _gameManager.TilePreSelected.SideTiles[tileIndex].GetTopMaterial();
        _gameManager.SelectTileController(_gameManager.TilePreSelected.SideTiles[tileIndex]);
        _gameManager.TilePreSelected = _gameManager.TilePreSelected.SideTiles[tileIndex];
        
        Debug.Log("_gameManager._direction = " + _gameManager._direction);
    }
    
    private void NavigateControllerTileLeft()
    {
        int tileIndex = 3;
        
        if (_gameManager._direction == GameManager.Direction.North)
        {
            tileIndex = 2;
        }
        else if (_gameManager._direction == GameManager.Direction.Est)
        {
            tileIndex = 0;
        }
        else if (_gameManager._direction == GameManager.Direction.West)
        {
            tileIndex = 1;
        }
        
        _gameManager.IsController = true;
        if(_gameManager.TilePreSelected == null || _gameManager.TilePreSelected.SideTiles[tileIndex] == null || _gameManager.MenuIsOpen) {return;}
        
        if (_TempSelectTileMaterial != null)
        {
            _gameManager.TilePreSelected.SetTopMaterial(_TempSelectTileMaterial);
        }
        
        _TempSelectTileMaterial = _gameManager.TilePreSelected.SideTiles[tileIndex].GetTopMaterial();
        _gameManager.SelectTileController(_gameManager.TilePreSelected.SideTiles[tileIndex]);
        _gameManager.TilePreSelected = _gameManager.TilePreSelected.SideTiles[tileIndex];
       
        Debug.Log("_gameManager._direction = " + _gameManager._direction);
    }
    
    private void SelectTile()
    {
        if (!_gameManager.MenuIsOpen)
        {
            _gameManager.SelectTile(_gameManager.TilePreSelected);
        }
    }
    
    private void RotateCameraRight()
    {
        if (_NextSelectableCharaterSpawn.gameObject.activeInHierarchy)
        {
            _NextSelectableCharaterSpawn.onClick.Invoke();
        }
        _gameManager.IsController = true;
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        _gameManager.SetRotationCameraRight();
    }
    
    private void RotateCameraLeft()
    {
        if (_PreviousSelectableCharaterSpawn.gameObject.activeInHierarchy)
        {
            _PreviousSelectableCharaterSpawn.onClick.Invoke();
            return;
        }
        
        _gameManager.IsController = true;
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        _gameManager.SetRotationCameraLeft();
    }
}
