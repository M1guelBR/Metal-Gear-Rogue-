using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Globalization;
using UnityEditor;


//Los datos que puede albergar un pensamiento
[System.Serializable]
public class Dato
{
    int dato_int;
    float dato_float;
    Vector3 dato_Vec;
    [HideInInspector]public int tipo_dato;
    public string valor;

    //El dato es un int
    public Dato(int num)
    {
        tipo_dato = 0;
        dato_int = num;
        dato_float = -1.0f;
        dato_Vec = Vector3.zero;
        valor = num.ToString();
    }

    //El dato es una float
    public Dato(float f)
    {
        tipo_dato = 1;
        dato_int = -1;
        dato_float = f;
        dato_Vec = Vector3.zero;
        valor = f.ToString();
    }

    //El dato es un Vector
    public Dato(Vector3 v)
    {
        tipo_dato = 2;
        dato_int = -1;
        dato_float = -1.0f;
        dato_Vec = v;
        valor = v.ToString();
    }


    public Vector3 GetVector()
    {
        return dato_Vec;
    }
    public int GetInt()
    {
        return dato_int;
    }
    public float GetFloat()
    {
        return dato_float;
    }

    public void SetVector(Vector3 input)
    {
        dato_Vec = input;
    }
    public void SetFloat(float input)
    {
        dato_float = input;
    }
    public void SetInt(int input)
    {
        dato_int = input;
    }

}

//Los pensamientos de cada soldado
//Almacena el tipo de pensamiento, el tiempo de cada paso, el paso en el que esta y los datos asociados
[System.Serializable]
public class Pensamiento
{
    public static int EXPLOSIVO_CERCA = 0;
    public static int FALTA_ARMA = 1;
    public static int JUGADOR = 2;
    public static int COMP_INC = 3;
    public static int REHEN = 4;
    public static int CHECK_ZONA = 5;

    
    public int tipo;
    public float tiempo;
    public int step;

    //Formato
    //Dato 1 -> sitio
    //Dato n > 1 -> cosas concretas del pensamiento
    public Dato[] datos;

    //Constructor estandar
    public Pensamiento()
    {
        tipo = 0;
        step = 0;
        tiempo = 0;
        datos = new Dato[0];
    }

    //Solo se especifica el tipo
    public Pensamiento(int tipo_)
    {
        
        tipo = tipo_;
        step = 0;
        tiempo = 0;
        datos = new Dato[0];
    }

    public Pensamiento(int tipo_, params Dato[] datos_)
    {
        tipo = tipo_;
        tiempo = 0;
        step = 0;

        //Vamos a suponer que todo lo que le pasamos es del tipo que admiten los datos
        datos = new Dato[datos_.Length];

        for(int i = 0; i< datos.Length; i++)
        {
            
            //Si el dato es un int, lo metemos como int
            if (datos_[i].tipo_dato == 0)
            {
                datos[i] = new Dato(datos_[i].GetInt());
            }
            //Si es un float, lo metemos como float
            else if (datos_[i].tipo_dato == 1)
            {
                datos[i] = new Dato(datos_[i].GetFloat());
            }
            //Si es un Vector3, lo metemos como Vector3
            else if (datos_[i].tipo_dato == 2)
            {
                datos[i] = new Dato(datos_[i].GetVector());
            }
        }
    }
    
    public void SetTiempo(float t)
    {
        //tiempo = t;
        tiempo = t;
    }
    public void SetStep(int s)
    {
        step = s;
    }

    public Dato ultimoDato()
    {
        //Debug.Log("ultimo dato");
        Dato dato;

        if (datos.Length > 0)
        {
            dato = datos[datos.Length - 1];
        }
        else
        {
            //Debug.Log("nada");
            dato = new Dato(-1);
        }

        return dato;
    }

}

public class Soldier : MonoBehaviour
{
    public int ID = -1;
    [SerializeField] Animator soldierAnimator;
    public Transform Rig;
    [SerializeField] NavMeshAgent agente;
    [SerializeField]public CharacterController controller;
    List<int> indicesFinal = new List<int>();

    //----------MOVIMIENTO
    public List<Vector3> posiciones;
    public List<float> tiempoEspera;
    public List<float> rotacionesY;

    bool espera = true;
    float tiempoQueda = 0;

    Vector3 movimiento;
    Vector3 lastPos;
    int index;
    float vida = 100;

    float oxigeno = 100;



    bool antesApagado = true;

    [Range(0, 3)]
    public float velocidadAndar;
    [Range(1, 3)]
    public float multCorrer;

    bool correr, agachado;
    float vel;

    //----------PENSAMIENTO
    public List<Pensamiento> pensamientos;
    int pensamiento_step = 0;
    Vector3 destination = Vector3.zero;
    float tDesiredVelocity = 0;


    public GameObject col;
    public GameObject interacCol;
    public GameObject vistaCol;

    public LayerMask suelo;

    //----------CQC
    public Transform jugadorAg;
    Vector3 cqcPos;
    Vector3 cqcNormal;
    public bool pillado;
    public bool para;
    [HideInInspector]public float oxigTarget;
    public List<string> informacion;
    float tiempoInterrog = 0;
    float tiempoPausaPPK = 0;
    bool pegar = false;
    List<GameObject> collidersAPegar = new List<GameObject>();

    //--------------TIRAR A SOLDADOS
    Vector3 velocidadTiro;
    [HideInInspector]public bool tirado = false;
    bool levantandose = false;


    //--------------ARMAS
    public Transform armaHolder;
    List<Arma> armasSoldado;
    List<int> balas; List<int> balasFront;
    Arma armaEnMano;
    float distActual = 0;
    float tiempoCad = 0;
    bool apuntar = false;
    [SerializeField] Transform lArmAux, rArmAux;
    [SerializeField] Transform lArm, rArm;
    [SerializeField] Transform chest, cabeza;
    [SerializeField] LineRenderer trailBala;
    Vector3 sitioApuntar;
    bool dispAux = false;
    public bool puedeDisparar = false;

    //-------------AUDIO
    public AudioSource sonidoSoldado;
    [SerializeField] AudioClip cqcCaerSonido, cqcCaerGrito;
    [SerializeField] AudioClip dolorSonido, ahogueSonido;
    [SerializeField] AudioClip pasosSonido;
    [SerializeField] AudioClip alertaSonido, noticeSonido;
    bool paso = false;
    [HideInInspector]public string terminoGenero = "";

    //-----------------PARTES DEL CUERPO
    public Transform hips, LLeg, RLeg;
    float facSangre = 1;
    [SerializeField] SkinnedMeshRenderer cuerpo;
    [SerializeField] MeshRenderer gorro, buf, lazo1, lazo2;
    [SerializeField] Mesh femCuerpo;

    //----------RADAR
    [SerializeField] MeshRenderer fovRadar;
    int modoRadar = 0;

    //---------------SISTEMA DE DETECCION
    bool caution = false;
    [SerializeField] SonidoSoldado sonido;


    // Start is called before the first frame update
    void Start()
    {


        balas = new List<int>();
        balasFront = new List<int>();
        armasSoldado = new List<Arma>();



        armasSoldado.Add(Resources.Load<Arma>(Random.Range(0,101) > 99 ? "Armas/COLT" : "Armas/BERETTA"));
        armasSoldado.Add(Resources.Load<Arma>("Armas/FAMAS"));

        //Quitamos las granadas porque no se implementarlas

        //armasSoldado.Add(Resources.Load<Arma>("Armas/GRENADE"));

        // 2 Cargadores por arma con posibilidad de hasta dos mas

        for (int i = 0; i < armasSoldado.Count; i++) {
            //Granadas  implementadas lets gooooooo
            balas.Add(2 * armasSoldado[i].balasArma() + Random.Range(0, 1 + (2* armasSoldado[i].balasArma())));
            balasFront.Add(armasSoldado[i].balasArma()); 
        }

        SeleccionaArma(0);

        //controller = this.GetComponent<CharacterController>();
        agente.updateRotation = false;

        //cosasVistas = new List<string>();
        pensamientos = new List<Pensamiento>();

        while(rotacionesY.Count < posiciones.Count)
        {
            rotacionesY.Add(0);
        }
        while(tiempoEspera.Count < posiciones.Count)
        {
            tiempoEspera.Add(0.1f);
        }
        if (posiciones.Count == 0)
            para = true;


    }

