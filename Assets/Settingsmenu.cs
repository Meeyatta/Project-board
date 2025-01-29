using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using TMPro;

public class Settingsmenu : MonoBehaviour
{
    List<Resolution> Resolutions = new List<Resolution>();
    public TMP_Dropdown ResDropdown;

    public GameObject Canvas;
    public AudioMixer Mixer;
    private void Start()
    {
        ResolutionsSetup();
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
        ResDropdown.value = curRes;
        ResDropdown.RefreshShownValue();

     }
    public void FullScreenChange(bool f)
    {
        Screen.fullScreen = f;
    }
    public void ResolutionChange(int ind) 
    {
        Screen.SetResolution(Resolutions[ind].width, Resolutions[ind].height, Screen.fullScreen);
    }

    public void GraphicsChange(int ind)
    {
        QualitySettings.SetQualityLevel(ind);
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
    public void VolumeChange_Master(float volume)
    {
        Mixer.SetFloat("VolumeMaster", volume);
    }
    public void VolumeChange_Music(float volume)
    {
        Mixer.SetFloat("VolumeMusic", volume);
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
