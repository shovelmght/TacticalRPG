using System;
using UnityEngine;
using UnityEngine.UI;

public class VirtualKeyboard : MonoBehaviour
{
    [SerializeField] private InputField _InputField;
    [SerializeField] private GameObject _GameObjectToDestroy;
    [SerializeField] private GameObject _GameObjectToDestroy2;

    private string _CurrentText = "";
    

    //Called by button
    public void SetText(string textToAdd)
    {
        _CurrentText += textToAdd;
        _InputField.text = _CurrentText;
    }
    

    
    public void BackSpaceText()
    {
        Debug.Log("BackSpaceText");
        if (_CurrentText.Length > 0)
        {
            Debug.Log("BackSpaceText 2 _CurrentText = " + _CurrentText  + "   _InputField.text = " + _InputField.text);
            _CurrentText = _CurrentText.Remove(_CurrentText.Length - 1);
            Debug.Log("BackSpaceText 3 _CurrentText = " + _CurrentText  + "   _InputField.text = " + _InputField.text);
            _InputField.text = _CurrentText;
            Debug.Log("BackSpaceText 4 _CurrentText = " + _CurrentText  + "   _InputField.text = " + _InputField.text);
        }
    }
}