    // Update is called once per frame
    void Update()
    {

        soldierAnimator.SetBool("Held", pillado);
        pillado = false;
        if (!agente.enabled && !antesApagado)
            antesApagado = true;


        //Movimiento Normal
        //Ponemos el paso de IA, para optimizar
        ActualizaPensamiento();

        CheckRadar();

        if (jugadorAg == null && !levantandose)
        {

            if (EstaPensando())
                agente.SetDestination(destination);
            apuntar = false;
            if (tiempoCad > 0)
            {
                tiempoCad -= Time.deltaTime;
                if (tiempoCad < 0)
                    tiempoCad = 0;
            }


            if (movimiento.magnitude > 0)
            {
                //Debug.Break();
                float t = soldierAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                t -= (int)t;

                if (t > 0.45f && t < 0.9f && !paso)
                {
                    paso = true;
                    ReproduceSonido(pasosSonido, movimiento.magnitude * .15f / (velocidadAndar * multCorrer));
                }
                else if (t > 0.9f && paso)
                {
                    paso = false;
                    ReproduceSonido(pasosSonido, movimiento.magnitude * .05f / (velocidadAndar * multCorrer));
                }

            }
            else
            {
                paso = false;
            }


            if (tiempoPausaPPK > 0)
            {
                tiempoPausaPPK -= Time.deltaTime;
                if (tiempoPausaPPK < 0)
                    tiempoPausaPPK = 0;
            }
            //movimiento = Vector3.zero;
            if (posiciones.Count > 0 && pensamientos.Count == 0 && !espera)
            //(posiciones.Count > 0 && cosasVistas.Count == 0 && !espera)
            {
                //if (para)
                // para = false;

                agachado = false;
                correr = false;
                int siguiente = (index + 1) % posiciones.Count;
                SetDestination(posiciones[siguiente]);
                if (HaLlegado()) 
                {
                    index = siguiente;
                    espera = true;
                    tiempoQueda = tiempoEspera[index];
                    SetDestination(posiciones[siguiente], true);
                    agente.isStopped = true;

                }

            }

            else if (espera)
            {
                tiempoQueda -= Time.deltaTime;
                if (tiempoQueda <= 0)
                {
                    espera = false;
                    agente.isStopped = false;
                    int siguiente = (index + 1) % posiciones.Count;
                    SetDestination(posiciones[siguiente], true);
                    tiempoQueda = 0;

                }
                else
                    RotaRig(rotacionesY[index], true);
            }

            else if (pensamientos.Count > 0)
            //(cosasVistas.Count > 0)
            {
                CompletaPensamiento();
            }

            if (!agachado)
                vel = velocidadAndar * (correr ? multCorrer : 1);
            else
                vel = 0;


            if ((balas.Contains(0) || armasSoldado.Count < 2) && !ContienePensamiento(Pensamiento.FALTA_ARMA) && EstaVivo() && Consciente())
            {
                //cosasVistas.Add("falArma");
                float d = Mathf.Infinity; int j = -1;
                for (int i = 0; i < FindObjectOfType<GameManager>().crates.Count; i++)
                {
                    if (d > Vector3.SqrMagnitude(FindObjectOfType<GameManager>().crates[i].transform.position - transform.position))
                    {
                        d = Vector3.SqrMagnitude(FindObjectOfType<GameManager>().crates[i].transform.position - transform.position);
                        j = i;
                    }
                }

                if (j != -1)
                {

                    Dato pos_crate = new Dato(FindObjectOfType<GameManager>().crates[j].transform.position);
                    pensamientos.Add(new Pensamiento(Pensamiento.FALTA_ARMA, pos_crate));
                    //pensamientos[0].step = 1;//cosasSteps[0] = 1;
                }
            }

            bool pararMoverse = para || (tiempoPausaPPK != 0);

            agente.speed = vel;
            //agente.isStopped = false;
            if (agente.desiredVelocity != Vector3.zero)
            {
                //print("velocidad" + agente.desiredVelocity);
                //Debug.Break();
                RotaRig();
            }
            agente.isStopped = pararMoverse;

            agente.updatePosition = agente.enabled;

        }
        //agarre cqc en lateUpdate
        else if (controller.enabled && tirado && !levantandose)
        {
            velocidadTiro.y -= Time.deltaTime * 9.8f;
            controller.Move(velocidadTiro * Time.deltaTime);
            //Cuando toca el suelo, cae y se queda inconsciente
            if (controller.isGrounded)
            {
                BajaOxigeno(100);
                SonidoRadio(4);
                ReproduceSonido(cqcCaerSonido, .15f);
                tirado = false;
                controller.enabled = false;
                transform.position += Vector3.up * agente.height * 0.175f;
                controller.enabled = true;
                //Debug.Break();
                soldierAnimator.Play("Base Layer.GenomeLandT", 0, 0);
                DropAll(Vector3.zero, true);
                FindObjectOfType<GameManager>().AddAlerta(this, true);
                vistaCol.GetComponent<VistaSoldado>().QuitaAsumidos();
            }
        }
        else if (controller.enabled && !tirado && !levantandose)
        {
            if (oxigeno == 100 && EstaVivo())
            {
                //KO = false;
                controller.enabled = false;
                //jugadorAg = null;
                LiberaSoldado(jugadorAg.GetComponent<Snake>(), false, false);
                agente.enabled = false;

                //v = Vector3.down;
                agachado = true;
                //vieneDeCQC = true;
                vistaCol.SetActive(true);
                //vistaCol.GetComponent<VistaSoldado>().QuitaAsumidos();
                soldierAnimator.Play("Base Layer.GenomeWakeUp", 0, 0);
                levantandose = true;
                //agente.enabled = true;
                velocidadTiro = Vector3.zero;
                //Debug.Break();
            }

        }
        else if (levantandose && soldierAnimator.GetCurrentAnimatorStateInfo(0).IsName("Agachado"))
        {
            print("vuelve");
            interacCol.name = "InteraccionSoldado";
            levantandose = false;
            agente.enabled = true;
        }
        //El oxigeno sube mas lento cuando esta KO
        if (vida > 0)
            oxigeno = Mathf.Clamp(oxigeno + (Time.deltaTime *(controller.enabled && !tirado ? .75f : 75)), 0, 100);


        if (sonido.jug != null && (pensamientos.Count == 0 || pensamientos[0].tipo != Pensamiento.JUGADOR))
            //(cosasVistas.Count == 0 || !cosasVistas[0].Contains("verAJug")))
        {
            sonido.jug = null;
            sonido.gameObject.SetActive(false);
        }

        movimiento = agente.enabled ? agente.velocity : (transform.position - lastPos) / Time.deltaTime;

        if(jugadorAg == null)
            soldierAnimator.SetFloat("Velocity", movimiento.magnitude);


        soldierAnimator.SetBool("Agachado", agachado);
        soldierAnimator.SetBool("Tirar", controller.enabled && tirado);
        soldierAnimator.SetBool("Aterriza", controller.isGrounded);
        //soldierAnimator.SetBool("KO", KO);
        //soldierAnimator.SetBool("PassOut", controller.enabled && !tirado);
        soldierAnimator.SetBool("Apuntar", apuntar);
        soldierAnimator.SetBool("Rehen", ContienePensamiento(Pensamiento.REHEN));

        //print(KO);
    }

    private void LateUpdate()
    {

        //Si se bugea y pierde el jugador el contacto con el soldado pero realmente no lo ha liberado, que lo re-pille
        //Descontamos los casos muertos e inconscientes
        if (!controller.enabled && !pillado && jugadorAg != null)
        {
            jugadorAg.GetComponent<Snake>().PillaSoldado(this);
            pillado = true;
        }

        if (!agente.enabled)
            lastPos = transform.position;

        if (antesApagado && agente.enabled)
        {
            antesApagado = false;

            int siguiente = (index + 1) % posiciones.Count;
            SetDestination(posiciones[siguiente]);

        }


        //Pongo aqui lo de que lo agarren de cqc por que tiene menos latencia con la posicion
        if (jugadorAg != null && !controller.enabled && !tirado)// && !KO && !vieneDeCQC)
        {
            //Si no se ha quitado el arma y le han agarrado, que se la quite
            if (armaEnMano != null && Rig.forward != cqcNormal) { armaEnMano = null; SetArma(); }

            transform.position = cqcPos;
            Rig.forward = cqcNormal;
            Rig.localPosition = new Vector3(0, -.832f, 0);


            Debug.DrawRay(transform.position, Rig.forward * 0.25f, Color.red);
            Debug.DrawRay(transform.position + (Rig.forward) * 0.25f, Vector3.up, Color.black);


            if (tiempoInterrog > 0)
            {
                tiempoInterrog -= Time.deltaTime;
                if (tiempoInterrog <= 0)
                {
                    Interroga();
                }
            }
            //Por si de alguna forma el soldado no lo apaga el solo
            if(pensamientos.Count > 0)
            {
                pensamientos = new List<Pensamiento>();
                vistaCol.SetActive(false);
                FindObjectOfType<GameManager>().AddAlerta(this, true);
                FindObjectOfType<GameManager>().AddAlerta(this, true, true);

            }
        }

        //Hacer que si te ven y no lo pongan en alerta lo pongan en alerta
        //Hecho?

        //Apuntar
        if (apuntar && armaEnMano != null && armaEnMano.TipoObjeto() >= 1 && jugadorAg == null)
        {
            /*
            if (pensamientos.Count == 0 || pensamientos[0].tipo != Pensamiento.JUGADOR)
            {
                apuntar = false;
            }
            */
            //Asi apunta con cierto angulo

            //Vector3 angulos = Quaternion.AngleAxis(angBrazos, Rig.right).eulerAngles;

            //lArmAux.Rotate(angulos.x,angulos.y,angulos.z, Space.World);
            
            Vector3 dist = sitioApuntar - transform.position;
            dist.y = 0;

            float angChest = Vector3.SignedAngle(new Vector3(chest.forward.x, 0, chest.forward.z), dist, Vector3.up);

            chest.Rotate(0, angChest, 0, Space.World);
            //chest.Rotate( angChest, 0,0, Space.World);
            //chest.Rotate(0, 0,angChest, Space.World);
            //chest.forward = new Vector3(dist.x, chest.forward.y, dist.z).normalized;

            float dif = 20;
            float angBrazos = Vector3.SignedAngle(lArm.up, dist, chest.right) + dif;
            Vector3 angulos = Quaternion.AngleAxis(angBrazos, chest.right).eulerAngles;
            lArmAux.Rotate(angulos, Space.World); 
            rArmAux.Rotate(angulos, Space.World);

            //Ponemos que no esta a tiro por si acaso
            apuntar = false;

        }
        else if (apuntar)
        {
            apuntar = false;
        }

        if(!apuntar &&
            lArmAux.localEulerAngles != new Vector3(55.40f, 18.18f, 122.1f))
        {

            lArmAux.localEulerAngles = new Vector3(55.40f, 18.18f, 122.1f);
            rArmAux.localEulerAngles = new Vector3(356.5f, 10.2f, 252.4f);
        }

        if (dispAux)
        {
            DispararMetodo();
            dispAux = false;
        }

        //ponemos que pegue si ahora va a pegar
        if (soldierAnimator.GetNextAnimatorStateInfo(1).IsName("GenomePunch1") ||
            soldierAnimator.GetNextAnimatorStateInfo(1).IsName("GenomeCulatazo"))
            pegar = true;

        //si está pegando, calcula los golpes
        if (pegar && collidersAPegar.Count > 0)
        {
            print("golpeEfectivo");
            foreach (GameObject persona in collidersAPegar)
            {

                int i = Random.Range(1, 3);
                ReproduceSonido(Resources.Load<AudioClip>("Audio/Snake/Golpe" + i.ToString()), .5f);

                if (persona.GetComponent<Snake>())
                {
                    persona.GetComponent<Snake>().QuitaVida(2.5f);

                    if (soldierAnimator.GetInteger("PunchCount") == 3)
                        persona.GetComponent<Snake>().Tirate(Rig.forward * 3.5f);
                    continue;
                }
                else if (persona.GetComponent<Soldier>())
                {
                    print("soldadito de plomo");
                    persona.GetComponent<Soldier>().QuitaVida(.5f, transform, false);
                    if (soldierAnimator.GetInteger("PunchCount") == 3)
                        persona.GetComponent<Soldier>().BajaOxigeno(-100);
                }

            }
            collidersAPegar = new List<GameObject>();
            pegar = false;
        }

        else if (pegar && collidersAPegar.Count == 0 && soldierAnimator.GetCurrentAnimatorStateInfo(1).IsName("SinArma"))
        {
            soldierAnimator.SetInteger("PunchCount", 0);
            //print("mierdon de golpe");
            pegar = false;
        }


        if(tiempoPausaPPK == 0 && soldierAnimator.GetInteger("PunchCount") > 0)
        {
            soldierAnimator.SetInteger("PunchCount", 0);
        }

        if (tDesiredVelocity > 0)
            tDesiredVelocity -= Time.deltaTime;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        foreach (Vector3 pos in posiciones)
            Gizmos.DrawLine(pos, pos+ Vector3.up);

        if (agente.enabled)
        {
            Debug.DrawRay(agente.destination, Vector3.up, Color.green);

            Debug.DrawRay(transform.position, agente.desiredVelocity, Color.blue);

            Debug.DrawLine(transform.position, agente.destination, Color.red);
        }
    }

