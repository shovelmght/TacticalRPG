using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterSelectable : MonoBehaviour
{
    [SerializeField] private DataCharacterSpawner _PlayerDataCharacterSpawner;
    [SerializeField] private TMP_Text _CharacterName;
    [SerializeField] private TMP_Text _CharacterClass;
    [SerializeField] private Transform _CharacterContainer;
    [SerializeField] private GameObject _EmptyGameObject;
    private List<int> CharacterIndexAlreadySpawn = new List<int>();
    private int _CurrentCharacterIndex;
    private int _CurrentAmoutPlayerDataCharacterSpawner;
    void Start()
    {
        _CurrentAmoutPlayerDataCharacterSpawner = _PlayerDataCharacterSpawner.DataSpawn.Count;
        SetSelectableCharacter();
    }

    public void NextCharacter()
    {
        if (!HasRemainingCharacters())
        {
            return;
        }
        
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        _CurrentCharacterIndex++;
        
        if (_CurrentCharacterIndex > _PlayerDataCharacterSpawner.DataSpawn.Count - 1)
        {
            _CurrentCharacterIndex = 0;
        }

        foreach (var characterIndexAlreadySpawn in CharacterIndexAlreadySpawn)
        {
            if (characterIndexAlreadySpawn == _CurrentCharacterIndex)
            {
                NextCharacter();
                break;
            }
        }
        
        SetSelectableCharacter();
    }
    
    public void PreviousCharacter()
    {
        if (!HasRemainingCharacters())
        {
            return;
        }
        
        AudioManager._Instance.SpawnSound(AudioManager._Instance._ClickSfx);
        _CurrentCharacterIndex--;
        
        if (_CurrentCharacterIndex < 0)
        {
            _CurrentCharacterIndex = _PlayerDataCharacterSpawner.DataSpawn.Count - 1;
        }
        
        foreach (var characterIndexAlreadySpawn in CharacterIndexAlreadySpawn)
        {
            if (characterIndexAlreadySpawn == _CurrentCharacterIndex)
            {
                PreviousCharacter();
                break;
            }
        }

        SetSelectableCharacter();
    }

    private void SetSelectableCharacter()
    {
        _CharacterName.text = _PlayerDataCharacterSpawner.DataSpawn[_CurrentCharacterIndex].Name;
        _CharacterClass.text = _PlayerDataCharacterSpawner.DataSpawn[_CurrentCharacterIndex].Class;
        for (int i = 0; i < _CharacterContainer.childCount; i++)
        {
            Destroy(_CharacterContainer.GetChild(i).gameObject);
        }
        
        GameObject characterGameObject = GameManager.Instance.InstantiateCharacter(_PlayerDataCharacterSpawner.DataSpawn[_CurrentCharacterIndex].CharactersPrefab, Vector3.zero);
        characterGameObject.transform.parent = _CharacterContainer;
        characterGameObject.transform.localPosition = Vector3.zero;
        characterGameObject.transform.localRotation = Quaternion.identity;
        characterGameObject.transform.localScale = new Vector3(3, 3, 3);
    }

    public bool HasRemainingCharacters()
    {
        if (_CurrentAmoutPlayerDataCharacterSpawner <= 0)
        {
            gameObject.SetActive(false);
            return false;
        }
        else
        {
            return true;
        }
    }

    public DataCharacterSpawner.DataSpawner SpawnCurrentCharacterSelected()
    {
        DataCharacterSpawner.DataSpawner tempCharacterDataSpawner = _PlayerDataCharacterSpawner.DataSpawn[_CurrentCharacterIndex];
        CharacterIndexAlreadySpawn.Add(_CurrentCharacterIndex);
        _CurrentAmoutPlayerDataCharacterSpawner--;
        return tempCharacterDataSpawner;
    }

}
