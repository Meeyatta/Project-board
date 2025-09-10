using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorChange : MonoBehaviour
{
    public Color cLight;
    public Color cNight;

    Image image;

    public void ToLight()
    {
        if (image != null) image.color = cLight;
        else { Debug.LogError(gameObject.name + " no image"); }
    }

    public void ToNight()
    {
        if (image != null) image.color = cNight;
        else { Debug.LogError(gameObject.name + " no image"); }
    }


    void Start()
    {
        image = GetComponent<Image>();

        MoodControl.Instance.eToLight.AddListener(ToLight);
        MoodControl.Instance.eToNight.AddListener(ToNight);

    }

    private void OnDisable()
    {
        MoodControl.Instance.eToLight.RemoveListener(ToLight);
        MoodControl.Instance.eToNight.RemoveListener(ToNight);
    }
}