    private void FixedUpdate()
    {
        //print(pensamiento_step);

        if (sonido.gameObject.activeInHierarchy && sonido.name.Contains("_") && sonido.jug == null)
        {
            sonido.gameObject.name = sonido.gameObject.name.Substring(0, sonido.gameObject.name.Length - 1);
            sonido.gameObject.SetActive(false);
        }


        else if (sonido.gameObject.activeInHierarchy && !sonido.name.Contains("_") && sonido.jug == null)
            sonido.gameObject.name += "_";




    }

    public void Vestirse(string hash, bool mujer = false)
    {
        string inicialAColor(string inp)
        {
            if(inp == "R")
            {
                return "Red";
            }
            else if (inp == "M")
            {
                return "Brown";
            }
            else if (inp == "G")
            {
                return "Green";
            }
            else if (inp == "B")
            {
                return "Blue";
            }
            else if (inp == "Y")
            {
                return "Yellow";
            }

            return "";
        }

        string gorroT = hash[0].ToString();
        string bufT = hash[1].ToString();
        string l1T = hash[2].ToString();
        string l2T = hash[3].ToString();


        if (gorroT == "-")
            gorro.enabled = false;

        else
        {
            Material matACargar = Resources.Load<Material>("Materials/RopaSoldados/" + inicialAColor(gorroT));
            Material[] materials = gorro.materials;
            materials[0] = matACargar;
            gorro.materials = materials;

        }


        if (bufT == "-")
            buf.enabled = false;
        else
        {
            Material matACargar = Resources.Load<Material>("Materials/RopaSoldados/" + inicialAColor(bufT));
            Material[] materials = buf.materials;
            materials[0] = matACargar;
            buf.materials = materials;

        }
        if (l1T == "-")
            lazo1.enabled = false;
        else
        {
            Material matACargar = Resources.Load<Material>("Materials/RopaSoldados/" + inicialAColor(l1T));
            Material[] materials = lazo1.materials;
            materials[0] = matACargar;
            lazo1.materials = materials;

        }
        if (l2T == "-")
            lazo2.enabled = false;
        {

            Material matACargar = Resources.Load<Material>("Materials/RopaSoldados/" + inicialAColor(l2T));
            Material[] materials = lazo2.materials;
            materials[0] = matACargar;
            lazo2.materials = materials;
        }

        //Ponemos los audios para el caso de que el soldado sea mujer
        if (mujer)
        {
            //Falta cambiar el modelo
            cuerpo.sharedMesh = femCuerpo;
            terminoGenero = "W";
            cqcCaerGrito = Resources.Load<AudioClip>("Audio/Soldiers/CQCThrowScreamW");
            dolorSonido = Resources.Load<AudioClip>("Audio/Soldiers/SoldadoDolorW");
            ahogueSonido = Resources.Load<AudioClip>("Audio/Soldiers/CQCAhogarW");
            noticeSonido = Resources.Load<AudioClip>("Audio/Soldiers/HesHereW");
        }


    }

    public void SonidoAlerta()
    {
        ReproduceSonido(alertaSonido);
    }
    void RotaRig(float yR = 0, bool conF = false)
    {
        float tiempoRot = 1;
        Vector2 mov = new Vector2(movimiento.x, movimiento.z);
        float newAngle = conF ? yR :Vector2.SignedAngle(mov, Vector2.up);

        float angle = Mathf.MoveTowardsAngle(Rig.eulerAngles.y, newAngle, tiempoRot * 1000 * Time.deltaTime);
        Rig.eulerAngles = new Vector3(0, angle, 0);
    }

    void Golpe(int numero, bool override_ = false)
    {
        print("se espera golpe : " + numero);
        //print("no quiero hacer el void");
        pegar = true;
        //Si no lleva un arma a dos manos, puñetazos y patada normal
        if(override_ || armaEnMano == null || (armaEnMano != null && armaEnMano.TipoObjeto() < 2))
        {
            string nombreEstado = "ArmasLayer.GenomePunch" + numero.ToString();

            tiempoPausaPPK = .2f;

            if (numero == 1)
            {
                nombreEstado = "ArmasLayer.GenomePrepPunch";
                //print("inicial");
                tiempoPausaPPK = .8f;
                pegar = false;
            }

            soldierAnimator.SetInteger("PunchCount", numero);
            soldierAnimator.Play(nombreEstado, 1, 0);
        }
        //Si no, culatazo al canto POM
        else
        {
            soldierAnimator.Play("ArmasLayer.GenomePrepCulatazo", 1, 0);
            tiempoPausaPPK = .8f;
            pegar = false;
        }
        //if (numero == 3)
           // tiempoPausaPPK = 3;
    }

    void AddGolpe()
    {
        //Metodo para hacer que el soldado golpee

        bool enEstadoGolpe = soldierAnimator.GetCurrentAnimatorStateInfo(1).IsName("GenomePunch1") ||
            soldierAnimator.GetCurrentAnimatorStateInfo(1).IsName("GenomePunch2") || soldierAnimator.GetCurrentAnimatorStateInfo(1).IsName("GenomePunch3") ||
            soldierAnimator.GetCurrentAnimatorStateInfo(1).IsName("GenomePrepPunch");

        bool enEstadoCulatazo = soldierAnimator.GetCurrentAnimatorStateInfo(1).IsName("GenomeCulatazo") || 
            soldierAnimator.GetCurrentAnimatorStateInfo(1).IsName("GenomePrepCulatazo");

        //Si esta dando un culatazo ya, no añade nada
        if (enEstadoCulatazo)
            return;

        if (!enEstadoCulatazo && armaEnMano != null && armaEnMano.TipoObjeto() >= 2 && tiempoPausaPPK == 0)
        {
            Golpe(0);
            return;
        }

        int cantDeGolpes = soldierAnimator.GetInteger("PunchCount");
        cantDeGolpes = cantDeGolpes + 1;
        if (cantDeGolpes > 3)
            cantDeGolpes -= 3;
        //print("cantidad de golpes " + cantDeGolpes);
        if((enEstadoGolpe && tiempoPausaPPK <= 0.05f) || !enEstadoGolpe)
        {
            Golpe(cantDeGolpes);

            //if (cantDeGolpes == 3)
                //Debug.Break();
        }


    }

    void ReordenaCosasVistas()
    {

        //Si es tick de "pensar", reordena. Si no, vuelve
        if (pensamiento_step != 0)
            return;


        /* PRIORIDADES EN COSAS ( n >>> n+1)
         *
         * 1) Rehén
         * 
         * 1) Correr de explosivos
         * 
         * 2) Ver a un Jugador
         * 
         * 3) No tiene arma
         * 
         * 4) Ver compañero caído
         * 
         * 5) Checkear zona (para buscar jugador?)
         * 
         */

        //Hacemos que haya la misma cantidad de steps y tiempo que de cosas vistas

        List<int> jugInd = new List<int>();
        List<int> faltaInd = new List<int>();
        List<int> compInd = new List<int>();
        List<int> rehenInd = new List<int>();
        List<int> zoneInd = new List<int>();
        List<int> explInd = new List<int>();


        for(int i = 0; i < pensamientos.Count; i++)
        {
            if (pensamientos[i].tipo == Pensamiento.EXPLOSIVO_CERCA)
                explInd.Add(i);
            else if (pensamientos[i].tipo == Pensamiento.JUGADOR)
                jugInd.Add(i);
            else if (pensamientos[i].tipo == Pensamiento.FALTA_ARMA)
                faltaInd.Add(i);
            else if (pensamientos[i].tipo == Pensamiento.COMP_INC)
                compInd.Add(i);
            else if (pensamientos[i].tipo == Pensamiento.REHEN)
                rehenInd.Add(i);
            else if (pensamientos[i].tipo == Pensamiento.CHECK_ZONA)
                zoneInd.Add(i);
        }

        List<int>[] listaDeTipos = new List<int>[6];
        listaDeTipos[0] = rehenInd; listaDeTipos[1] = explInd;  listaDeTipos[2] = jugInd;
        listaDeTipos[3] = faltaInd; listaDeTipos[4] = compInd; listaDeTipos[5] = zoneInd;


        //Ordenamos cada lista de cada tipo
        for (int j = 0; j < listaDeTipos.Length; j ++)
        {
            //Si ha visto a un jugador el  resto de cosas las "olvida"; Ahorra en memoria
            if (j > 2 && listaDeTipos[2].Count > 0)
            {
                listaDeTipos[j] = new List<int>();
                continue;
            }

            List<int> tipo = listaDeTipos[j];
            List<float> distL = new List<float>();
            int[] sortedTipo = new int[tipo.Count];

            for(int i = 0; i < tipo.Count; i++)
            {
                Vector3 sitioPos = pensamientos[tipo[i]].datos[0].GetVector();
                float dist = Vector3.SqrMagnitude(sitioPos - transform.position);
                distL.Add(dist);
            }

            distL.Sort();

            for(int i = 0; i < tipo.Count; i++)
            {
                Vector3 sitioPos = pensamientos[tipo[i]].datos[0].GetVector();
                float dist = Vector3.SqrMagnitude(sitioPos - transform.position);
                int pos = distL.IndexOf(dist);
                sortedTipo[pos] = tipo[i];
            }

            listaDeTipos[j] = new List<int>();
            listaDeTipos[j].AddRange(sortedTipo);
        }

        //Hacemos una lista con los índices ordenados
        indicesFinal = new List<int>();
        for (int i = 0; i < 5; i++) { indicesFinal.AddRange(listaDeTipos[i]); }

        //Mapeamos todo a su indice ordenado
        List<Pensamiento> nuevosPensam = new List<Pensamiento>();
        for (int i = 0; i < indicesFinal.Count; i++)
        {
            nuevosPensam.Add(pensamientos[indicesFinal[i]]);
        }
        pensamientos = nuevosPensam;
    }

