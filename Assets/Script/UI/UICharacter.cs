using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICharacter : MonoBehaviour
{
    [SerializeField]
    private Image _fillBar;
    [SerializeField]
    private float _updateSpeedSeconds = 0.5f;
    [SerializeField]
    private TMP_Text _currentHealthText;
    [SerializeField]
    private TMP_Text _hitSuccessChance;
    [SerializeField]
    private Character _Character;
    [SerializeField]
    private Animator _damageAnimator;
    [SerializeField]
    private Animator _popUpCharacterInfoAnimator;
    [SerializeField]
    private TextMeshProUGUI _damageTexte;
    [SerializeField]
    private GameObject _popUpCharacterInfoGameObject;
    [SerializeField]
    private GameObject _hitSuccess;
    [SerializeField]
    private GameObject CanvaHealthBar;
    [SerializeField]
    private Camera camera;
    [SerializeField]
    private GameObject[] CharacterPart;
    [SerializeField]
    private int _LayerMaskCharacterCameraLeft;
    [SerializeField]
    private int _LayerMaskCharacterCameraRight;
    [SerializeField]
    private int _LayerMaskUI;
    [SerializeField]
    private Transform _rightCameraTransform;
    [SerializeField]
    private Transform _leftCameraTransform;
    [SerializeField]
    private RectTransform _characterUihpRectTransform;
    [SerializeField]
    private Vector3 _rightCharacterUihpRectTransformPosition;
    [SerializeField]
    private Vector3 _leftCharacterUihpRectTransformPosition;

    private bool _canClose;

    private string BlockTexte = "Block";
    private static readonly int TakeDamage = Animator.StringToHash("TakeDamage");
    private static readonly int Close = Animator.StringToHash("Close");
    private static readonly int OpenLeft = Animator.StringToHash("OpenLeft");
    private static readonly int OpenLeftQuick = Animator.StringToHash("OpenLeftQuick");

    public virtual void Awake()
    {
        _Character.OnHealthPctChange += HandleHealthChanged;//It adds event callback
        _Character.DestroyUI += RemoveUI;
        _Character.ShowUIPopUpCharacterInfo += ShowPopUpCharacterInfo;
        _Character.ShowUIHitSuccess += ShowHitSuccessPct;
        _Character.RemoveUIPopUpCharacterInfo += RemoveUIPopUpCharacterInfo;
    }

    //make ui element face to the camera
    public virtual void LateUpdate()
    {
        CanvaHealthBar.transform.LookAt(Camera.main.transform);
        CanvaHealthBar.transform.Rotate(0, 180, 0);
    }

    private void ShowDamage(string damage)
    {
        _damageTexte.text = damage;
        _damageAnimator.SetTrigger(TakeDamage);
    }

    private void HandleHealthChanged(int currentHealth, int damage, int maxHealth)
    {
        if (damage == 0)
        {
            ShowDamage(BlockTexte);
        }
        else
        {
            float currentHealthPct = (float)currentHealth / maxHealth;
            StartCoroutine(ChangeToPct(currentHealthPct));
            ShowDamage(damage.ToString());
            _currentHealthText.text = currentHealth.ToString() + "/" + maxHealth.ToString();
        }
    }

    //Add a little bit of smoothing
    private IEnumerator ChangeToPct(float pct)
    {
        float preChangePct = _fillBar.fillAmount; // cache the percent of that is was at whent we first call
        float elapsed = 0f;
        while (elapsed < _updateSpeedSeconds)
        {
            elapsed += Time.deltaTime;
            _fillBar.fillAmount = Mathf.Lerp(preChangePct, pct, elapsed / _updateSpeedSeconds); //interpolate betwen preChangePct and pct
            yield return null;
        }

        _fillBar.fillAmount = pct; //make sure to go to the correct amount
    }
    
    private void ShowPopUpCharacterInfo(bool isRight, bool isQuick)
    {
        if (_popUpCharacterInfoAnimator.gameObject.activeInHierarchy)
        {
            _popUpCharacterInfoAnimator.SetBool(OpenLeft, false);
            _popUpCharacterInfoAnimator.SetBool(Close, false);
        }

        _canClose = false;

        _popUpCharacterInfoGameObject.SetActive(true);
        if (isRight)
        {
            SetSideCameraSetting(_rightCameraTransform, _LayerMaskCharacterCameraRight, _rightCharacterUihpRectTransformPosition);
        }
        else
        {
            SetSideCameraSetting(_leftCameraTransform, _LayerMaskCharacterCameraLeft, _leftCharacterUihpRectTransformPosition);
      
            _hitSuccess.SetActive(false);
            if (isQuick)
            {
                _popUpCharacterInfoAnimator.SetBool(OpenLeftQuick, true);
            }
            else
            {
                _popUpCharacterInfoAnimator.SetBool(OpenLeft, true);
            }
        }
    }

    private void SetSideCameraSetting(Transform cameraTransform, LayerMask layerMastCharacterCamera, Vector3 characterUihpRectTransformPosition)
    {
        _popUpCharacterInfoGameObject.transform.localPosition = cameraTransform.localPosition;
        _popUpCharacterInfoGameObject.transform.localRotation = cameraTransform.localRotation;
        camera.cullingMask = (1 << layerMastCharacterCamera) | ( 1 << _LayerMaskUI) ;
        foreach (var characterPart in CharacterPart)
        {
            characterPart.layer = layerMastCharacterCamera;
        }
        _characterUihpRectTransform.localPosition = characterUihpRectTransformPosition;
    }
    
    private void ShowHitSuccessPct(int hitSuccessChance)
    {
        _hitSuccess.SetActive(true);
        _hitSuccessChance.text = "Hit Success " + hitSuccessChance + "%";
    }
    
    private void RemoveUIPopUpCharacterInfo(bool isQuickSwitchBetweenCharacter)
    {
        _canClose = true;
        if (isQuickSwitchBetweenCharacter)
        {
            DeactivatePopUpCharacterInfo();
        }
        _popUpCharacterInfoAnimator.SetBool(Close, true);
        Invoke(nameof(DeactivatePopUpCharacterInfo), 0.5f);
    }

    private void RemoveUI()
    {
        Destroy(this.gameObject);
    }

    //THIS METHODE IS CALLED BY ANIMATOR (UICharacterPopUpClose)
    public void DeactivatePopUpCharacterInfo()
    {
        if (!_canClose) return;
        foreach (var characterPart in CharacterPart)
        {
            characterPart.layer = 1;
        }
        _popUpCharacterInfoGameObject.SetActive(false);
    }
}
