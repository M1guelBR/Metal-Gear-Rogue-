using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;

public class SettingsManager : MonoBehaviour
{
    public float[] sensitivity = new float[4] { 0.75f , 0.75f, 0.75f, 0.75f};
    public bool[] invertYT = new bool[4] { true, true, true, true};
    public bool[] invertXT = new bool[4] { false, false, false, false};
    public bool[] invertYF = new bool[4] { false, false, false, false};
    public bool[] invertXF = new bool[4] { false, false, false, false};
    internal int resInd = 0;
    public bool fullscreen = true;
    public float musicVol = 1;
    public float effVol = 1;
    public int[] TFov = new int[4] { 75, 75, 75, 75};
    public int[] FFov = new int[4] { 75, 75, 75, 75 };


    public AudioMixer mixer;
    [SerializeField] bool autoLoad = true;



    public void Sensitivity(float value, int id)
    {
        value = Mathf.Clamp(value, .25f, 1);
        string key = "Sensitivity" + id.ToString();
        PlayerPrefs.SetFloat(key, value);

        if (FindObjectOfType<GameManager>())
        {
            FindObjectOfType<GameManager>().SetSensitivity(value, id);
        }
    }
    public void YThird(bool input, int id)
    {
        invertYT[id] = input;
        PlayerPrefs.SetInt("invertYT" + id.ToString(), invertYT[id] ? 1 : 0);

        if (FindObjectOfType<GameManager>())
        {
            FindObjectOfType<GameManager>().SetInversiones(1, input, id);
        }
    }
    public void XThird(bool input, int id)
    {

        invertXT[id] = input;
        PlayerPrefs.SetInt("invertXT" + id.ToString(), invertXT[id] ? 1 : 0);

        if (FindObjectOfType<GameManager>())
        {
            FindObjectOfType<GameManager>().SetInversiones(0, input, id);
        }
    }
    public void YFirst(bool input, int id)
    {
        invertYF[id] = input;
        PlayerPrefs.SetInt("invertYF" + id.ToString(), invertYF[id] ? 1 : 0);

        if (FindObjectOfType<GameManager>())
        {
            FindObjectOfType<GameManager>().SetInversiones(3, input, id);
        }
    }
    public void XFirst(bool input, int id)
    {

        invertXF[id] = input;
        PlayerPrefs.SetInt("invertXF" + id.ToString(), invertXF[id] ? 1 : 0);

        if (FindObjectOfType<GameManager>())
        {
            FindObjectOfType<GameManager>().SetInversiones(2, input, id);
        }
    }
    public void ThirdFov(int input, int id)
    {
        TFov[id] = input;
        PlayerPrefs.SetInt("TFov" + id.ToString(), input);

        if (FindObjectOfType<GameManager>())
            FindObjectOfType<GameManager>().SetFov(input, false, id);
    }
    public void FirstFov(int input, int id)
    {
        FFov[id] = input;
        PlayerPrefs.SetInt("FFov" + id.ToString(), input);

        if (FindObjectOfType<GameManager>())
            FindObjectOfType<GameManager>().SetFov(input, true, id);
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


    public void SaveAll(int id)
    {
        //return;
        //Primero los que dependen de cada jugador
        //for (int id = 0; id < 1; id++)
        if (id >= 0)
        {
            Sensitivity(sensitivity[id], id);
            YThird(invertYT[id], id);
            XThird(invertXT[id], id);
            YFirst(invertYF[id], id);
            XFirst(invertXF[id], id);
            ThirdFov(TFov[id], id);
            FirstFov(FFov[id], id);
        }

        //Esto es lo unico que puede cambiar el jugador 1
        if (id == 0)
        {
            Resolution(resInd);
            Fullscreen(fullscreen);
            MusicVol(musicVol);
            //return;
            EffectsVol(effVol);
        }

    }
    public void LoadAll(int id = -1)
    {
        
        //Primero los que dependen de cada jugador
        if(id >= 0)
        {
            sensitivity[id] = PlayerPrefs.GetFloat("Sensitivity" + id.ToString(), .5f);
            invertYT[id] = PlayerPrefs.GetInt("invertYT" + id.ToString(), 1) == 1;
            invertXT[id] = PlayerPrefs.GetInt("invertXT" + id.ToString(), 0) == 1;
            invertYF[id] = PlayerPrefs.GetInt("invertYF" + id.ToString(), 0) == 1;
            invertXF[id] = PlayerPrefs.GetInt("invertXF" + id.ToString(), 0) == 1;
            TFov[id] = PlayerPrefs.GetInt("TFov" + id.ToString(), 75);
            FFov[id] = PlayerPrefs.GetInt("FFov" + id.ToString(), 75);

        }

        //Esto es lo que solo puede cambiar el jugador 1
        if (id == 0)
        {
            resInd = PlayerPrefs.GetInt("resInd", Screen.resolutions.Length - 1);
            fullscreen = PlayerPrefs.GetInt("fullscreen", 1) == 1;

            musicVol = PlayerPrefs.GetFloat("musicVol", 1);
            effVol = PlayerPrefs.GetFloat("effVol", 1);
        }

        SaveAll(id);

    }

    private void Start()
    {
        if (autoLoad)
        {
            LoadAll(0);
        }
    }

    private float ValueToVolume(float value, float maxVolume = 0)
    {
        return Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * (maxVolume - (-80)) / 4f + maxVolume;
    }

    public void ResetSettings()
    {
        sensitivity = new float[] { 0.75f , 0.75f, 0.75f, 0.75f};
        invertYT = new bool[] { true, true, true, true };
        invertXT = new bool[] { false, false, false, false };
        invertYF = new bool[] { false, false, false, false };
        invertXF = new bool[] { false, false, false, false };
        resInd = Screen.resolutions.Length - 1;
        fullscreen = true;
        musicVol = 1;
        effVol = 1;
        TFov = new int[] { 75, 75, 75, 75 };
        FFov = new int[] { 75, 75, 75, 75 };
        SaveAll(0);
        SaveAll(1);
        SaveAll(2);
        SaveAll(3);
    }

}