    public float SacaNumString(string input)
    {
        float numero = float.Parse(input, CultureInfo.InvariantCulture.NumberFormat);
        return numero;
    }

    public Vector3 SacaVecDeString(string input)
    {
        //Lo separamos con ;
        string[] modulos = input.Split(";");
        if (modulos.Length < 3)
            return Vector3.zero;

        float x = SacaNumString(modulos[0]);
        float y = SacaNumString(modulos[1]);
        float z = SacaNumString(modulos[2]);
        return new Vector3(x, y, z);

    }

    public void AgarreSoldado(Snake jugador, bool contraAt = false, float cantCorrer = 0)
    {
        jugadorAg = jugador.transform;
        agente.enabled = false;
        col.SetActive(false);
        pillado = true;
        //DropArma(armaEnMano);
        //Balance para que no sea tan descarado lo de las armas
        //DropAll(Rig.forward, true);

        //Si es un contra-ataque
        if (contraAt)
        {
            Vector3 jugPos = jugadorAg.position;
            Vector3 direccionAMirar = -jugador.Rig.forward;
            direccionAMirar.y = 0; direccionAMirar = direccionAMirar.normalized;
            Rig.forward = direccionAMirar;
            transform.position = jugPos - (direccionAMirar * (0.25f + (cantCorrer * 0.15f)));// + (Vector3.down * 0.1f);
            Debug.DrawRay(transform.position, direccionAMirar * 0.25f, Color.red);
            Debug.DrawRay(jugPos, Vector3.up, Color.black);

            //Vemos que animacion de contra-ataque hay que reproducir
            bool golpe = armaEnMano == null || (armaEnMano != null && armaEnMano.TipoObjeto() < 2);

            if (golpe)
                soldierAnimator.Play("Base Layer.GenomePunchFail", 0, 0);
            //Debug.Break();
        }

        //Si no, reproducimos la animacion de agarrar
        else
        {
            Vector3 direccionAMirar = -jugadorAg.position + transform.position;
            float dotRigs = Vector3.Dot(Rig.forward, direccionAMirar);

            //Reproducimos la animacion de recibir el golpe dependiendo de donde mire y tal
        }


        //Esto lo ponemos para cuando lo pille
        //armaEnMano = null;
        //SetArma();

        //Borra todo lo que ha visto y desactiva el collider de visión. También se quita de alertas y/o búsquedas
        pensamientos = new List<Pensamiento>();

        vistaCol.SetActive(false);


        FindObjectOfType<GameManager>().AddAlerta(this, true);
        FindObjectOfType<GameManager>().AddAlerta(this, true, true);


    }

    public void LiberaSoldado(Snake jugador, bool moverse = true, bool alerta = true)
    {
        print("entro");
        jugadorAg = null;
        RaycastHit compPar;
        float dist = 0.35f;
        if(Physics.Raycast(transform.position, Rig.forward,out compPar, dist, suelo))
        {
            cqcNormal *= -1;
        }

        if(moverse)
            transform.position += cqcNormal * dist;

        agente.enabled = true;
        col.SetActive(true);
        agachado = false;
        pillado = false;
        vistaCol.SetActive(!controller.enabled);
        //vistaCol.GetComponent<VistaSoldado>().CheckZone(cqcPos);

        //No hay caja porque no puedes pillar con cqc a nadie vestido de caja
        if(jugador != null && !controller.enabled && alerta)
            vistaCol.GetComponent<VistaSoldado>().Alerta(jugador.transform, false, Vector3.zero, false, false, jugador.playerID);

        SeleccionaArma();
    }

    void SeleccionaArma(int ind = 0)
    {
        if (armasSoldado.Count == 0)
            return;
        ind = Mathf.Clamp(ind, 0, armasSoldado.Count - 1);
        armaEnMano = armasSoldado[ind];
        SetArma();
    }

    public void DatosCQC(Vector3 normal, bool agach, Vector3 pos, bool posB = true, float vel = 0)
    {
        agachado = agach;
        //Hacer que mire en la normal solo si esta en held
        //Si no, la normal es rig forward
        if (soldierAnimator.GetCurrentAnimatorStateInfo(0).IsName("GenomeHeldStand") ||
            soldierAnimator.GetCurrentAnimatorStateInfo(0).IsName("GenomeHeldAgach"))
        {
            cqcNormal = normal;
            cqcPos = pos;
            //  Debug.Break();
        }
        else
        {
            cqcNormal = Rig.forward;
            cqcPos = transform.position;
        }


        soldierAnimator.SetFloat("Velocity", vel);
        pillado = true;
        /*
        if (controller.enabled)
        {
            pillado = false;
            vistaCol.SetActive(false);
            controller.enabled = false;
            transform.position = cqcPos;
            controller.enabled = true;
            setPos = posB;
            v = normal * (impulsa ? 1 : 0) * 3f;
            v.y = 1;

            if (EstaVivo())
            {
                ReproduceSonido(cqcCaerGrito, .15f);
            }

            if (jugadorAg != null)
                jugadorAg.GetComponent<Snake>().EliminaSoldado();
        }*/
    }

    public bool Rehen(Vector3 posicionJug, bool ag, bool fuerza = false)
    {
        if (!ContienePensamiento(Pensamiento.REHEN) && jugadorAg == null)//(!cosasVistas.Contains("rehen") && jugadorAg == null)
        {
            //Hay que ver si el jugador no está a la vista
            if (!fuerza)
            {
                Vector3 direccionMirada = Rig.forward;
                Vector3 direccionJug = posicionJug - transform.position;
                if (Vector3.Dot(direccionJug.normalized, direccionMirada) > 0.35f && Mathf.Abs(direccionJug.y) < controller.height / 3)
                    return false;
            }
            //cosasVistas.Add("rehen");
            Dato datoPos = new Dato(Vector3.zero);
            pensamientos.Add(new Pensamiento(Pensamiento.REHEN, datoPos));

            armaEnMano = null;
            SetArma();
            agachado = ag;
            FindObjectOfType<GameManager>().AddAlerta(this, true);
            return !(soldierAnimator.GetCurrentAnimatorStateInfo(0).IsName("RehenStand") || soldierAnimator.GetCurrentAnimatorStateInfo(0).IsName("RehenAg"));
        }
        else if(jugadorAg == null)
        {

            agachado = ag;
            int indexRehen = IndexOfPensamiento(Pensamiento.REHEN);//cosasVistas.IndexOf("rehen");
            if (indexRehen < 0)
                return false;
            pensamientos[indexRehen].step = 0;//cosasSteps[indexRehen] = 0;
        }
        return false;
    }

    public void Throw(Snake jugador, Vector3 direccion, float fuerza, bool mira = true, float vY = 0)
    {
        //Si ya esta tirandose o en el suelo no se tira otra vez
        if (controller.enabled)
            return;

        {
            /*
            if (pillado)
            {
                Snake elQueTePilla = jugadorAg.GetComponent<Snake>();
                elQueTePilla.Libera();
            }
            impulsa = tirar;

            int signo = -1;
            if(jugador != null)
                jugadorAg = jugador.transform;
            else if(aux != null)
            {
                signo = 1;
                jugadorAg = aux;
            }

            //para = true;
            SetPPKT(1);
            agente.enabled = false;
            col.SetActive(true);
            controller.enabled = true;
            oxigTarget = target;
            //DropArma(armaEnMano, signo * Rig.forward, true);
            DropAll(signo * Rig.forward, true);
            armaEnMano = null;
            SetArma();
            SonidoRadio(4.5f);
            if(FindObjectOfType<GameManager>().haIncrementado())
            {
                FindObjectOfType<GameManager>().cambiaAlertas(-1);
                FindObjectOfType<GameManager>().AddAlerta(this, true);
            }
            */
        }

        //Ponemos a un jugador como referencia para que no entre en el metodo de movimiento normal
        if (jugador != null)
            jugadorAg = jugador.transform;
        else
            jugadorAg = FindObjectOfType<Snake>().transform;

        //Le decimos que se ponga como inconsciente
        interacCol.name = "InteraccionSoldadoInc";

        //Activamos al controller y desactivamos el movimiento del agente
        controller.enabled = true;
        agente.enabled = false;

        //Ponemos la velocidad en la que se tiene que mover
        velocidadTiro = (direccion * fuerza) + ((1 + vY) * Vector3.up);

        //Le avisamos de que está siendo tirado, para no confundirlo con que le estan agarrando
        tirado = true;

        //Le decimos que mire en la direccion en la que le tiran si tiene que mirar
        if (mira)
            Rig.forward = direccion;

        //Hacemos que no tenga el arma en la mano
        armaEnMano = null;
        SetArma();

        //Hacemos que no vea
        DesactivaVista();

        //Debug.Break();
    }

