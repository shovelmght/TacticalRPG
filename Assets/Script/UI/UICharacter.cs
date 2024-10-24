using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UICharacter : MonoBehaviour
{
    [SerializeField] private Image _BackgroundImage;
    [SerializeField]
    private Image _fillBar;
    [SerializeField]
    private float _updateSpeedSeconds = 0.5f;
    [SerializeField]
    private TMP_Text _currentHealthText;
    [SerializeField]
    private TMP_Text _hitSuccessChance;
    [SerializeField]
    private TMP_Text _XpEarnedText;
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
    [SerializeField] private TMP_Text _BuffText;
    [SerializeField] private TMP_Text _DebuffText;
    [SerializeField] private Animator _CanvasAnimator;
    [SerializeField] private Animator _DialogueBubble;
    [SerializeField] private TMP_Text _DialogueText;
    
    private static readonly int ShowBuffDebuff = Animator.StringToHash("ShowBuffDebuff");
    private bool _canClose;
    private int XpEarned;
    private string BlockTexte = "Block";
    private static readonly int TakeDamage = Animator.StringToHash("TakeDamage");
    private static readonly int Close = Animator.StringToHash("Close");
    private static readonly int OpenLeft = Animator.StringToHash("OpenLeft");
    private static readonly int OpenLeftQuick = Animator.StringToHash("OpenLeftQuick");
    private static readonly int GainHealth = Animator.StringToHash("GainHealth");

    
    public void Start()
    {
        _Character.OnHealthPctChange += HandleHealthChanged;//It adds event callback
        _Character.ActionShowUIPopUpCharacterInfo += ShowPopUpCharacterInfo;
        _Character.ActionShowUIHitSuccess += ShowHitSuccessPct;
        _Character.ActionRemoveUIPopUpCharacterInfo += RemoveUIPopUpCharacterInfo;
        _Character.ActionShowHideHealthBar += HideHealthBar;
        _Character.ActionShowBuffDebuffPotionEffect += ShowBuffDebuffPotionEffect;
        _Character.ActionStartDialogue += StartNewDialogueText;
        _Character.ActionShowDialogueBubble += ShowDialogueBubble;
        _Character.SetXpEarned += SetXpEarnedText;
    }

    private void OnDestroy()
    {
        _Character.OnHealthPctChange -= HandleHealthChanged;//It adds event callback
        _Character.ActionShowUIPopUpCharacterInfo -= ShowPopUpCharacterInfo;
        _Character.ActionShowUIHitSuccess -= ShowHitSuccessPct;
        _Character.ActionRemoveUIPopUpCharacterInfo -= RemoveUIPopUpCharacterInfo;
        _Character.ActionShowHideHealthBar -= HideHealthBar;
        _Character.ActionShowBuffDebuffPotionEffect -= ShowBuffDebuffPotionEffect;
        _Character.ActionStartDialogue -= StartNewDialogueText;
        _Character.ActionShowDialogueBubble -= ShowDialogueBubble;
        _Character.SetXpEarned -= SetXpEarnedText;
    }

    //make ui element face to the camera
    public virtual void LateUpdate()
    {
        CanvaHealthBar.transform.LookAt(Camera.main.transform);
        CanvaHealthBar.transform.Rotate(0, 180, 0);
    }
    

    private void HandleHealthChanged(int currentHealth, int damage, int maxHealth, bool isDamage)
    {
        
        
        if (damage == 0)
        {
            _damageTexte.text = BlockTexte;
            _damageAnimator.SetTrigger(TakeDamage);
        }
        else
        {
            float currentHealthPct = (float)currentHealth / maxHealth;
            StartCoroutine(ChangeToPct(currentHealthPct));
            _damageTexte.text = damage.ToString();
            
            if (isDamage)
            {
                _damageAnimator.SetTrigger(TakeDamage);
            }
            else
            {
                _damageAnimator.SetTrigger(GainHealth);
            }
            
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

    private void HideHealthBar(bool newValue)
    {
        _BackgroundImage.enabled = newValue;
        _fillBar.enabled = newValue;
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
    

    //THIS METHODE IS CALLED BY ANIMATOR (UICharacterPopUpClose)
    public void DeactivatePopUpCharacterInfo()
    {
        if (!_canClose) return;
        foreach (var characterPart in CharacterPart)
        {
            characterPart.layer = 1;
        }
        //_popUpCharacterInfoGameObject.SetActive(false);
    }
    

    public void ShowBuffDebuffPotionEffect(string buffText, string debuffText)
    {
        _BuffText.text = buffText;
        _DebuffText.text = debuffText;
        _CanvasAnimator.SetTrigger(ShowBuffDebuff);
    }
    
    
    private void StartNewDialogueText(string text)
    {
        StartCoroutine(DialogueTextManager.Instance.StarDialogue(text, _DialogueText));
    }

    private bool _DoOnceSetXpEarnedText;
    private void SetXpEarnedText(int newValue)
    {
        if(_DoOnceSetXpEarnedText) {return;}

        _DoOnceSetXpEarnedText = true;
        StartCoroutine(StartXpEarnedText(newValue));
    }

    private IEnumerator StartXpEarnedText(int newValue, float duration = 1f)
    {
        AudioManager._Instance.SpawnSound(AudioManager._Instance._GetXP);
        _CanvasAnimator.SetTrigger("ShowXp");
        int startValue = XpEarned;
        float elapsedTime = 0f;

        while (elapsedTime < duration && XpEarned < newValue)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
        
            // Interpolate XP earned value
            XpEarned = Mathf.RoundToInt(Mathf.Lerp(startValue, newValue, t));
            _XpEarnedText.text = XpEarned.ToString();

            yield return null; // Wait for the next frame
        }

        // Ensure we set the final value
        XpEarned = newValue;
        _XpEarnedText.text = newValue.ToString();

        _DoOnceSetXpEarnedText = false;
    }

    
    private void ShowDialogueBubble(bool value)
    {
        HideHealthBar(!value);
        if (value)
        {
            _DialogueBubble.gameObject.SetActive(true);
        }
        else
        {
            _DialogueBubble.SetTrigger(Close);
        }
    }
}
