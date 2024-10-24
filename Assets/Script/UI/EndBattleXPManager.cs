using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class EndBattleXPManager : MonoBehaviour
{
    [SerializeField] private UIXPComponent[] _UIXPComponent;
    [SerializeField] private GameObject[] _uixpComponentGameObjects;
    [SerializeField] private Character _SquireCharacter;
    [SerializeField] private Character _WizardCharacter;
    [SerializeField] private Character _RobotCharacter;
    [SerializeField] private Character _FatherNatureCharacter;

    public IEnumerator StartGiveXp(List<Character> characterInGame)
    {
        
        for (int i = 0; i < characterInGame.Count; i++)
        {
            if (characterInGame[i].NextXPLevel == 0)
            {
                characterInGame[i].NextXPLevel = 300;
            }
            _uixpComponentGameObjects[i].SetActive(true);
            _UIXPComponent[i].CurrentLevelText.text = characterInGame[i].Level.ToString();
            _UIXPComponent[i].XPToGive.text = characterInGame[i].XPEarned.ToString();
            _UIXPComponent[i].CurrentXPText.text = characterInGame[i].CurrentXP + " / " + characterInGame[i].NextXPLevel;
            _UIXPComponent[i].CurrentXP = characterInGame[i].CurrentXP;
            _UIXPComponent[i].NextXP = characterInGame[i].CurrentXP;
        }

        yield return new WaitForSeconds(1.5f);
        
        for (int i = 0; i < characterInGame.Count; i++)
        {
            if (characterInGame[i].Class == DataCharacterSpawner.CharactersPrefab.Squire)
            {
                _SquireCharacter.transform.position = _UIXPComponent[i].CharacterPosition;
            }
            else if (characterInGame[i].Class == DataCharacterSpawner.CharactersPrefab.Wizard)
            {
                _WizardCharacter.transform.position = _UIXPComponent[i].CharacterPosition;
            }
            else if (characterInGame[i].Class == DataCharacterSpawner.CharactersPrefab.Robot)
            {
                _RobotCharacter.transform.position = _UIXPComponent[i].CharacterPosition;
            }
            else if (characterInGame[i].Class == DataCharacterSpawner.CharactersPrefab.MotherNature)
            {
                _FatherNatureCharacter.transform.position = _UIXPComponent[i].CharacterPosition;
            }
        }
        
        yield return new WaitForSeconds(1.5f);
        for (int i = 0; i < characterInGame.Count; i++)
        {
            yield return GiveXp(_UIXPComponent[i], characterInGame[i]);
        }
        
        yield return new WaitForSeconds(1.5f);
    }

    public IEnumerator GiveXp(UIXPComponent uIXPComponent, Character character)
    {
        while (character.XPEarned > 0)
        {
            character.XPEarned--;
            uIXPComponent.CurrentXP++;
            uIXPComponent.SliderXP.value = uIXPComponent.CurrentXP / uIXPComponent.NextXP;
            uIXPComponent.CurrentXPText.text = uIXPComponent.CurrentXP.ToString() + " / " + uIXPComponent.NextXP.ToString();
            if (uIXPComponent.CurrentXP > uIXPComponent.NextXP)
            {
                uIXPComponent.CurrentLevelAnimator.SetTrigger("LvlUp");
                uIXPComponent.CurrentXP = 0;
                uIXPComponent.NextXP *= 3;
                uIXPComponent.CurrentLevel++;
                uIXPComponent.CurrentLevelText.text =  uIXPComponent.CurrentLevel.ToString();

            }

            yield return new WaitForSeconds(0.00001f);
        }
       
        if (character.Class == DataCharacterSpawner.CharactersPrefab.Squire)
        {
            FBPP.SetInt("SquireLevel", uIXPComponent.CurrentLevel);
            FBPP.SetInt("SquireXP", uIXPComponent.CurrentXP);
            FBPP.SetInt("SquireNextXP", uIXPComponent.NextXP);
        }
        else if (character.Class == DataCharacterSpawner.CharactersPrefab.Wizard)
        {
            FBPP.SetInt("WizardLevel", uIXPComponent.CurrentLevel);
            FBPP.SetInt("WizardXP", uIXPComponent.CurrentXP);
            FBPP.SetInt("WizardXP", uIXPComponent.NextXP);
        }
        else if (character.Class == DataCharacterSpawner.CharactersPrefab.Robot)
        {
            FBPP.SetInt("RobotLevel", uIXPComponent.CurrentLevel);
            FBPP.SetInt("RobotXP", uIXPComponent.CurrentXP);
            FBPP.SetInt("RobotXP", uIXPComponent.NextXP);
        }
        else if (character.Class == DataCharacterSpawner.CharactersPrefab.MotherNature)
        {
            FBPP.SetInt("FatherNatureLevel", uIXPComponent.CurrentLevel);
            FBPP.SetInt("FatherNatureXP", uIXPComponent.CurrentXP);
            FBPP.SetInt("FatherNatureXP", uIXPComponent.NextXP);
        }
        
    }

    [SerializeField] private Vector3 _POsitionTest;
    
    [ContextMenu("SetCharacterPosition")]
    public void SetCharacterPosition()
    {
        _FatherNatureCharacter.transform.position = _POsitionTest;
    }

[Serializable]
    public class UIXPComponent
    {
        public Vector3 CharacterPosition;
        public TMP_Text CurrentXPText;
        public TMP_Text CurrentLevelText;
        public TMP_Text XPToGive;
        public int CurrentXP;
        public int NextXP;
        public int CurrentLevel;
        public Slider SliderXP;
        public Animator CurrentLevelAnimator;
        
    }
    
}