    public void BajaOxigeno(float valor)
    {
        oxigeno -= valor;
        oxigeno = Mathf.Clamp(oxigeno, 0, 100);
    }

    public void QuitaVida(float cantidad, Transform tirador, bool sonido = true,float tiempoParar = 0.25f)
    {
        print("quita vida");
        vida = Mathf.Clamp(vida - cantidad, 0, 100);
        tiempoPausaPPK = tiempoParar;
        float fac = vida / 100;
        if(fac < facSangre)
        {
            facSangre = fac;
            cuerpo.materials[0].SetFloat("_SangreFac", 1-fac);
        }

        //Animacion Daño
        if (EstaVivo())
        {
            Dolor(sonido);
        }

        //Al morir quitar CQC
        else
        {
            if (jugadorAg != null && !controller.enabled)
            {
                Snake snake = jugadorAg.GetComponent<Snake>();
                controller.enabled = true;
                jugadorAg.GetComponent<Snake>().Libera();
                controller.enabled = false;
                Throw(snake, cqcNormal, .1f);

                pensamientos = new List<Pensamiento>();//cosasVistas = new List<string>();


                DesactivaVista();
            }

            if (cantidad >= 100 && sonido)
            {
                ReproduceSonido(Resources.Load<AudioClip>("Audio/Armas/Headshot"), 0.15f);
            }
            
            FindObjectOfType<GameManager>().AddAlerta(this, true, true);
            FindObjectOfType<GameManager>().AddAlerta(this, true);
            FindObjectOfType<GameManager>().MataSoldado(gameObject, tirador);
        }


    }

    public bool Consciente()
    {
        return oxigeno > 0;//&& !KO;
    }

    public void SonidoAgarre()
    {
        ReproduceSonido(ahogueSonido);
        SonidoRadio(1.5f);
    }

    public bool EstaVivo()
    {
        return vida > 0;
    }

    public bool Agachado()
    {
        return agachado;
    }

    public void SetArma()
    {
        if(armaEnMano == null)
        {
            armaHolder.GetComponent<MeshFilter>().mesh = null;
            armaHolder.GetComponent<MeshRenderer>().material = null;
            soldierAnimator.SetFloat("ArmaTipo", 0);
            return;

        }
        else
        {
            armaHolder.GetComponent<MeshFilter>().mesh = armaEnMano.modeloArma;
            armaHolder.GetComponent<MeshRenderer>().material = armaEnMano.materialArma;
            soldierAnimator.SetFloat("ArmaTipo", armaEnMano.TipoObjeto() > 0 ? armaEnMano.TipoObjeto() : .5f);
        }
    }

    public void EmpiezaInterroga(Snake jug)
    {
        tiempoInterrog = 1;
    }

    void Interroga()
    {
        if (informacion.Count > 0 && FindObjectOfType<GameManager>().RecibeInformacion(informacion[0]))
        {

            //Hacemos que el jugador reciba informacion


            int i = Random.Range(1, 4);
            ReproduceSonido(Resources.Load<AudioClip>("Audio/Soldiers/Interroga" + i.ToString() + terminoGenero));

            informacion.RemoveAt(0);

        }
        else
        {
            //Si ha fallado el interrogatorio porque es información de algo ya hecho, entonces borra esa información y ya
            if (informacion.Count > 0)
                informacion.RemoveAt(0);


            ReproduceSonido(ahogueSonido);
        }
    }

    public float Vida()
    {
        return vida;
    }

    public bool RecibeArma(Arma arma, int balas_ = 0)
    {
        if (!EstaVivo() || !Consciente() || jugadorAg != null)
            return false;
        if (armasSoldado.Contains(arma))
        {
            print("balas");
            int indexArma = armasSoldado.IndexOf(arma);
            int b = balas[indexArma];
            balas[indexArma] = Mathf.Min(balas[indexArma] + balas_, arma.cargador());
            return true;
        }
        else
        {
            print("arma");
            armasSoldado.Add(arma);
            balas.Add(balas_);
            balasFront.Add(Mathf.Min(balas_, arma.balasArma()));
            if(armaEnMano == null)
            {
                armaEnMano = armasSoldado[0];
                SetArma();
            }
            return true;
        }
    }

    public bool RecibeBalas(Arma arma, int balas_ = 0)
    {
        if (armasSoldado.Contains(arma))
            RecibeArma(arma, balas_);
        return false;
    }

    public void TriggerEnter(Collider other)
    {
        if(other.name == "InteraccionSoldadoInc")
        {
            CustomThrow(other.gameObject);
        }
        else if(pensamientos.Count > 0 && other.name == "InteraccionSoldado" && 
            pensamientos[0].tipo == Pensamiento.REHEN && other != interacCol)
        {
            cabeza.GetComponent<RecibeSonidos>().disabled = false;
            pensamientos.RemoveAt(0);
            interacCol.name = "InteraccionSoldado";
        }
    }

    public void TriggerExit(Collider other)
    {

    }

    public void TriggerStay(Collider other)
    {
        if(other.name.Contains("InteraccionSoldado"))
        {
            if (pegar)
            {
                //Quitale vida al jugador
                if (collidersAPegar.Contains(other.transform.parent.parent.gameObject))
                    return;

                collidersAPegar.Add(other.transform.parent.parent.gameObject);

            }
            if(other.name == "InteraccionSoldadoInc")
            CustomThrow(other.gameObject);
        }


        else if(pegar && other.gameObject.layer == LayerMask.NameToLayer("InteraccionSnake"))
        {
            //Quitale vida al jugador
            Vector3 distNormalizada = (other.transform.position - transform.position).normalized;
            if (Vector3.Dot(distNormalizada, Rig.forward) < 0.2f || collidersAPegar.Contains(other.transform.parent.parent.gameObject))
                return;
            collidersAPegar.Add(other.transform.parent.parent.gameObject);

        }
    }

    void CustomThrow(GameObject other)
    {
        //Chequea si lo están tirando contra el y si a el lo pueden mover
        if (controller.enabled)
            return;
        else
        {
            Soldier soldadoImpulsado = other.transform.parent.parent.GetComponent<Soldier>();

            if (!soldadoImpulsado.controller.enabled || (soldadoImpulsado.controller.enabled && !soldadoImpulsado.tirado)) //Si no lo están tirando o ya ha caído al suelo, no te impulsa
                return;

            Throw(soldadoImpulsado.jugadorAg.GetComponent<Snake>(), soldadoImpulsado.velocidadTiro.normalized, soldadoImpulsado.velocidadTiro.magnitude);

            //Throw(soldadoImpulsado.jugadorAg.GetComponent<Snake>(), soldadoImpulsado.impulsa, soldadoImpulsado.oxigTarget);


            //interacCol.name = "InteraccionSoldadoInc";

            //DatosCQC(soldadoImpulsado.cqcNormal, agachado, transform.position, EstaVivo() || Consciente());
        }
    }

    public void SetPPKT(float t)
    {
        t = Mathf.Clamp01(t);
        tiempoPausaPPK = t;
    }

    void CheckRadar()
    {
        if((!EstaVivo() || !Consciente()) &&modoRadar != -1)
        {
            modoRadar = -1;
            fovRadar.materials[0].EnableKeyword("_MODO_MUERTO");
            fovRadar.materials[0].DisableKeyword("_MODO_CAUTION");
            fovRadar.materials[0].DisableKeyword("_MODO_NORMAL");
            fovRadar.materials[0].DisableKeyword("_MODO_ALERT");
        }

        else if (modoRadar != 1 && pensamientos.Count > 0 && pensamientos[0].tipo != Pensamiento.JUGADOR)
        {
            modoRadar = 1;
            fovRadar.materials[0].EnableKeyword("_MODO_CAUTION");
            fovRadar.materials[0].DisableKeyword("_MODO_NORMAL");
            fovRadar.materials[0].DisableKeyword("_MODO_ALERT");
            fovRadar.materials[0].DisableKeyword("_MODO_MUERTO");
        }

        else if (modoRadar != 2 && pensamientos.Count > 0 && pensamientos[0].tipo == Pensamiento.JUGADOR)
        {
            modoRadar = 2;
            fovRadar.materials[0].EnableKeyword("_MODO_ALERT");
            fovRadar.materials[0].DisableKeyword("_MODO_NORMAL");
            fovRadar.materials[0].DisableKeyword("_MODO_CAUTION");
            fovRadar.materials[0].DisableKeyword("_MODO_MUERTO");
        }

        else if(modoRadar != 0 && pensamientos.Count == 0 && EstaVivo() && Consciente())
        {
            modoRadar = 0;
            fovRadar.materials[0].EnableKeyword("_MODO_NORMAL");
            fovRadar.materials[0].DisableKeyword("_MODO_ALERT");
            fovRadar.materials[0].DisableKeyword("_MODO_CAUTION");
            fovRadar.materials[0].DisableKeyword("_MODO_MUERTO");
        }
    }

    public bool Caution()
    {
        return caution;
    }

    public void ActivaCaution()
    {
        caution = true;
        vistaCol.transform.localScale = new Vector3(.075f, .075f, .075f);
    }

    public void DesactivaCaution()
    {
        caution = false;
        vistaCol.transform.localScale = new Vector3(.055f, .055f, .055f);
    }

