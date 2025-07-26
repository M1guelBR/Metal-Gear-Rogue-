using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using UnityEngine.InputSystem.Users;

using UnityEngine.UI;
using System.Linq;
using UnityEngine.Rendering.Universal;
//using UnityEngine.Rendering;

public class Front
{
    public int front { get; set; }
    public List<int> listaIDs { get; set; }
}
public class Snake : MonoBehaviour
{
    //----------------------------COMPONENTES
    [Header("COMPONENTES")]
    //CharacterController controller;
    Rigidbody rb;
    public CapsuleCollider col;

    
    public PlayerInput playerInput;
    InputActionMap input;
    [HideInInspector] public Transform Cam;
    [SerializeField]Transform CamContainer;
    public UI interfaz;

    public Vector3 movement;
    public Transform Rig;
    [SerializeField] Animator snakeAnimator;
    [SerializeField] AnimacionAyuda animAy;

    public int playerID = 0;

    //----------------------------------- MOVIMIENTO BASE
    [Header("MOVIMIENTO BASE")]
    [Range(0, 5)] public float tiempoRot = 1f;

    [HideInInspector] public float vel;
    [Range(0, 5)] public float walkVel, sprVel, arrVel;
    float vMag;
    public LayerMask sueloLayers;

    //---------------------------------- CAMARA
    [Header("CAMARA")]
    [Range(0, 1)] public float sensitivity = 1f;
    public float dCam = 5f; float actualDistC;
    [SerializeField] Vector2 capCamX, capFPSX;
    Vector3 rot;
    public Vector3 FPSrot;
    int[] inversiones = new int[4] { 1, 1, 1, 1 };
    int TPSFov = 75, FPSFov = 75;


    [SerializeField] Vector3 rotCl;
    [SerializeField] float distCl;
    bool camClasica;
    public GameObject cabeza;
    bool FPS = false;
    [HideInInspector]public bool trueFPS;
    float tCam, tCamTarg;


    //--------------------------- AGACHARSE Y ARRASTRARSE
    [Header("AGACHARSE Y ARRASTRARSE")]
    public bool agach; public bool arrast;
    [Range(0, 1), SerializeField] float tiempoCtrlArr = 0.4f;
    float tiempoPressAg = 0; bool pressAg;
    int estadoInicial;
    [Range(0, 2), SerializeField] float alturaW, alturaC, alturaArr;
    bool levantaTiron = false, movAg = false;



    //-------------------------------------------- SUAVIZADO INPUT
    [Header("SUAVIZADO INPUT")]
    [Range(0, 6), SerializeField] float inputAcel;
    [Range(0, 6), SerializeField] float inputDecel;
    Vector3 dirS = Vector3.zero;

    //------------------------------------ ARMAS Y OBJETOS
    [Header("ARMAS Y OBJETOS")]
    [SerializeField] Transform pecho;
    [SerializeField] Transform brazoI;
    [SerializeField] Transform brazoD;
    float angX = 0;
    [SerializeField, Range(-1, 1)] float offY;

    public Arma armaEnMano;
    public ObjetoUsable objEnMano;

    public List<Arma> armasInv;

    public List<ObjetoUsable> objetosInv;
    List<int> balas = new List<int>(); List<Front> balasFront = new List<Front>();
    
    List<int> cantObjs = new List<int>();

    public int indArm = -1, indObj = -1;
    int antIndArm = -1, antIndObj = -1;
    [SerializeField] GameObject armaHolder;
    [SerializeField] GameObject objetoHolder;
    [SerializeField] GameObject partRef;
    [SerializeField] GameObject humoCig;
    float tiempoCadencia;
    bool puedeDisparar = true;
    float anguloArma = 0;
    [SerializeField] AudioClip clipCogerObj;
    [SerializeField] AudioClip clipCambioRapido;
    [SerializeField] AudioClip clipFull;
    [SerializeField] AudioClip clipCura, clipNVG;
    [SerializeField] LineRenderer trailBala;
    [SerializeField] ScriptableRendererFeature filtroTermico;
    [SerializeField] ScriptableRendererFeature filtroNVG;
    //[SerializeField] LayerMask layersDisp;


    //---------------------------SISTEMAS
    [Header("SISTEMAS")]
    [Range(0, 100)] public float vida = 100;
    float tInvArmas = 1, tInvObj = 1;
    float tiempoRondas = 0;
    bool miraArmas, miraObjetos;
    public bool caja;
    [HideInInspector]public bool binoc;
    [SerializeField] AudioSource sonidoLocal;
    [SerializeField] AudioClip abreInventario, mueveInventario;
    [SerializeField] InteraccionSnake colliderSnake;
    float dañoMult = 1;
    float tPared = 0.5f;
    public bool tienePist = false;
    bool grounded = false;

    //---------------------------MENUS
    bool pausa = false;
    bool informacion = false;


    //-------------------------RADAR
    [Header("RADAR")]
    GameObject radarImg;
    float tRadar = -1;


    //---------------CQC
    [Header("CQC")]
    bool CQC = false;
    float tiempoCQC = 0;
    float tiempoDetecCQC = 0.1f;
    bool Interroga = false;
    int punchCount;
    bool pega = true;
    bool entraCQC = false;


    //------------------SONIDO
    [Header("SONIDO")]
    [SerializeField] SphereCollider sonidoCollider;
    bool paso = false;
    [SerializeField] AudioClip pasoClip;
    [SerializeField] AudioClip scream;
    [SerializeField] AudioClip sonidoClear;

    //--------------------MODELO DE PERSONAJE
    [SerializeField] SkinnedMeshRenderer playerModel;

    //---------------MODELO BIG BOSS
    [SerializeField] Mesh NakedSnakeMesh;
    [SerializeField] Material NakedSnakeMat;

    bool mouse; bool morir = false;


    private void Awake()
    {
        //CamContainer = GameObject.Find("CamContainer").transform;
        Cam = CamContainer.GetChild(0);
        //FindObjectOfType<SettingsManager>().LoadAll();
    }

