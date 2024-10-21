using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    [SerializeField] private Slider _SfxSlider;
    [SerializeField] private Slider _MusicSlider;
    [SerializeField] private TMP_Dropdown _ScreenModeDropdown;

    void Start()
    {
        if (GameManager.Instance.IsController)
        {
            _SfxSlider.Select();
        }
        
        float value = 0;
        string sfxPlayerPref = FBPP.GetString("Sfx");

        if (sfxPlayerPref != "")
        {
            float.TryParse(sfxPlayerPref, out value);
            _SfxSlider.value = value;
        }
        
        string musicPlayerPref = FBPP.GetString("Music");

        if (musicPlayerPref != "")
        {
            float.TryParse(musicPlayerPref, out value);
            _MusicSlider.value = value;
        }
    }
    
    public void SetSfxVolume()
    {
        FBPP.SetString("Sfx", _SfxSlider.value.ToString());
        FBPP.Save();
    }
    
    public void SetMusicVolume()
    {
        FBPP.SetString("Music", _MusicSlider.value.ToString());
        FBPP.Save();
    }

    public void SetScreenMode()
    {
        if (_ScreenModeDropdown.value == 0)
        {
            Screen.fullScreen = true;
        }
        else if(_ScreenModeDropdown.value == 1)
        {
            Screen.fullScreen = false;
        }
    }
}