    void DispararMetodo()
    {
        if (armaEnMano == null)
            return;
        soldierAnimator.Play("ArmasLayer.Disparar", 1, 0);
        apuntar = true;
        if(armaEnMano.TipoObjeto() >= 1)
        { 
                //Pone el tiempo a esperar para disparar
                tiempoCad = 1/armaEnMano.Cadencia();

                //Reproduce sonido de disparo
                ReproduceSonido(armaEnMano.sonido(), 0.5f);

                //Quita balas
                int indArma = armasSoldado.IndexOf(armaEnMano);
                balas[indArma] -= 1;
                balasFront[indArma] -= 1;

            

        }
        else
        {
            //Suponemos que es una granada, entonces
            tiempoCad = 0.25f;
        }

        Vector3 direccionDisparar = sitioApuntar - transform.position;
        Vector3? posDisparo = armaEnMano.Disparar(transform.position, direccionDisparar, Vector3.up, transform, false);
        if(posDisparo != null)
        {
            
            Vector3 inicio = armaHolder.position; Vector3 final = posDisparo.Value;

            //Añadimos las nuevas posiciones
            int cantPos = trailBala.positionCount + 2;
            Vector3[] posiciones = new Vector3[cantPos];
            for (int i = 0; i < trailBala.positionCount; i++)
            {
                posiciones[i] = trailBala.GetPosition(i);
            }
            posiciones[trailBala.positionCount] = inicio; posiciones[trailBala.positionCount + 1] = final;


            trailBala.positionCount = cantPos;
            trailBala.SetPositions(posiciones);
            StartCoroutine(TiempoBalas());
            //Debug.Break();
        }

        //Debug.Break();
    }

    void DropArma(Arma arma, Vector3 direccionTiro = new Vector3(), bool customDir = false)
    {
        if (arma == null)
            return;

        GameObject caja = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Default_Caja"));

        int indexArma = armasSoldado.IndexOf(arma);

        int balasAñadir = (balas[indexArma] - arma.balasArma());

        caja.GetComponent<CajaObjeto>().Crear(arma, false, balasAñadir);

        caja.transform.position = transform.position + (customDir ? direccionTiro : Rig.forward).normalized * .15f;


        caja.GetComponent<Rigidbody>().velocity = (customDir ? direccionTiro : Rig.forward) * 3;

        if (armasSoldado[indexArma] == armaEnMano && armasSoldado.Count > 1)
        {
            int indArm = ((indexArma-1) % armasSoldado.Count + armasSoldado.Count) % armasSoldado.Count;
            armaEnMano = armasSoldado[indexArma];
        }
        else if (armasSoldado[indexArma] == armaEnMano && armasSoldado.Count == 1)
        {
            armaEnMano = null;
        }

        armasSoldado.RemoveAt(indexArma);
        balas.RemoveAt(indexArma);
        balasFront.RemoveAt(indexArma);
        SetArma();
    }

    void DropAll(Vector3 direccionTiro, bool customDir = false)
    {
        if (armasSoldado.Count == 0)
            return;

        int cant = armasSoldado.Count; //??
        for (int i = 0; i < cant; i++)
        {
            Vector3 cambio = Vector3.zero;
            if(customDir)
                cambio= (Rig.right * Random.Range(-1.0f, 1.0f)) + (Vector3.up * Random.Range(-1.0f, 1.0f));
            DropArma(armasSoldado[0], direccionTiro + cambio, customDir);
        }
        armaEnMano = null;
    }

    void Recargar()
    {
        if (armaEnMano == null || (armaEnMano != null && armaEnMano.TipoObjeto() == 0))
            return;

        int indArm = armasSoldado.IndexOf(armaEnMano);
        balasFront[indArm] = Mathf.Min(armaEnMano.balasArma(), balas[indArm]);
    }

    public void Agachate(bool v)
    {
        agachado = v;
    }

    public void BuscaJug(Snake jug)
    {
        int j = -1;
        for(int i = 0; i< pensamientos.Count; i++)
        {
            //string[] modulos = cosasVistas[i].Split("_");
            if (pensamientos[i].tipo == Pensamiento.CHECK_ZONA)
            { j = i; break; }
            else if (pensamientos[i].tipo == Pensamiento.JUGADOR &&
                pensamientos[i].datos[pensamientos[i].datos.Length - 1].GetInt() == jug.playerID)
            { return; }

        }

        Vector3 posBuscar = jug.transform.position + ((jug.transform.position - transform.position).normalized * 0.1f);
        //Debug.DrawLine(posBuscar, posBuscar + Vector3.up, Color.cyan);
        para = false;
        //Debug.Break();
        if ( j > -1)
        {
            if (pensamientos[j].datos.Length == 2)
            {
                pensamientos[j].datos[0] = new Dato(posBuscar);
                pensamientos[j].datos[1] = new Dato(jug.playerID);
            }
            else
            {
                Dato[] datos = new Dato[2];
                datos[0] = new Dato(posBuscar);
                datos[1] = new Dato(jug.playerID);
                pensamientos[j].datos = datos;

            }

            pensamientos[j].tiempo = 0;
            pensamientos[j].step = 0;
        }
        else
        {
            Dato dato_pos = new Dato(posBuscar);
            Dato dato_id = new Dato(jug.playerID);
            Pensamiento pensam_busca = new Pensamiento(Pensamiento.CHECK_ZONA, dato_pos, dato_id);
            /*string cosa = "chkZone_" + VectorAString(jug.transform.position) + "_" + jug.playerID.ToString();
            cosasVistas.Add(cosa);*/
            pensamientos.Add(pensam_busca);
        }
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
        int cantPos = trailBala.positionCount - 2;
        if (cantPos <= 0)
            trailBala.positionCount = 0;
        Vector3[] posiciones = new Vector3[cantPos];
        for (int i = 0; i < trailBala.positionCount - 2; i++)
        {
            posiciones[i] = trailBala.GetPosition(i + 2);
        }
        trailBala.positionCount = cantPos;
        trailBala.SetPositions(posiciones);

    }

    public void DesactivaVista()
    {
        vistaCol.SetActive(false);
    }

    public bool EnAlerta()
    {
        return ContienePensamiento(Pensamiento.JUGADOR);
    }

    public void SonidoRadio(float radius)
    {
        if ((sonido.GetComponent<SphereCollider>().radius > radius && sonido.gameObject.activeInHierarchy)
            || sonido.jug != null)
            return;

        sonido.GetComponent<SphereCollider>().radius = radius;
        sonido.gameObject.SetActive(true);
    }

    public void AddInfo(int infoClave, int modo = 0) //Modo = 0 -> eliminacion | modo = 1 -> ficheros | modo = 2 -> columnas
    {
        string sitio = "";
        string info = "";
        switch (modo)
        {
            case 0:

                if (infoClave == 4)
                {
                    sitio = FindObjectOfType<GameManager>().patrolZone;

                    //Quitale los numeros a los nombres
                    sitio = sitio.Replace("1", "");
                    sitio = sitio.Replace("2", "");
                    sitio = sitio.Replace("3", "");
                    sitio = sitio.Replace("4", "");

                    informacion.Add("The soldier patrols around the " + sitio+ ";" + infoClave.ToString());
                }
                else
                {

                    string[] vestimenta = FindObjectOfType<GameManager>().GetVest(infoClave);

                    if (vestimenta[1] == "-")
                    {
                        informacion.Add("The soldier doesn't wear a " + vestimenta[0]+ ";" + infoClave.ToString());
                    }
                    else
                    {
                        informacion.Add("The soldier wears a " + vestimenta[1] + " " + vestimenta[0]+ ";" + infoClave.ToString());
                    }
                }
                break;
            case 1:
                //Si es mision de recuperar ficheros, la información que pueden tener los soldados es la sala donde se 
                //encuentran

                Transform ficheroConocido = FindObjectOfType<GameManager>().ficheros[infoClave].transform;
                sitio = ficheroConocido.parent.name;


                //Quitale los numeros a los nombres
                sitio = sitio.Replace("1", "");
                sitio = sitio.Replace("2", "");
                sitio = sitio.Replace("3", "");
                sitio = sitio.Replace("4", "");


                info = "A classified file is in the " + sitio + ";" + infoClave.ToString();
                informacion.Add(info);
                break;
            case 2:
                //Si es mision de destruir columnas, la información que pueden tener los soldados es la sala donde se 
                //encuentran

                Transform columnaConocida = FindObjectOfType<GameManager>().columnas[infoClave].transform;
                sitio = columnaConocida.parent.name;


                //Quitale los numeros a los nombres
                sitio = sitio.Replace("1", "");
                sitio = sitio.Replace("2", "");
                sitio = sitio.Replace("3", "");
                sitio = sitio.Replace("4", "");


                info = "A main pillar is in the " + sitio + ";" + infoClave.ToString();
                informacion.Add(info);
                break;
        }
    }

    public MeshRenderer ropa(int num)
    {
        if (num == 0 && gorro.enabled)
            return gorro;
        else if (num == 1 && buf.enabled)
            return buf;
        else if (num == 2 && lazo1.enabled)
            return lazo1;
        else if (num == 3 && lazo2.enabled)
            return lazo2;
        return null;
    }

    public void Manta()
    {
        GameObject manta = Instantiate(Resources.Load<GameObject>("Prefabs/Bag"));
        manta.transform.position = this.transform.position + (Vector3.down * .55f);
        manta.transform.eulerAngles = new Vector3(0, -chest.eulerAngles.y, 0);
        Destroy(this.gameObject, 0);


    }

    void ReproduceSonido(AudioClip clip, float volumeMult = 1)
    {
        sonidoSoldado.pitch = 1;
        if (sonidoSoldado.spatialBlend == 1)
            sonidoSoldado.PlayOneShot(clip, volumeMult);
        else
            FindObjectOfType<GameManager>().SonidoFisico(sonidoSoldado, clip, 2, 0, volumeMult);
    }

    void SetDestination(Vector3 destination_, bool override_ = false)
    {
        destination = destination_;
        if (override_)
            agente.SetDestination(destination);
    }

    public bool ContienePensamiento(int tipo)
    {
        for(int i = 0; i < pensamientos.Count; i++)
        {
            if (pensamientos[i].tipo == tipo)
                return true;
        }
        return false;
    }


    public bool ContieneExacPensamiento(int tipo, Vector3 sitio)
    {
        for (int i = 0; i < pensamientos.Count; i++)
        {
            if (pensamientos[i].tipo == tipo && pensamientos[i].datos[0].GetVector() == sitio)
                return true;
        }
        return false;
    }

