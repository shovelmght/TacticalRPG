using UnityEngine;
using UnityEngine.UI;

public class UIBoardGame : MonoBehaviour
{
    public Button MoveButton;
    public Button AttackButton;
    public Button WaitButton;
    public Button NormalAttackButton;
    public Button SkillAttackButton;
    public Button ReturnToMenuFromAttackButton;
    public Button CameraRotateRightButton;
    public Button CameraRotateLeftButton;
    public Animator Animator;

    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _pressesColor;
    private GameManager _boardManager;
    private bool _canMove = true;
    private bool _canAttack = true;
    private bool _canWait = true;
   
    
    private static readonly int Open = Animator.StringToHash("Open");
    public static UIBoardGame Instance { get; private set; }
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

    private void Start()
    {
        MoveButton.onClick.AddListener(ShowTilesMove);
        AttackButton.onClick.AddListener(ShowAttackMenu);
        WaitButton.onClick.AddListener(WaitActionCharacter);
        NormalAttackButton.onClick.AddListener(ShowNormalTilesAttack);
        SkillAttackButton.onClick.AddListener(ShowSkillTilesAttack);
        ReturnToMenuFromAttackButton.onClick.AddListener(WaitActionCharacter);
        CameraRotateRightButton.onClick.AddListener(RotateCameraRight);
        CameraRotateLeftButton.onClick.AddListener(RotateCameraLeft);
        _boardManager = GameManager.Instance;
        _boardManager.SelectCharacter += ShowUICharacter;
        _boardManager.RemoveUICharacter += RemoveUICharacter;
        _boardManager.DesableMoveCharacterUIButtons += DeactivateMoveButton;
        _boardManager.DesableAttackCharacterUIButtons += DeactivateAttackButton;
        _boardManager.ActivateUIButtonCharacter += ReactivateUICharacterButton;
        _boardManager.DeactivateUIButtonCharacter += DeactivateUICharacterButton;
        _boardManager.SetInteractableWaitButton += SetInteractableWaitButton;
    }

    private void ShowTilesMove()
    {
        if (_boardManager.IsAIChatacterTurn) { return; }
        
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        
        if (!_boardManager.CurrentCharacter.HaveMoved && GameManager.Instance.CurrentCharacter.CurrentTile != null && !_boardManager.Wait)
        {
            _boardManager.ShowPossibleMove(GameManager.Instance.CurrentCharacter.CurrentTile);
            if (_boardManager.IsController)
            {
                RemoveUICharacter();
            }
        }
    }

    private void DeactivateMoveButton()
    {
        MoveButton.image.color = _pressesColor;
    }
    
    private void ShowAttackMenu()
    {
        if (_boardManager.IsAIChatacterTurn) { return; }
        
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        
        if (!_boardManager.CurrentCharacter.HaveAttacked && GameManager.Instance.CurrentCharacter.CurrentTile != null && !_boardManager.Wait)
        {
            MoveButton.gameObject.SetActive(false);
            AttackButton.gameObject.SetActive(false);
            WaitButton.gameObject.SetActive(false);
            NormalAttackButton.gameObject.SetActive(true);
            SkillAttackButton.gameObject.SetActive(true);
            ReturnToMenuFromAttackButton.gameObject.SetActive(true);
        }
    }
    

    private void ShowNormalTilesAttack()
    {
        if (_boardManager.IsAIChatacterTurn) { return; }
        
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        
        if (!_boardManager.CurrentCharacter.HaveAttacked && GameManager.Instance.CurrentCharacter.CurrentTile != null && !_boardManager.Wait)
        {
            _boardManager.ShowPossibleAttack(GameManager.Instance.CurrentCharacter.CurrentTile, false, false);
            if (_boardManager.IsController)
            {
                RemoveUICharacter();
            }
        }
    }
    
    private void ShowSkillTilesAttack()
    {
        if (_boardManager.IsAIChatacterTurn) { return; }
        
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        
        if (!_boardManager.CurrentCharacter.HaveAttacked && GameManager.Instance.CurrentCharacter.CurrentTile != null && !_boardManager.Wait)
        {
            _boardManager.ShowPossibleAttack(GameManager.Instance.CurrentCharacter.CurrentTile, false, true);
            if (_boardManager.IsController)
            {
                RemoveUICharacter();
            }
        }
    }
    
    private void ReturnToMenuFromAttack()
    {
        if (_boardManager.IsAIChatacterTurn) { return; }
        
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        
        if (!_boardManager.CurrentCharacter.HaveAttacked && GameManager.Instance.CurrentCharacter.CurrentTile != null && !_boardManager.Wait)
        {
            MoveButton.gameObject.SetActive(true);
            AttackButton.gameObject.SetActive(true);
            WaitButton.gameObject.SetActive(true);
            NormalAttackButton.gameObject.SetActive(false);
            SkillAttackButton.gameObject.SetActive(false);
            ReturnToMenuFromAttackButton.gameObject.SetActive(false);
        }
    }
    
    private void DeactivateAttackButton()
    {
        AttackButton.image.color = _pressesColor;
        _canAttack = false;
    }

    private void WaitActionCharacter()
    {
        if (_boardManager.IsAIChatacterTurn) { return; }
        
        WaitButton.interactable = false;
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        
        if (_canWait && !_boardManager.Wait)
        {
            StartCoroutine(_boardManager.EndOfCharacterTurn(0));
            if (_boardManager.IsController)
            {
                RemoveUICharacter();
            }
        }
    }
    
    private void DeactivateWaitButton()
    {
        WaitButton.image.color = _pressesColor;
        _canWait = false;
    }

    private void ShowUICharacter()
    {
        ReactivateUICharacterButton();
        Animator.SetBool(Open, true);
        GameManager.Instance.MenuIsOpen = true;
        MoveButton.interactable = true;
        AttackButton.interactable = true;
        WaitButton.interactable = true;
    }

    private void RemoveUICharacter()
    {
        Animator.SetBool(Open, false);
        GameManager.Instance.MenuIsOpen = false;
        MoveButton.interactable = false;
        AttackButton.interactable = false;
        WaitButton.interactable = false;
    }
    
    public void ReactivateUICharacterButton()
    {
        if (_boardManager.CurrentCharacter)
        {
            MoveButton.image.color = _boardManager.CurrentCharacter.HaveMoved ? _pressesColor : _normalColor;
            AttackButton.image.color = _boardManager.CurrentCharacter.HaveAttacked ? _pressesColor : _normalColor;
            WaitButton.image.color = _normalColor;
            _canWait = true;
            if (_boardManager.IsController)
            {
                MoveButton.Select();
            }
            
        }
    }

    public void DeactivateUICharacterButton()
    {
        DeactivateAttackButton();
        DeactivateMoveButton();
        DeactivateWaitButton();
    }
    
    
    public void SetInteractableWaitButton()
    {
        WaitButton.interactable = true;
    }

    private void RotateCameraRight()
    {
        if (_boardManager.IsAIChatacterTurn) { return; }
        
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        _boardManager.SetRotationCameraRight();
    }
    
    private void RotateCameraLeft()
    {
        if (_boardManager.IsAIChatacterTurn) { return; }
        
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        _boardManager.SetRotationCameraLeft();
    }
    
    public void HideMenuUI()
    {
        transform.rotation = Quaternion.Euler(0,90,0);
    }
    
    public void UnHideMenuUI()
    {
        transform.rotation = Quaternion.identity;
    }
   
}
