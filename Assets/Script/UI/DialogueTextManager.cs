using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueTextManager : MonoBehaviour
{
    [SerializeField] private float _TypeSpeed;
    [SerializeField] private float _WaitingTimeBeforeStart;
    private Color _TextColor;
    private bool _DoOnce;
    private string _StringReference;
    private bool _CoroutineIsFinish = true;
    private bool _Coroutine;
    private int _Itteration;
    private bool _StopCoroutine;

    public static DialogueTextManager Instance { get; private set; }

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
    
    public IEnumerator StarDialogue(string textToDisplay, TMP_Text textReference)
    {
        _Itteration++;
        int tempItteration = _Itteration;
        if(!_CoroutineIsFinish)
        {
            _StopCoroutine = true;
        }

        _StopCoroutine = false;
        _CoroutineIsFinish = false;
        yield return new WaitForSeconds(_WaitingTimeBeforeStart);
        string textTotDisplay = textToDisplay;
        textReference.text = string.Empty;

        
        for (int i = 0; i < textTotDisplay.Length; i++)
        {
            if (tempItteration == _Itteration)
            {
                AudioManager._Instance.SpawnSound(AudioManager._Instance._ButtonClick);
                if (_StopCoroutine)
                {
                  
                    _StopCoroutine = false;
                    _CoroutineIsFinish = true;
                    yield break;
                }

                textReference.text += textTotDisplay[i];
                yield return new WaitForSeconds(_TypeSpeed);
            }
        }
        
        _CoroutineIsFinish = true;
    }
    

    public void SetWaitingStartTime(float newValue)
    {
        _WaitingTimeBeforeStart = newValue;
    }
    
}
