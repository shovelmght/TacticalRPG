using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CheckIfButtonIsSelect : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public bool _IsSelected { get; set; }
    [SerializeField] private Image _BackgroundImage;
    [SerializeField] private Text _Text;
    [SerializeField] private Color _StartColor;
    [SerializeField] private Color _ColorHighlight;
    [SerializeField] private bool _SelectButtonOnStart;

    private void OnEnable()
    {
        if (GameManager.Instance.IsController && _SelectButtonOnStart)
        {
            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.Select();
                return;
            }
            
            Slider slider = GetComponent<Slider>();
            if (slider != null)
            {
                slider.Select();
            }
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        _IsSelected = true;
        if (_BackgroundImage != null)
        {
            _BackgroundImage.color = _ColorHighlight;
        }

        if (_Text != null)
        {
            _Text.color = _ColorHighlight;
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _IsSelected = false;
        if (_BackgroundImage != null)
        {
            _BackgroundImage.color = _StartColor;
        }

        if (_Text != null)
        {
            _Text.color = _StartColor;
        }
    }

    private void OnDisable()
    {
        _IsSelected = false;
    }
}
