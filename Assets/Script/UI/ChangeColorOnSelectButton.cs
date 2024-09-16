using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChangeColorOnSelectButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image[] _ImageSwapColor;
    [SerializeField] private Color _ColorWanted;
    [SerializeField] private Color _StarColor;
    [SerializeField] private Text[] _TextReference;
    [SerializeField] private Color _TextColorWanted;
    [SerializeField] private Color _TextStarColor;
    private Button _Button;

    private void Awake()
    {
        _Button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        foreach (var imageSwapColor in _ImageSwapColor)
        {
            imageSwapColor.color = _StarColor;
        }
        
        foreach (var textReference in _TextReference)
        {
            textReference.color = _TextStarColor;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_ImageSwapColor != null)
        {
            if (_Button != null)
            {
                if (_Button.interactable)
                {
                    foreach (var imageSwapColor in _ImageSwapColor)
                    {
                        imageSwapColor.color = _ColorWanted;
                    }
                }
            }
            else
            {
                foreach (var imageSwapColor in _ImageSwapColor)
                {
                    imageSwapColor.color = _ColorWanted;
                }
            }
        }

        if (_TextReference != null)
        {
            if (_Button != null)
            {
                if (_Button.interactable)
                {
                    foreach (var textReference in _TextReference)
                    {
                        textReference.color = _TextColorWanted;
                    }
                }
            }
            else
            {
                foreach (var textReference in _TextReference)
                {
                    textReference.color = _TextColorWanted;
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        foreach (var imageSwapColor in _ImageSwapColor)
        {
            imageSwapColor.color = _StarColor;
        }

        foreach (var textReference in _TextReference)
        {
            textReference.color = _TextStarColor;
        }
    }
}