    public int IndexOfPensamiento(int tipo)
    {
        for(int i = 0; i < pensamientos.Count; i++)
        {
            if (pensamientos[i].tipo == tipo)
                return i;
        }
        return -1;
    }

    public void ActualizaPensamiento()
    {
        pensamiento_step = (pensamiento_step + 1 + ID) % 20;
    }

    public bool EstaPensando()
    {
        return pensamiento_step == 0;
    }

    public Vector3 Velocity()
    {
        return agente.enabled ? agente.desiredVelocity : Vector3.zero;
    }

    void PersigueJugador()
    {
        pensamientos[0].tiempo -= Time.deltaTime;
        //Si ve al jugador, que le tome un segundo reaccionar
        if (pensamientos[0].step == -2)
        {
            if (tiempoPausaPPK <= 0)
            {
                pensamientos[0].step = -1;
                FindObjectOfType<GameManager>().AddAlerta(this, overlook: true);
                ReproduceSonido(noticeSonido);
            }
            return;
        }
        else if (pensamientos[0].step == -3)
        {
            tiempoPausaPPK = .5f;
            pensamientos[0].step = -2;
            return;
        }

        //Pone el sonido para que los otros lo detecten
        if (pensamientos[0].step == -1)
        {
            sonido.gameObject.SetActive(true);
            //Radio amplio, las alertas penalizan
            //9 parece que es demasiado injusto
            sonido.GetComponent<SphereCollider>().radius = 6.75f;
            foreach (Snake jug in FindObjectsOfType<Snake>())
            {
                if (jug.playerID == pensamientos[0].datos[pensamientos[0].datos.Length - 1].GetInt())
                {
                    sonido.jug = jug;
                    break;
                }
            }

            pensamientos[0].step = 0;
            return;
        }
        sonido.time = pensamientos[0].tiempo;
        Vector3 sitioPos = pensamientos[0].datos[0].GetVector();
        //Apunta si lo tiene a tiro y no lleva un rehen
        bool rehenSnake = pensamientos[0].datos[3].GetInt() == 1;
        if (rehenSnake)
        {
            rehenSnake = Vector3.Dot(Rig.forward, pensamientos[0].datos[4].GetVector()) < 0.1f;
        }

        apuntar = pensamientos[0].tiempo > 2.0f && puedeDisparar && !rehenSnake;//pensamientos[0].datos[1].GetInt() == 1;

        //Si no puede disparar intenta alejarse un poco del jugador

        //if (!puedeDisparar && distActual < distNiveles[1])
        //{
        //    CorreDe(sitioPos);
        //}

        if (pensamientos[0].step == 0)
            AddGolpe();

        sitioApuntar = sitioPos;

        //Si pierde al jugador, que transforme esta instruccion a checkear la zona donde lo perdió
        if (pensamientos[0].tiempo < 0)
        {
            //Lo intentamos quitar de asumidos si esta ahi
            vistaCol.GetComponent<VistaSoldado>().QuitaAsumido(pensamientos[0].ultimoDato().GetInt());
            vistaCol.GetComponent<VistaSoldado>().QuitaPerseguido(pensamientos[0].ultimoDato().GetInt());

            //Debug.Break();

            Dato datoPos = new Dato(sitioPos);
            Dato datoID = new Dato(pensamientos[0].ultimoDato().GetInt());//new Dato(pensamientos[0].datos[pensamientos[0].datos.Length - 1].GetInt());
            pensamientos[0] = new Pensamiento(Pensamiento.CHECK_ZONA, datoPos, datoID);

            FindObjectOfType<GameManager>().AddAlerta(this, true);
            FindObjectOfType<GameManager>().AddAlerta(this, false, true);
            Agachate(false);
            return;
        }

        //Hacer tácticas de combate
    }

    void CorreDe(Vector3 pos)
    {

        Vector3 direccion = (pos - transform.position).normalized;
        Vector3 direccionOpuesta = transform.position - (direccion * 5);
        SetDestination(direccionOpuesta);
    }

    public bool HaLlegado()
    {
        Vector2 posTop = new Vector2(transform.position.x, transform.position.z);
        Vector2 destTop = new Vector2(destination.x, destination.z);

        bool res = (Vector2.SqrMagnitude(posTop - destTop) < 0.1f) || (Vector3.SqrMagnitude(agente.desiredVelocity) == 0 && tDesiredVelocity <= 0);
        if (Vector3.SqrMagnitude(agente.desiredVelocity) == 0 && tDesiredVelocity <= 0)
        {
            tDesiredVelocity = 0.5f;
            Debug.DrawRay(transform.position, Vector3.up, new Color(.5f, .1f, .5f));
            //Debug.Break();
        }
        else if (res)
        {
            Debug.DrawRay(transform.position, Vector3.up, Color.black);
        }
        return res;
    }