    // Start is called before the first frame update
    void Start()
    {

        FindObjectOfType<GameManager>().jugadores.Add(this);

        filtroTermico.SetActive(false);
        filtroNVG.SetActive(false);
        interfaz.snake = this;

        if (PlayerPrefs.GetInt("NSnake", -1) == 1)
        {
            playerModel.sharedMesh = NakedSnakeMesh;
            playerModel.sharedMaterial = NakedSnakeMat;
            interfaz.RadarDesact();
        }

        tiempoPressAg = tiempoCtrlArr;
        actualDistC = dCam;

        //controller = this.GetComponent<CharacterController>();
        rb = this.GetComponent<Rigidbody>();
        //controller.height = alturaW;
        
        if(playerInput == null)
            playerInput = this.GetComponent<PlayerInput>();

        input = playerInput.currentActionMap;
        playerID = playerInput.splitScreenIndex;



        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rot = CamContainer.eulerAngles;
        Cam.localPosition = new Vector3(0, 0, dCam);


        armasInv.Insert(0, Resources.Load<Arma>("Armas/EMPTY"));
        objetosInv.Insert(0, Resources.Load<ObjetoUsable>("Objetos/EMPTY"));

        //Las balas del emptyObject
        balas.Add(0);
        Front front = new Front();
        balasFront.Add(front);
        cantObjs.Add(0);


        //SetArma();
        if (objetosInv.Count > 1)
        {
            indObj = 1;
            antIndObj = 1;
            for (int i = 1; i < objetosInv.Count; i++)
                cantObjs.Add(1);

        }
        if (armasInv.Count > 1)
        {
            indArm = 1;
            antIndArm = 1;
            for (int i = 1; i < armasInv.Count; i++)
            {
                balas.Add(armasInv[i].balasArma());
                Front front_ = new Front(); front_.front = armasInv[i].balasArma();
                balasFront.Add(front_);

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (GetButtonDown("MouseToggle"))
        {
            mouse = !mouse;
            Cursor.lockState = mouse ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = mouse;
        }

        if (morir)
        {
            if (snakeAnimator.GetCurrentAnimatorStateInfo(0).IsName("Morir"))
            {
                return;
            }
            else if (snakeAnimator.GetCurrentAnimatorStateInfo(0).IsName("Muerto"))
            {
                //Quitar jug de GameManager y borrar el script de snake
                FindObjectOfType<GameManager>().MataJugador(this, true);
                Destroy(this, 0);
            }
        }
        //Time.timeScale = timeSc;
        if (tCam != tCamTarg)
            tCam = Mathf.MoveTowards(tCam, tCamTarg, 3 *Time.deltaTime);


        //Sonido pasos

        vMag = new Vector2(rb.velocity.x, rb.velocity.z).magnitude;

        if (HashInHashes("Standing_CQCStand_ParedIzq_ParedDer", snakeAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash) && vMag > 0)
        {
            float t = snakeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            t -= (int)t;

            //print(t);

            //Primer paso
            if (t > 0.01f && t < 0.5f && !paso)
            {
                paso = true;
                SonidoRadio(2.5f * vMag / sprVel);
                this.GetComponent<AudioSource>().PlayOneShot(pasoClip, vMag * .05f / sprVel);

            }
            else if (t > 0.5f && paso)
            {
                paso = false;
                SonidoRadio(2.5f * vMag / sprVel);
                sonidoLocal.PlayOneShot(pasoClip, vMag * .05f / sprVel);
            }

        }
        else
        {
            paso = false;
        }

        if (!pausa)
        {

            snakeAnimator.SetFloat("Velocidad", vMag);
            snakeAnimator.SetFloat("FPS", FPS ? 1 : 0);
            snakeAnimator.SetBool("Agachado", agach); snakeAnimator.SetBool("Arrastrado", arrast);
            snakeAnimator.SetBool("Caja", caja);
            snakeAnimator.SetBool("CQC", CQC);
            colliderSnake.comprueba = tiempoCQC == 0;
            if (snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("CQCThrow"))
                snakeAnimator.SetBool("Throw", false);


        }
        if (tiempoCadencia > 0)
        {
            tiempoCadencia -= Time.deltaTime;
            if (tiempoCadencia < 0)
            {
                partRef.SetActive(false);
                tiempoCadencia = 0;
            }
        }
        if (snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("SinApuntar"))
            snakeAnimator.SetBool("ArmaLista", GetButton("Apuntar") && armaEnMano != null && !caja && !binoc && !miraArmas && !miraObjetos);
        else if (snakeAnimator.GetBool("ArmaLista") && (!GetButton("Apuntar") || armaEnMano == null || caja || binoc || miraArmas || miraObjetos))
        {
            snakeAnimator.SetBool("ArmaLista", false);
        }

        snakeAnimator.SetInteger("Balas", armaEnMano != null ? (balasFront[indArm].front) : 0);
        if (snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("Disparar") || snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("Recargar"))
            snakeAnimator.SetBool("ArmaLista", true);


        if (armaEnMano != null && armaEnMano.TipoObjeto() >= 1)
        {

            bool condicionBalas = tiempoCadencia <= .1f && puedeDisparar && balas[indArm] > 0;
            bool condicionRecarga = (snakeAnimator.GetBool("Disparar") && snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("Recargar"));
            bool condicionCQC = !CQC || (CQC && GetButton("Apuntar"));

            snakeAnimator.SetBool("Disparar", (GetButton("Disparar") && condicionBalas && tiempoRondas <= 0.1f && condicionCQC) || condicionRecarga );
            if (!condicionCQC && GetButton("Disparar"))
            {
                tiempoCadencia = Mathf.Max(1 / armaEnMano.Cadencia(), 1/5.75f); 
                snakeAnimator.SetBool("Disparar", false);
            }

            if (armaEnMano.TipoObjeto() == 2)
                anguloArma = 10f;
            else if (armaEnMano.TipoObjeto() == 1)
                anguloArma = 0;

        }

        else if (armaEnMano != null && armaEnMano.TipoObjeto() == 0 && tiempoCadencia == 0)
        {

            bool cuentaBalas = (armaEnMano.granada().remote == false) || (armaEnMano.granada().remote == true && GetButton("Apuntar"));
            bool condicionBalas = tiempoCadencia <= .1f && puedeDisparar && (cuentaBalas && balas[indArm] > 0) == cuentaBalas;
            bool condicionCQC = !CQC || (CQC && GetButton("Apuntar"));

            snakeAnimator.SetBool("Disparar", (GetButton("Disparar") && condicionBalas && condicionCQC)); 
            if (!condicionCQC && GetButton("Disparar"))
            {
                tiempoCadencia = 1/5.75f;
                snakeAnimator.SetBool("Disparar", false);
            }
        }


        snakeAnimator.SetFloat("ArmaTipo", (armaEnMano != null && !caja && !binoc && !miraArmas && !miraObjetos) ? (armaEnMano.TipoObjeto() >= 1 ? armaEnMano.TipoObjeto() - 1 : .5f) : -1);
        if(armaEnMano != null)
            snakeAnimator.SetFloat("ArmaTipo", snakeAnimator.GetFloat("ArmaTipo") + armaEnMano.remoto());


        if (tiempoCQC > 0 && snakeAnimator.GetFloat("ArmaTipo") == 0)
            snakeAnimator.SetFloat("ArmaTipo", .75f);

        //Animaciones c4 plantar y detonar


        if (!GetButton("Disparar") && !puedeDisparar && (armaEnMano.TipoObjeto() >= 1 || (armaEnMano.TipoObjeto() == 0 && !puedeDisparar && !snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("Disparar"))))
        {

            puedeDisparar = true;
        }





        int realInd = -1;
        if (armaEnMano != null)
            realInd = indArm;

        int realObjInd = -1;
        if (objEnMano != null)
            realObjInd = indObj;
        //Inventario
        {
            if (!snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("Disparar"))
            {

                //Hacer el dibujado de balas de los arrojadizos

                if (GetButton("InvArmas") && tInvArmas > 0 && !binoc && !caja && !miraObjetos && indArm != -1 && armasInv.Count > 1)
                {
                    tInvArmas -= Time.deltaTime;
                    if (tInvArmas < 0)
                    {
                        tInvArmas = 0;
                        miraArmas = true;
                        sonidoLocal.PlayOneShot(abreInventario);
                        interfaz.armaAnt.SetActive(true); interfaz.armaPos.SetActive(true);
                        interfaz.vidaObj.SetActive(true);
                    }
                    else if (tInvArmas < 0)
                        tInvArmas = 0;
                }

                else if (!GetButton("InvArmas") && indArm != -1 && tInvArmas != 1)
                {
                    SalirInvArma(realInd);
                }

                else if(GetButton("InvObjetos") && tInvObj > 0 && !miraArmas && indObj != -1 && objetosInv.Count > 1)
                {
                    tInvObj -= Time.deltaTime;
                    if (tInvObj < 0)
                    {
                        tInvObj = 0;
                        miraObjetos = true;
                        sonidoLocal.PlayOneShot(abreInventario);
                        interfaz.objAnt.SetActive(true); interfaz.objPos.SetActive(true);
                        interfaz.vidaObj.SetActive(true);
                    }
                }
                else if (!GetButton("InvObjetos") && indObj != -1 && tInvObj != 1)
                {
                    SalirInvObj(realObjInd);
                }




                if (miraArmas)
                {
                    int movimiento = (GetButtonDown("InvDer") ? 1 : 0) + (GetButtonDown("InvIzq") ? -1 : 0);
                    if (movimiento != 0)
                    {
                        sonidoLocal.PlayOneShot(mueveInventario);
                        brazoD.localRotation = Quaternion.Euler(0, 90f, 188.7f);
                        brazoI.localRotation = Quaternion.Euler(0, 90f, 188.7f);

                    }
                    indArm += movimiento; indArm = (indArm % armasInv.Count + armasInv.Count) % armasInv.Count;

                    int indAnt = ((indArm - 1) % armasInv.Count + armasInv.Count) % armasInv.Count;
                    int indPos = ((indArm + 1) % armasInv.Count + armasInv.Count) % armasInv.Count;

                    interfaz.armaAnt.GetComponent<Image>().sprite = armasInv[indAnt].UIArma; interfaz.armaPos.GetComponent<Image>().sprite = armasInv[indPos].UIArma;

                    armaEnMano = armasInv[indArm];
                    SetArma();


                }
                else if (miraObjetos)
                {
                    int movimiento = -(GetButtonDown("InvDer") ? 1 : 0) - (GetButtonDown("InvIzq") ? -1 : 0);
                    if (movimiento != 0)
                        sonidoLocal.PlayOneShot(mueveInventario);
                    indObj += movimiento; indObj = (indObj % objetosInv.Count + objetosInv.Count) % objetosInv.Count;

                    int indAnt = ((indObj - 1) % objetosInv.Count + objetosInv.Count) % objetosInv.Count;
                    int indPos = ((indObj + 1) % objetosInv.Count + objetosInv.Count) % objetosInv.Count;

                    interfaz.objAnt.GetComponent<Image>().sprite = objetosInv[indAnt].UIObj; interfaz.objPos.GetComponent<Image>().sprite = objetosInv[indPos].UIObj;

                    objEnMano = objetosInv[indObj];
                    SetObjeto();

                    if (objEnMano.nombre == "RATION" && GetButtonDown("Interactuar"))
                        UsaObjeto(objEnMano);


                }

            }

            //OBJETOS Y VIDA
            

            if(objEnMano != null && objEnMano.nombre == "CIGS" && !miraObjetos)
            {
                vida = Mathf.Clamp(vida - (1.5f * Time.deltaTime), 0, 100);

                var partHumo = humoCig.GetComponent<ParticleSystem>().velocityOverLifetime;
                partHumo.z = (vMag / sprVel) * -0.25f;


            }

        }



        interfaz.SetUI();


        //Debug.DrawLine(cabeza.transform.position, cabeza.transform.position + (cabeza.transform.forward * 0.15f) + (Vector3.up * 0.15f), Color.cyan);

        if (GetButtonDown("Escape") && !pausa)
            Pausa();

        if (pausa)
            Cursor.lockState = CursorLockMode.None;


        if (GetButtonDown("Info"))
            Informacion();

    }

    bool HashInHashes(string aHashear, int hashB)
    {
        string[] cadaNombre = aHashear.Split("_");
        int[] hashes = new int[cadaNombre.Length];
        for(int i = 0; i < cadaNombre.Length; i++)
        {
            hashes[i] = Animator.StringToHash(cadaNombre[i]);
        }


        return hashes.Contains<int>(hashB);
    }
    private void LateUpdate()
    {
        //Camara
        {
            if (GetButtonDown("PrimeraPersona") && !binoc)
            {
                FPS = !FPS;
                if (FPS)
                {
                    CamAPrimera();
                }
                else
                {
                    CamATercera();
                }
            }

            if (!FPS && !binoc)
            {
                if (GetButtonDown("CamToggle"))
                {
                    camClasica = !camClasica;
                    AjustaCamaraTercera();
                }


                CamContainer.position = Vector3.Lerp(transform.position,
                    Rig.position + (col.center.y / (Rig.localScale.y * transform.localScale.y)) * Vector3.up, tCam);

                if (!camClasica)
                {
                    rot.x += (GetAxis("Mouse Y") + (GetAxis("Cam Y") * Time.deltaTime)) * sensitivity * inversiones[1];
                    float cap = Mathf.Lerp(capCamX.x, -14, tCam);
                    rot.x = Mathf.Clamp(rot.x, cap, capCamX.y);

                    rot.y += (GetAxis("Mouse X") + (GetAxis("Cam X") * Time.deltaTime)) * sensitivity * inversiones[0];
                }
                CamContainer.rotation = Quaternion.Euler(rot);
                AjustaCamDist();

                float tempF = Cam.GetComponent<Camera>().fieldOfView;
                bool boolFOV = CQC || snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("CQCThrow") || snakeAnimator.GetNextAnimatorStateInfo(1).IsName("CQCThrow") ||
                    (punchCount != 0 && colliderSnake.masProximo != null && colliderSnake.masProximo.name != "InteraccionSoldadoInc");
                tempF = Mathf.MoveTowards(tempF, boolFOV ? 60 : TPSFov, 100 * Time.deltaTime);
                Cam.GetComponent<Camera>().fieldOfView = tempF;
                Cam.GetChild(0).GetComponent<Camera>().fieldOfView = tempF;

            }
            else if (!trueFPS && !binoc)
            {
                CamContainer.position = transform.position;
                Vector3 posT = cabeza.transform.position + (.1f * cabeza.transform.up);
                float traslacion = (posT - Cam.position).magnitude;
                traslacion = Mathf.Max(0, traslacion - (3 * Time.deltaTime));
                print(traslacion);
                Cam.position = posT - (cabeza.transform.forward * traslacion);
                if (Cam.position == posT)
                    trueFPS = true;

            }
            else if (trueFPS || binoc)
            {
                Cam.position = cabeza.transform.position + (0.1f * cabeza.transform.up);
            }
        }

        if (morir)
            return;


        interfaz.CamaraRadar();
        if (caja)
            return;

        //Choque paredes
        {
            if (arrast)
            {
                RaycastHit hitArrP;
                bool choqueFrontal = false;
                Vector3 posR = transform.position + Vector3.Scale(transform.localScale, col.center);
                float long_ = .7f;

                if (Physics.Raycast(posR, Rig.forward, out hitArrP, long_, sueloLayers))
                {
                    choqueFrontal = true;
                }
                if (Physics.Raycast(posR, -Rig.forward, out hitArrP, long_, sueloLayers))
                {
                    if (choqueFrontal && PosibleCambioPos(alturaC))
                        arrast = false;

                }

            }
        }

        //Armas de fuego y tomar rehenes
        {
            if (armaEnMano != null&& !binoc && !miraArmas && !miraObjetos)
            {


                if (GetButton("Apuntar") || snakeAnimator.GetBool("Disparar") || snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("Disparar")
                    || snakeAnimator.GetBool("Recargar") || snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("Recargar"))
                {

                    Vector3 proyBr = brazoD.right - (cabeza.transform.right * Vector3.Dot(brazoD.right, cabeza.transform.right));


                    angX = -Vector3.SignedAngle(cabeza.transform.forward, proyBr, cabeza.transform.right);
                    angX += anguloArma;

                    if (armaEnMano.TipoObjeto() == 0 && arrast)
                        angX -= 90;


                    float dY = Mathf.Max(-.01f, Mathf.Min(0.1f, cabeza.transform.position.y - brazoI.position.y));
                    //print(dY);


                    float t = Mathf.Max((0.01f - dY) / 0.01f, 0); t = Mathf.Clamp(t, 0, 1);
                    //print(t);
                    offY = Mathf.Lerp(0.95f, 0.85f, t);

                    angX *= offY;



                    //Debug.DrawRay(brazoI.position, brazoI.up, Color.red);
                    //Debug.DrawRay(brazoI.position, proyBr, Color.green);
                    //Debug.DrawRay(brazoI.position, cabeza.transform.forward, Color.green);

                    if (armaEnMano.TipoObjeto()>=1)
                        brazoI.RotateAround(brazoI.position, pecho.right, angX * (animAy.apuntarCant));

                    brazoD.RotateAround(brazoD.position, pecho.right, angX * (animAy.apuntarCant));
                }
                else if (brazoD.localRotation != Quaternion.Euler(0, 90f, 188.7f))
                {
                    brazoD.localRotation = Quaternion.Euler(0, 90f, 188.7f);
                    brazoI.localRotation = Quaternion.Euler(0, 90f, 188.7f);
                }


                if (armaEnMano.TipoObjeto() >= 1)
                {
                    DisparaArmaFuego(armaEnMano.pistolaFusil());
                    if (GetButton("Apuntar"))
                    {
                        RaycastHit rayoSoldadoRehen;
                        LayerMask layersRehen = (1<<LayerMask.NameToLayer("Interaccion")) | (1<<LayerMask.NameToLayer("EscenarioColliders"));
                        if(Physics.Raycast(transform.position, cabeza.transform.forward, out rayoSoldadoRehen, .3f,  layersRehen, QueryTriggerInteraction.Collide))
                        {
                            //Si es un collider que nos interesa
                            if(rayoSoldadoRehen.collider.name != "InteraccionSoldadoInc" && rayoSoldadoRehen.collider.tag == "SoldCol")
                            {
                                bool sonido = rayoSoldadoRehen.collider.transform.parent.parent.GetComponent<Soldier>().Rehen(transform.position + col.center, agach);
                                if (sonido) 
                                    this.GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("Audio/Snake/Freeze"), .375f);
                            }
                        }
                    }
                }

                //Hacer c4
                else if (armaEnMano.TipoObjeto() == 0)
                    DispararExplosivo(armaEnMano.granada());

            }
            else if (brazoD.localRotation != Quaternion.Euler(0, 90f, 188.7f))
            {
                brazoD.localRotation = Quaternion.Euler(0, 90f, 188.7f);
                brazoI.localRotation = Quaternion.Euler(0, 90f, 188.7f);
            }

        }

        //Punch Punch Kick
        {
            bool condicionNombre = snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("Punch1") || snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("Punch2") || snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("Kick");


            if (armaEnMano == null && GetButtonDown("Disparar") && punchCount < 3 && !CQC && !binoc && !miraArmas && !miraObjetos)
            {
                punchCount += 1;
                


                if ((agach || colliderSnake.soldadoCQC == null) && punchCount > 1)
                {
                    punchCount = 1;
                }
                else if (colliderSnake.masProximo != null && colliderSnake.masProximo.name == "InteraccionSoldadoInc" && punchCount > 0)
                {
                    if (!agach)
                    {
                        punchCount = 3;
                        snakeAnimator.Play("ArmasLayer.Kick", 1, 0);

                        if (pega)
                        {
                            int i = Random.Range(1, 4);
                            this.GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("Audio/Snake/Golpe" + i.ToString()), .35f);
                            pega = false;

                        }
                    }

                    if (colliderSnake.masProximo.transform.parent.parent.GetComponent<Soldier>().EstaVivo())
                        colliderSnake.masProximo.transform.parent.parent.GetComponent<Soldier>().BajaOxigeno(-100);
                }

                if (colliderSnake.soldadoCQC != null)
                {
                    Rig.forward = Vector3.Scale(colliderSnake.soldadoCQC.transform.position - transform.position, new Vector3(1, 0, 1));
                    

                }
                if (punchCount == 1)
                    snakeAnimator.Play("ArmasLayer.Punch1", 1, 0);
            }
            if (snakeAnimator.GetNextAnimatorStateInfo(1).IsName("SinApuntar") && !snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("SinApuntar") && punchCount > 0)
            {
                punchCount = 0;
            }

            if ((snakeAnimator.GetCurrentAnimatorStateInfo(1).normalizedTime > 0.8f || !condicionNombre) && !pega)
                pega = true;

            snakeAnimator.SetInteger("PunchType", punchCount);


            if(punchCount > 0 && colliderSnake.soldadoCQC != null)
            {
                if (snakeAnimator.GetCurrentAnimatorStateInfo(1).normalizedTime < 0.15f && pega && condicionNombre)
                {
                    colliderSnake.soldadoCQC.QuitaVida(.5f, !snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("Kick"));
                    colliderSnake.soldadoCQC.BuscaJug(this);
                    int i = Random.Range(1, 4);
                    this.GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("Audio/Snake/Golpe" + i.ToString()), .35f);
                    pega = false;
                }

            }
            else if(punchCount > 1 && colliderSnake.soldadoCQC == null)
            {
                punchCount = 0;
            }


            if (snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("Kick") && colliderSnake.soldadoCQC != null)
            {
                Vector3 pos = transform.position + (Rig.forward * .1f) + (Vector3.down * .1f) - (Rig.right * 0.1f);
                SetSoldadoInc(colliderSnake.soldadoCQC, colliderSnake.masProximo, pos, Rig.forward, true, 20);
            }
        }


