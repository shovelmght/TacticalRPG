using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetachGameObjectOnDestroy : MonoBehaviour
{
    [SerializeField] private Transform _GameOjectToDetach;
    private void OnDestroy()
    {
        Vector3 tempLocation = _GameOjectToDetach.position;
        _GameOjectToDetach.parent = null;
        _GameOjectToDetach.position = tempLocation;
    }
}
