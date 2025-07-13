using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class SettingsManager : MonoBehaviour
{
    public bool invertYT;
    public bool invertXT;
    public bool invertYF;
    public bool invertXF;
    internal int resInd = 0;
    public bool fullscreen = true;
    public float musicVol = 1;
    public float effVol = 1;
    public int TFov = 90;
    public int FFov = 90;


    public AudioMixer mixer;
    [SerializeField] bool autoLoad = true;




    public void YThird(bool input)
    {
        invertYT = input;
        PlayerPrefs.SetInt("invertYT", invertYT ? 1 : 0);
        if (FindObjectOfType<Snake>())
        {
            FindObjectOfType<Snake>().SetInversiones(1, input);
        }
    }
    public void XThird(bool input)
    {

        invertXT = input;
        PlayerPrefs.SetInt("invertXT", invertXT ? 1 : 0);
        if (FindObjectOfType<Snake>())
        {
            FindObjectOfType<Snake>().SetInversiones(0, input);
        }
    }
    public void YFirst(bool input)
    {
        invertYF = input;
        PlayerPrefs.SetInt("invertYF", invertYF ? 1 : 0);
        if (FindObjectOfType<Snake>())
        {
            FindObjectOfType<Snake>().SetInversiones(3, input);
        }
    }
    public void XFirst(bool input)
    {

        invertXF = input;
        PlayerPrefs.SetInt("invertXF", invertXF ? 1 : 0);
        if (FindObjectOfType<Snake>())
        {
            FindObjectOfType<Snake>().SetInversiones(2, input);
        }
    }
    public void Resolution(int input)
    {
        resInd = Mathf.Clamp(input, 0, Screen.resolutions.Length-1);
        PlayerPrefs.SetInt("resInd", resInd);
        Screen.SetResolution(Screen.resolutions[resInd].width, Screen.resolutions[resInd].height, fullscreen, Screen.resolutions[resInd].refreshRate);
    }
    public void Fullscreen(bool input)
    {
        fullscreen = input;
        PlayerPrefs.SetInt("fullscreen", fullscreen ? 1 : 0);
        Screen.SetResolution(Screen.resolutions[resInd].width, Screen.resolutions[resInd].height, fullscreen, Screen.resolutions[resInd].refreshRate);
    }

    //Hacer los canales de musica y efectos
    public void MusicVol(float input)
    {
        musicVol = input;
        PlayerPrefs.SetFloat("musicVol", musicVol);
        //Hacer bien
        mixer.SetFloat("musicVol", ValueToVolume(musicVol));
    }
    public void EffectsVol(float input)
    {
        effVol = input;
        PlayerPrefs.SetFloat("effVol", effVol);
        //Hacer bien
        mixer.SetFloat("effVol", ValueToVolume(effVol));
    }
    public void ThirdFov(int input)
    {
        TFov = input;
        PlayerPrefs.SetInt("TFov", input);
        if (FindObjectOfType<Snake>())
            FindObjectOfType<Snake>().SetFov(input, false);
    }
    public void FirstFov(int input)
    {
        FFov = input;
        PlayerPrefs.SetInt("FFov", input);
        if (FindObjectOfType<Snake>())
            FindObjectOfType<Snake>().SetFov(input, true);
    }


    public void SaveAll()
    {
        YThird(invertYT);
        XThird(invertXT);
        YFirst(invertYF);
        XFirst(invertXF);
        Resolution(resInd);
        Fullscreen(fullscreen);
        MusicVol(musicVol);
        EffectsVol(effVol);
        ThirdFov(TFov);
        FirstFov(FFov);

    }
    public void LoadAll()
    {


        invertYT = PlayerPrefs.GetInt("invertYT", 1) == 1;
        invertXT = PlayerPrefs.GetInt("invertXT", 0) == 1;
        invertYF = PlayerPrefs.GetInt("invertYF", 0) == 1;
        invertXF = PlayerPrefs.GetInt("invertXF", 0) == 1;

        resInd = PlayerPrefs.GetInt("resInd", Screen.resolutions.Length - 1);
        fullscreen = PlayerPrefs.GetInt("fullscreen", 1) == 1;

        musicVol = PlayerPrefs.GetFloat("musicVol", 1);
        effVol = PlayerPrefs.GetFloat("effVol", 1);

        TFov = PlayerPrefs.GetInt("TFov", 75);
        FFov = PlayerPrefs.GetInt("FFov", 75);
        
        SaveAll();

    }

    private void Start()
    {
        if (autoLoad)
        {
            LoadAll();
        }
    }

    private float ValueToVolume(float value, float maxVolume = 0)
    {
        return Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * (maxVolume - (-80)) / 4f + maxVolume;
    }

    public void ResetSettings()
    {
        invertYT = true;
        invertXT = false;
        invertYF = false;
        invertXF = false;
        resInd = Screen.resolutions.Length - 1;
        fullscreen = true;
        musicVol = 1;
        effVol = 1;
        TFov = 75;
        FFov = 75;
        SaveAll();
    }

}