        //CQC
        {

            bool enCondiciones = colliderSnake.soldadoCQC != null && GetButton("Interactuar");
            bool axis = GetAxis("Horizontal") != 0 || GetAxis("Vertical") != 0;
            if (!entraCQC && colliderSnake.soldadoCQC != null && GetButtonDown("Interactuar"))
            {
                entraCQC = true;
            }

            if (enCondiciones && tiempoDetecCQC > 0 && tiempoCQC < 1 && entraCQC)
            {
                if (snakeAnimator.GetFloat("ArmaTipo") >= 1)
                {
                    armaEnMano = null;
                    SetArma();
                }
                if (GetButtonDown("Interactuar"))
                {
                    tiempoDetecCQC = 0.1f;
                    tiempoCQC = .99f;
                    colliderSnake.soldadoCQC.BajaOxigeno(20);
                    colliderSnake.soldadoCQC.SonidoAgarre();

                    if (!colliderSnake.soldadoCQC.Consciente())
                    {
                        SetSoldadoInc(colliderSnake.soldadoCQC, colliderSnake.masProximo, colliderSnake.soldadoCQC.transform.position, Rig.forward, false, 100, false);
                        CQC = false;
                        tiempoCQC = 0;
                        tiempoDetecCQC = 0;
                        return;
                    }

                }

                print("decide");

                //colliderSnake.soldadoCQC.para = true;
                colliderSnake.soldadoCQC.SetPPKT(1);
                tiempoDetecCQC -= Time.deltaTime;

                if (tiempoDetecCQC <= 0 && !axis)
                {
                    colliderSnake.soldadoCQC.SetPPKT(0);
                    //colliderSnake.soldadoCQC.para = false;
                    tiempoDetecCQC = 0.1f;
                    tiempoCQC = 1;
                }

                else if (tiempoDetecCQC <= 0 && axis)
                {
                    colliderSnake.soldadoCQC.SetPPKT(0);
                    //colliderSnake.soldadoCQC.para = false;
                    tiempoCQC = 0;
                    tiempoDetecCQC = 0.1f;
                    CQC = false;
                    CQCThrow();
                    entraCQC = false;
                }
            }
            else if (!enCondiciones && tiempoDetecCQC < 0.1f)
            {
                tiempoDetecCQC = 0.1f;
            }

            else if ((!GetButton("Interactuar")) && !GetButton("Recargar") && tiempoCQC > 0 && colliderSnake.soldadoCQC != null)
            {
                tiempoCQC -= Time.deltaTime * 4f;
                if (tiempoCQC < 0)
                {
                    tiempoCQC = 0;
                    tiempoDetecCQC = 0.1f;
                }
            }

            if (CQC && GetButtonDown("Recargar") && !GetButton("Apuntar") && tiempoCQC <= 1)
            {
                tiempoCQC = 1;
                //Interrogar soldado
                int i = Random.Range(1, 4);
                this.GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("Audio/Snake/Interroga" + i.ToString()));
                SonidoRadio(1.5f);
                colliderSnake.soldadoCQC.EmpiezaInterroga(this);
            }
            else if (CQC && GetButtonDown("Disparar") && !GetButton("Apuntar") && tiempoCQC <= 1)
            {
                colliderSnake.soldadoCQC.QuitaVida(100);
            }


