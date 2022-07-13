using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISelect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image Image;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Image.color = new Color(1,1,1,1);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        Image.color = new Color(0,0,0,0);
    }
}
