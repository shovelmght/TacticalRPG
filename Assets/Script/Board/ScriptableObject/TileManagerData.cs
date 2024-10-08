using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileManagerData", menuName = "ScriptableObjects/TileManagerData")]
[System.Serializable]
public class TileManagerData : ScriptableObject
{
    [field: SerializeField] public int Column{ get; private set; } 
    [field: SerializeField] public int Row{ get; private set; }
    [field: SerializeField] public float TileGap { get; private set; }
    [field: SerializeField] public Material WaterTileMaterial { get; private set; }
    [field: SerializeField] public Material WaterfallTileMaterial { get; private set; }
    [field: SerializeField] public GameObject[] DecorsPrefab{ get; private set; } 
    [field: SerializeField] public PotionData[] _PotionData{ get; private set; }
    [field: SerializeField] public GameObject[] SmallRockPrefab{ get; private set; } 
    [field: SerializeField] public int NumberOfDecor{ get; private set; } 
    [field: SerializeField] public int NumberOfPotion{ get; private set; } 
    [field: SerializeField] public int NumberOfSmallRock{ get; private set; }
    [field: SerializeField] public int _SecondTileChance { get; private set; } = 3;
    [field: SerializeField] public int NumberGroundBackgroundTilesRow{ get; set; } 
    [field: SerializeField] public int NumberGroundBackgroundTilesColumn{ get; set; } 
    [field: SerializeField] public GameObject EmptyTileNormalPrefab{ get; private set; } 
    [field: SerializeField] public GameObject EmptyTileSecondPrefab{ get; private set; } 
    [field: SerializeField] public int BoardTileHeight{ get; private set; } 
    [field: SerializeField] public GameObject TilePrefab{ get; private set; } 
    [field: SerializeField] public GameObject SecondTilePrefab{ get; private set; } 
    [field: SerializeField] public GameObject EmptyTileMudPrefab{ get; private set; }
    [field: SerializeField] public GameObject BridgeTilePrefab{ get; private set; } 
    [field: SerializeField] public GameObject BridgeTile4SidePrefab{ get; private set; } 
    [field: SerializeField] public float HeightChance{ get; private set; } 
    [field: SerializeField] public float WaterChance{ get; private set; } 
    [field: SerializeField] public float WaterChanceBackgroundTiles{ get; private set; } 
    [field: SerializeField] public float HeightChanceBackgroundTiles{ get; private set; }
    [field: SerializeField] public float decorsChanceBackgroundTiles{ get; private set; } 
    [field: SerializeField] public bool WantColumn{ get; private set; } 
}
