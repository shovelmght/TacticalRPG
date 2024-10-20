using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public PotionData PotionData { get; private set; }
    public Animator PotionAnimator { get; private set; }
    public GameObject CurrentGameObject { get; }
    public Tile[] PreviousMoveTilesList{ get; }
    public MeshRenderer[] MeshRenderer { get; }
    public Material StartMaterial { get;  set; }
    public Material StartSecondMaterial { get; set; }
    public Character CharacterReference { get;  set; }
    public Vector3 Position { get;  set; }
    public int CoordX { get; }
    public int CoordY { get; }
    public bool IsOccupied { get;  set; }
    public bool IsPotionTile { get;  set; }
    public bool CanInteract { get;  set; }
    public bool IsValidSpawnTile { get;  set; }
    public bool IsWater { get; set; }
    public bool IsBridge { get;  set; }
    public int Height { get;  set; }
    public int WaterLenghtNorthSouth { get;  set; }
    public int WaterLenghtEstWest { get;  set; }
    public Tile[] SideTiles { get; }
    public Tile[] DiagonalSideTiles { get; }
    
    public MapTilesManager MapTilesManager { get;  set; }

    private int _diagonalSideTileIndex = 0;
    private Transform[] _cameraNearTransform = new Transform[4];
    private Transform[] _cameraFarTransform = new Transform[4];
    private int _index;
    private TileData _tileData;
    private const int CHILD_INDEX_LOD1 = 8;
    private const int CHILD_INDEX_LOD2 = 9;
    private const int CHILD_INDEX_LOD3 = 10;
    
    public Tile( TileData tileData, GameObject gameObject, int coordX, int coordY, Vector3 position, float height, MapTilesManager mapTilesManager)
    {
        _tileData = tileData;
        CurrentGameObject = gameObject;
        CoordX = coordX;
        CoordY = coordY;
        MeshRenderer = new MeshRenderer[3];
        MeshRenderer[0] = gameObject.transform.GetChild(CHILD_INDEX_LOD1).GetComponent<MeshRenderer>();
        MeshRenderer[1] = gameObject.transform.GetChild(CHILD_INDEX_LOD2).GetComponent<MeshRenderer>();
        MeshRenderer[2] = gameObject.transform.GetChild(CHILD_INDEX_LOD3).GetComponent<MeshRenderer>();
        var materials = MeshRenderer[0].materials;
        StartMaterial = materials[1];
        StartSecondMaterial = materials[0];
        CharacterReference = null;
        Position = position + new Vector3(0, _tileData.HeightGapPosition, 0);
        PreviousMoveTilesList =  new Tile[_tileData.MaxPreviousTile];
        SideTiles = new Tile[4];
        DiagonalSideTiles = new Tile[4];
        if (mapTilesManager != null)
        {
            MapTilesManager = mapTilesManager;
        }
        
        
        if (height > 0)
        {
            Height++;
        }
        for (int i = 0; i < 4; i++)
        {
            _cameraNearTransform[i] = gameObject.transform.GetChild(i);
            _cameraFarTransform[i] = gameObject.transform.GetChild(i + 4);
        }
    }

    public void SetCharacter(Character character)
    {
        if (!GameManager.Instance._IsMapScene)
        {
            IsOccupied = true;
        }
        
        CharacterReference = character;
    }
    
    public void SetPotion(Animator animator, PotionData potionData)
    {
        PotionAnimator = animator;
        PotionData = potionData;
    }
    
    public void UnSetCharacter()
    {
        IsOccupied = false;
        CharacterReference = null;
    }

    public void AddPreviousMoveTile(Tile tile)
    {
        if (_index < _tileData.MaxPreviousTile)
        {
            PreviousMoveTilesList[_index] = tile;
            _index++;
        }
    }
    
    public void ClearPreviousMoveTile()
    {
        _index = 0;
    }
    public int GetPreviousMoveTileLenght()
    {
        return _index;
    }

    public Transform GetCameraTransform(int index, bool isNear)
    {
        if (isNear)
        {
            return _cameraNearTransform[index - 1];
        }
        return _cameraFarTransform[index - 1];
    }

    public void SetTopMaterial(Material material)
    {
        if (GameManager.Instance._GameIsFinish) { return; }
        foreach (var meshRenderer in MeshRenderer)
        {
            var materials = meshRenderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }
            meshRenderer.materials = materials;
        }
    }
    
    public void RemoveMeshRenderers()
    {

        foreach (var meshRenderer in MeshRenderer)
        {
            meshRenderer.enabled = false;
        }
    }

    public void ResetSetTopMaterial()
    {
        foreach (var meshRenderer in MeshRenderer)
        {
            var materials = meshRenderer.materials;
            if (materials.Length > 1)
            {
                materials[0] = StartSecondMaterial;
                materials[1] = StartMaterial;
            }
            else
            {
                materials[0] = StartMaterial;
            }
            

            meshRenderer.materials = materials;
        }
    }

    public Material GetTopMaterial()
    {
        var materials = MeshRenderer[0].materials;
        if (materials.Length > 1)
        {
            return materials[1];
        }

        return materials[0];

    }

    public void SetSideTiles()
    {
        if (MapTilesManager != null)
        {
            if (CoordX <  MapTilesManager.TileManagerData.Column - 1)
            {
                SideTiles[(int)CardinalPoint.Orientation.North] = MapTilesManager.GetTile(CoordX + 1, CoordY);
            }
        
            if (CoordX > 0)
            { 
                SideTiles[(int)CardinalPoint.Orientation.South] = MapTilesManager.GetTile(CoordX - 1 , CoordY);
            }
            if (CoordY > 0)
            { 
                SideTiles[(int)CardinalPoint.Orientation.West] = MapTilesManager.GetTile(CoordX , CoordY - 1);
            }
            if (CoordY < MapTilesManager.TileManagerData.Row - 1)
            { 
                SideTiles[(int)CardinalPoint.Orientation.Est] = MapTilesManager.GetTile(CoordX , CoordY + 1);
            }
        }
        else
        {
            TilesManager tilesManager = TilesManager.Instance;
            if (CoordX <  tilesManager.TileManagerData.Column - 1)
            {
                SideTiles[(int)CardinalPoint.Orientation.North] = tilesManager.GetTile(CoordX + 1, CoordY);
            }
        
            if (CoordX > 0)
            { 
                SideTiles[(int)CardinalPoint.Orientation.South] = tilesManager.GetTile(CoordX - 1 , CoordY);
            }
            if (CoordY > 0)
            { 
                SideTiles[(int)CardinalPoint.Orientation.West] = tilesManager.GetTile(CoordX , CoordY - 1);
            }
            if (CoordY < tilesManager.TileManagerData.Row - 1)
            { 
                SideTiles[(int)CardinalPoint.Orientation.Est] = tilesManager.GetTile(CoordX , CoordY + 1);
            }
        }

    }
    
    public void SetDiagonalSideTiles()
    {
        if (MapTilesManager != null)
        {
            if (CoordX <  MapTilesManager.TileManagerData.Column - 1 && CoordY > 0)
            {
                DiagonalSideTiles[(int)CardinalPoint.DiagonalOrientation.NorthEst] = MapTilesManager.GetTile(CoordX + 1, CoordY - 1);
            }
        
            if (CoordX > 0 && CoordY > 0)
            { 
                DiagonalSideTiles[(int)CardinalPoint.DiagonalOrientation.SouthEst] = MapTilesManager.GetTile(CoordX - 1 , CoordY - 1);
            }
            if (CoordX <  MapTilesManager.TileManagerData.Column - 1 && CoordY < MapTilesManager.TileManagerData.Row - 1)
            { 
                DiagonalSideTiles[(int)CardinalPoint.DiagonalOrientation.NorthWest] = MapTilesManager.GetTile(CoordX + 1 , CoordY + 1);
            }
            if (CoordY < MapTilesManager.TileManagerData.Row - 1 && CoordX > 0)
            { 
                DiagonalSideTiles[(int)CardinalPoint.DiagonalOrientation.SouthWest] = MapTilesManager.GetTile(CoordX - 1 , CoordY + 1);
            }
        }
        else
        {
            TilesManager tilesManager = TilesManager.Instance;
            if (CoordX <  tilesManager.TileManagerData.Column - 1 && CoordY > 0)
            {
                DiagonalSideTiles[(int)CardinalPoint.DiagonalOrientation.NorthEst] = tilesManager.GetTile(CoordX + 1, CoordY - 1);
            }
        
            if (CoordX > 0 && CoordY > 0)
            { 
                DiagonalSideTiles[(int)CardinalPoint.DiagonalOrientation.SouthEst] = tilesManager.GetTile(CoordX - 1 , CoordY - 1);
            }
            if (CoordX <  tilesManager.TileManagerData.Column - 1 && CoordY < tilesManager.TileManagerData.Row - 1)
            { 
                DiagonalSideTiles[(int)CardinalPoint.DiagonalOrientation.NorthWest] = tilesManager.GetTile(CoordX + 1 , CoordY + 1);
            }
            if (CoordY < tilesManager.TileManagerData.Row - 1 && CoordX > 0)
            { 
                DiagonalSideTiles[(int)CardinalPoint.DiagonalOrientation.SouthWest] = tilesManager.GetTile(CoordX - 1 , CoordY + 1);
            }
        }

    }

    private void SetValidSpawnTile()
    {
        IsValidSpawnTile = true;
        
    }
}
