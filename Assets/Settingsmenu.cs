using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class Settingsmenu : MonoBehaviour
{
    int Resolution = 18;
    int Graphics = 5;
    bool VSync = false;
    bool Fullscreen = true;
    float MasterVolume = 0.4f;
    float MusicVolume = 0.4f;


    List<Resolution> Resolutions = new List<Resolution>();
    public TMP_Dropdown ResDropdown;
    public TMP_Dropdown GraphicsDropdown;
    public Toggle VSyncToggle;
    public Toggle FullScreenToggle;
    public Slider MasterSlider;
    public Slider MusicSlider;

    public GameObject Canvas;
    public AudioMixer Mixer;
    private void Start()
    {
        ResolutionsSetup();

        Graphics = QualitySettings.GetQualityLevel();
        GraphicsDropdown.value = Graphics;

        if (VSync) { VSyncToggle.isOn = true; QualitySettings.vSyncCount = 1; } else { VSyncToggle.isOn = false; QualitySettings.vSyncCount = 0; }

        FullScreenToggle.isOn = Fullscreen;
        MasterSlider.value = MasterVolume;
        MusicSlider.value = MusicVolume;
        ApplySettings();
    }
    void ResolutionsSetup()
    {
        Resolution[] ress;
        ress = Screen.resolutions;
        RefreshRate CurRefreshRate = Screen.currentResolution.refreshRateRatio;

        for (int i = 0; i < ress.Length; i++)
        {
            Resolutions.Add(ress[i]);
        }

        int curRes = 0;
        List<string> ResOptions = new List<string>();
        for (int i = 0; i < ress.Length; i++)
        {
            string option = Resolutions[i].width + "x" + Resolutions[i].height + " " + Resolutions[i].refreshRateRatio + " Hz";
            ResOptions.Add(option);
            if (Screen.currentResolution.width == Resolutions[i].width && Screen.currentResolution.height == Resolutions[i].height)
            {
                curRes = i;
            }
        }

        ResDropdown.ClearOptions();
        ResDropdown.AddOptions(ResOptions);
        Resolution = curRes;
        ResDropdown.value = curRes;
        ResDropdown.RefreshShownValue();

     }
    public void ResolutionChange(int ind)
    {
        Resolution = ind;
    }
    public void GraphicsChange(int ind)
    {
        Graphics = ind;
    }
    public void VSyncChange(bool v)
    {
        VSync = v;
    }
    public void FullScreenChange(bool f)
    {
        Fullscreen = f;
    }
   
    public void VolumeChange_Master(float volume)
    {
        MasterVolume = volume;
    }
    public void VolumeChange_Music(float volume)
    {
        MusicVolume = volume;
    }
    
    public void ApplySettings()
    {
        //Resolution
        Screen.SetResolution(Resolutions[Resolution].width, Resolutions[Resolution].height, Screen.fullScreen);
        //Graphics
        QualitySettings.SetQualityLevel(Graphics);
        //VSync
        if (VSync) { QualitySettings.vSyncCount = 1; }
        else { QualitySettings.vSyncCount = 0; }
        //Fullscreen
        Screen.fullScreen = Fullscreen;
        //Master Volume
        Mixer.SetFloat("VolumeMaster", Mathf.Log10(MasterVolume)*20);
        //Music Volume
        Mixer.SetFloat("VolumeMusic", Mathf.Log10(MusicVolume) * 20);

        Debug.Log(string.Format("Applied settings - {0} {1} {2} {3} {4} {5}", Resolution, Graphics, VSync, Fullscreen, MasterVolume, MusicVolume));

    }
    public void Toggle(InputAction.CallbackContext context)
    {
        if (Canvas.activeSelf)
        {
            TurnOff();
        }
        else
        {
            TurnOn();
        }
    }
    void TurnOn()
    {
        Canvas.SetActive(true);
    }
    void TurnOff()
    {
        Canvas.SetActive(false);
    }
    
}
