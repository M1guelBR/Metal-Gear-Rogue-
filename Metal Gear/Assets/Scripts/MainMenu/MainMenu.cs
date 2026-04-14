using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("INICIO")]
    [SerializeField] Image blackFade;
    [SerializeField] AudioSource titleTheme;
    float tiempo = 0;
    [Range(.5f, 2), SerializeField] float velocidadFadeIn = 1;

    [SerializeField] Button botonInicioDef;

    //Seleccion de personajes
    [Header("SELECCION DE PERSONAJES")]
    [SerializeField] GameObject characterSelect;
    [SerializeField] string[] nombresPers;
    [SerializeField] Sprite[] imagenesPers;

    [Header("NAVEGACION")]
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
    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
        SetDefaultButton(botonInicioDef.gameObject);
        blackFade.color = new Color(blackFade.color.r, blackFade.color.g, blackFade.color.b, 1 - tiempo);

        //Si vienes de una partida en multijugador, borra los indicadores auxiliares de mandos
        MultipAux[] auxABorrar = FindObjectsOfType<MultipAux>();
        print(auxABorrar.Length);
        for (int i = 0; i < auxABorrar.Length; i++)
            Destroy(auxABorrar[i].gameObject, 0);

        //Seleccion de personajes
        characterSelect.SetActive(PlayerPrefs.GetInt("Personajes", 1) > 1);

        //extrasMenu.SetActive(PlayerPrefs.GetInt("NSnake", -1) > -1);
        //nSnakeCheck.SetActive(PlayerPrefs.GetInt("NSnake", -1) > 0);
    }

    // Update is called once per frame
    void Update()
    {
        if(tiempo < 1)
        {
            tiempo += Time.deltaTime * velocidadFadeIn;

            //Cuando está cargando una escena, hacemos que tiempo sea el porcentaje de carga
            //Cargamos la escena de manera asíncrona


            if (tiempo > 1)
            {
                tiempo = 1;
                if (loading)
                    SceneManager.LoadScene("CargaMedio");
            }
            blackFade.color = new Color(blackFade.color.r, blackFade.color.g, blackFade.color.b, loading ? tiempo : (1 - tiempo));
            titleTheme.volume = loading ? 1 - tiempo : tiempo;
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
        FindObjectOfType<CargaMedioScript>().escena = escena;
    }
    public void Cerrar()
    {
        Application.Quit();
    }

    public void SetCharacter(int dir)
    {

    }
    public void OpenMP3Folder()
    {
        //Hay que ver como obtener el directorio del juego
        string directorio = System.IO.Directory.GetCurrentDirectory();
        if (Application.isEditor)
            directorio = Application.dataPath;

        //No se por que las @ pero sin eso no funciona
        directorio.Replace(@"\",@"/");

        directorio += "/Resources/MP3Player/";
        print(directorio);

        //Ver si hay que cambiar algo con Mac o Linux

        Application.OpenURL("file:///"+directorio);
    }

}
