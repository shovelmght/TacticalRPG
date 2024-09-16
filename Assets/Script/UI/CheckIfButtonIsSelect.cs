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
    public void OnSelect(BaseEventData eventData)
    {
        _IsSelected = true;
        _BackgroundImage.color = _ColorHighlight;
        _Text.color = _ColorHighlight;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _IsSelected = false;
        _BackgroundImage.color = _StartColor;
        _Text.color = _StartColor;
    }

    private void OnDisable()
    {
        _IsSelected = false;
    }
}
