using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactersManager : MonoBehaviour
{
    enum CharacterType
    {
        None = 0,
        SwordMan
    }
    public List<Character> CharacterList { get; set; } = new List<Character>();
    
    // public static CharactersManager Instance { get; private set; }
    

    private void Awake() 
    { 
        // If there is an instance, and it's not me, delete myself.
    
        // if (Instance != null && Instance != this) 
        // { 
        //     Destroy(this); 
        // } 
        // else 
        // { 
        //     Instance = this; 
        // } 
    }
}
