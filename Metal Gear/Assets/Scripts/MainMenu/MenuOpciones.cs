using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MenuOpciones : MonoBehaviour
{
    public SettingsManager setMan;
    public Toggle invertYT;
    public Toggle invertXT;
    public Toggle invertYF;
    public Toggle invertXF;
    public int resInd;
    public Toggle fullscreen;
    public Slider musicVol;
    public Slider effVol;
    public Slider TFov;
    public TMP_Text TFText;
    public Slider FFov;
    public TMP_Text FFText;

    public TMP_Text resText;
    [SerializeField]bool autoIniciar = true;
    bool pantallaSistemaParche = false;

    void Update()
    {
        if (!pantallaSistemaParche)
        {
            Resolution();
            pantallaSistemaParche = true;
        }
    }
    private void Start()
    {
        if (autoIniciar)
            Iniciar();
    }

    public void Iniciar()
    {

        LoadSettings();
    }

    public void YThird()
    {
        setMan.YThird(invertYT.isOn);
    }
    public void XThird()
    {
        setMan.XThird(invertXT.isOn);
    }
    public void YFirst()
    {
        setMan.YFirst(invertYF.isOn);
    }
    public void XFirst()
    {
        setMan.XFirst(invertXF.isOn);
    }
    public void Resolution()
    {
        setMan.Resolution(resInd);
        CambiaResInd(0);
    }
    public void Fullscreen()
    {
        setMan.Fullscreen(fullscreen.isOn);
    }
    public void MusicVolume()
    {
        setMan.MusicVol(musicVol.value);
    }
    public void EffectsVolume()
    {
        setMan.EffectsVol(effVol.value);
    }
    public void ThirdFov()
    {
        setMan.ThirdFov((int)TFov.value);
        TFText.text = ((int)TFov.value).ToString();
    }
    public void FirstFov()
    {
        setMan.FirstFov((int)FFov.value);
        FFText.text = ((int)FFov.value).ToString();
    }

    public void LoadSettings()
    {
        //;

        if (setMan == null)
            setMan = FindObjectOfType<SettingsManager>();

        setMan.LoadAll();


        invertYT.isOn = setMan.invertYT;
        invertYF.isOn = setMan.invertYF;
        invertXT.isOn = setMan.invertXT;
        invertXF.isOn = setMan.invertXF;

        resInd = setMan.resInd;
        CambiaResInd(0);

        fullscreen.isOn = setMan.fullscreen;

        musicVol.value = setMan.musicVol;
        effVol.value = setMan.effVol;

        TFov.value = setMan.TFov;
        TFText.text = ((int)TFov.value).ToString();
        FFov.value = setMan.FFov;
        FFText.text = ((int)FFov.value).ToString();


    }

    public void ResetSettings()
    {
        setMan.ResetSettings();
        LoadSettings();
    }

    public void RecoverResInd()
    {
        resInd = setMan.resInd;
    }
    public void CambiaResInd(int cant)
    {
        resInd += cant;
        resInd = (resInd % Screen.resolutions.Length + Screen.resolutions.Length) % Screen.resolutions.Length;
        resText.text = Screen.resolutions[resInd].width.ToString() + "x" + Screen.resolutions[resInd].height.ToString() + " " +
            Screen.resolutions[resInd].refreshRate.ToString() + "Hz";

        resText.color = new Color(1, resInd == setMan.resInd ? 1 : 0, resInd == setMan.resInd ? 1 : 0);

    }
}
