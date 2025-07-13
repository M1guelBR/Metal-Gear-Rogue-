using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] Image blackFade;
    [SerializeField] AudioSource titleTheme;
    float tiempo = 0;
    [Range(.5f, 2), SerializeField] float velocidadFadeIn = 1;

    [SerializeField] Button botonInicioDef;

    [SerializeField] GameObject extrasMenu;
    [SerializeField] Toggle modoNSnake;

    public Button buttonDef;
    public Toggle toggleDef;
    public enum Modo
    {
        Boton,
        Toggle
    };
    public Modo modo = Modo.Boton;

    Button volverButton;
    public EventSystem eventSystem;
    bool loading = false;
    string escenaLoad = "";

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        SetDefaultButton(botonInicioDef.gameObject);
        blackFade.color = new Color(blackFade.color.r, blackFade.color.g, blackFade.color.b, 1 - tiempo);
        extrasMenu.SetActive(PlayerPrefs.GetInt("NSnake", -1) > -1);
        modoNSnake.isOn = PlayerPrefs.GetInt("NSnake", -1) > 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(tiempo < 1)
        {
            tiempo += Time.deltaTime * velocidadFadeIn;
            if (tiempo > 1)
                tiempo = 1;
            blackFade.color = new Color(blackFade.color.r, blackFade.color.g, blackFade.color.b, loading ? tiempo : (1 - tiempo));


            titleTheme.volume = loading ? 1 - tiempo : tiempo;
            if(tiempo == 1 && loading)
            {
                SceneManager.LoadScene(escenaLoad);
            }
        }


        bool gamepad = false;
        if (Gamepad.all.Count > 0)
            gamepad = Gamepad.current.IsActuated();
        if (eventSystem.currentSelectedGameObject == null && (Keyboard.current.IsActuated() || gamepad))
        {

            if(modo == Modo.Boton)
                buttonDef.Select();
            if (modo == Modo.Toggle)
                toggleDef.Select();

        }
        if (Keyboard.current.escapeKey.isPressed)
            Volver();

    }

    public void SetDefaultButton(GameObject def)
    {


        if(def.GetComponent<Button>())
        {
            buttonDef = def.GetComponent<Button>();
            toggleDef = null;
            modo = Modo.Boton;
        }
        else if(def.GetComponent<Toggle>())
        {
            toggleDef = def.GetComponent<Toggle>();
            buttonDef = null;
            modo = Modo.Toggle;
        }



    }
    public void Volver()
    {
        if (volverButton == null)
            return;
        volverButton.onClick.Invoke();
    }


    public void SetVolverButton(Button boton)
    {
        if(boton.name == "Nulo")
        {
            volverButton = null;
            return;
        }
        volverButton = boton;
    }

    public void AbrirEscena(string escena)
    {
        loading = true;
        tiempo = 0;
        escenaLoad = escena;
    }
    public void Cerrar()
    {
        Application.Quit();
    }

    public void SetNSnake()
    {
        if (PlayerPrefs.GetInt("NSnake", -1) == -1)
            return;
        bool value = PlayerPrefs.GetInt("NSnake", -1) == 1;
        value = !value;
        PlayerPrefs.SetInt("NSnake", value ? 1 : 0);
    }

}
