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

    public Slider sensitSlider;
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

    int id = 0;

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
            LoadSettings();
    }

    public void Sensitivity()
    {
        setMan.Sensitivity(sensitSlider.value, id);
    }
    public void YThird()
    {
        setMan.YThird(invertYT.isOn, id);
    }
    public void XThird()
    {
        setMan.XThird(invertXT.isOn, id);
    }
    public void YFirst()
    {
        setMan.YFirst(invertYF.isOn, id);
    }
    public void XFirst()
    {
        setMan.XFirst(invertXF.isOn, id);
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
        setMan.ThirdFov((int)TFov.value, id);
        TFText.text = ((int)TFov.value).ToString();
    }
    public void FirstFov()
    {
        setMan.FirstFov((int)FFov.value, id);
        FFText.text = ((int)FFov.value).ToString();
    }

    public void LoadSettings()
    {
        //;
        if (setMan == null)
            setMan = FindObjectOfType<SettingsManager>();

        setMan.LoadAll(id);

        sensitSlider.value = setMan.sensitivity[id];
        invertYT.isOn = setMan.invertYT[id];
        invertYF.isOn = setMan.invertYF[id];
        invertXT.isOn = setMan.invertXT[id];
        invertXF.isOn = setMan.invertXF[id];

        if (id == 0)
        {
            musicVol.value = setMan.musicVol;
            effVol.value = setMan.effVol;
            resInd = setMan.resInd;
            CambiaResInd(0);

            fullscreen.isOn = setMan.fullscreen;
        }
        else 
        {
            fullscreen.transform.parent.gameObject.SetActive(false);
            resText.transform.parent.gameObject.SetActive(false);
            musicVol.transform.parent.gameObject.SetActive(false);
            effVol.transform.parent.gameObject.SetActive(false); 
        }

        TFov.value = setMan.TFov[id];
        TFText.text = ((int)TFov.value).ToString();
        FFov.value = setMan.FFov[id];
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

    public void SetId(int id_, bool load = false)
    {
        id = id_;
        if (load)
        {
            LoadSettings();
        }
    }
}
