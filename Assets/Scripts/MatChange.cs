using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatChange : MonoBehaviour
{
    public Material mLight;
    public Material mNight;

    Renderer rend;

    public void ToLight()
    {
        if (rend != null) rend.material = mLight;
        else { Debug.LogError(gameObject.name + " doesn't have a renderer"); }
    }

    public void ToNight()
    {
        if (rend != null) rend.material = mNight;
        else { Debug.LogError(gameObject.name + " doesn't have a renderer"); }
    }


    void Start()
    {
        rend = GetComponent<Renderer>();

        MoodControl.Instance.eToLight.AddListener(ToLight);
        MoodControl.Instance.eToNight.AddListener(ToNight);

    }

    private void OnDisable()
    {
        MoodControl.Instance.eToLight.RemoveListener(ToLight);
        MoodControl.Instance.eToNight.RemoveListener(ToNight);
    }

    void Update()
    {
        
    }
}
