using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoRotation : MonoBehaviour
{

    public float Speed = 15;
    public Vector3 vec;

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(vec * Speed * Time.deltaTime);
    }
}
