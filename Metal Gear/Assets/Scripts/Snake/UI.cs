using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class UI : MonoBehaviour
{
    public Snake snake;

    //-------- UI GENERAL
    public Image puntero, vidaBarra;
    public GameObject invArmas, invObj, vidaObj;

    //---------RECIBIR OBJETOS
    public TMP_Text textoObjetos; 
    public float textoTiempo = 0;

    //--------------INVENTARIO
    public Image armaLogo;
    public Image objetoLogo;
    public RectTransform balasRect;
    public TMP_Text textoArmas, textoBalasArm;
    public TMP_Text textoObjeto, textoCantObj;
    public GameObject armaAnt, armaPos;
    public GameObject objAnt, objPos;
    public TMP_Text tDesArm, tDesObj;


    //-----------------PRISMATICOS
    [HideInInspector]public float fov = 75; 
    [HideInInspector]public float maxZoom = 20;
    public GameObject UIbinoc, zoomIndicador;
    public TMP_Text textoZoom, textoAngle, textoPitch;

    //-----------EFECTOS
    public Image stunScreen;


    //---------RADAR
    [SerializeField]GameObject camaraRadar;
    [SerializeField] GameObject radarImg, jamImg, alertImg;
    [SerializeField] GameObject cautionImg;
    bool alerta = false;
    [SerializeField] TMP_Text tiempoTexto;


    //----------MENU DE PAUSA
    public GameObject pausaMenu;

    //--------------MENU DE INFORMACION
    public GameObject infMenu;
    public TMP_Text informTexto;


    // Start is called before the first frame update
    void Start()
    {
        //camaraRadar = GameObject.Find("CameraRadar");
        textoObjetos.text = "";
        //FindObjectOfType<MenuOpciones>().gameObject.SetActive(true);
        //FindObjectOfType<MenuOpciones>().Iniciar();
        //FindObjectOfType<MenuOpciones>().gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        //Escala la pantalla con la resolución
        float coefAspecto = Screen.currentResolution.width / Screen.currentResolution.height; coefAspecto = Mathf.Clamp01(coefAspecto);
        this.GetComponent<CanvasScaler>().matchWidthOrHeight = coefAspecto;
        


    }

    public void SetUI()
    {
        //UI

        int realInd = -1;
        if (snake.armaEnMano != null)
            realInd = snake.indArm;

        int realObjInd = -1;
        if (snake.objEnMano != null)
            realObjInd = snake.indObj;
        
        puntero.gameObject.SetActive(snake.GetButton("Apuntar") && snake.trueFPS && snake.arrast);
        vidaBarra.fillAmount = (snake.vida / 100);
        invArmas.SetActive(realInd >= 0);
        invObj.SetActive(realObjInd >= 0);
        //vidaObj.SetActive(vida < 100);

        if (textoTiempo > 0)
        {
            textoTiempo -= Time.deltaTime;
            if (textoTiempo < 0)
            {
                textoTiempo = 0;
                textoObjetos.gameObject.SetActive(false);
                textoObjetos.text = "";
            }
        }


        if (snake.binoc)
        {
            string angSt = (-snake.FPSrot.x).ToString();
            textoAngle.text = "ANGLE - " + angSt.Substring(0, Mathf.Min(5, angSt.Length));

            string pitchSt = snake.Rig.eulerAngles.y.ToString();
            textoPitch.text = "PITCH - " + pitchSt.Substring(0, Mathf.Min(5, pitchSt.Length));

            float valZoom = (fov - snake.Cam.GetComponent<Camera>().fieldOfView) / (fov - maxZoom);
            string zoomSt = (100 * valZoom).ToString();
            textoZoom.text = "ZOOM --- " + zoomSt.Substring(0, Mathf.Min(5, zoomSt.Length));

            zoomIndicador.transform.localScale = (new Vector3(11.64f, 6.86f, 6.86f) * (1 - valZoom)) + (new Vector3(5.16f, 3.1f, 3.1f) * valZoom);
        }
        if(stunScreen.color.a > 0)
        {
            stunScreen.color = new Color(1, 1, 1, Mathf.Max(stunScreen.color.a - (Time.deltaTime /2.5f), 0));
        }



        if (!alerta)
        {
            bool radActivo = snake.EsRadar();
            if (!radActivo && snake.GetTJamming() > 0)
            {
                snake.SetTJamming(snake.GetTJamming() - Time.deltaTime);
                if (snake.GetTJamming() <= 0)
                    snake.SetTJamming(-1);
            }

            radarImg.SetActive(radActivo);
            jamImg.SetActive(!radActivo);
            alertImg.SetActive(false);
            tiempoTexto.text = "";

        }
        else
        {
            radarImg.SetActive(false);
            jamImg.SetActive(false);
            alertImg.SetActive(true);

        }

    }
    private void LateUpdate()
    {
    }
    public void CamaraRadar()
    {

        camaraRadar.transform.position = new Vector3(snake.transform.position.x, camaraRadar.transform.position.y, snake.transform.position.z);
        camaraRadar.transform.rotation = Quaternion.Euler(0, snake.AnguloCam(), 0) * Quaternion.Euler(90, 0, 0);

    }

    public void CambiarBalasUI(int balasFront, int balas)
    {
        //Rect r = balasRect.rect;
        //r.width = 40 * balasFront[indArm];
        //r.x = 94 - (40 * balasFront[indArm]);

        if (balasFront >= 0)
        {
            textoBalasArm.text = (balasFront) + "/" + (balas - balasFront);
            balasRect.sizeDelta = new Vector2((15 * balasFront), 20f);
            int coef = (int)(balasRect.sizeDelta.x - 15) / 15;
            balasRect.anchoredPosition = new Vector2(94 - (7.5f * (coef - 1)), -58);
        }
        else
        {
            textoBalasArm.text = balas.ToString();
            balasRect.sizeDelta = new Vector2((15 * 0), 20f);
            int coef = (int)(balasRect.sizeDelta.x - 15) / 15;
            balasRect.anchoredPosition = new Vector2(94 - (7.5f * (coef - 1)), -58);
        }

    }


    public void SetCamaraRadarPlanes(float yIn, float yFin)
    {
        camaraRadar.transform.position = new Vector3(camaraRadar.transform.position.x, yIn, camaraRadar.transform.position.z); ;
        camaraRadar.GetComponent<Camera>().farClipPlane = yIn - yFin;
    }
    public void ActivaCaution()
    {
        cautionImg.SetActive(true);
    }

    public void DesactivaCaution()
    {
        cautionImg.SetActive(false);
    }

    public void Alerta(bool al)
    {
        alerta = al;
    }
    public void TAlerta(float t)
    {
        string tiempo = t.ToString(System.Globalization.CultureInfo.InvariantCulture);
        tiempo = tiempo.Substring(0, Mathf.Min(5, tiempo.Length));
        tiempoTexto.text = tiempo;
    }
    public void Despausa()
    {
        snake.Pausa();
    }

    public void Pausa(bool pausa)
    {
        pausaMenu.SetActive(pausa);
        if (pausa)
            pausaMenu.GetComponent<UI_Nav>().SelectDef();
    }

    public void AbreEscena(string nombre)
    {
        SceneManager.LoadScene(nombre);
    }

    public void ReloadEscena()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MenuInformacion(bool activado)
    {
        infMenu.SetActive(activado);
    }

    public void Informacion(string input, int modo = 0) // 0 = añade, 1 = reemplaza
    {
        if (modo == 0)
            informTexto.text += input;
        else if (modo == 1)
            informTexto.text = input;
    }
    public void RadarDesact()
    {
        radarImg.transform.parent.gameObject.SetActive(false);
    }

}
