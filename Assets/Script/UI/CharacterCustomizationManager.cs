using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterCustomizationManager : MonoBehaviour
{
    [SerializeField] private Button _LeftArrowColor;
    [SerializeField] private Button _RightArrowColor;
    [SerializeField] private CharacterMaterial _AllPossibleCharacterMaterials;
    [SerializeField] private MeshRenderer[] _CharacterMaterials;
    [SerializeField] private Button _ChangeNameButton;
    [SerializeField] private TMP_Text _Name;
    [SerializeField] private TMP_InputField _InputField;
    [SerializeField] private GameObject _VirtualKeyboard;
    [SerializeField] private GameObject[] _GameObjectToDeactivate;
    [SerializeField] private Animator _Animator;
    private int _IndexMaterial;
   
    void Start()
    {
        _ChangeNameButton.onClick.AddListener(OnChangeNameButtonPress);
        _LeftArrowColor.onClick.AddListener(NextColor);
        _RightArrowColor.onClick.AddListener(NextPrevious);
    }
    
    void OnEnable()
    {
        if (GameManager.Instance.IsController)
        {
            _ChangeNameButton.Select();
        }
    }

    private void Update()
    {
        if (_InputField.gameObject.activeInHierarchy)
        {
            _Name.text = _InputField.text;
        }
    }

    private void OnChangeNameButtonPress()
    {
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        if (GameManager.Instance.IsController)
        {
            if (_VirtualKeyboard.activeInHierarchy)
            {
                _Animator.enabled = true;
                foreach (var gameObjectToDeactivate in _GameObjectToDeactivate)
                {
                    gameObjectToDeactivate.SetActive(true);
                }
        
                _VirtualKeyboard.SetActive(false);
            }
            else
            {
                _Animator.enabled = false;
                foreach (var gameObjectToDeactivate in _GameObjectToDeactivate)
                {
                    gameObjectToDeactivate.SetActive(false);
                }
                _VirtualKeyboard.SetActive(true);
            }
            
        }
        else
        {
            _InputField.gameObject.SetActive(true);
            _InputField.Select();
        }

    }
    
    private void NextColor()
    {
        _InputField.gameObject.SetActive(false);
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        _IndexMaterial++;
        
        if (_IndexMaterial > _AllPossibleCharacterMaterials.AllPossibleMaterials.Length - 1)
        {
            _IndexMaterial = 0;
        }

        foreach (var characterMaterial in _CharacterMaterials)
        {
            characterMaterial.material = _AllPossibleCharacterMaterials.AllPossibleMaterials[_IndexMaterial];
        }
        Debug.Log(_AllPossibleCharacterMaterials.AllPossibleMaterials[_IndexMaterial].name);

    }
    
    private void NextPrevious()
    {
        _InputField.gameObject.SetActive(false);
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        
        _IndexMaterial--;
        
        if (_IndexMaterial < 0)
        {
            _IndexMaterial = _AllPossibleCharacterMaterials.AllPossibleMaterials.Length - 1;
        }

        foreach (var characterMaterial in _CharacterMaterials)
        {
            characterMaterial.material = _AllPossibleCharacterMaterials.AllPossibleMaterials[_IndexMaterial];
        }

    }

    [SerializeField] private DataCharacterSpawner _DataCharacterSpawner;

    public void OnConfirmButtonClick()
    {
        _DataCharacterSpawner.TeamColor = _AllPossibleCharacterMaterials.AllPossibleMaterials[_IndexMaterial];
        var dataSpawner = _DataCharacterSpawner.DataSpawn[0];
        dataSpawner.Name = _Name.text;
    }
}
