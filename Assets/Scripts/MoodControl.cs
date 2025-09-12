using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;

public class MoodControl : MonoBehaviour
{
    public PostProcessVolume PostProcessing;
    Bloom Bloom_;

    public bool IsLit = true;
    [Header("Light")]
    public List<GameObject> Lights_Light;
    public float lIntensity;
    public float lThreshold;
    public float lSoftknee;
    public float lDiffusion;
    public Color lColor;

    [Header("Night")]
    public List<GameObject> Lights_Night;
    public float nIntensity;
    public float nThreshold;
    public float nSoftknee;
    public float nDiffusion;
    public Color nColor;


    public UnityEvent eToLight;
    public UnityEvent eToNight;

    #region Singleton
    public static MoodControl Instance;
    void Singleton()
    {
        if (Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Awake()
    {
        Singleton();
    }
    #endregion

    void LightOff()
    {
        IsLit = false;

        Bloom_set(nIntensity, nThreshold, nSoftknee, nDiffusion, nColor);

        foreach (var v in Lights_Light)
        {
            v.SetActive(false);
        }

        foreach (var v in Lights_Night)
        {
            v.SetActive(true);
        }

        eToNight.Invoke();
    }

    void LightOn()
    {
        IsLit = true;

        Bloom_set(lIntensity, lThreshold, lSoftknee, lDiffusion, lColor);

        foreach (var v in Lights_Night)
        {
            v.SetActive(false);
        }

        foreach (var v in Lights_Light)
        {
            v.SetActive(true);
        }

        eToLight.Invoke();
    }

    void Bloom_set(float intensity, float threshold, float softKnee, float diffusion, Color color)
    {
        if (PostProcessing.profile.TryGetSettings<Bloom>(out Bloom_))
        {
            Bloom_.intensity.value = intensity;
            Bloom_.threshold.value = threshold;
            Bloom_.softKnee.value = softKnee;
            Bloom_.diffusion.value = diffusion;
            Bloom_.color.value = color;
        }
    }

    public void Swap()
    {
        if (IsLit) { LightOff(); }
        else { LightOn(); }
    }
    
    void Start()
    {
        LightOn();
    }


    void Update()
    {
        if (Input.GetKeyDown("p"))
        {
            Swap();
        }
    }
}