            //Agarra soldado
            if (tiempoCQC > 0f && colliderSnake.soldadoCQC != null && colliderSnake.soldadoCQC.col.activeInHierarchy)
            {
                arrast = false;
                colliderSnake.soldadoCQC.AgarreSoldado(this);
            }

            //Libera soldado
            else if (colliderSnake.soldadoCQC != null && tiempoCQC == 0 && colliderSnake.comprueba == false)
            {
                Libera();
            }
            if (colliderSnake.soldadoCQC == null && tiempoCQC != 0)
                tiempoCQC = 0;

            CQC = tiempoCQC > 0f;
            Interroga = CQC && (!GetButton("Apuntar") && GetButton("Recargar"));

            if (CQC)
            {
                Vector3 pos = transform.position + (Rig.forward * .1f) + (Vector3.down * .1f) + (Rig.right * -.1f);
                colliderSnake.soldadoCQC.DatosCQC(Rig.forward, agach, pos, true, vMag);
            }
        }



        if (miraObjetos && GetButtonDown("Disparar"))
        {
            DropObjeto(objEnMano, -1);
        }
        if (miraObjetos && GetButtonDown("Apuntar"))
        {
            DropObjeto(objEnMano, 1, false);
        }
        else if (miraArmas && GetButtonDown("Disparar"))
        {
            DropObjeto(armaEnMano, -1);
        }
        else if (miraArmas && GetButtonDown("Apuntar"))
        {
            DropObjeto(armaEnMano, (int)armaEnMano.balasArma() / 2 , false);
        }


    }

    bool isGrounded()
    {
        return grounded;
    }

    private void FixedUpdate()
    {
        if (morir)
            return;
        Movimiento(Time.fixedDeltaTime);

        //Layer Collider

        if (GetButtonDown("Interactuar"))
        {
            colliderSnake.gameObject.tag = "JugColButDown";
        }
        else if (GetButton("Interactuar"))
        {
            colliderSnake.gameObject.tag = "JugColButton";
        }
        else if (GetButtonUp("Interactuar"))
        {
            colliderSnake.gameObject.tag = "JugColButUp";
        }
        else
        {
            colliderSnake.gameObject.tag = "JugCol";
        }





        //El collider de sonido espera 1 frame hasta ser desactivado
        if (sonidoCollider.enabled && sonidoCollider.name.Contains("_"))
        {
            sonidoCollider.name = sonidoCollider.name.Substring(0, sonidoCollider.name.Length - 1);
            sonidoCollider.enabled = false;
        }

        else if (sonidoCollider.enabled && !sonidoCollider.name.Contains("_"))
        {
            sonidoCollider.name += "_";
        }

        if (trueFPS || binoc)
        {
            FPSrot.x -= (GetAxis("Mouse Y") + (GetAxis("Cam Y") * Time.deltaTime)) * sensitivity * inversiones[3];

            FPSrot.x = Mathf.Clamp(FPSrot.x, capFPSX.x, capFPSX.y);

            FPSrot.y += (GetAxis("Mouse X") + (GetAxis("Cam X") * Time.deltaTime)) * sensitivity * inversiones[2];

            if (tPared > 0)
            {
                cabeza.transform.eulerAngles = FPSrot;
                Cam.eulerAngles = FPSrot;
            }
            else
            {
                cabeza.transform.eulerAngles = FPSrot + (180 * Vector3.up);
                Cam.eulerAngles = FPSrot + (180 * Vector3.up);
            }
            Rig.eulerAngles = new Vector3(0, FPSrot.y, 0);

            if (binoc)
            {
                Cam.GetComponent<Camera>().fieldOfView += ((GetButton("Disparar") ? 1 : 0) - (GetButton("Apuntar") ? 1 : 0)) * Time.deltaTime * (interfaz.maxZoom - interfaz.fov);
                Cam.GetComponent<Camera>().fieldOfView = Mathf.Clamp(Cam.GetComponent<Camera>().fieldOfView, interfaz.maxZoom, interfaz.fov);
                Cam.GetChild(0).GetComponent<Camera>().fieldOfView = Cam.GetComponent<Camera>().fieldOfView;
            }
        }




    }
    //Calculo movimiento
    void Movimiento(float deltaTime)
    {
        
        bool lockMove = ((miraObjetos && indObj != -1) || (miraArmas && indArm != -1)) || binoc || tiempoDetecCQC < 0.1f || Interroga;
        Vector3 dir = new Vector3(GetAxis("Horizontal"), 0, GetAxis("Vertical")) * (lockMove ? 0 : 1);
        //Te puedes mover en primera persona
        dir = Vector3.ClampMagnitude(dir, 1);
        if (tPared == 0 && trueFPS)
            dir.x = -dir.x;
        {
            if (GetButton("Walk"))
            {
                dir = dir.normalized * 0.5f;
            }
            dirS = Vector3.MoveTowards(dirS, dir, (dir.magnitude > dirS.magnitude ? inputAcel : inputDecel) * deltaTime);

        }

        //Control de Agacharse
        if (!caja)
        {
            if (GetButton("Agach") && !pressAg && tiempoPressAg == tiempoCtrlArr)
            {
                pressAg = true;
                estadoInicial = (agach ? 1 : 0) + (arrast ? 1 : 0);
                if (!agach || arrast)
                    CambiaPostura();

            }
            if (movAg && tiempoPressAg > 0 && dir == Vector3.zero && agach && !arrast)
            {
                movAg = false;
                pressAg = false;
                tiempoPressAg = tiempoCtrlArr;
            }
            if (!GetButton("Agach") && pressAg && !(agach && !arrast && dir != Vector3.zero))
            {
                if (tiempoPressAg > 0 && estadoInicial == 1 && !snakeAnimator.GetBool("Pared"))
                {
                    CambiaPostura();
                }

                pressAg = false;

                tiempoPressAg = tiempoCtrlArr;
            }

            if (pressAg && tiempoPressAg >= 0)
            {

                tiempoPressAg -= deltaTime;


                if (tiempoPressAg < 0)
                {
                    movAg = false;
                    CambiaPostura();
                }
            }
        }


        //Movimiento Relativo a Camara
        Vector3 f = Cam.forward; Vector3 r = Cam.right;
        Vector3 a = new Vector3(f.x, 0, f.z).normalized;
        if (a == Vector3.zero)
        {
            a = Cam.up;
        }

        if (FPS)
        {
            a = Rig.forward;
            r = Rig.right;
        }


        movement = dirS.x * r + dirS.z * a;

        //return;
        //Velocidad
        velocidad();

        //Animaciones
        //vMag = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;


        //Controla Rotacion y moverse arrast
        if (dir != Vector3.zero)
        {
            if (!FPS && !snakeAnimator.GetBool("Pared"))
                RotaRig();

            //else if (snakeAnimator.GetBool("Pared"))
            //RotaRig(false);

            if (agach && !arrast && (pressAg == false || levantaTiron == true))
            {
                pressAg = true; estadoInicial = 1 + (levantaTiron ? 1 : 0);
                movAg = true;
            }

        }


        //Movimiento Y
        if (snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("CQCThrow") || snakeAnimator.GetNextAnimatorStateInfo(1).IsName("CQCThrow"))
            movement = Vector3.zero;
        else
            movement *= vel;


        if (snakeAnimator.GetCurrentAnimatorStateInfo(0).IsName("Tirarse"))
        {
            //print(controller.velocity);
            //Debug.Break();

            //if (controller.isGrounded)
            movement = rb.velocity;
            if (isGrounded())
            {
                snakeAnimator.SetBool("Grounded", true);
                movement = Vector3.zero;
            }
            if (movement.x != 0 || movement.z != 0)
                RotaRig(false);
        }




        //Pegarse a paredes
        {

            RaycastHit paredR = new RaycastHit();
            Vector3 noYMov = new Vector3(movement.x, 0, movement.z);
            if (dir != Vector3.zero && Physics.Raycast(transform.position, noYMov, out paredR, .25f, sueloLayers))
            {

                RaycastHit lat1, lat2;

                Vector3 pos1 = Vector3.zero, pos2 = Vector3.zero, normPared = Vector3.zero;
                Vector3 dirLatR = Vector3.zero;
                normPared = paredR.normal; normPared.y = 0;

                dirLatR = Vector3.Cross(Vector3.up, normPared);

                //Comprobamos los laterales para ver si se va
                if (Physics.Raycast(transform.position + (col.radius * transform.localScale.x * dirLatR), noYMov, out lat1, .25f, sueloLayers))
                {
                    pos1 = lat1.point;
                    //Debug.DrawRay(pos1, lat1.normal, Color.green);
                }
                else
                    dir = Vector3.zero;
                if (Physics.Raycast(transform.position - (col.radius * transform.localScale.x * dirLatR), noYMov, out lat2, .25f, sueloLayers))
                {
                    pos2 = lat2.point;
                    //Debug.DrawRay(pos2, lat2.normal, Color.green);
                }

                else
                    dir = Vector3.zero;


                if (tPared > 0)
                {
                    tPared -= deltaTime;
                    if (tPared < 0)
                        tPared = 0;
                }

                if (tPared == 0)
                {
                    col.material.dynamicFriction = 0;
                    col.material.staticFriction = 0;
                    snakeAnimator.SetBool("Pared", true);
                    //Hacer que haga "espalda con espalda" con la pared

                    if (pos1 == Vector3.zero) pos1 = paredR.point;
                    if (pos2 == Vector3.zero) pos2 = paredR.point;


                    Vector3 normalOpt = normPared - Vector3.Project(normPared, pos1 - paredR.point)
                        - Vector3.Project(normPared, paredR.point - pos2);
                    normalOpt.y = 0;
                    //Debug.DrawRay(transform.position, normalOpt);

                    Rig.forward = -normalOpt;
                    col.radius = 0.05f;

                    //Hacer que los movimientos leves no se procesen ARREGLAR
                    Vector3 movOrt = movement - Vector3.Project(movement, normalOpt);
                    Debug.DrawRay(transform.position, movOrt, Color.red);
                    movement -= movOrt;

                    float escala = arrVel * 0.75f;
                    if (movOrt.magnitude < arrVel / 2)
                        escala = 0;

                    movement += movOrt.normalized * escala;

                    //Debug.DrawRay(transform.position, new Vector3(movement.x, 0, movement.z), Color.red);

                }
            }

            else if (tPared != 0.5f && !(tPared == 0 && Physics.Raycast(pecho.position, -Rig.forward, .35f, sueloLayers)))
            {
                if (tPared == 0)
                {
                    col.material.dynamicFriction = 0.6f;
                    col.material.staticFriction = 0.6f;
                    if (dir == Vector3.zero && !trueFPS)
                    {
                        Rig.Rotate(0, 180, 0);
                    }

                    //Rota solo si se ha pegado
                    else if (trueFPS)
                        FPSrot += Vector3.up * 180;
                }
                col.radius = 0.1f;
                tPared = 0.5f;
                snakeAnimator.SetBool("Pared", false);
                //Si no choca porque se ha despegado, no rota
            }
            Debug.DrawRay(pecho.position, -Rig.forward * .35f);

            snakeAnimator.SetFloat("Hor", tPared == 0 ? (Vector3.Dot(movement, Rig.right)) : 0);
        }



        //Debug.DrawRay(transform.position, movement, Color.green);
        //controller.Move(movement * Time.deltaTime);
        Vector3 aceleracion = (movement - rb.velocity) / deltaTime;
        aceleracion.y = 0;
        rb.AddForce(aceleracion, ForceMode.Acceleration);
        //rb.velocity = new Vector3(movement.x, rb.velocity.y, movement.z);
        grounded = false;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1;
        filtroNVG.SetActive(false);
        filtroTermico.SetActive(false);
    }

    void DropObjeto(Objeto obj, int balasAdd = 0, bool todas = true)
    {
        //Si balas = -1, lo echa con todas las balas
        if (obj.nombre == "EMPTY")
            return;

        if (obj.TipoObjeto() >= 0)
        {
            GameObject caja = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Default_Caja"));

            int indexArma = armasInv.IndexOf(obj.arma());

            int balasAñadir = todas ? (balas[indexArma] - obj.arma().balasArma()) : balasAdd;

            caja.GetComponent<CajaObjeto>().Crear(obj, !todas, balasAñadir);

            caja.transform.position = transform.position + Rig.forward;
            caja.GetComponent<Rigidbody>().velocity = Rig.forward * 3;

            if (todas)
            {
                QuitaArma(indexArma);
            }

            else
            {
                balas[indexArma] = Mathf.Max(0, balas[indexArma] - balasAdd);
                balasFront[indexArma].front = Mathf.Max(0, balasFront[indexArma].front - balasAdd);
            }


            SetArma();
        }
        else if(obj.TipoObjeto() == -2)
        {
            GameObject caja = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Default_Caja"));

            int indexArma = objetosInv.IndexOf(obj.objeto());

            int balasAñadir = todas ? (cantObjs[indexArma]) : balasAdd;

            bool todasReal = todas || (obj.objeto().maxCant == balasAñadir);

            caja.GetComponent<CajaObjeto>().Crear(obj, !todasReal, Mathf.Max(balasAñadir-1,0));

            caja.transform.position = transform.position + Rig.forward;
            caja.GetComponent<Rigidbody>().velocity = Rig.forward * 3;

            if (todasReal)
            {
                QuitaObjeto(indexArma);
            }

            else
            {
                cantObjs[indexArma] = Mathf.Max(0, cantObjs[indexArma] - balasAdd);
            }

            SetObjeto();

        }

    }

    void QuitaArma(int indexArma)
    {

        armasInv.RemoveAt(indexArma);
        balas.RemoveAt(indexArma);
        balasFront.RemoveAt(indexArma);

        if (indexArma == indArm && armasInv.Count > 1)
        {
            indArm = (indArm % armasInv.Count + armasInv.Count) % armasInv.Count;
            armaEnMano = armasInv[indArm];
        }

        //Si solo queda el empty
        else if (armasInv.Count == 1)
        {
            indArm = -1;
            antIndArm = -1;
            SalirInvArma(-1);
            armaEnMano = null;
        }
        SetArma();
    }
    void QuitaObjeto(int indexObjeto)
    {
        objetosInv.RemoveAt(indexObjeto);
        cantObjs.RemoveAt(indexObjeto);
        if (indexObjeto == indObj && cantObjs.Count > 1)
        {
            indObj = (indObj % objetosInv.Count + objetosInv.Count) % objetosInv.Count;
            objEnMano = objetosInv[indObj];
        }

        //Si solo queda el empty
        else if (objetosInv.Count == 1)
        {
            indObj = -1;
            antIndObj = -1;
            SalirInvObj(-1);
            objEnMano = null;

        }
        SetObjeto();
    }

    public void CamRadarY(float yIn, float yFin)
    {
        interfaz.SetCamaraRadarPlanes(yIn, yFin);
    }
    void velocidad()
    {
        if (!agach && !caja && tPared != 0)
            vel = (dirS.magnitude >= 0.75f && !CQC) ? sprVel : walkVel;
        else if (((agach && !arrast) || (arrast && GetButton("Apuntar"))) && !caja && tPared != 0)
            vel = 0;
        else
            vel = arrVel;

    }
    void SalirInvArma(int realInd)
    {

        tInvArmas = 1;
        if (!miraArmas)
        {
            realInd = (armaEnMano == null) ? indArm : -1;
        }
        else
        {
            miraArmas = false;
            interfaz.armaAnt.SetActive(false); interfaz.armaPos.SetActive(false);
            CheckVida();
        }

        sonidoLocal.PlayOneShot(clipCambioRapido, .1f);
        armaEnMano = realInd == -1 ? null : armasInv[realInd];

        SetArma();
        if (armaEnMano != null)
        {

            if ((armaEnMano.nombre != "EMPTY" && !(CQC && armaEnMano.TipoObjeto() > 1)) || armaEnMano.TipoObjeto() == 0)
                antIndArm = indArm;
            else
            {

                indArm = antIndArm;
                //realObjInd = -1;
                armaEnMano = null;
                SetArma();
            }
        }
        if (!armaHolder.activeInHierarchy)
            armaHolder.SetActive(true);
    }

    void SalirInvObj(int realObjInd)
    {

        tInvObj = 1;
        if (!miraObjetos)
        {
            realObjInd = (objEnMano == null) ? indObj : -1;
        }
        else
        {
            miraObjetos = false;
            interfaz.objAnt.SetActive(false); interfaz.objPos.SetActive(false);
            CheckVida();
        }

        sonidoLocal.PlayOneShot(clipCambioRapido, .1f);
        objEnMano = realObjInd == -1 ? null : objetosInv[realObjInd];
        SetObjeto();
        if (objEnMano != null)
        {
            if (objEnMano.nombre != "EMPTY" && !(objEnMano.nombre == "C. BOX" && arrast && !PosibleCambioPos(alturaC - 0)) && !(objEnMano.nombre == "C. BOX" && CQC))
                antIndObj = indObj;
            if (objEnMano.nombre != "C. BOX")
            {

                caja = false;
                if (!agach && !PosibleCambioPos(alturaW - alturaC))
                {
                    agach = true;
                }
                AjustaController();
            }
            if (objEnMano.nombre != "SCOPE" && !FPS && binoc)
            {
                CamATercera();
                trueFPS = false;
                binoc = false;

                Cam.GetComponent<Camera>().fieldOfView = TPSFov;
                Cam.GetChild(0).GetComponent<Camera>().fieldOfView = TPSFov;
            }
            else if (objEnMano.nombre != "SCOPE" && FPS && binoc)
            {
                binoc = false;
                Cam.GetComponent<Camera>().fieldOfView = FPSFov;
                Cam.GetChild(0).GetComponent<Camera>().fieldOfView = FPSFov;
            }

            switch (objEnMano.nombre)
            {
                case "CIGS":
                    interfaz.vidaObj.SetActive(true);
                    humoCig.SetActive(true);
                    break;
                case "N.V.G.":
                    //Cam.GetComponent<Volume>().enabled = true;
                    filtroTermico.SetActive(true);
                    filtroNVG.SetActive(true);
                    sonidoLocal.PlayOneShot(clipNVG);
                    break;
                case "BODY ARMOR":
                    dañoMult = .5f;
                    break;
                case "C. BOX":
                    if ((arrast && !PosibleCambioPos(alturaC - 0)) || CQC)
                    {
                        indObj = antIndObj;
                        //realObjInd = -1;
                        objEnMano = null;
                        SetObjeto();
                        break;
                    }
                    caja = true;
                    AjustaController();
                    break;
                case "SCOPE":
                    binoc = true;

                    if (!FPS)
                        CamAPrimera();

                    trueFPS = true;
                    Cam.position = cabeza.transform.position + (0.1f * cabeza.transform.up);
                    interfaz.UIbinoc.SetActive(true);
                    break;
                case "EMPTY":
                    indObj = antIndObj;
                    //realObjInd = -1;
                    objEnMano = null;
                    SetObjeto();
                    break;
            }
        }
        else
        {
            if (!FPS && binoc)
            {
                CamATercera();
                trueFPS = false;
                Cam.GetComponent<Camera>().fieldOfView = TPSFov;
                Cam.GetChild(0).GetComponent<Camera>().fieldOfView = TPSFov;
            }
            else if (FPS && binoc)
            {
                Cam.GetComponent<Camera>().fieldOfView = FPSFov;
                Cam.GetChild(0).GetComponent<Camera>().fieldOfView = FPSFov;
            }
            binoc = false;
            caja = false;
            if (!agach && !PosibleCambioPos(alturaW - alturaC))
            {
                agach = true;
            }
            AjustaController();
        }
    }

    public void DisparaArmaFuego(Pistola_Fusil arma)
    {

        if ((snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("Disparar"))
            && tiempoCadencia == 0 && puedeDisparar && balas[indArm] > 0 && balasFront[indArm].front > 0)
        {

            tiempoCadencia = 1 / arma.cadencia;
            partRef.SetActive(true);
            armaHolder.GetComponent<AudioSource>().PlayOneShot(armaEnMano.sonidoDisparo, 1);
            Disparar();
            SonidoRadio(7);
            if (!arma.automatica)
                puedeDisparar = false;

            if (arma.rondas == 1 || (arma.rondas > 1 && tiempoRondas <= 0.1f))
            {
                float cantidadB = arma.balas - balasFront[indArm].front;
                int cadaR = arma.balas / arma.rondas;
                //print(cadaR);
                if (arma.rondas > 1 && balasFront[indArm].front % cadaR == 0 && cantidadB > 0 && tiempoRondas > 0)
                {
                    tiempoRondas = 0;
                }
                balas[indArm] -= 1;
                balasFront[indArm].front -= 1;
            }

            interfaz.textoBalasArm.text = (balasFront[indArm].front) + "/" + (balas[indArm] - balasFront[indArm].front);
            interfaz.CambiarBalasUI(balasFront[indArm].front, balas[indArm]);


        }
        else if (balas[indArm] > 0 && balasFront[indArm].front == 0 && GetButton("Disparar") && !snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("Recargar"))
        {
            snakeAnimator.SetBool("Recargar", true);
            snakeAnimator.SetBool("Disparar", true);
        }


        if (GetButton("Disparar") && arma.rondas > 1 && !snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("Recargar"))
        {
            if (tiempoRondas > 0.1f)
            {
                tiempoRondas -= Time.deltaTime;
                if (tiempoRondas < 0.1f)
                    tiempoRondas = 0.1f;
            }


            int cadaR = (int)(arma.balas / arma.rondas);
            float cantidadB = arma.balas - balasFront[indArm].front;
            if (balasFront[indArm].front % cadaR == 0 && cantidadB > 0 && tiempoRondas == 0)
            {
                tiempoRondas = arma.tiempoRondas;
                //print("entro por aqui");
            }
        }
        else
        {
            tiempoRondas = 0;
        }


        if (GetButtonDown("Recargar") && balasFront[indArm].front < arma.balas && balas[indArm] - balasFront[indArm].front > 0)
        {
            snakeAnimator.SetBool("Recargar", true);
            armaHolder.GetComponent<AudioSource>().Play();
        }

        //Sonidos recargar
        if (!snakeAnimator.GetNextAnimatorStateInfo(1).IsName("Recargar") && armaHolder.GetComponent<AudioSource>().time > 0.15f)
        {
            //print("para");
            //armaHolder.GetComponent<AudioSource>().Stop();
        }

        if (snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("Recargar") && snakeAnimator.GetCurrentAnimatorStateInfo(1).normalizedTime > .2f)
        {
            if (snakeAnimator.GetBool("Recargar"))
            {
                snakeAnimator.SetBool("Recargar", false);
            }
            else if (snakeAnimator.GetCurrentAnimatorStateInfo(1).normalizedTime > .85f)
                Recargar(arma);

            partRef.SetActive(false);
        }
        else if (snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("Disparar")
            && snakeAnimator.GetCurrentAnimatorStateInfo(1).normalizedTime > .65f && partRef.activeInHierarchy&& arma.cadencia < 5)
        {
            partRef.SetActive(false);
        }


    }

    public void DispararExplosivo(Granada gran)
    {
        
        if (snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("Disparar") && puedeDisparar)
        {

            bool cuentaBalas = (gran.remote == false) || (gran.remote == true && GetButton("Apuntar"));
            if (cuentaBalas && balas[indArm] == 0)
            {
                print("no hay balas");
                return;
            }


            if (cuentaBalas)
                armaHolder.GetComponent<AudioSource>().PlayOneShot(armaEnMano.sonidoDisparo, 1);
            else
                armaHolder.GetComponent<AudioSource>().PlayOneShot(armaEnMano.recargaClip, 1);

            puedeDisparar = false;


            if (cuentaBalas)
            {
                balas[indArm] -= 1;
                interfaz.CambiarBalasUI(-1, balas[indArm]);
            }

            //Lanzar el explosivo

            gran.Disparar(cabeza.transform.position, cabeza.transform.forward, Vector3.zero, transform, true);

            if (!gran.remote && balas[indArm] == 0)
                QuitaArma(armasInv.IndexOf(gran));

        }
        
    }

    public float GetAxis(string axis)
    {
        if (pausa || informacion)
            return 0;

        return input.FindAction(axis).ReadValue<float>();

    }

    public bool GetButton(string button)
    {

        if ((pausa && button != "Escape") || (informacion && button != "Info"))
            return false;

        return input.FindAction(button).IsPressed();
    }
    public bool GetButtonDown(string button)
    {
        if (pausa && button != "Escape")
            return false;

        else if (informacion && !pausa && button != "Escape" && button != "Info")
            return false;


        return input.FindAction(button).WasPressedThisFrame();
    }
    public bool GetButtonUp(string button)
    {

        if ((pausa && button != "Escape") || (informacion && button != "Info"))
            return false;

        return input.FindAction(button).WasReleasedThisFrame();
    }
    
    //Hacer que rote con el suelo?
    void RotaRig(bool smooth = true)
    {
        Vector2 mov = new Vector2(movement.x, movement.z) * 
            ((!CQC) || (CQC && GetAxis("Horizontal") == 0 && GetAxis("Vertical") == 0 && !agach) || (CQC && agach) ? 1 : -1);
        float angle;
        if (smooth)
            angle = Mathf.MoveTowardsAngle(Rig.eulerAngles.y, Mathf.Atan2(mov.x, mov.y) * Mathf.Rad2Deg, tiempoRot * 1000 * Time.deltaTime);
        else
            angle = Mathf.Atan2(mov.x, mov.y) * Mathf.Rad2Deg;


        if(!snakeAnimator.GetCurrentAnimatorStateInfo(1).IsName("CQCThrow") && !snakeAnimator.GetNextAnimatorStateInfo(1).IsName("CQCThrow"))
            Rig.eulerAngles = new Vector3(0, angle, 0);

    }

    void Recargar(Pistola_Fusil arma)
    {
        //print("R");
        int dif = Mathf.Min(arma.balas - balasFront[indArm].front, balas[indArm] - balasFront[indArm].front);
        balasFront[indArm].front += dif;

        interfaz.CambiarBalasUI(balasFront[indArm].front, balas[indArm]);
    }



    void CambiaPostura()
    {
        if (snakeAnimator.GetCurrentAnimatorStateInfo(0).IsName("Tirarse") ||
            snakeAnimator.GetCurrentAnimatorStateInfo(0).IsName("Aterrizar")){return;}
        if (tiempoPressAg > 0)
        {
            if (!arrast && (!agach || (agach && PosibleCambioPos(alturaW - alturaC))))
            {
                agach = !agach;
            }
            else if(arrast && PosibleCambioPos(alturaC - 0))
            {
                arrast = false;
                if (GetAxis("Horizontal") != 0 || GetAxis("Vertical") != 0)
                {
                    levantaTiron = true;
                }
            }
            tiempoPressAg = tiempoCtrlArr;

        }
        else
        {
            if(agach && PosibleCambioPos(alturaW - alturaC) && ((estadoInicial == 2 && !CQC) || (estadoInicial < 2 && CQC)))
            {
                agach = false;
                if (levantaTiron)
                    levantaTiron = false;
            }
            else if(agach && estadoInicial < 2 && !CQC)
            {
                arrast = true;
            }
        }

        AjustaController(true);
    }


    void AjustaController(bool cambiaPos = false)
    {


        //Altura personaje
        bool prevArr = false;
        if (arrast)
            prevArr = col.direction == 2;

        if (!agach)
            col.height = alturaW;
        else if (agach && !arrast)
            col.height = alturaC;
        else
            col.height = alturaArr;

        if(caja)
            col.height = alturaC;

        //AjustaCentro
        float offset = .5f;
        col.center = (Vector3.down * (alturaW - col.height) / 2.0f) + (Vector3.up * offset);
        col.direction = 1;
        Cam.localPosition = Vector3.back * (camClasica ? -distCl : dCam);

        tCamTarg = 0;
        if (arrast)
        {
            print("ajusta");
            if(cambiaPos && !prevArr)
                transform.position += 0.1f * Vector3.up;


            col.center = .07f * Vector3.up;
            col.direction = 2;
            if(!camClasica)
                Cam.localPosition += Vector3.up * 0.05f;

            tCamTarg = 1;
        }

        
        
    }

    void AjustaCamaraTercera()
    {
        if (camClasica)
        {
            rot = rotCl;
            Cam.localPosition = new Vector3(0, 0, distCl);
        }
        else
        {
            rot = Rig.eulerAngles;
            float alt = arrast ? 0.05f : 0;
            Cam.localPosition = new Vector3(0, alt, dCam);
        }
    }

    void SetArma()
    {
        if (armaEnMano != null)
        {
            armaHolder.GetComponent<MeshFilter>().mesh = armaEnMano.modeloArma;
            armaHolder.GetComponent<MeshRenderer>().material = armaEnMano.materialArma;
            armaHolder.GetComponent<AudioSource>().clip = armaEnMano.recargaClip;
            interfaz.tDesArm.text = armaEnMano.nombre + ": \n" + armaEnMano.descripcion;


            interfaz.armaLogo.sprite = armaEnMano.UIArma;
            interfaz.textoArmas.text = armaEnMano.nombre;
            if (armaEnMano.TipoObjeto() >= 1)
            {
                interfaz.CambiarBalasUI(balasFront[indArm].front, balas[indArm]);

                partRef.transform.localPosition = armaEnMano.posParticulas();
            }
            else if (armaEnMano.TipoObjeto() == 0)
            {

                partRef.transform.localPosition = armaEnMano.posParticulas();
                interfaz.CambiarBalasUI(-1, balas[indArm]);
            }

            if (armaEnMano.nombre == "EMPTY")
                interfaz.textoBalasArm.text = "";



        }
        else
        {
            armaHolder.GetComponent<MeshFilter>().mesh = null;
            armaHolder.GetComponent<MeshRenderer>().material = null;
            armaHolder.GetComponent<AudioSource>().clip = null;
        }
    }

    void SetObjeto()
    {

        if (objEnMano != null)
        {
            interfaz.objetoLogo.sprite = objEnMano.UIObj;
            interfaz.textoObjeto.text = objEnMano.nombre;
            interfaz.tDesObj.text = objEnMano.nombre + ": \n" + objEnMano.descripcion;

            if (objEnMano.maxCant > 1)
                interfaz.textoCantObj.text = cantObjs[objetosInv.IndexOf(objEnMano)].ToString();
            else
                interfaz.textoCantObj.text = "";

            objetoHolder.GetComponent<MeshFilter>().mesh = objEnMano.objetoVisual;
            objetoHolder.GetComponent<MeshRenderer>().material = objEnMano.material;

            switch (objEnMano.parent)
            {
                case ObjetoUsable.Parent.Ninguno:
                    objetoHolder.transform.parent = Rig;
                    break;
                case ObjetoUsable.Parent.Pecho:
                    objetoHolder.transform.parent = pecho;
                    break;
                case ObjetoUsable.Parent.Cabeza:
                    objetoHolder.transform.parent = cabeza.transform;
                    break;
            }
            objetoHolder.transform.localPosition = objEnMano.localPos;
            objetoHolder.transform.localScale = objEnMano.escala;
            objetoHolder.transform.localEulerAngles = objEnMano.localEuler;


            if (objEnMano.nombre != "CIGS")
            {
                humoCig.SetActive(false);
            }
            if(objEnMano.nombre != "N.V.G.")
            {

                filtroTermico.SetActive(false);
                filtroNVG.SetActive(false);

            }
            if (objEnMano.nombre != "BODY ARMOR")
            {
                dañoMult = 1;
            }
            if (objEnMano.nombre != "SCOPE")
            {
                interfaz.UIbinoc.SetActive(false);
                if (!CQC)
                {
                    Cam.GetComponent<Camera>().fieldOfView = FPS ? FPSFov : TPSFov;
                    Cam.GetChild(0).GetComponent<Camera>().fieldOfView = FPS ? FPSFov : TPSFov;
                }
            }


            //textoBalasArm.text = (balasFront[indArm]) + "/" + (balas[indArm] - balasFront[indArm]);
            //CambiarBalasUI();
        }
        else
        {
            interfaz.UIbinoc.SetActive(false);

            filtroTermico.SetActive(false);
            filtroNVG.SetActive(false);
            objetoHolder.GetComponent<MeshFilter>().mesh = null;
            objetoHolder.GetComponent<MeshRenderer>().material = null;
            humoCig.SetActive(false);
            dañoMult = 1;
            caja = false;
            if (!CQC)
            {
                Cam.GetComponent<Camera>().fieldOfView = FPS ? FPSFov : TPSFov;
                Cam.GetChild(0).GetComponent<Camera>().fieldOfView = FPS ? FPSFov : TPSFov;
            }
            AjustaController();
        }
    }
    public bool RecibObjeto(Objeto obj)
    {
        int tipoObjeto = obj.TipoObjeto();


        if (tipoObjeto >= 1 && !armasInv.Contains(obj.arma()))
        {
            tienePist = true;
            armasInv.Add(obj.arma());
            if(indArm == -1)
                indArm = 1;
            balas.Add(obj.balasArma());
            Front f = new Front(); f.front = obj.balasArma();
            balasFront.Add(f);
            if (armasInv[indArm] == obj.arma())
                interfaz.textoBalasArm.text = (balasFront[indArm].front) + "/" + (balas[indArm] - balasFront[indArm].front);

            if (antIndArm == -1)
            {
                antIndArm = 1;
            }
        }
        else if (tipoObjeto >= 1 && armasInv.Contains(obj.arma()))
        {
            return RecibeBalas(obj, obj.balasArma()); 
        }
        else if (tipoObjeto == -2 && !objetosInv.Contains(obj.objeto()))
        {
            objetosInv.Add(obj.objeto());
            cantObjs.Add(1);
            if (indObj == -1)
            {
                indObj = 0;
                antIndObj = 0;
            }
        }
        else if(tipoObjeto == -2 && objetosInv.Contains(obj.objeto()))
        {
            return RecibeBalas(obj, 1);
        }
        else if(tipoObjeto == 0 && !armasInv.Contains(obj.arma()))
        {

            armasInv.Add(obj.arma());
            if (indArm == -1)
                indArm = 1;
            balas.Add(5);
            Front f = new Front(); f.front = 0; f.listaIDs = new List<int>();
            balasFront.Add(f);
            if (armasInv[indArm] == obj.arma())
                interfaz.textoBalasArm.text = balas[indArm].ToString();

            if (antIndArm == -1)
            {
                antIndArm = 1;
            }
        }
        else if(tipoObjeto == 0 && armasInv.Contains(obj.arma()))
        {
            return RecibeBalas(obj, 2);
        }


        sonidoLocal.PlayOneShot(clipCogerObj, .25f);
        interfaz.textoObjetos.gameObject.SetActive(true);
        interfaz.textoObjetos.text = "+ " + (obj.nombre) + " OBTAINED +";
        interfaz.textoTiempo = 1;

        //Actualizamos la interfaz de ambos inventarios

        return true;
    }

    public bool RecibeBalas(Objeto obj, int balasCantidad, bool texto = true)
    {
        //print(balasCantidad);
        if (obj.TipoObjeto() >= 0 && armasInv.Contains(obj.arma()))
        {
            if (balas[armasInv.IndexOf(obj.arma())] - balasFront[armasInv.IndexOf(obj.arma())].front == obj.cargador())
            {
                sonidoLocal.PlayOneShot(clipFull, .25f);
                interfaz.textoObjetos.gameObject.SetActive(true);
                interfaz.textoObjetos.text = "+ " + obj.nombre + " AMMO FULL +";
                interfaz.textoTiempo = 1;
                return false;
            }
            balas[armasInv.IndexOf(obj.arma())] += balasCantidad;
            if (balas[armasInv.IndexOf(obj.arma())] < obj.arma().balasArma())
                balasFront[armasInv.IndexOf(obj.arma())].front = balas[armasInv.IndexOf(obj.arma())];


            balas[armasInv.IndexOf(obj.arma())] = Mathf.Clamp(balas[armasInv.IndexOf(obj.arma())] - balasFront[armasInv.IndexOf(obj.arma())].front, 0, obj.cargador())
                + balasFront[armasInv.IndexOf(obj.arma())].front;
            if (obj.arma() == armaEnMano) { SetArma(); }


        }
        else if(obj.TipoObjeto() >= 0 && !armasInv.Contains(obj.arma()))
        {

            sonidoLocal.PlayOneShot(clipFull, .25f);
            interfaz.textoObjetos.gameObject.SetActive(true);
            interfaz.textoObjetos.text = "+ THERE IS NO " + obj.nombre + " +";
            interfaz.textoTiempo = 1;
            return false;
        }
        else if(obj.TipoObjeto() == -2 && objetosInv.Contains(obj.objeto()))
        {
            if (obj.maxCantidad() == cantObjs[objetosInv.IndexOf(obj.objeto())])
            {
                sonidoLocal.PlayOneShot(clipFull, .25f);
                interfaz.textoObjetos.gameObject.SetActive(true);
                interfaz.textoObjetos.text = "+ " + obj.nombre + " CAPACITY FULL +";
                interfaz.textoTiempo = 1;
                return false;
            }
            else
            {
                cantObjs[objetosInv.IndexOf(obj.objeto())] += balasCantidad;
            }
            if (obj == objEnMano)
                SetObjeto();

        }
        else if (obj.TipoObjeto() == -2 && !objetosInv.Contains(obj.objeto()))
        {

            sonidoLocal.PlayOneShot(clipFull, .25f);
            interfaz.textoObjetos.gameObject.SetActive(true);
            interfaz.textoObjetos.text = "+ THERE IS NO " + obj.nombre + " +";
            interfaz.textoTiempo = 1;
            return false;
        }

        if (texto)
        {

            sonidoLocal.PlayOneShot(clipCogerObj, .25f);
            interfaz.textoObjetos.gameObject.SetActive(true);
            interfaz.textoObjetos.text = "+ " + (obj.nombre) + " AMMO OBTAINED +";
            interfaz.textoTiempo = 1;

        }

        return true;
    }

    void UsaObjeto(ObjetoUsable obj)
    {
        if(obj == null)
        {
            //QUITA TODOS LOS OBJETOS
            print("no se hace nada");

            return;

        }
        switch (obj.nombre)
        {
            case "RATION":
                vida = Mathf.Clamp(vida + 30, 0, 100);
                sonidoLocal.PlayOneShot(clipCura);
                if(!miraObjetos)
                    CheckVida();
                break;
        }
        //Si se pueden stackear (solo raciones por el momento)
        if(obj.maxCant > 1)
        {
            int indOb = objetosInv.IndexOf(obj);
            cantObjs[indOb] -= 1;
            if (cantObjs[indOb] == 0)
                QuitaObjeto(indOb);

        }
    }

    void CheckVida()
    {
        interfaz.vidaObj.SetActive(vida < 100);
    }


    bool PosibleCambioPos(float d)
    {
        float add = arrast ? .05f : 0;
        return !Physics.Raycast(cabeza.transform.position, Vector3.up,add + d, sueloLayers);
    }


    void CamAPrimera()
    {
        FPSrot = Rig.eulerAngles;
        FPSrot.x = Mathf.Clamp(FPSrot.x, capFPSX.x, capFPSX.y);
        rot = Rig.eulerAngles;
        CamContainer.rotation = Quaternion.Euler(rot);
        Cam.position = cabeza.transform.position - (cabeza.transform.forward * (1 - animAy.apuntarCant)) + (0.1f * cabeza.transform.up);
        Cam.GetComponent<Camera>().fieldOfView = FPSFov;
        Cam.GetChild(0).GetComponent<Camera>().fieldOfView = FPSFov;
    }

    void CamATercera()
    {
        FPSrot = Vector3.zero;
        cabeza.transform.localRotation = Quaternion.Euler(-22.5f, 0, 0);
        Cam.localRotation = Quaternion.Euler(0, 0, 0);
        trueFPS = false;
        Cam.GetComponent<Camera>().fieldOfView = TPSFov;
        Cam.GetChild(0).GetComponent<Camera>().fieldOfView = TPSFov;
        AjustaCamaraTercera();
        AjustaCamDist();
    }

    void AjustaCamDist()
    {
        if (camClasica)
            return;

        RaycastHit camRayo;


        if (Physics.Raycast(CamContainer.position, -CamContainer.forward, out camRayo, dCam, sueloLayers))
        {
            actualDistC = Vector3.Distance(transform.position, camRayo.point) - 0.25f;
        }
        else if (actualDistC < dCam)
        {
            actualDistC += Time.deltaTime * 10;
        }
        
        if (actualDistC > dCam)
            actualDistC = dCam;

        Cam.localPosition = new Vector3(0, Cam.localPosition.y, -actualDistC);

    }

    void CQCThrow()
    {
        Vector3 f = Cam.forward; Vector3 r = Cam.right;
        Vector3 a = new Vector3(f.x, 0, f.z).normalized;

        if (a == Vector3.zero)
        {
            a = Cam.up;
        }

        Vector3 forw = (GetAxis("Horizontal") * r + GetAxis("Vertical") * a).normalized;
        Vector3 right = Vector3.Cross(forw, Vector3.up);


        agach = false;
        snakeAnimator.SetBool("Throw", true);

        //Hacer animaciones de CQC direccional dependiendo de la direccion del rig y la escogida
        snakeAnimator.SetFloat("CQCHor", Vector3.Dot(forw, Rig.right));
        snakeAnimator.SetFloat("CQCVer", Vector3.Dot(forw, Rig.forward));

        Vector3 pos = transform.position + (forw * .1f) + (Vector3.down * .1f) - (right * 0.1f);
        SetSoldadoInc(colliderSnake.soldadoCQC,colliderSnake.masProximo,  pos, forw, true);
    }


    void SetSoldadoInc(Soldier soldado, Collider soldadoCol, Vector3 pos, Vector3 normal, bool tirar, float target = 100, bool cajaAux = true)
    {
        //soldado.DesactivaVista();
        if(cajaAux)
            soldado.Throw(this, tirar, target);
        else
            soldado.Throw(null ,tirar, target, this.transform);
        soldadoCol.name = "InteraccionSoldadoInc";
        soldado.DatosCQC(normal, soldado.Agachado(), pos, soldado.EstaVivo() || soldado.Consciente());
        //Debug.Break();

    }

    void Disparar()
    {
        //Debug.Break();
        Vector3 add = tiempoCQC > 0 ? Rig.right * 0.2f : Vector3.zero;
        Vector3? posDisparo = armaEnMano.Disparar(cabeza.transform.position + add, cabeza.transform.forward, cabeza.transform.up, transform, true);
        if(posDisparo != null)
        {
            trailBala.enabled = true;
            trailBala.positionCount = 2;

            float facEscala = 1 / transform.localScale.x;
            Vector3 inicio = facEscala*(partRef.transform.position - transform.position); Vector3 final = facEscala * (posDisparo.Value - transform.position);


            trailBala.SetPositions(new Vector3[] { inicio, final });
            StopCoroutine(TiempoBalas());
            StartCoroutine(TiempoBalas());
            //Debug.Break();
        }
    }


    public void EliminaSoldado()
    {
        colliderSnake.soldadoCQC = null;
    }
    public void Libera()
    {
        if (colliderSnake.soldadoCQC == null)
            return;
        colliderSnake.soldadoCQC.LiberaSoldado(this);
        entraCQC = false;
        CQC = false;
    }

    public Vector3 Direccion()
    {
        return Vector3.Scale(movement, new Vector3(1,0,1)).normalized;
    }


    public void Tirate(Vector3 direccion)
    {
        snakeAnimator.Play("Base Layer.Tirarse", 0, 0);
        Vector3 mov = direccion;
        mov.y = 2;
        rb.velocity = mov;
        grounded = false;

        agach = true;
        arrast = true;
        AjustaController();
    }

    public void QuitaVida(float cantidad)
    {
        print("vida");
        cantidad = Mathf.Clamp(cantidad, 0, 100);

        vida -= cantidad * dañoMult;

        snakeAnimator.Play("DolorLayer.Dolor", 2, 0);

        CheckVida();
        if(vida <= 0)
        {
            if (objEnMano != null && objEnMano.nombre == "RATION")
            {
                vida = 0;
                UsaObjeto(objEnMano);
            }
            else
            {
                morir = true;
                this.GetComponent<AudioSource>().PlayOneShot(scream);
                if (colliderSnake.soldadoCQC != null)
                    Libera();

                colliderSnake.gameObject.SetActive(false);
                Time.timeScale = 1;
                snakeAnimator.Play("Base Layer.Morir", 0, 0);
                snakeAnimator.SetLayerWeight(1, 0);
                snakeAnimator.SetLayerWeight(2, 0);
            }

        }



    }
    public void StunG()
    {
        interfaz.stunScreen.color = new Color(1, 1, 1, 1);
        sonidoLocal.PlayOneShot(Resources.Load<AudioClip>("Audio/Snake/Flash"));
    }

    public float AnguloCam()
    {
        return -Vector3.SignedAngle(Vector3.Scale(Cam.forward, new Vector3(1,0,1)), Vector3.forward, Vector3.up);
    }

    public bool enMovimiento()
    {
        return rb.velocity.x != 0 || rb.velocity.z != 0 || 
            (FPS && (GetAxis("Mouse X") != 0 || GetAxis("Cam X") != 0 || GetAxis("Mouse Y") != 0 || GetAxis("Cam Y") != 0));
    }
    public bool EstaVivo()
    {
        return vida > 0;
    }
    IEnumerator CuentaTBalas()
    {
        print("set segundos");
        yield return new WaitForSeconds(.15f);
    }

    IEnumerator TiempoBalas()
    {
        yield return StartCoroutine(CuentaTBalas());
        print("set 0");
        trailBala.positionCount = 0;

    }


    public void SetRadar(GameObject r, float yIn = 0)
    {


        radarImg = r;
        if(r != null)
            interfaz.SetCamaraRadarPlanes(yIn, r.transform.position.y - 0.01f);


    }
    public bool EsRadar()
    {
        return (radarImg != null && tRadar < 0);
    }
    public void SonidoClear()
    {
        sonidoLocal.PlayOneShot(sonidoClear);
    }
    public void PillaSoldado(Soldier sold)
    {
        //print("recupera al soldado");
        colliderSnake.soldadoCQC = sold;
        sold.AgarreSoldado(this);
        Vector3 pos = transform.position + (Rig.forward * .1f) + (Vector3.down * .1f) + (Rig.right * -.1f);
        sold.DatosCQC(Rig.forward, agach, pos);
        //Debug.Break();

    }
    public void SetTJamming(float t)
    {
        tRadar = t;
    }
    public float GetTJamming()
    {
        return tRadar;
    }

    void SonidoRadio(float radius)
    {
        if (sonidoCollider.radius > radius && sonidoCollider.enabled)
        {
            //print("no hace nada");
            return;
        }


        sonidoCollider.radius = radius;
        sonidoCollider.enabled = true;
        //print("cambiaRadio");

    }

    public void SetInversiones(int index, bool value)
    {
        inversiones[index] = value ? -1 : 1;
    }
    public void SetFov(int cant, bool FPS_)
    {
        if (FPS_)
            FPSFov = cant;
        else
            TPSFov = cant;

        if (FPS_ == FPS)
        {
            Cam.GetComponent<Camera>().fieldOfView = cant;
            Cam.GetChild(0).GetComponent<Camera>().fieldOfView = cant;
        }

    }

    public void Pausa()
    {
        pausa = !pausa;
        Time.timeScale = pausa ? 0 : 1;
        interfaz.Pausa(pausa);
        if (!pausa)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void Informacion()
    {
        informacion = !informacion;
        interfaz.MenuInformacion(informacion);
    }
    public void RecibeInformacion(string info, bool nuevaLinea = true, bool replace = false)
    {
        if (replace)
            interfaz.informTexto.text = info;
        else
            interfaz.informTexto.text += string.Concat(Enumerable.Repeat( nuevaLinea ? "\n" : "", 3)) + info.ToUpper();
    }

    public void SetFrontID(int ind, int id, bool remove)
    {
        if (remove)
            balasFront[ind].listaIDs.Remove(id);
        else
            balasFront[ind].listaIDs.Add(id);
    }
    public int GetFirstFrontID(int ind)
    {
        if (balasFront.Count <= ind || balasFront[ind].listaIDs.Count == 0)
            return -1;
        return balasFront[ind].listaIDs[0];
    }
    public int GetBalas(int ind)
    {
        return balas[ind];
    }

    public string SacaInfo()
    {
        return interfaz.informTexto.text;
    }
    public void QuitaInfo(int linea)
    {

        //Como la informacion es compartida, no importa si se la saco a uno cualquiera
        string info = interfaz.informTexto.text;

        string[] infoMod = info.Split("\n");

        //Quitar informacion del objetivo ya completado
        RecibeInformacion(infoMod[0], false, true);
        List<string> nuevInfo = new List<string>();

        for(int i= 1; i < infoMod.Length; i++)
        {
            if (infoMod[i] != "")
                nuevInfo.Add(infoMod[i]);
        }

        nuevInfo.RemoveAt(linea);
        for (int i = 0; i < nuevInfo.Count; i++)
        {
            print(nuevInfo[i]);
        }
        print(linea);

        for (int i = 0; i < nuevInfo.Count; i++)
        {
            RecibeInformacion(nuevInfo[i]);
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        Vector3 direccion = collision.GetContact(0).normal;
        //Debug.DrawRay(transform.position, direccion, new Color(.5f, 0, .1f));
        //Si el choque es total, se le dice al animator que pare
        if (Vector3.Dot(movement.normalized, direccion) < -.2f && tPared > 0)
        {
            movement -= Vector3.Project(movement, direccion);
            Vector3 aceleracion = (movement - rb.velocity) / Time.fixedDeltaTime;
            aceleracion.y = 0;
            rb.AddForce(aceleracion, ForceMode.Acceleration);
            vMag = new Vector2(rb.velocity.x, rb.velocity.z).magnitude;
        }
        if(Vector3.Dot(Vector3.up, direccion) > .9f * direccion.magnitude)
        {
            grounded = true;
        }

        //Quita la parte de velocidad que choca
        //movement -= Vector3.Project(movement, collision.impulse);


        Debug.DrawRay(transform.position, rb.velocity, new Color(1, 0, 1));
    }

    public Vector3 ActualPos()
    {
        return pecho.position;
    }

    public void AjustaCamMarco(int cantActual, int cantTotal)
    {
        if (cantTotal == 1)
            return;

        Camera cam_ = Cam.GetComponent<Camera>();

        Rect r = cam_.rect;
        if (cantTotal == 2)
        {
            r.size = new Vector2(.5f, 1);
            r.position = new Vector2(cantActual > 0 ? .5f : 0, 0);
        }
        else if (cantTotal == 3)
        {
            r.size = new Vector2(.5f, .5f);
            r.position = new Vector2(cantActual > 0 ? .5f : 0, 0);
        }
        else if (cantTotal == 4)
        {
            r.size = new Vector2(.5f, 1);
            r.position = new Vector2(cantActual > 0 ? .5f : 0, 0);
        }

        cam_.rect = r;
    }


}