    void CompletaPensamiento()
    {

        //(cosasVistas.Count > 1)
        if (pensamientos.Count > 1)
            ReordenaCosasVistas();
        //para = true;
        //Formato de cosas vistas

        /*
        tipo -> tipo de pensamiento
        tiempo -> tiempo asociado al paso del pensamiento
        step -> paso/proceso en el que se encuentra el pensamiento
        datos -> datos asociados
            datos[0] -> posicion (OBLIGATORIO)

        En caso de que el pensamiento sea seguir a jugador
            datos[1] -> en caja
            datos[2] -> si tiene a alguien agarrado
            datos[3] -> a donde mira, si no tiene a nadie agarrado, V3zero
            datos[4] -> tactica de combate
            datos[5] -> id jugador

        En caso de que el pensamiento sea esquivar una granada
            datos[1] -> id del explosivo
            datos[2] -> si el tirador es este soldado

        En caso de que el pensamiento sea chequear una zona
            datos[1] -> id de jugador si checkea la zona donde lo perdio de vista

        En caso de que el pensamiento sea levantar a un compañero
            datos[1] -> id del compañero

        */
        correr = true;
        if (agachado && pensamientos[0].tipo != Pensamiento.JUGADOR)
            agachado = false;


        //Dependiendo del pensamiento prioritario (el primero), hace una cosa u otra
        //Se aleja de los explosivos
        if (pensamientos[0].tipo == Pensamiento.EXPLOSIVO_CERCA)
        {
            if (pensamientos[0].step == 0)
            //(cosasSteps[0] == 0)
            {
                agachado = false;
                pensamientos[0].tiempo = 1f;//cosasTiempo[0] = 1f;
                pensamientos[0].step = 1;//cosasSteps[0] = 1;
            }

            //cosasTiempo[0] -= Time.deltaTime;
            pensamientos[0].tiempo -= Time.deltaTime;

            Vector3 direccion = (pensamientos[0].datos[0].GetVector() - transform.position).normalized;
            Vector3 direccionOpuesta = transform.position - (direccion * 5);
            if (Vector3.Distance(transform.position, pensamientos[0].datos[0].GetVector()) <= 6)
            {
                SetDestination(direccionOpuesta);
            }

            Debug.DrawLine(transform.position, direccionOpuesta, Color.white);
            if (pensamientos[0].tiempo < 0)
            //(cosasTiempo[0] < 0)
            {
                pensamientos.RemoveAt(0);
                //cosasVistas.RemoveAt(0);
                //cosasSteps.RemoveAt(0);
                //cosasTiempo.RemoveAt(0);
                int siguiente = (index + 1) % posiciones.Count;
                SetDestination(posiciones[siguiente]);
            }
        }

        //Si no tiene un arma, que se diriga al armería
        else if (pensamientos[0].tipo == Pensamiento.FALTA_ARMA)
        {
            if (pensamientos[0].step == 0)//(cosasSteps[0] == 0)
            {
                SetDestination(pensamientos[0].datos[0].GetVector());
            }
            else if (pensamientos[0].step == 1 && agente.remainingDistance == 0)//(cosasSteps[0] == 1 && agente.remainingDistance == 0)
            {
                if (Vector3.Distance(agente.destination, transform.position) < 1f)
                {
                    pensamientos[0].step = 2;//cosasSteps[0] = 2;
                    pensamientos[0].tiempo = 1.5f;//cosasTiempo[0] = 1.5f;
                }
            }
            else if (pensamientos[0].step == 2)//(cosasSteps[0] == 2)
            {
                //cosasTiempo[0] -= Time.deltaTime;
                pensamientos[0].tiempo -= Time.deltaTime;
                if (pensamientos[0].tiempo <= 0)//(cosasTiempo[0] <= 0)
                {
                    string armasQueTiene = "";
                    for (int i = 0; i < armasSoldado.Count; i++)
                    {
                        //Granadas  implementadas lets gooooooo
                        balas[i] = Mathf.Max(balas[i], (2 * armasSoldado[i].balasArma() + Random.Range(0, 1 + (2 * armasSoldado[i].balasArma()))));
                        balasFront[i] = Mathf.Max(balasFront[i], (armasSoldado[i].balasArma()));
                        armasQueTiene += armasSoldado[i].nombre;
                    }
                    if (armasSoldado.Count < 2)
                    {
                        //Quitamos las granadas porque no se hacerlas basicamente
                        /*
                        if (!armasQueTiene.Contains("GRENADE"))
                        {
                            RecibeArma(Resources.Load<Arma>("Armas/GRENADE"));
                        }
                        */
                        if (!armasQueTiene.Contains("FAMAS"))
                        {
                            RecibeArma(Resources.Load<Arma>("Armas/FAMAS"));
                        }
                        if (!armasQueTiene.Contains("BERETTA"))
                        {
                            RecibeArma(Resources.Load<Arma>("Armas/BERETTA"));
                        }
                    }
                    pensamientos.RemoveAt(0);
                    //cosasVistas.RemoveAt(0);
                    //cosasTiempo.RemoveAt(0);
                    //cosasSteps.RemoveAt(0);
                }
            }
        }

        //Si ve un compañero inconsciente, que vaya a verlo y a despertarlo
        else if (pensamientos[0].tipo == Pensamiento.COMP_INC)
        {
            //Se queda 3 segundos mirando el cuerpo
            if (pensamientos[0].step == 0 && pensamientos[0].tiempo > 0)//(cosasSteps[0] == 0 && cosasTiempo[0] > 0)
            {
                //cosasTiempo[0] -= Time.deltaTime;
                pensamientos[0].tiempo -= Time.deltaTime;
                if (pensamientos[0].tiempo < 0)//(cosasTiempo[0] < 0)
                {
                    //cosasSteps[0] = 1;
                    //cosasTiempo[0] = 2;

                    pensamientos[0].step = 1;
                    pensamientos[0].tiempo = 2;

                    //Vector3 direccionMirar = -transform.position + sitioPos;
                    Vector3 direccionMirar = -transform.position + pensamientos[0].datos[0].GetVector();
                    direccionMirar *= 0.85f;
                    direccionMirar += transform.position;
                    SetDestination(direccionMirar);
                }

            }
            else if (pensamientos[0].step == 0 && pensamientos[0].tiempo == 0)//(cosasSteps[0] == 0 && cosasTiempo[0] == 0)
            {
                //Vector3 direccionMirar = -transform.position + sitioPos;
                Vector3 direccionMirar = -transform.position + pensamientos[0].datos[0].GetVector();
                direccionMirar = direccionMirar.normalized * 0.01f;
                direccionMirar += transform.position;
                SetDestination(direccionMirar);
                //cosasTiempo[0] = 3;
                pensamientos[0].tiempo = 3;
                ReproduceSonido(Resources.Load<AudioClip>("Audio/Soldiers/WhosThat" + terminoGenero));
            }

            else if (pensamientos[0].step == 1 && agente.remainingDistance == 0)//(cosasSteps[0] == 1 && agente.remainingDistance == 0)
            {
                pensamientos[0].tiempo -= Time.deltaTime;//cosasTiempo[0] -= Time.deltaTime;
                                                         //Debug.DrawLine(transform.position, sitioPos, Color.red);
                if (pensamientos[0].tiempo < 0)//(cosasTiempo[0] < 0)
                {
                    //Tira un rayo para detectar al compañero
                    RaycastHit rayoDet;
                    LayerMask layersADet = new LayerMask();
                    layersADet = (1 << LayerMask.NameToLayer("EscenarioColliders")) | (1 << LayerMask.NameToLayer("Interaccion"));

                    if (Physics.Raycast(transform.position,
                        (pensamientos[0].datos[0].GetVector() - transform.position).normalized, out rayoDet, Mathf.Infinity, layersADet))
                    {
                        print(rayoDet.collider.name);
                        if (rayoDet.collider.gameObject.name == "InteraccionSoldadoInc")
                        {
                            Soldier soldadoCaido = rayoDet.collider.transform.parent.parent.GetComponent<Soldier>();
                            print(soldadoCaido.transform.name);
                            if (soldadoCaido.EstaVivo())
                            {
                                print("levantalo");
                                //soldadoCaido.BajaOxigeno(-100);
                                ReproduceSonido(Resources.Load<AudioClip>("Audio/Soldiers/GetOutOfTheWay" + terminoGenero));
                                Golpe(3, true);
                            }
                            else
                            {
                                print("pasa de el");
                                //Le tapa con una manta y eso
                                //Poner sonido cremallera
                                soldadoCaido.Manta();
                            }

                            pensamientos[0].step = 2;
                            pensamientos[0].tiempo = 2;
                            //cosasSteps[0] = 2;
                            //cosasTiempo[0] = 2;

                        }
                        else
                        {
                            Vector3 nuevoVector = Vector3.Cross(Rig.forward, transform.up);
                            SetDestination(transform.position + (.5f * nuevoVector));
                            pensamientos[0].tiempo = 2;
                            //cosasTiempo[0] = 2;

                        }
                    }
                    else
                    {
                        //print("hola");
                        Vector3 nuevoVector = Vector3.Cross(Rig.forward, transform.up);
                        SetDestination(transform.position + (.5f * nuevoVector));
                        pensamientos[0].tiempo = 2;
                        //cosasTiempo[0] = 2;
                    }


                }
            }
            // Step = 1 espera un momento antes de despertarlo

            //Step = 2 si no esta muerto le mete una patada y se queda un rato esperando -> sale del bucle
            else if (pensamientos[0].step == 2)//(cosasSteps[0] == 2)
            {
                pensamientos[0].tiempo -= Time.deltaTime;//cosasTiempo[0] -= Time.deltaTime;
                if (pensamientos[0].tiempo < 0)//(cosasTiempo[0] < 0)
                {
                    pensamientos.RemoveAt(0);
                }
            }
        }

        //Si ve al jugador, lo persigue o alerta al resto(dependiendo de si tiene arma o no)
        else if (pensamientos[0].tipo == Pensamiento.JUGADOR)
        {
            PersigueJugador();
        }

        //Si le toman como rehén
        else if (pensamientos[0].tipo == Pensamiento.REHEN)
        {
            if (pensamientos[0].step == 0)//(cosasSteps[0] == 0)
            {
                cabeza.GetComponent<RecibeSonidos>().disabled = true;
                pensamientos[0].tiempo = 10;//cosasTiempo[0] = 10;
                pensamientos[0].step = 1;//cosasSteps[0] = 1;
                SetDestination(transform.position + pensamientos[0].datos[0].GetVector());
                interacCol.name = "InteraccionSoldadoRehen";
            }
            else if (pensamientos[0].step == 1)//(cosasSteps[0] == 1)
            {
                pensamientos[0].tiempo -= Time.deltaTime;//cosasTiempo[0] -= Time.deltaTime;
                if (pensamientos[0].tiempo <= 0)//(cosasTiempo[0] <= 0)
                {
                    cabeza.GetComponent<RecibeSonidos>().disabled = false;
                    pensamientos.RemoveAt(0);
                    interacCol.name = "InteraccionSoldado";
                }
            }
        }

        //Checkea zona | paso par : va hacia un sitio aleatorio | paso impar : mira hacia un sitio aleatorio 10 segundos. En 10 pasos termina
        else if (pensamientos[0].tipo == Pensamiento.CHECK_ZONA)
        {
            if (pensamientos[0].step % 2 == 0 && pensamientos[0].step < 10)//(cosasSteps[0] % 2 == 0 && cosasSteps[0] < 10)
            {
                //se salta este paso
                agente.SetDestination(pensamientos[0].datos[0].GetVector());
                if (agente.remainingDistance <= agente.radius)
                {
                    pensamientos[0].step += 1;//cosasSteps[0] += 1;
                    pensamientos[0].tiempo = 10;//cosasTiempo[0] = 10;

                    float newX = Random.Range(-1.0f, 1.0f);
                    float newZ = Random.Range(-1.0f, 1.0f);

                    agente.SetDestination(transform.position + (new Vector3(newX, 0, newZ).normalized * 0.1f));
                    //para = true;
                }
            }
            else if (pensamientos[0].step % 2 == 1 && pensamientos[0].step < 10)//(cosasSteps[0] % 2 == 1 && cosasSteps[0] < 10)
            {
                pensamientos[0].tiempo -= Time.deltaTime;//cosasTiempo[0] -= Time.deltaTime;
                if (pensamientos[0].tiempo <= 0)//(cosasTiempo[0] <= 0)
                {
                    //para = false;
                    pensamientos[0].step += 1;//cosasSteps[0] += 1;
                    pensamientos[0].tiempo = 0;//cosasTiempo[0] = 0;

                    float newAngle = Random.Range(-3.139f, 3.14f);
                    float newRadius = Random.Range(0.1f, 2.5f);

                    Vector3 newPos = new Vector3(Mathf.Cos(newAngle), 0, Mathf.Sin(newAngle)) * newRadius;
                    Vector3 sitioPos = pensamientos[0].datos[0].GetVector();
                    sitioPos += newPos;
                    pensamientos[0].datos[0] = new Dato(sitioPos);
                    //cosasVistas[0] = nuevaCosa;

                    SetDestination(sitioPos);

                }
            }

            else
            {
                //Si busca a un jugador en concreto
                if (pensamientos[0].datos.Length > 1)//("0123".Contains(modulos[modulos.Length - 1]))
                {
                    FindObjectOfType<GameManager>().AddAlerta(this, true, true);
                }
                pensamientos.RemoveAt(0);
                //cosasSteps.RemoveAt(0);
                //cosasTiempo.RemoveAt(0);


            }

            //Debug.DrawRay(sitioPos, Vector3.up, Color.black);
        }

    }

    public Arma Arma()
    {
        return armaEnMano;
    }
    public int[] balasArma()
    {
        if(armaEnMano != null)
        {
            int indexArma = armasSoldado.IndexOf(armaEnMano);
            if (armaEnMano.TipoObjeto() >= 1)
            {
                return new int[] { balas[indexArma], balasFront[indexArma] };
            }
            else
                return new int[] { balas[indexArma], 0 };
        }
        return new int[] { 0, 0 };
    }

    public void PierdeArma(Arma arma)
    {
        if (!armasSoldado.Contains(arma))
            return;
        if (armaEnMano == arma)
            armaEnMano = null;
        int indexArma = armasSoldado.IndexOf(arma);
        armasSoldado.RemoveAt(indexArma);
        balas.RemoveAt(indexArma);
        balasFront.RemoveAt(indexArma);
        SetArma();
    }

    public void Dolor(bool sonido = true)
    {
        if (sonido)
            ReproduceSonido(dolorSonido);

        soldierAnimator.Play("DolorLayer.Dolor", 2, 0);

    }

    public void SpinCQC(float f)
    {
        soldierAnimator.SetFloat("ThrowSent", f);
        if(f >= 1.0f)
        {
            soldierAnimator.Play("Base Layer.GenomeGetToThrow", 0, 0);
        }
    }

    public bool counter()
    {
        bool counterGolpe = soldierAnimator.GetCurrentAnimatorStateInfo(1).IsName("GenomePrepPunch");
        bool counterCulatazo = soldierAnimator.GetCurrentAnimatorStateInfo(1).IsName("GenomePrepCulatazo");

        return counterGolpe || counterCulatazo;
    }

}
