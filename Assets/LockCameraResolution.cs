using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockCameraResolution : MonoBehaviour
{
    [SerializeField] private Camera _Camera;
    [SerializeField] private Rect GateFitMode;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log("Screen.height = " + Screen.height);
        Debug.Log("Screen.width = " + Screen.width);
        _Camera.pixelRect = GateFitMode;
    }
}
