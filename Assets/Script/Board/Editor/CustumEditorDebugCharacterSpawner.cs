using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustumEditorDebugCharacterSpawner : MonoBehaviour
{
    public struct DataSpawnDebug
    {
        private CharactersPrefab charactersPrefab;
        private int CoordX;
        private int CoordY;
    }
    
    public enum CharactersPrefab
    {
        Character = 0,
        CharacterAI = 1
    }
}
