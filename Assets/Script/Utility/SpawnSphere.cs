using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SpawnSphere
{

    public static void InstantiateSphere(Vector3 position)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = position;
    }
}
