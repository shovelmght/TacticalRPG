using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    [field: SerializeField] public Camera Cam { get; private set; }
    [field: SerializeField] public LayerMask LayerMask { get; private set; }
    private float distanceRay = 100;
    [SerializeField] private Vector3 Gap;
    private GameManager _gameManager;

    private void Start()
    {
        _gameManager = GameManager.Instance;
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if(!EventSystem.current.IsPointerOverGameObject())
            {
                if (Physics.Raycast(ray,out hit, distanceRay, LayerMask))
                {
                    _gameManager.SelectTile(hit.collider.gameObject);
                }
            }
        }
    }
}
