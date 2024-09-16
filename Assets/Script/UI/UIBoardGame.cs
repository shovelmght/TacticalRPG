using UnityEngine;
using UnityEngine.UI;

public class UIBoardGame : MonoBehaviour
{
    public Button MoveButton;
    public Button AttackButton;
    public Button WaitButton;
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

    private void Start()
    {
        MoveButton.onClick.AddListener(ShowTilesMove);
        AttackButton.onClick.AddListener(ShowTilesAttack);
        WaitButton.onClick.AddListener(WaitActionCharacter);
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
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        
        if (!_boardManager.CurrentCharacter.HaveMoved && _boardManager.TileSelected != null && !_boardManager.Wait)
        {
            _boardManager.ShowPossibleMove(GameManager.Instance.TileSelected);
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
    

    private void ShowTilesAttack()
    {
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        
        if (!_boardManager.CurrentCharacter.HaveAttacked && _boardManager.TileSelected != null && !_boardManager.Wait)
        {
            _boardManager.ShowPossibleAttack(GameManager.Instance.TileSelected, false);
            if (_boardManager.IsController)
            {
                RemoveUICharacter();
            }
        }
    }
    
    private void DeactivateAttackButton()
    {
        AttackButton.image.color = _pressesColor;
        _canAttack = false;
    }

    private void WaitActionCharacter()
    {
        WaitButton.interactable = false;
        Debug.Log("Bug Controller wait WaitActionCharacter 11 t");
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        
        if (_canWait && !_boardManager.Wait)
        {
            Debug.Log("Bug Controller wait WaitActionCharacter 22 t");
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
    }

    private void RemoveUICharacter()
    {
        Animator.SetBool(Open, false);
        GameManager.Instance.MenuIsOpen = false;
    }
    
    public void ReactivateUICharacterButton()
    {
        if (_boardManager.CurrentCharacter)
        {
            MoveButton.image.color = _boardManager.CurrentCharacter.HaveMoved ? _pressesColor : _normalColor;
            AttackButton.image.color = _boardManager.CurrentCharacter.HaveAttacked ? _pressesColor : _normalColor;
            WaitButton.image.color = _normalColor;
            _canWait = true;
            MoveButton.Select();
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
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        _boardManager.SetRotationCameraRight();
    }
    
    private void RotateCameraLeft()
    {
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        _boardManager.SetRotationCameraLeft();
    }
   
}
