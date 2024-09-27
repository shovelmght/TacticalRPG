using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsManager : MonoBehaviour
{
    private void Awake()
    {
        var congif = new FBPPConfig()
        {
            SaveFileName = "my-save-file-txt",
            AutoSaveData = false,
            ScrambleSaveData = true,
            EncryptionSecret = "my-secret",
            SaveFilePath = Application.persistentDataPath
        };
        
        
        FBPP.Start(congif);
    }

    public static void DeleteAllKeys()
    {
        FBPP.DeleteAll();
        FBPP.Save();
    }
}
