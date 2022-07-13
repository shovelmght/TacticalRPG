using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileData", menuName = "ScriptableObjects/TileData")]
public class TileData : ScriptableObject
{
    [field: SerializeField] public float HeightGapPosition { get; private set; }
    [field: SerializeField] public int MaxPreviousTile { get; private set; }
}
