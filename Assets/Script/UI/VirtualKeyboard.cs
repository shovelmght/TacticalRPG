using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VirtualKeyboard : MonoBehaviour
{
    [SerializeField] private TMP_Text _Text;
    [SerializeField] private InputField _InputField;
    [SerializeField] private int _MaxLetter;
    private string _CurrentText = "";
    

    //Called by button
    public void SetText(string textToAdd)
    {
        AudioManager._Instance.SpawnSelectSfx();
        Debug.Log("_CurrentText.Length = " + _CurrentText.Length);
        if (_CurrentText.Length < _MaxLetter)
        {
            _CurrentText += textToAdd;
            if (_InputField != null)
            {
                _InputField.text = _CurrentText;
            }
            else
            {
                _Text.text = _CurrentText;
            }
        }
    }
    

    
    public void BackSpaceText()
    {
        AudioManager._Instance.SpawnSelectSfx();

        if (_CurrentText.Length > 0)
        {
         
            _CurrentText = _CurrentText.Remove(_CurrentText.Length - 1);
     
            if (_InputField != null)
            {
                _InputField.text = _CurrentText;
            }
            else
            {
                _Text.text = _CurrentText;
            }
 
        }
    }
    
}


