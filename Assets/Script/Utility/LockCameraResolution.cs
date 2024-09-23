using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockCameraResolution : MonoBehaviour
{
    [SerializeField] private Camera _Camera;
    [SerializeField] private Rect GateFitMode;

    void Update()
    {
        _Camera.pixelRect = GateFitMode;
    }
}
