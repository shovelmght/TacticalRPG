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
    [SerializeField] private CheckIfButtonIsSelect[] _ButtonsCanBeSelect;
    [SerializeField] private Button _FirstSelecableButton;
    [SerializeField] private Button _SecondSelecableButton;
    [SerializeField] private Button _ThirdSelecableButton;
    private int _TreshHoldItteration = 2;
    private bool Canselect;
    private bool _OneOfButtonIsSelected;
    private int _Itteration = 0;
    private float distanceRay = 100;
    private GameManager _gameManager;
    private BattlesTacticInputAction playerInput;
    public Material _TempSelectTileMaterial;
    
    public static InputManager Instance { get; private set; }
    private void Awake()
    {
        Debug.Log("Awake called, initializing playerInput 1 ");
        if (Instance != null && Instance != this) 
        { 
            Debug.Log("Awake called, initializing playerInput 2");
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        }
        
        Debug.Log("Awake called, initializing playerInput 3");
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
        playerInput.BattleTacticIA.Back.performed += _ => Back();
    }
    
    public void OnEnable()
    {
        Debug.Log("OnEnable called, trying to enable playerInput");
        if (playerInput != null)
        {
            playerInput.Enable();
        }
        else
        {
            Debug.LogError("playerInput is null in OnEnable");
        }
    }
    public void OnDisable()
    {
        playerInput.Disable();
    }

    void Update()
    {
        if (_gameManager.IsAIChatacterTurn) { return; }
        
        if(Input.GetMouseButtonDown(0))
        {
            StartCoroutine(_gameManager.PressRepeatableAttackInput());
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
        if (_gameManager.IsAIChatacterTurn || _gameManager._IsMapScene && !_gameManager.PossibleMapMoveTileIsFinished) { return; }
        
        if (_gameManager.MenuIsOpen || _gameManager._IsStartScene)
        {
            bool oneIsSelected = false;
            foreach (var button in _ButtonsCanBeSelect)
            {
                if (button._IsSelected)
                {
                    oneIsSelected = true;
                    _OneOfButtonIsSelected = true;
                    break;
                }
            }

            if (!oneIsSelected)
            {
                _OneOfButtonIsSelected = false;
            }

            if (Canselect && !_OneOfButtonIsSelected)
            {
                if (_FirstSelecableButton.gameObject.activeInHierarchy)
                {
                    _FirstSelecableButton.Select();
                }
                else if(_SecondSelecableButton.gameObject.activeInHierarchy)
                {
                    _SecondSelecableButton.Select();
                }
                else
                {
                    _ThirdSelecableButton.Select();
                }
                
                Debug.Log(" _ConfirmButton.Select();");
            }
            else if (!Canselect && !_OneOfButtonIsSelected)
            {
                Debug.Log("_Itteration = " + _Itteration);
                _Itteration++;
                if (_Itteration > _TreshHoldItteration)
                {
                    _Itteration = 0;
                    if (_FirstSelecableButton.gameObject.activeInHierarchy)
                    {
                        _FirstSelecableButton.Select();
                    }
                    else if(_SecondSelecableButton.gameObject.activeInHierarchy)
                    {
                        _SecondSelecableButton.Select();
                    }
                    else
                    {
                        _ThirdSelecableButton.Select();
                    }
                }
            }
        }
        
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
        Tile tile = null;
        
        if (_gameManager.TilePreSelected == null || _gameManager.TilePreSelected.SideTiles[tileIndex] == null || _gameManager.MenuIsOpen)
        {
            if (_gameManager._IsMapScene && _gameManager.TilePreSelected.SideTiles[tileIndex] == null)
            {
                tile = GetEndMapTile(_gameManager.TilePreSelected);

                if (tile == null)
                {
                    return;
                }

                if (_TempSelectTileMaterial != null)
                {
                    if (_gameManager._IsMapScene && _gameManager.IsController)
                    {
                
                    }
                    else
                    {
                        _gameManager.TilePreSelected.SetTopMaterial(_TempSelectTileMaterial);
                    }
                }
        
                _TempSelectTileMaterial = tile.GetTopMaterial();
                _gameManager.SelectTileController(tile);
                _gameManager.TilePreSelected = tile;
                _gameManager.PossibleMapMoveTileIsFinished = false;
                _gameManager.SelectTile(_gameManager.TilePreSelected);
            }
       
            return;
        }

        if (_gameManager._IsMapScene && _gameManager.TilePreSelected.SideTiles[tileIndex].IsOccupied)
        {
            return;
        }
        
        if (_TempSelectTileMaterial != null)
        {
            if (_gameManager._IsMapScene && _gameManager.IsController)
            {
                
            }
            else
            {
                _gameManager.TilePreSelected.SetTopMaterial(_TempSelectTileMaterial);
            }
        }
        
        _TempSelectTileMaterial = _gameManager.TilePreSelected.SideTiles[tileIndex].GetTopMaterial();
        _gameManager.SelectTileController(_gameManager.TilePreSelected.SideTiles[tileIndex]);
        _gameManager.TilePreSelected = _gameManager.TilePreSelected.SideTiles[tileIndex];

        if (_gameManager._IsMapScene)
        {
            _gameManager.PossibleMapMoveTileIsFinished = false;
            _gameManager.SelectTile(_gameManager.TilePreSelected);
        }
        
        Debug.Log("_gameManager._direction = " + _gameManager._direction);
    }
    
    private void NavigateControllerTileUp()
    {
        if (_gameManager.IsAIChatacterTurn || _gameManager._IsMapScene && !_gameManager.PossibleMapMoveTileIsFinished) { return; }
        
        if (_gameManager.MenuIsOpen || _gameManager._IsStartScene)
        {
            bool oneIsSelected = false;
            foreach (var button in _ButtonsCanBeSelect)
            {
                if (button._IsSelected)
                {
                    oneIsSelected = true;
                    _OneOfButtonIsSelected = true;
                    break;
                }
            }

            if (!oneIsSelected)
            {
                _OneOfButtonIsSelected = false;
            }

            if (Canselect && !_OneOfButtonIsSelected)
            {
                if (_FirstSelecableButton.gameObject.activeInHierarchy)
                {
                    _FirstSelecableButton.Select();
                }
                else if(_SecondSelecableButton.gameObject.activeInHierarchy)
                {
                    _SecondSelecableButton.Select();
                }
                else
                {
                    _ThirdSelecableButton.Select();
                }
                
                Debug.Log(" _ConfirmButton.Select();");
            }
            else if (!Canselect && !_OneOfButtonIsSelected)
            {
                Debug.Log("_Itteration = " + _Itteration);
                _Itteration++;
                if (_Itteration > _TreshHoldItteration)
                {
                    _Itteration = 0;
                    if (_FirstSelecableButton.gameObject.activeInHierarchy)
                    {
                        _FirstSelecableButton.Select();
                    }
                    else if(_SecondSelecableButton.gameObject.activeInHierarchy)
                    {
                        _SecondSelecableButton.Select();
                    }
                    else
                    {
                        _ThirdSelecableButton.Select();
                    }
                }
            }
        }
        
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
        Tile tile = null;
        
        if (_gameManager.TilePreSelected == null || _gameManager.TilePreSelected.SideTiles[tileIndex] == null || _gameManager.MenuIsOpen)
        {
            if (_gameManager._IsMapScene && _gameManager.TilePreSelected.SideTiles[tileIndex] == null)
            {
                tile = GetEndMapTile(_gameManager.TilePreSelected);

                if (tile == null)
                {
                    return;
                }

                if (_TempSelectTileMaterial != null)
                {
                    if (_gameManager._IsMapScene && _gameManager.IsController)
                    {
                
                    }
                    else
                    {
                        _gameManager.TilePreSelected.SetTopMaterial(_TempSelectTileMaterial);
                    }
                }
        
                _TempSelectTileMaterial = tile.GetTopMaterial();
                _gameManager.SelectTileController(tile);
                _gameManager.TilePreSelected = tile;
                _gameManager.PossibleMapMoveTileIsFinished = false;
                _gameManager.SelectTile(_gameManager.TilePreSelected);
            }
       
            return;
        }
      
        if (_gameManager._IsMapScene && _gameManager.TilePreSelected.SideTiles[tileIndex].IsOccupied)
        {
            return;
        }
        
        if (_TempSelectTileMaterial != null)
        {
            if (_gameManager._IsMapScene && _gameManager.IsController)
            {
                
            }
            else
            {
                _gameManager.TilePreSelected.SetTopMaterial(_TempSelectTileMaterial);
            }
        }
        
        _TempSelectTileMaterial = _gameManager.TilePreSelected.SideTiles[tileIndex].GetTopMaterial();
        Debug.Log("SelectTileController tile NavigateControllerTileUp  SideTiles[tileIndex]= " +_gameManager.TilePreSelected.SideTiles[tileIndex].CoordX + "  " +_gameManager.TilePreSelected.SideTiles[tileIndex].CoordY + " TilePreSelected = " + _gameManager.TilePreSelected.CoordX + "  " + _gameManager.TilePreSelected.CoordY);
        _gameManager.SelectTileController(_gameManager.TilePreSelected.SideTiles[tileIndex]);
        _gameManager.TilePreSelected = _gameManager.TilePreSelected.SideTiles[tileIndex];

        if (_gameManager._IsMapScene)
        {
            _gameManager.PossibleMapMoveTileIsFinished = false;
            _gameManager.SelectTile(_gameManager.TilePreSelected);
        }
        
        Debug.Log("_gameManager._direction = " + _gameManager._direction);
    }

    private void NavigateControllerTileRight()
    {
        if (_gameManager.IsAIChatacterTurn || _gameManager._IsMapScene && !_gameManager.PossibleMapMoveTileIsFinished) { return; }
        
        if (_gameManager.MenuIsOpen)
        {
            bool oneIsSelected = false;
            foreach (var button in _ButtonsCanBeSelect)
            {
                if (button._IsSelected)
                {
                    oneIsSelected = true;
                    _OneOfButtonIsSelected = true;
                    break;
                }
            }

            if (!oneIsSelected)
            {
                _OneOfButtonIsSelected = false;
            }

            if (Canselect && !_OneOfButtonIsSelected)
            {
                if (_FirstSelecableButton.gameObject.activeInHierarchy)
                {
                    _FirstSelecableButton.Select();
                }
                else if(_SecondSelecableButton.gameObject.activeInHierarchy)
                {
                    _SecondSelecableButton.Select();
                }
                else
                {
                    _ThirdSelecableButton.Select();
                }
                
                Debug.Log(" _ConfirmButton.Select();");
            }
            else if (!Canselect && !_OneOfButtonIsSelected)
            {
                Debug.Log("_Itteration = " + _Itteration);
                _Itteration++;
                if (_Itteration > _TreshHoldItteration)
                {
                    _Itteration = 0;
                    if (_FirstSelecableButton.gameObject.activeInHierarchy)
                    {
                        _FirstSelecableButton.Select();
                    }
                    else if(_SecondSelecableButton.gameObject.activeInHierarchy)
                    {
                        _SecondSelecableButton.Select();
                    }
                    else
                    {
                        _ThirdSelecableButton.Select();
                    }
                }
            }
        }
        
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
        Tile tile = null;
        
        if (_gameManager.TilePreSelected == null || _gameManager.TilePreSelected.SideTiles[tileIndex] == null || _gameManager.MenuIsOpen)
        {
            if (_gameManager._IsMapScene && _gameManager.TilePreSelected.SideTiles[tileIndex] == null)
            {
                tile = GetEndMapTile(_gameManager.TilePreSelected);

                if (tile == null)
                {
                    return;
                }

                if (_TempSelectTileMaterial != null)
                {
                    if (_gameManager._IsMapScene && _gameManager.IsController)
                    {
                
                    }
                    else
                    {
                        _gameManager.TilePreSelected.SetTopMaterial(_TempSelectTileMaterial);
                    }
                }
        
                _TempSelectTileMaterial = tile.GetTopMaterial();
                _gameManager.SelectTileController(tile);
                _gameManager.TilePreSelected = tile;
                _gameManager.PossibleMapMoveTileIsFinished = false;
                _gameManager.SelectTile(_gameManager.TilePreSelected);
            }
       
            return;
            
        }
        
        if (_gameManager._IsMapScene && _gameManager.TilePreSelected.SideTiles[tileIndex].IsOccupied)
        {
            return;
        }
   
        if (_TempSelectTileMaterial != null)
        {
            if (_gameManager._IsMapScene && _gameManager.IsController)
            {
                
            }
            else
            {
                _gameManager.TilePreSelected.SetTopMaterial(_TempSelectTileMaterial);
            }
        }
        
        _TempSelectTileMaterial = _gameManager.TilePreSelected.SideTiles[tileIndex].GetTopMaterial();
        _gameManager.SelectTileController(_gameManager.TilePreSelected.SideTiles[tileIndex]);
        _gameManager.TilePreSelected = _gameManager.TilePreSelected.SideTiles[tileIndex];
        
        if (_gameManager._IsMapScene)
        {
            _gameManager.PossibleMapMoveTileIsFinished = false;
            _gameManager.SelectTile(_gameManager.TilePreSelected);
        }
        
        Debug.Log("_gameManager._direction = " + _gameManager._direction);
    }
    
    private void NavigateControllerTileLeft()
    {
        if (_gameManager.IsAIChatacterTurn || _gameManager._IsMapScene && !_gameManager.PossibleMapMoveTileIsFinished) { return; }
        
        if (_gameManager.MenuIsOpen)
        {
            bool oneIsSelected = false;
            foreach (var button in _ButtonsCanBeSelect)
            {
                if (button._IsSelected)
                {
                    oneIsSelected = true;
                    _OneOfButtonIsSelected = true;
                    break;
                }
            }

            if (!oneIsSelected)
            {
                _OneOfButtonIsSelected = false;
            }

            if (Canselect && !_OneOfButtonIsSelected)
            {
                if (_FirstSelecableButton.gameObject.activeInHierarchy)
                {
                    _FirstSelecableButton.Select();
                }
                else if(_SecondSelecableButton.gameObject.activeInHierarchy)
                {
                    _SecondSelecableButton.Select();
                }
                else
                {
                    _ThirdSelecableButton.Select();
                }
                
                Debug.Log(" _ConfirmButton.Select();");
            }
            else if (!Canselect && !_OneOfButtonIsSelected)
            {
                Debug.Log("_Itteration = " + _Itteration);
                _Itteration++;
                if (_Itteration > _TreshHoldItteration)
                {
                    _Itteration = 0;
                    if (_FirstSelecableButton.gameObject.activeInHierarchy)
                    {
                        _FirstSelecableButton.Select();
                    }
                    else if(_SecondSelecableButton.gameObject.activeInHierarchy)
                    {
                        _SecondSelecableButton.Select();
                    }
                    else
                    {
                        _ThirdSelecableButton.Select();
                    }
                }
            }
        }
        
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

        Tile tile = null;
        if (_gameManager.TilePreSelected == null || _gameManager.TilePreSelected.SideTiles[tileIndex] == null || _gameManager.MenuIsOpen)
        {
            if (_gameManager._IsMapScene && _gameManager.TilePreSelected.SideTiles[tileIndex] == null)
            {
                tile = GetEndMapTile(_gameManager.TilePreSelected);

                if (tile == null)
                {
                    return;
                }

                if (_TempSelectTileMaterial != null)
                {
                    if (_gameManager._IsMapScene && _gameManager.IsController)
                    {
                
                    }
                    else
                    {
                        _gameManager.TilePreSelected.SetTopMaterial(_TempSelectTileMaterial);
                    }
                }
        
                _TempSelectTileMaterial = tile.GetTopMaterial();
                _gameManager.SelectTileController(tile);
                _gameManager.TilePreSelected = tile;
                _gameManager.PossibleMapMoveTileIsFinished = false;
                _gameManager.SelectTile(_gameManager.TilePreSelected);
            }
       
            return;
        }
        
        if (_gameManager._IsMapScene && _gameManager.TilePreSelected.SideTiles[tileIndex].IsOccupied)
        {
            return;
        }
        
        if (_TempSelectTileMaterial != null)
        {
            if (_gameManager._IsMapScene && _gameManager.IsController)
            {
                
            }
            else
            {
                _gameManager.TilePreSelected.SetTopMaterial(_TempSelectTileMaterial);
            }
            
        }
        
        _TempSelectTileMaterial = _gameManager.TilePreSelected.SideTiles[tileIndex].GetTopMaterial();
        _gameManager.SelectTileController(_gameManager.TilePreSelected.SideTiles[tileIndex]);
        _gameManager.TilePreSelected = _gameManager.TilePreSelected.SideTiles[tileIndex];
        
        if (_gameManager._IsMapScene)
        {
          
            _gameManager.PossibleMapMoveTileIsFinished = false;
            _gameManager.SelectTile(_gameManager.TilePreSelected);
        }
       
        Debug.Log("_gameManager._direction = " + _gameManager._direction);
    }

    private Tile GetEndMapTile(Tile currentTile)
    {
         if (currentTile.CoordY == 0)
         {
             if (currentTile.MapTilesManager._WestMapTilesManager != null)
             {
                 Tile tile = currentTile.MapTilesManager._WestMapTilesManager.GetTile(currentTile.CoordX, currentTile.MapTilesManager._WestMapTilesManager.TileManagerData.Column - 1);
             
                 if (tile != null && !tile.IsOccupied)
                 {
                     return tile;
                 }
             }
         }
                        
         if (currentTile.CoordX == 0)
         {
             if (currentTile.MapTilesManager._NorthMapTilesManager != null)
             {
                 Tile tile = currentTile.MapTilesManager._NorthMapTilesManager.GetTile(currentTile.MapTilesManager._NorthMapTilesManager.TileManagerData.Column - 1, currentTile.CoordY);
                 
                 if (tile != null && !tile.IsOccupied)
                 {
                     return tile;
                 }
             }
         }
                        
         if (currentTile.CoordX == currentTile.MapTilesManager.TileManagerData.Column -1)
         {
             if (currentTile.MapTilesManager._SouthMapTilesManager != null)
             {
                 Tile tile = currentTile.MapTilesManager._SouthMapTilesManager.GetTile(0, currentTile.CoordY);
                 
                 if (tile != null && !tile.IsOccupied)
                 {
                     return tile;
                 }
             }
         }
                        
         if (currentTile.CoordY == currentTile.MapTilesManager.TileManagerData.Row -1)
         {
             if (currentTile.MapTilesManager._EstMapTilesManager != null)
             {
                 Tile tile = currentTile.MapTilesManager._EstMapTilesManager.GetTile(currentTile.CoordX, 0);
                 
                 if (tile != null && !tile.IsOccupied)
                 {
                     return tile;
                 }
             }
         }

         return null;
    }
    

    
    private void SelectTile()
    {
        if (_gameManager._IsMapScene) { return; }
        
        StartCoroutine(_gameManager.PressRepeatableAttackInput());
        if (_gameManager.IsAIChatacterTurn) { return; }
        
        if (!_gameManager.MenuIsOpen)
        {
            _gameManager.SelectTile(_gameManager.TilePreSelected);
        }
    }

    [SerializeField] private Button[] _BackButtons;
    
    private void Back()
    {
        if (_gameManager._IsMapScene) { return; }
        if (_gameManager.IsAIChatacterTurn) { return; }

        if (_gameManager._IsStartScene)
        {
            foreach (var backButtons in _BackButtons)
            {
                if (backButtons.gameObject.activeInHierarchy)
                {
                    backButtons.onClick.Invoke();
                    break;
                }
            }
            
            return;
        }
        
        if (!_gameManager.MenuIsOpen)
        {
          
            _gameManager.SelectCharacter?.Invoke();
            _gameManager.TilePreSelected.SetTopMaterial(_TempSelectTileMaterial);
  
            _TempSelectTileMaterial = _gameManager.CurrentCharacter.CurrentTile.StartMaterial;
            _gameManager.TilePreSelected = _gameManager.CurrentCharacter.CurrentTile;
            StartCoroutine(_gameManager.MoveCamera(_gameManager.CurrentCharacter.CurrentTile.GetCameraTransform((int)_gameManager._direction, _gameManager.IsCameraNear)));
            _gameManager._tileManager.DeselectTiles();
        }
        else
        {
            if (UIBoardGame.Instance.ReturnToMenuFromAttackButton.gameObject.activeInHierarchy)
            {
                UIBoardGame.Instance.ReturnToMenuFromAttackButton.onClick.Invoke();
            }
        }
    }
    
    private void RotateCameraRight()
    {
        _gameManager.IsController = true;
        if (_gameManager.IsAIChatacterTurn) { return; }
        
        if (_NextSelectableCharaterSpawn.gameObject.activeInHierarchy)
        {
            _NextSelectableCharaterSpawn.onClick.Invoke();
        }
        else
        {
            _gameManager.SetRotationCameraRight();
        }
    }
    
    private void RotateCameraLeft()
    {
        _gameManager.IsController = true;
        if (_gameManager.IsAIChatacterTurn) { return; }

        if (_PreviousSelectableCharaterSpawn.gameObject.activeInHierarchy)
        {
            _PreviousSelectableCharaterSpawn.onClick.Invoke();
        }
        else
        {
            _gameManager.SetRotationCameraLeft();
        }
    }
}
