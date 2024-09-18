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
        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        }
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
        playerInput.Enable();
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
        if (_gameManager.IsAIChatacterTurn) { return; }
        
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
                else
                {
                    _SecondSelecableButton.Select();
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
                    else
                    {
                        _SecondSelecableButton.Select();
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
        if (_gameManager.IsAIChatacterTurn) { return; }
        
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
                else
                {
                    _SecondSelecableButton.Select();
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
                    else
                    {
                        _SecondSelecableButton.Select();
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
        if (_gameManager.IsAIChatacterTurn) { return; }
        
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
                else
                {
                    _SecondSelecableButton.Select();
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
                    else
                    {
                        _SecondSelecableButton.Select();
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
        if (_gameManager.IsAIChatacterTurn) { return; }
        
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
                else
                {
                    _SecondSelecableButton.Select();
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
                    else
                    {
                        _SecondSelecableButton.Select();
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
        if (_gameManager.IsAIChatacterTurn) { return; }
        
        if (!_gameManager.MenuIsOpen)
        {
            _gameManager.SelectTile(_gameManager.TilePreSelected);
        }
    }
    
    private void Back()
    {
        if (_gameManager.IsAIChatacterTurn) { return; }
        
        if (!_gameManager.MenuIsOpen)
        {
            _gameManager.SelectCharacter?.Invoke();
            _gameManager.TilePreSelected.SetTopMaterial(_TempSelectTileMaterial);
            _TempSelectTileMaterial = _gameManager.CurrentCharacter.CurrentTile.StartMaterial;
            _gameManager.TilePreSelected = _gameManager.CurrentCharacter.CurrentTile;
            StartCoroutine(_gameManager.MoveCamera(_gameManager.CurrentCharacter.CurrentTile.GetCameraTransform((int)_gameManager._direction, _gameManager.IsCameraNear)));
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
        if (_gameManager.IsAIChatacterTurn) { return; }
        
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
        if (_gameManager.IsAIChatacterTurn) { return; }
        
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
