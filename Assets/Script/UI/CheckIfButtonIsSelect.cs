using UnityEngine;
using UnityEngine.EventSystems;

public class CheckIfButtonIsSelect : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public bool _IsSelected { get; set; }
    public void OnSelect(BaseEventData eventData)
    {
        _IsSelected = true;

    }

    public void OnDeselect(BaseEventData eventData)
    {
        _IsSelected = false;

    }

    private void OnDisable()
    {
        _IsSelected = false;
    }
}
