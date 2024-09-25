using UnityEngine;
using UnityEngine.UI;

public class UIBoardGame : MonoBehaviour
{
    public Button MoveButton;
    public Button AttackButton;
    public Button WaitButton;
    public Button NormalAttackButton;
    public Text NormalAttackName;
    public Button SkillAttackButton;
    public Text SkillAttackName;
    public Button ReturnToMenuFromAttackButton;
    public Button CameraRotateRightButton;
    public Button CameraRotateLeftButton;
    public Animator Animator;
    public Animator ElemantalButtonAnimator;

    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _pressesColor;
    private GameManager _boardManager;
    private bool _canMove = true;
    private bool _canAttack = true;
    private bool _canWait = true;
   
    
    private static readonly int Open = Animator.StringToHash("Open");
    private static readonly int Water = Animator.StringToHash("Water");
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
        ReturnToMenuFromAttackButton.onClick.AddListener(ReturnToMenuFromAttack);
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
            WaitButton.image.color = _normalColor;
            WaitButton.interactable = true;
            if (_boardManager.IsController)
            {
                RemoveUICharacter();
            }
        }
    }

    private void DeactivateMoveButton()
    {
        MoveButton.image.color = _pressesColor;
        MoveButton.interactable = false;
    }
    
    private void ShowAttackMenu()
    {
        if (_boardManager.IsAIChatacterTurn) { return; }
        
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        
        if (!_boardManager.CurrentCharacter.HaveAttacked && GameManager.Instance.CurrentCharacter.CurrentTile != null && !_boardManager.Wait)
        {
            ElemantalButtonAnimator.SetBool(Water, _boardManager.CurrentCharacter.CurrentTile.IsWater);

            if (_boardManager.CurrentCharacter.CurrentTile.IsWater)
            {
                NormalAttackName.text = _boardManager.CurrentCharacter._WaterAttack.AttackName;
            }
            else
            {
                NormalAttackName.text = _boardManager.CurrentCharacter._Attack.AttackName;
            }
            WaitButton.image.color = _normalColor;
            WaitButton.interactable = true;
            MoveButton.gameObject.SetActive(false);
            AttackButton.gameObject.SetActive(false);
            WaitButton.gameObject.SetActive(false);
            NormalAttackButton.gameObject.SetActive(true);
            SkillAttackButton.gameObject.SetActive(true);
            ReturnToMenuFromAttackButton.gameObject.SetActive(true);
            
            SkillAttackName.text = _boardManager.CurrentCharacter._SkillAttack.AttackName;
            if (_boardManager.IsController)
            {
                NormalAttackButton.Select();
            }
        }
    }
    

    private void ShowNormalTilesAttack()
    {
        if (_boardManager.IsAIChatacterTurn) { return; }
        
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        
        if (!_boardManager.CurrentCharacter.HaveAttacked && GameManager.Instance.CurrentCharacter.CurrentTile != null && !_boardManager.Wait)
        {
            if (GameManager.Instance.CurrentCharacter.CurrentTile.IsWater)
            {
                _boardManager.StateAttackCharacter._Attack = GameManager.Instance.CurrentCharacter._WaterAttack;
                _boardManager.ShowPossibleAttack(GameManager.Instance.CurrentCharacter.CurrentTile, false, GameManager.Instance.CurrentCharacter._WaterAttack);
            }
            else
            {
                _boardManager.StateAttackCharacter._Attack = GameManager.Instance.CurrentCharacter._Attack;
                _boardManager.ShowPossibleAttack(GameManager.Instance.CurrentCharacter.CurrentTile, false, GameManager.Instance.CurrentCharacter._Attack);
            }
            
            if (_boardManager.IsController)
            {
                RemoveUICharacter();
                NormalAttackButton.interactable = false;
                SkillAttackButton.interactable = false;
                ReturnToMenuFromAttackButton.interactable = false;
            }
        }
    }
    
    private void ShowSkillTilesAttack()
    {
        if (_boardManager.IsAIChatacterTurn) { return; }
        
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        
        if (!_boardManager.CurrentCharacter.HaveAttacked && GameManager.Instance.CurrentCharacter.CurrentTile != null && !_boardManager.Wait)
        {
            _boardManager.StateAttackCharacter._Attack = GameManager.Instance.CurrentCharacter._SkillAttack;
            _boardManager.ShowPossibleAttack(GameManager.Instance.CurrentCharacter.CurrentTile, false, GameManager.Instance.CurrentCharacter._SkillAttack);
            if (_boardManager.IsController)
            {
                RemoveUICharacter();
                NormalAttackButton.interactable = false;
                SkillAttackButton.interactable = false;
                ReturnToMenuFromAttackButton.interactable = false;
            }
        }
    }
    
    public void ReturnToMenuFromAttack()
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

            if (_boardManager.IsController)
            {
                MoveButton.Select();
            }
        }
    }
    
    private void DeactivateAttackButton()
    {
        AttackButton.image.color = _pressesColor;
        AttackButton.interactable = false;
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
        WaitButton.interactable = false;
        _canWait = false;
    }

    private void ShowUICharacter()
    {
        ReactivateUICharacterButton();
        Animator.SetBool(Open, true);
        GameManager.Instance.MenuIsOpen = true;
        MoveButton.interactable = !_boardManager.CurrentCharacter.HaveMoved;
        MoveButton.image.color = _boardManager.CurrentCharacter.HaveMoved ? _pressesColor : _normalColor;
        AttackButton.interactable = !_boardManager.CurrentCharacter.HaveAttacked;
        AttackButton.image.color = _boardManager.CurrentCharacter.HaveAttacked ? _pressesColor : _normalColor;
        WaitButton.image.color = _normalColor;
        WaitButton.interactable = true;
        NormalAttackButton.interactable = true;
        SkillAttackButton.interactable = true;
        ReturnToMenuFromAttackButton.interactable = true;
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
            MoveButton.interactable = !_boardManager.CurrentCharacter.HaveMoved;
            MoveButton.image.color = _boardManager.CurrentCharacter.HaveMoved ? _pressesColor : _normalColor;
            AttackButton.interactable = !_boardManager.CurrentCharacter.HaveAttacked;
            AttackButton.image.color = _boardManager.CurrentCharacter.HaveAttacked ? _pressesColor : _normalColor;
            WaitButton.image.color = _normalColor;
            WaitButton.interactable = true;
            _canWait = true;
            if (_boardManager.IsController)
            {
                if (MoveButton.gameObject.activeInHierarchy && MoveButton.interactable)
                {
                    MoveButton.Select();
                }
                else if (AttackButton.gameObject.activeInHierarchy && AttackButton.interactable)
                {
                    AttackButton.Select();
                }
                else if (NormalAttackButton.gameObject.activeInHierarchy)
                {
                    NormalAttackButton.Select();
                }
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
