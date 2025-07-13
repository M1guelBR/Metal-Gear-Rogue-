using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Globalization;
using UnityEditor;

public class Soldier : MonoBehaviour
{
    public int ID = -1;
    [SerializeField] Animator soldierAnimator;
    public Transform Rig;
    [SerializeField] NavMeshAgent agente;
    [SerializeField] CharacterController controller;
    List<int> indicesFinal = new List<int>();

    //Movimiento
    public List<Vector3> posiciones;
    public List<float> tiempoEspera;
    public List<float> rotacionesY;

    bool espera = true;
    float tiempoQueda = 0;

    Vector3 movimiento;
    Vector3 lastPos;
    int index;
    float vida = 100;

    float estres = 0;
    float oxigeno = 100;



    bool antesApagado = true;

    [Range(0, 3)]
    public float velocidadAndar;
    [Range(1,3)]
    public float multCorrer;

    bool correr, agachado;
    float vel;

    public List<string> cosasVistas;
    public List<float> cosasTiempo;
    public List<int> cosasSteps;


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
    bool KO;
    Vector3 v;
    bool vieneDeCQC;
    [HideInInspector]public float oxigTarget;
    public List<string> informacion;
    bool impulsa = true;
    bool setPos = true;
    float tiempoInterrog = 0;
    float tiempoPausaPPK = 0;
    bool pegar = false;
    List<GameObject> collidersAPegar = new List<GameObject>();

    //Armas
    public Transform armaHolder;
    List<Arma> armasSoldado;
    List<int> balas; List<int> balasFront;
    Arma armaEnMano;
    float tiempoArmaCambio = 0;
    float distActual = 0;
    float tiempoCad = 0;
    bool apuntar = false;
    [SerializeField] Transform lArmAux, rArmAux;
    [SerializeField] Transform lArm, rArm;
    [SerializeField] Transform chest, cabeza;
    [SerializeField] LineRenderer trailBala;
    Vector3 sitioApuntar;
    bool dispAux = false;

    //Audio
    public AudioSource sonidoSoldado;
    [SerializeField] AudioClip cqcCaerSonido, cqcCaerGrito;
    [SerializeField] AudioClip dolorSonido, ahogueSonido;
    [SerializeField] AudioClip pasosSonido;
    [SerializeField] AudioClip alertaSonido, noticeSonido;
    bool paso = false;

    // Partes del cuerpo
    public Transform hips, LLeg, RLeg;
    float facSangre = 1;
    [SerializeField] SkinnedMeshRenderer cuerpo;
    [SerializeField] MeshRenderer gorro, buf, lazo1, lazo2;

    //Radar
    [SerializeField] MeshRenderer fovRadar;
    int modoRadar = 0;

    //Sistema de deteccion
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
        armasSoldado.Add(Resources.Load<Arma>("Armas/GRENADE"));

        // 2 Cargadores por arma con posibilidad de hasta dos mas

        for (int i = 0; i < armasSoldado.Count; i++) {
            //Granadas  implementadas lets gooooooo


            balas.Add(2 * armasSoldado[i].balasArma() + Random.Range(0, 1 + (2* armasSoldado[i].balasArma())));
            balasFront.Add(armasSoldado[i].balasArma()); 
        }

        SeleccionaArma();

        //controller = this.GetComponent<CharacterController>();
        agente.updateRotation = false;
        cosasVistas = new List<string>();


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

        CheckRadar();

        if (jugadorAg == null && !KO)
        {
            apuntar = false;
            if (tiempoArmaCambio > 0)
            {
                tiempoArmaCambio -= Time.deltaTime;
                if (tiempoArmaCambio < 0)
                    tiempoArmaCambio = 0;
            }
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
                    sonidoSoldado.PlayOneShot(pasosSonido, movimiento.magnitude * .15f / (velocidadAndar * multCorrer));

                }
                else if (t > 0.9f && paso)
                {
                    paso = false;
                    sonidoSoldado.PlayOneShot(pasosSonido, movimiento.magnitude * .05f / (velocidadAndar * multCorrer));
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
            if (posiciones.Count > 0 && cosasVistas.Count == 0 && !espera)
            {
                //if (para)
                   // para = false;

                agachado = false;
                correr = false;
                int siguiente = (index + 1) % posiciones.Count;
                if (agente.destination == null)
                {
                    agente.SetDestination(posiciones[siguiente]);
                }
                if (agente.remainingDistance == 0)
                {
                    index = siguiente;
                    espera = true;
                    tiempoQueda = tiempoEspera[index];
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
                    agente.SetDestination(posiciones[index]);

                }
                else
                    RotaRig(rotacionesY[index], true);
            }
            else if (cosasVistas.Count > 0)
            {
                AjustaStepsYTiempo();
                if(cosasVistas.Count > 1)
                    ReordenaCosasVistas();
                //para = true;
                //Formato de cosas vistas

                //cosa = (string) : [7 caracteres de tipo] + _ + [vector con componentes separadas por ; de donde ha visto eso/ debe ir] (opcional,+_+¿caja?+ _ +indiceJug)

                correr = true;
                string cosaPrioritaria = "";
                //Hay veces que hace un cast invalido (?). He intentado de todo, pero nada
                try
                {
                    cosaPrioritaria = cosasVistas[0];
                }
                catch
                {
                    return;
                }

                string[] modulos = cosaPrioritaria.Split("_");

                //los primeros 7 caracteres determinan que tipo de cosa es
                string tipo = modulos[0];
                string[] sitio;

                if (agachado && !cosasVistas[0].Contains("verAJug"))
                    agachado = false;

                //Intenta sacar el sitio de lo que tiene visto, si no nada
                try
                {
                    sitio = modulos[1].Split(";");
                }
                catch
                {
                    sitio = new string[] { "0", "0", "0" };
                }

                Vector3 sitioPos = new Vector3(SacaNumString(sitio[0]), SacaNumString(sitio[1]), SacaNumString(sitio[2]));


                switch (tipo)
                {

                    case "explCer":

                        if(cosasSteps[0] == 0)
                        {
                            agachado = false;
                            cosasTiempo[0] = 1f;
                            cosasSteps[0] = 1;
                        }
                        cosasTiempo[0] -= Time.deltaTime;
                        Vector3 direccion = (sitioPos - transform.position).normalized;
                        Vector3 direccionOpuesta = transform.position - (direccion * 5);
                        if (Vector3.Distance(transform.position, sitioPos) <= 6)
                        {
                            agente.SetDestination(direccionOpuesta);
                        }

                        Debug.DrawLine(transform.position, direccionOpuesta, Color.white);
                        if (cosasTiempo[0] < 0)
                        {
                            cosasVistas.RemoveAt(0);
                            cosasSteps.RemoveAt(0);
                            cosasTiempo.RemoveAt(0);
                            int siguiente = (index + 1) % posiciones.Count;
                            agente.SetDestination(posiciones[siguiente]);
                        }

                        break;

                    //Si no tiene un arma, que se diriga al armería
                    case "falArma":

                        if (cosasSteps[0] == 0)
                        {
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
                                cosasSteps[0] = 1;
                                agente.SetDestination(FindObjectOfType<GameManager>().crates[j].transform.position);
                            }
                        }

                        else if(cosasSteps[0] == 1 && agente.remainingDistance == 0)
                        {
                            if (Vector3.Distance(agente.destination, transform.position) < 1f)
                            {
                                cosasSteps[0] = 2;
                                cosasTiempo[0] = 1.5f;
                            }
                        }
                        else if(cosasSteps[0] == 2)
                        {
                            cosasTiempo[0] -= Time.deltaTime;
                            if(cosasTiempo[0] <= 0)
                            {
                                string armasQueTiene = "";
                                for (int i = 0; i < armasSoldado.Count; i++)
                                {
                                    //Granadas  implementadas lets gooooooo
                                    balas[i] = Mathf.Max(balas[i], (2 * armasSoldado[i].balasArma() + Random.Range(0, 1 + (2 * armasSoldado[i].balasArma()))));
                                    balasFront[i] = Mathf.Max(balasFront[i], (armasSoldado[i].balasArma()));
                                    armasQueTiene += armasSoldado[i].nombre;
                                }
                                if(armasSoldado.Count < 3)
                                {
                                    if (!armasQueTiene.Contains("GRENADE"))
                                    {
                                        RecibeArma(Resources.Load<Arma>("Armas/GRENADE"));
                                    }

                                    if (!armasQueTiene.Contains("FAMAS"))
                                    {
                                        RecibeArma(Resources.Load<Arma>("Armas/FAMAS"));
                                    }
                                    if (!armasQueTiene.Contains("BERETTA"))
                                    {
                                        RecibeArma(Resources.Load<Arma>("Armas/BERETTA"));
                                    }
                                } 
                                cosasVistas.RemoveAt(0);
                                cosasTiempo.RemoveAt(0);
                                cosasSteps.RemoveAt(0);
                            }
                        }


                        break;
                    //Si ve un compañero inconsciente, que vaya a verlo y a despertarlo
                    case "compInc":
                        //Se queda 3 segundos mirando el cuerpo
                        if(cosasSteps[0] == 0 && cosasTiempo[0] > 0)
                        {
                            cosasTiempo[0] -= Time.deltaTime;
                            if(cosasTiempo[0] < 0)
                            {
                                cosasSteps[0] = 1;
                                cosasTiempo[0] = 2;
                                Vector3 direccionMirar = -transform.position + sitioPos;
                                direccionMirar *= 0.85f;
                                direccionMirar += transform.position;
                                agente.SetDestination(direccionMirar);
                            }

                        }
                        else if(cosasSteps[0] == 0 && cosasTiempo[0] == 0)
                        {
                            Vector3 direccionMirar = -transform.position + sitioPos;
                            direccionMirar = direccionMirar.normalized * 0.01f;
                            direccionMirar += transform.position;
                            agente.SetDestination(direccionMirar);
                            cosasTiempo[0] = 3;
                            sonidoSoldado.PlayOneShot(Resources.Load<AudioClip>("Audio/Soldiers/WhosThat"));
                        }

                        else if(cosasSteps[0] == 1 && agente.remainingDistance == 0)
                        {
                            cosasTiempo[0] -= Time.deltaTime;
                            Debug.DrawLine(transform.position, sitioPos, Color.red);
                            if (cosasTiempo[0] < 0)
                            {
                                //Tira un rayo para detectar al compañero
                                RaycastHit rayoDet;
                                LayerMask layersADet = new LayerMask();
                                layersADet = (1 << LayerMask.NameToLayer("EscenarioColliders")) | (1 << LayerMask.NameToLayer("Interaccion"));

                                if (Physics.Raycast(transform.position, (sitioPos - transform.position).normalized,out rayoDet, Mathf.Infinity, layersADet))
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
                                            sonidoSoldado.PlayOneShot(Resources.Load<AudioClip>("Audio/Soldiers/GetOutOfTheWay"));
                                            Golpe(3, true);
                                        }
                                        else
                                        {
                                            print("pasa de el");
                                            //Le tapa con una manta y eso
                                            //Poner sonido cremallera
                                            soldadoCaido.Manta();
                                        }
                                        cosasSteps[0] = 2;
                                        cosasTiempo[0] = 2;

                                    }
                                    else
                                    {
                                        Vector3 nuevoVector = Vector3.Cross(Rig.forward, transform.up);
                                        agente.SetDestination(transform.position + (.5f * nuevoVector));
                                        cosasTiempo[0] = 2;

                                    }
                                }
                                else
                                {
                                    //print("hola");
                                    Vector3 nuevoVector = Vector3.Cross(Rig.forward, transform.up);
                                    agente.SetDestination(transform.position + (.5f * nuevoVector));
                                    cosasTiempo[0] = 2;
                                }


                            }
                        }
                        // Step = 1 espera un momento antes de despertarlo

                        //Step = 2 si no esta muerto le mete una patada y se queda un rato esperando -> sale del bucle
                        else if(cosasSteps[0] == 2)
                        {
                            cosasTiempo[0] -= Time.deltaTime;
                            if (cosasTiempo[0] < 0)
                            {
                                cosasVistas.RemoveAt(0);
                                cosasTiempo.RemoveAt(0);
                                cosasSteps.RemoveAt(0);
                            }
                        }
                        
                        break;
                    //Si ve al jugador, lo persigue o alerta al resto(dependiendo de si tiene arma o no)
                    case "verAJug":

                        cosasTiempo[0] -= Time.deltaTime;
                        //Si ve al jugador, que le tome un segundo reaccionar
                        if (cosasSteps[0] == -2 && cosasTiempo[0] <= 2.65f)
                        {
                            cosasSteps[0] = -1;
                            FindObjectOfType<GameManager>().AddAlerta(this);
                            sonidoSoldado.PlayOneShot(noticeSonido);
                        }
                        else if (cosasSteps[0] == -2)
                        {
                            tiempoPausaPPK = Time.deltaTime;
                            break;

                        }

                        //Pone el sonido para que los otros lo detecten
                        if (cosasSteps[0] == -1)
                        {
                            sonido.gameObject.SetActive(true);
                            sonido.GetComponent<SphereCollider>().radius = 5;
                            foreach (Snake jug in FindObjectsOfType<Snake>())
                            {
                                if (jug.playerID == int.Parse(modulos[modulos.Length - 1]))
                                {
                                    sonido.jug = jug.transform;
                                    break;
                                }
                            }
                            cosasSteps[0] = 0;
                        }
                        sonido.time = cosasTiempo[0];

                        //Estimacion de si cambia las distancias, que cambie su comportamiento
                        {
                            float[] distNiveles = { 5.5f, 2.5f, .3f };

                            float estimada = Vector3.SqrMagnitude(sitioPos - transform.position);
                            for (int i = 0; i < distNiveles.Length; i++)
                            {
                                float p = Mathf.Pow(distNiveles[i], 2);
                                if (Mathf.Sign(distActual - p) != Mathf.Sign(estimada - p))
                                {
                                    cosasSteps[0] = 0;
                                    print("cambio steps");
                                    if (i != 0)
                                        agachado = false;
                                    break;
                                }

                            }
                            distActual = Vector3.SqrMagnitude(sitioPos - transform.position);


                        }
                        agente.SetDestination(sitioPos);
                        //print(sitioPos.y - transform.position.y);
                        sitioApuntar = sitioPos;
                        //Si pierde al jugador, que transforme esta instruccion a checkear la zona donde lo perdió
                        if (cosasTiempo[0] < 0)
                        {
                            string cambioCosa = "chkZone_";
                            for (int i = 1; i < modulos.Length; i++)
                            {
                                cambioCosa += modulos[i];
                                if (i < modulos.Length - 1)
                                    cambioCosa += "_";
                            }
                            cosasVistas[0] = cambioCosa;
                            cosasTiempo[0] = 0;
                            FindObjectOfType<GameManager>().AddAlerta(this, true);
                            FindObjectOfType<GameManager>().AddAlerta(this, false, true);
                            Agachate(false);
                        }

                        if(armaEnMano == null && armasSoldado.Count > 0) { armaEnMano = armasSoldado[0]; SetArma(); }

                        //Dependiendo de la distancia que haga según que cosas
                        apuntar = true;
                        if (armaEnMano != null && agente.remainingDistance > 5.5f && (sitioPos.y - transform.position.y) >= -0.15f)
                        {
                            // Si no tiene granada en inv la tira; si no se agacha para disparar mejor
                            if (armasSoldado.Count == 0)
                                break;

                            if (cosasSteps[0] == 0)
                            {
                                for (int i = 0; i < armasSoldado.Count; i++)
                                {
                                    if ("GRANADA" != armaEnMano.nombre && armasSoldado[i].nombre == "GRANADA")
                                    {
                                        armaEnMano = armasSoldado[i];
                                        tiempoArmaCambio = .5f;
                                        break;
                                    }

                                    else if (!"GRANADASTUN".Contains(armaEnMano.nombre) && armasSoldado[i].nombre == "STUN")
                                    {
                                        armaEnMano = armasSoldado[i];
                                        tiempoArmaCambio = 0.5f;
                                        continue;
                                    }

                                    else if (armaEnMano != armasSoldado[i])
                                    {
                                        armaEnMano = armasSoldado[i];
                                        tiempoArmaCambio = 0.5f;
                                        continue;
                                    }


                                }
                                SetArma();
                                cosasSteps[0] = 1;
                                if (!"GRANADASTUN".Contains(armaEnMano.nombre))
                                {
                                    tiempoArmaCambio = 0.65f;
                                }
                            }
                            else if (cosasSteps[0] == 1 && tiempoArmaCambio == 0)
                            {
                                //Si tiene una granada, la tira
                                if ("GRENADESTUN".Contains(armaEnMano.nombre))
                                {
                                    //Dispara la granada
                                    print("Granada");
                                    Disparar();
                                    cosasSteps[0] = 3;
                                    tiempoArmaCambio = 30;
                                }
                                //Si tiene un arma, se agacha y te dispara
                                else
                                {
                                    agachado = true;
                                    if (soldierAnimator.GetCurrentAnimatorStateInfo(0).IsName("Agachado") && tiempoArmaCambio == 0)
                                    {
                                        Disparar();
                                        cosasSteps[0] = 2;
                                        tiempoPausaPPK = 0.5f;
                                        tiempoArmaCambio = 1;
                                    }
                                }

                            }
                            else if (cosasSteps[0] == 2)
                            {
                                if (armaEnMano != null)
                                {
                                    int indArm = armasSoldado.IndexOf(armaEnMano);
                                    if (balasFront[indArm] == 0)
                                        Recargar();
                                }

                                if (tiempoArmaCambio == 0)
                                {
                                    agachado = false;
                                    cosasSteps[0] = 1;
                                    if (!"GRANADASTUN".Contains(armaEnMano.nombre))
                                    {
                                        tiempoArmaCambio = 0.65f;
                                    }
                                }
                            }
                            else if (cosasSteps[0] == 3)
                            {
                                //Busca el arma que no sea una granada y la usa para disparar
                                int ind = -1;
                                for (int i = 0; i < armasSoldado.Count; i++)
                                {
                                    if (armasSoldado[i].TipoObjeto() == 0)
                                        continue;
                                    ind = i;
                                }
                                if (ind != -1)
                                {
                                    armaEnMano = armasSoldado[ind];

                                }
                                else
                                {
                                    armaEnMano = null;
                                }
                                SetArma();
                                cosasSteps[0] = 4;
                            }
                            else if (cosasSteps[0] == 4)
                            {
                                bool disparar = (armaEnMano != null && (int)tiempoArmaCambio % 2 == 0);
                                if (disparar)
                                {
                                    Disparar();
                                    cosasSteps[0] = 5;

                                }
                                if (tiempoArmaCambio == 0)
                                    cosasSteps[0] = 1;
                            }
                            else if (cosasSteps[0] == 5 && (int)tiempoArmaCambio % 2 == 1)
                            {
                                cosasSteps[0] = 4;
                            }
                        }
                        else if (armaEnMano != null && (agente.remainingDistance < 5.5f && agente.remainingDistance > 2.5f || (sitioPos.y - transform.position.y) < -0.15f))
                        {
                            if ( armaEnMano.TipoObjeto() == 0)
                                cosasSteps[0] = 0;
                            //Selecciona arma de alto alcance y dispara por rondas pero se para al disparar
                            if (cosasSteps[0] == 0)
                            {
                                int ind = -1;
                                for (int i = 0; i < armasSoldado.Count; i++)
                                {
                                    if (armasSoldado[i].TipoObjeto() == 0)
                                        continue;
                                    ind = i;
                                }
                                if (ind != -1)
                                {
                                    armaEnMano = armasSoldado[ind];
                                    tiempoPausaPPK = .5f;

                                }
                                else
                                {
                                    armaEnMano = null;
                                }
                                SetArma();
                                cosasSteps[0] = 1;

                            }
                            else if (cosasSteps[0] == 1 && armaEnMano != null)
                            {
                                int indArm = armasSoldado.IndexOf(armaEnMano);
                                if (tiempoCad == 0 && balasFront[indArm] > 0)
                                    Disparar();
                                if (tiempoPausaPPK <= 0 || balasFront[indArm] == 0)
                                {
                                    cosasSteps[0] = 2;
                                    tiempoArmaCambio = .75f;
                                }
                            }
                            else if (cosasSteps[0] == 1 && armaEnMano == null && tiempoPausaPPK > 0)
                                tiempoPausaPPK = 0;
                            else if(cosasSteps[0] == 2)
                            {
                                if (tiempoArmaCambio == 0)
                                {
                                    cosasSteps[0] = 1;
                                    if (armaEnMano != null)
                                    {
                                        tiempoPausaPPK = .35f;
                                        int indArm = armasSoldado.IndexOf(armaEnMano);
                                        if (balasFront[indArm] == 0)
                                            Recargar();
                                    }
                                }
                            }
                        }
                        else if(armaEnMano != null && agente.remainingDistance < 2.5f && agente.remainingDistance > .5f && (sitioPos.y - transform.position.y) >= -0.15f)
                        {
                            if (armaEnMano.TipoObjeto() == 0)
                                cosasSteps[0] = 0;
                            // Dispara a quemarropa con el arma que mas quite

                            //Selecciona el arma con mas daño
                            if (cosasSteps[0] == 0)
                            {
                                float daño = 0; int ind = -1;
                                for (int i = 0; i < armasSoldado.Count; i++)
                                {
                                    if (armasSoldado[i].TipoObjeto() == 0)
                                        continue;
                                    

                                    if(armasSoldado[i].pistolaFusil().daño > daño)
                                    {
                                        ind = i;
                                        daño = armasSoldado[i].pistolaFusil().daño;
                                    }
                                    
                                }
                                if (ind != -1)
                                {
                                    armaEnMano = armasSoldado[ind];
                                    print(daño);
                                    tiempoArmaCambio = 0.35f;
                                }
                                else
                                {
                                    armaEnMano = null;
                                }
                                SetArma();
                                cosasSteps[0] = 1;
                            }

                            //Cuando tenga el arma lista, que dispare POM
                            if(cosasSteps[0] == 1 && armaEnMano != null)
                            {
                                int indArm = armasSoldado.IndexOf(armaEnMano);
                                if (tiempoCad == 0 && balasFront[indArm] > 0)
                                    Disparar();
                                if (tiempoPausaPPK <= 0 || balasFront[indArm] == 0)
                                {
                                    cosasSteps[0] = 2;
                                    tiempoArmaCambio = .75f;
                                }

                            }
                            //Se espera un rato a recargar
                            if (cosasSteps[0] == 2 && tiempoArmaCambio == 0)
                            {
                                Recargar();
                                cosasSteps[0] = 1;
                                //Si no le quedan balas vuelve a buscar a ver si tiene un arma mejor
                                try
                                {
                                    int indArm = armasSoldado.IndexOf(armaEnMano);
                                    if (balas[indArm] == 0)
                                        cosasSteps[0] = 0;
                                }
                                catch
                                {

                                }
                            }


                            //if (!soldierAnimator.GetCurrentAnimatorStateInfo(1).IsName("Disparar"))
                            //    Disparar();
                        }
                        else if(agente.remainingDistance < .5f && (sitioPos.y - transform.position.y) >= -0.15f)
                        {
                            //CQC

                            /*
                             
                            step 0 => tiene que reaccionar (ventana para que el jugador le pueda pegar)
                            step > 0 => pum te pega
                             
                             */
                            apuntar = false;
                            tiempoPausaPPK = agente.remainingDistance < 0.3f ? .1f : 0;
                            if(cosasSteps[0] == 0)
                            {
                                tiempoArmaCambio = 0.75f;
                                cosasSteps[0] = 1;
                            }
                            //Si le están pegando [ state.isName("Dolor") ] pues no puede pegar
                            else if(cosasSteps[0] > 0 && tiempoArmaCambio == 0 && !soldierAnimator.GetCurrentAnimatorStateInfo(2).IsName("Dolor"))
                            {
                                Golpe(cosasSteps[0]);
                                tiempoArmaCambio = 0.4f;
                                cosasSteps[0] += 1;
                                if (cosasSteps[0] > 3)
                                    cosasSteps[0] = 1;
                            }


                        }


                        break;

                    //Si le toman como rehén
                    case "rehen": 
                        if(cosasSteps[0] == 0)
                        {
                            cabeza.GetComponent<RecibeSonidos>().disabled = true;
                            cosasTiempo[0] = 10;
                            cosasSteps[0] = 1;
                            agente.SetDestination(transform.position + sitioPos);
                            interacCol.name = "InteraccionSoldadoRehen";
                        }
                        else if(cosasSteps[0] == 1)
                        {
                            cosasTiempo[0] -= Time.deltaTime;
                            if(cosasTiempo[0] <= 0)
                            {
                                cabeza.GetComponent<RecibeSonidos>().disabled = false;
                                cosasVistas.RemoveAt(0);
                                cosasSteps.RemoveAt(0);
                                cosasTiempo.RemoveAt(0);
                                interacCol.name = "InteraccionSoldado";
                            }
                        }
                        break;

                    //Checkea zona | paso par : va hacia un sitio aleatorio | paso impar : mira hacia un sitio aleatorio 10 segundos. En 10 pasos termina
                    case "chkZone":
                        if(cosasSteps[0] % 2 == 0 && cosasSteps[0] < 10)
                        {
                            //se salta este paso
                            agente.SetDestination(sitioPos);
                            if(agente.remainingDistance <= agente.radius)
                            {
                                cosasSteps[0] += 1;
                                cosasTiempo[0] = 10;

                                float newX = Random.Range(-1.0f, 1.0f);
                                float newZ = Random.Range(-1.0f, 1.0f);

                                agente.SetDestination(transform.position + (new Vector3(newX, 0, newZ).normalized * 0.1f));
                                //para = true;
                            }
                        }
                        else if(cosasSteps[0]%2 == 1 && cosasSteps[0] < 10)
                        {
                            cosasTiempo[0] -= Time.deltaTime;
                            if(cosasTiempo[0] <= 0)
                            {
                                //para = false;
                                cosasSteps[0] += 1;
                                cosasTiempo[0] = 0;

                                float newAngle = Random.Range(-3.139f, 3.14f);
                                float newRadius = Random.Range(0.1f, 2.5f);

                                Vector3 newPos = new Vector3(Mathf.Cos(newAngle), 0, Mathf.Sin(newAngle)) * newRadius;

                                sitioPos += newPos;
                                string nuevoSitio = VectorAString(sitioPos);
                                string nuevaCosa = "";
                                for(int i = 0; i < modulos.Length; i++)
                                {
                                    if(i == 1)
                                    {
                                        nuevaCosa += nuevoSitio + "_";
                                        continue;
                                    }
                                    if(i == modulos.Length - 1)
                                    {
                                        nuevaCosa += modulos[i];
                                        continue;
                                    }
                                    else
                                    {
                                        nuevaCosa += modulos[i] + "_";
                                    }
                                }
                                cosasVistas[0] = nuevaCosa;

                                agente.SetDestination(sitioPos);

                            }
                        }

                        else
                        {
                            if ("0123".Contains(modulos[modulos.Length - 1]))
                            {
                                FindObjectOfType<GameManager>().AddAlerta(this, true, true);
                            }
                            cosasVistas.RemoveAt(0);
                            cosasSteps.RemoveAt(0);
                            cosasTiempo.RemoveAt(0);


                        }

                        Debug.DrawRay(sitioPos, Vector3.up, Color.black);
                        break;
                }
            }

            if (!agachado)
                vel = velocidadAndar * (correr ? multCorrer : 1);
            else
                vel = 0;


            if((balas.Contains(0) || armasSoldado.Count < 3) && !cosasVistas.Contains("falArma") && EstaVivo() && Consciente())
            {
                cosasVistas.Add("falArma");
                cosasSteps.Add(0);
                cosasTiempo.Add(0);
            }

            bool pararMoverse = para || (tiempoPausaPPK != 0);

            agente.speed = vel;
            agente.isStopped = pararMoverse;

            agente.updatePosition = agente.enabled;

            if (agente.desiredVelocity != Vector3.zero)
            {
                RotaRig();
            }
        }
        //agarre cqc en lateUpdate
        else if(controller.enabled && !KO)
        {
            Rig.forward = cqcNormal;
            v.y -= Time.deltaTime * 9.8f;
            controller.Move(v * Time.deltaTime);
            if (controller.isGrounded && !agachado)
            {
                SonidoRadio(4);
                sonidoSoldado.PlayOneShot(cqcCaerSonido, .15f);
                //controller.enabled = false;
                controller.enabled = false;
                transform.position += .55f * Vector3.up;
                controller.center = .55f * Vector3.down;
                controller.enabled = true;
                KO = true;
                BajaOxigeno(oxigTarget);
                SetPPKT(0);

                vistaCol.GetComponent<VistaSoldado>().collidersALaVista = new List<Collider>();
                vistaCol.SetActive(false);
                cosasVistas = new List<string>();
                cosasSteps = new List<int>();
                cosasTiempo = new List<float>();
                FindObjectOfType<GameManager>().AddAlerta(this, true, true);
                FindObjectOfType<GameManager>().AddAlerta(this, true);
                if (EstaVivo())
                    QuitaVida(Mathf.Abs(controller.velocity.y), false);

            }

        }

        if (KO)
        {

            controller.Move(Vector3.down * Time.deltaTime);
            if (oxigeno == 100 && EstaVivo())
            {
                KO = false;
                //controller.enabled = false;
                v = Vector3.down;
                agachado = true;
                vieneDeCQC = true;
                vistaCol.SetActive(true);
            }
        }
        if(!KO && agachado && soldierAnimator.GetCurrentAnimatorStateInfo(0).IsName("Agachado") && vieneDeCQC)
        {
            Rig.localPosition = Vector3.down * 0.832f;
            agente.enabled = true;
            controller.center = Vector3.zero;
            controller.enabled = false;
            agachado = false;
            vieneDeCQC = false;
            string chk = "chkZone_" + VectorAString(transform.position) + "_" + jugadorAg.GetComponent<Snake>().playerID.ToString();
            jugadorAg = null;
            interacCol.name = "InteraccionSoldado";
            SeleccionaArma();
            vistaCol.SetActive(true);

            cosasVistas.Add(chk);
            cosasSteps.Add(0);
            cosasTiempo.Add(0);
            FindObjectOfType<GameManager>().AddAlerta(this, false, true);
            //Debug.Break();
        }

        if(vida > 0)
            oxigeno = Mathf.Clamp(oxigeno + (Time.deltaTime * (KO ? .75f : 75)), 0, 100);


        if (sonido.jug != null && (cosasVistas.Count == 0 || !cosasVistas[0].Contains("verAJug")))
        {
            sonido.jug = null;
            sonido.gameObject.SetActive(false);
        }

        if (agente.enabled)
            lastPos = transform.position;

        movimiento = agente.enabled ? agente.velocity : (transform.position - lastPos) / Time.deltaTime;


        soldierAnimator.SetFloat("Velocity", movimiento.magnitude);
        soldierAnimator.SetBool("Agachado", agachado);
        soldierAnimator.SetBool("Tirar", controller.enabled && impulsa);
        soldierAnimator.SetBool("Aterriza", controller.isGrounded);
        soldierAnimator.SetBool("KO", KO);
        soldierAnimator.SetBool("PassOut", controller.enabled && !impulsa);
        soldierAnimator.SetBool("Apuntar", apuntar);
        soldierAnimator.SetBool("Rehen", cosasVistas.Contains("rehen"));

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

        lastPos = transform.position;

        if (antesApagado && agente.enabled)
        {
            antesApagado = false;

            int siguiente = (index + 1) % posiciones.Count;
            agente.SetDestination(posiciones[siguiente]);

        }


        //Pongo aqui lo de que lo agarren de cqc por que tiene menos latencia con la posicion
        if (jugadorAg != null && !controller.enabled && !KO && !vieneDeCQC)
        {
            transform.position = cqcPos;
            Rig.forward = cqcNormal;

            if (tiempoInterrog > 0)
            {
                tiempoInterrog -= Time.deltaTime;
                if (tiempoInterrog <= 0)
                {
                    Interroga();
                }
            }
            //Por si de alguna forma el soldado no lo apaga el solo
            if(cosasVistas.Count > 0)
            {
                cosasVistas = new List<string>();
                cosasTiempo = new List<float>();
                cosasSteps = new List<int>();
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
            if (cosasVistas.Count == 0 || !cosasVistas[0].Contains("verAJug"))
            {
                apuntar = false;
            }

            //Asi apunta con cierto angulo

            //Vector3 angulos = Quaternion.AngleAxis(angBrazos, Rig.right).eulerAngles;

            //lArmAux.Rotate(angulos.x,angulos.y,angulos.z, Space.World);
            
            Vector3 dist = sitioApuntar - transform.position;
            dist.y = 0;

            float angChest = Vector3.SignedAngle(new Vector3(chest.forward.x, 0, chest.forward.z), dist, Vector3.up);
            //print(angChest);
            Vector3 anguloChest = Quaternion.AngleAxis(angChest, Vector3.up).eulerAngles;
            chest.Rotate(0, angChest, 0, Space.World);
            //chest.Rotate( angChest, 0,0, Space.World);
            //chest.Rotate(0, 0,angChest, Space.World);
            //chest.forward = new Vector3(dist.x, chest.forward.y, dist.z).normalized;

            float dif = 20;
            float angBrazos = Vector3.SignedAngle(lArm.up, dist, chest.right) + dif;
            Vector3 angulos = Quaternion.AngleAxis(angBrazos, chest.right).eulerAngles;
            lArmAux.Rotate(angulos, Space.World); 
            rArmAux.Rotate(angulos, Space.World);

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


        //si está pegando, calcula los golpes
        if (pegar && collidersAPegar.Count > 0)
        {
            print("golpeEfectivo");
            foreach (GameObject persona in collidersAPegar)
            {

                int i = Random.Range(1, 3);
                sonidoSoldado.PlayOneShot(Resources.Load<AudioClip>("Audio/Snake/Golpe" + i.ToString()), .5f);

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
                    persona.GetComponent<Soldier>().QuitaVida(.5f, false);
                    if (soldierAnimator.GetInteger("PunchCount") == 3)
                        persona.GetComponent<Soldier>().BajaOxigeno(-100);
                }

            }
            soldierAnimator.SetInteger("PunchCount", 0);
            collidersAPegar = new List<GameObject>();
            pegar = false;
        }

        else if (pegar && collidersAPegar.Count == 0 && soldierAnimator.GetCurrentAnimatorStateInfo(1).IsName("SinArma"))
        {
            soldierAnimator.SetInteger("PunchCount", 0);
            print("mierdon de golpe");
            pegar = false;
        }

    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        foreach (Vector3 pos in posiciones)
            Gizmos.DrawLine(pos, pos+ Vector3.up);
    }

    private void FixedUpdate()
    {
        if (sonido.gameObject.activeInHierarchy && sonido.name.Contains("_") && sonido.jug == null)
        {
            sonido.gameObject.name = sonido.gameObject.name.Substring(0, sonido.gameObject.name.Length - 1);
            sonido.gameObject.SetActive(false);
        }


        else if (sonido.gameObject.activeInHierarchy && !sonido.name.Contains("_") && sonido.jug == null)
            sonido.gameObject.name += "_";


    }

    public void Vestirse(string hash)
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




    }

    public void SonidoAlerta()
    {
        sonidoSoldado.PlayOneShot(alertaSonido);
    }
    void RotaRig(float yR = 0, bool conF = false)
    {
        float tiempoRot = 1;
        Vector2 mov = new Vector2(movimiento.x, movimiento.z);
        float newAngle = conF ? yR : Mathf.Atan2(mov.x, mov.y) * Mathf.Rad2Deg;

        float angle = Mathf.MoveTowardsAngle(Rig.eulerAngles.y, newAngle, tiempoRot * 1000 * Time.deltaTime);
        Rig.eulerAngles = new Vector3(0, angle, 0);
    }

    void Golpe(int numero, bool override_ = false)
    {
        print("no quiero hacer el void");
        pegar = true;
        //Si no lleva un arma a dos manos, puñetazos y patada normal
        if(override_ || armaEnMano == null || (armaEnMano != null && armaEnMano.TipoObjeto() < 2))
        {
            string nombreEstado = "ArmasLayer.GenomePunch" + numero.ToString();
            soldierAnimator.SetInteger("PunchCount", numero);
            soldierAnimator.Play(nombreEstado, 1, 0);
        }
        //Si no, culatazo al canto POM
        else
        {
            soldierAnimator.Play("ArmasLayer.GenomeCulatazo", 1, 0);
        }
        tiempoPausaPPK = 0.2f;
        //if (numero == 3)
           // tiempoPausaPPK = 3;
    }


    void ReordenaCosasVistas()
    {
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


        for(int i = 0; i < cosasVistas.Count; i++)
        {
            if (cosasVistas[i].Contains("explCer"))
                explInd.Add(i);
            else if (cosasVistas[i].Contains("verAJug"))
                jugInd.Add(i);
            else if (cosasVistas[i].Contains("falArma"))
                faltaInd.Add(i);
            else if (cosasVistas[i].Contains("compInc"))
                compInd.Add(i);
            else if (cosasVistas[i].Contains("rehen"))
                rehenInd.Add(i);
            else if (cosasVistas[i].Contains("chkZone"))
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
                string[] modulos = cosasVistas[tipo[i]].Split("_");

                string[] sitio;
                try
                {
                    sitio = modulos[1].Split(";");
                }
                catch
                {
                    sitio = new string[] { "0", "0", "0" };
                }

                Vector3 sitioPos = new Vector3(SacaNumString(sitio[0]), SacaNumString(sitio[1]), SacaNumString(sitio[2]));
                float dist = Vector3.SqrMagnitude(sitioPos - transform.position);
                distL.Add(dist);
            }
            distL.Sort();
            for(int i = 0; i < tipo.Count; i++)
            {
                string[] modulos = cosasVistas[tipo[i]].Split("_");

                string[] sitio;
                try
                {
                    sitio = modulos[1].Split(";");
                }
                catch
                {
                    sitio = new string[] { "0", "0", "0" };
                }


                Vector3 sitioPos = new Vector3(SacaNumString(sitio[0]), SacaNumString(sitio[1]), SacaNumString(sitio[2]));
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
        List<string> nuevasCosas = new List<string>();
        List<int> nuevosSteps = new List<int>();
        List<float> nuevoTiempo = new List<float>();
        for (int i = 0; i < indicesFinal.Count; i++)
        {
            nuevasCosas.Add(cosasVistas[indicesFinal[i]]);
            nuevosSteps.Add(cosasSteps[indicesFinal[i]]);
            nuevoTiempo.Add(cosasTiempo[indicesFinal[i]]);
        }
        cosasVistas = nuevasCosas;
        cosasTiempo = nuevoTiempo;
        cosasSteps = nuevosSteps;
    }

    void AjustaStepsYTiempo()
    {

        while (cosasSteps.Count < cosasVistas.Count)
        {
            cosasSteps.Add(0);
        }
        if (cosasSteps.Count > cosasVistas.Count)
        {
            cosasSteps.RemoveRange(cosasVistas.Count, cosasSteps.Count - cosasVistas.Count);
        }

        while (cosasTiempo.Count < cosasVistas.Count)
        {
            cosasTiempo.Add(0);
        }
        if (cosasTiempo.Count > cosasVistas.Count)
        {
            cosasTiempo.RemoveRange(cosasVistas.Count, cosasTiempo.Count - cosasVistas.Count);
        }


    }

    public float SacaNumString(string input)
    {
        float numero = float.Parse(input, CultureInfo.InvariantCulture.NumberFormat);
        return numero;
    }

    public void AgarreSoldado(Snake jugador)
    {
        jugadorAg = jugador.transform;
        agente.enabled = false;
        col.SetActive(false);
        pillado = true;
        //DropArma(armaEnMano);
        DropAll(Rig.forward, true);
        armaEnMano = null;
        SetArma();

        //Borra todo lo que ha visto y desactiva el collider de visión. También se quita de alertas y/o búsquedas
        cosasSteps = new List<int>();
        cosasTiempo = new List<float>();
        cosasVistas = new List<string>();

        vistaCol.SetActive(false);


        FindObjectOfType<GameManager>().AddAlerta(this, true);
        FindObjectOfType<GameManager>().AddAlerta(this, true, true);

    }
    public void LiberaSoldado(Snake jugador)
    {
        print("entro");
        jugadorAg = null;
        RaycastHit compPar;
        float dist = 0.35f;
        if(Physics.Raycast(transform.position, Rig.forward,out compPar, dist, suelo))
        {
            cqcNormal *= -1;
        }

        transform.position += cqcNormal * dist;
        agente.enabled = true;
        col.SetActive(true);
        agachado = false;
        pillado = false;
        vistaCol.SetActive(!controller.enabled);
        //vistaCol.GetComponent<VistaSoldado>().CheckZone(cqcPos);

        //No hay caja porque no puedes pillar con cqc a nadie vestido de caja
        vistaCol.GetComponent<VistaSoldado>().Alerta(cqcPos, false, false, jugador.playerID);

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
    public void DatosCQC(Vector3 normal, bool agach, Vector3 pos, bool posB = true)
    {
        agachado = agach;
        cqcNormal = normal;
        cqcPos = pos;
        pillado = true;
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
                sonidoSoldado.PlayOneShot(cqcCaerGrito, .15f);
            }

            if (jugadorAg != null)
                jugadorAg.GetComponent<Snake>().EliminaSoldado();
        }
    }

    public bool Rehen(Vector3 posicionJug, bool ag)
    {
        if (!cosasVistas.Contains("rehen"))
        {
            //Hay que ver si el jugador no está a la vista

            Vector3 direccionMirada = Rig.forward;
            Vector3 direccionJug = posicionJug - transform.position;
            if (Vector3.Dot(direccionJug.normalized, direccionMirada) > 0.35f && Mathf.Abs(direccionJug.y) < controller.height / 3)
                return false;

            cosasVistas.Add("rehen");
            armaEnMano = null;
            SetArma();
            agachado = ag;
            return !(soldierAnimator.GetCurrentAnimatorStateInfo(0).IsName("RehenStand") || soldierAnimator.GetCurrentAnimatorStateInfo(0).IsName("RehenAg"));
        }
        else
        {

            agachado = ag;
            int indexRehen = cosasVistas.IndexOf("rehen");
            if (cosasSteps.Count <= indexRehen)
                return false;
            cosasSteps[indexRehen] = 0;
        }
        return false;
    }
    public void Throw(Snake jugador, bool tirar, float target, Transform aux = null)
    {
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

    }

    public void BajaOxigeno(float valor)
    {
        oxigeno -= valor;
        oxigeno = Mathf.Clamp(oxigeno, 0, 100);
    }

    public void QuitaVida(float cantidad, bool sonido = true, float tiempoParar = 0.25f)
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
            if(sonido)
                sonidoSoldado.PlayOneShot(dolorSonido);

            soldierAnimator.Play("DolorLayer.Dolor", 2, 0);
        }

        //Al morir quitar CQC
        else 
        {
            if (jugadorAg != null && !controller.enabled)
            {
                ThrowDirection(cqcNormal * .1f, jugadorAg.GetComponent<Snake>());
                jugadorAg.GetComponent<Snake>().Libera();
                //jugadorAg = null;
                cosasVistas = new List<string>();
                DesactivaVista();
            }

            if (cantidad >= 100 && sonido)
            {
                sonidoSoldado.PlayOneShot(Resources.Load<AudioClip>("Audio/Armas/Headshot"), 0.25f);
            }

            FindObjectOfType<GameManager>().AddAlerta(this, true, true);
            FindObjectOfType<GameManager>().AddAlerta(this, true);
            FindObjectOfType<GameManager>().MataSoldado(gameObject);
        }




        }
    public bool Consciente()
    {
        return oxigeno > 0 && !KO;
    }

    public void SonidoAgarre()
    {
        sonidoSoldado.PlayOneShot(ahogueSonido);
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
            sonidoSoldado.PlayOneShot(Resources.Load<AudioClip>("Audio/Soldiers/Interroga" + i.ToString()));

            informacion.RemoveAt(0);

        }
        else
        {
            //Si ha fallado el interrogatorio porque es información de algo ya hecho, entonces borra esa información y ya
            if (informacion.Count > 0)
                informacion.RemoveAt(0);


            sonidoSoldado.PlayOneShot(ahogueSonido);
        }
    }

    public float Vida()
    {
        return vida;
    }
    string VectorAString(Vector3 input, string separador = ";")
    {

        return input.x.ToString(CultureInfo.InvariantCulture.NumberFormat) + separador +
            input.y.ToString(CultureInfo.InvariantCulture.NumberFormat) + separador +
            input.z.ToString(CultureInfo.InvariantCulture.NumberFormat);

    }

    public bool RecibeArma(Arma arma, int balas_ = 0)
    {
        if (!EstaVivo() || !Consciente())
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
        else if(cosasVistas.Count > 0 && other.name == "InteraccionSoldado" && cosasVistas[0] == "rehen" && other != interacCol)
        {
            cabeza.GetComponent<RecibeSonidos>().disabled = false;
            cosasVistas.RemoveAt(0);
            cosasTiempo.RemoveAt(0);
            cosasSteps.RemoveAt(0);
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
        if (controller.enabled || KO)
            return;
        else
        {
            Soldier soldadoImpulsado = other.transform.parent.parent.GetComponent<Soldier>();

            if (!soldadoImpulsado.controller.enabled || soldadoImpulsado.KO) //Si no lo están tirando o ya ha caído al suelo, no te impulsa
                return;

            Throw(soldadoImpulsado.jugadorAg.GetComponent<Snake>(), soldadoImpulsado.impulsa, soldadoImpulsado.oxigTarget);


            interacCol.name = "InteraccionSoldadoInc";

            DatosCQC(soldadoImpulsado.cqcNormal, agachado, transform.position, EstaVivo() || Consciente());
        }

    } 

    public void ThrowDirection(Vector3 dir, Snake jug)
    {
        if (controller.enabled || KO)
            return;
        else
        {

            Throw(null, true, 100, jug.transform);


            interacCol.name = "InteraccionSoldadoInc";
            Vector3 pos = transform.position + (dir * .1f) + (Vector3.down * .1f);
            DatosCQC(dir, false, pos, true);
        }
    }

    public void SetPPKT(float t)
    {
        t = Mathf.Clamp01(t);
        tiempoPausaPPK = t;
    }

    void CheckRadar()
    {
        if (modoRadar != 1 && cosasVistas.Count > 0 && !cosasVistas[0].Contains("verAJug"))
        {
            modoRadar = 1;
            fovRadar.materials[0].EnableKeyword("_MODO_CAUTION");
            fovRadar.materials[0].DisableKeyword("_MODO_NORMAL");
            fovRadar.materials[0].DisableKeyword("_MODO_ALERT");
        }
        else if(modoRadar != 2 && cosasVistas.Count > 0 && cosasVistas[0].Contains("verAJug"))
        {
            modoRadar = 2;
            fovRadar.materials[0].EnableKeyword("_MODO_ALERT");
            fovRadar.materials[0].DisableKeyword("_MODO_NORMAL");
            fovRadar.materials[0].DisableKeyword("_MODO_CAUTION");
        }
        else if(modoRadar != 0 && cosasVistas.Count == 0)
        {
            modoRadar = 0;
            fovRadar.materials[0].EnableKeyword("_MODO_NORMAL");
            fovRadar.materials[0].DisableKeyword("_MODO_ALERT");
            fovRadar.materials[0].DisableKeyword("_MODO_CAUTION");
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

    void Disparar()
    {
        dispAux = true;
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
                sonidoSoldado.PlayOneShot(armaEnMano.sonido(), 0.5f);

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
            Vector3 armaInicio = armaHolder.position;

            float factorEscala = 1 / transform.localScale.x;
            Vector3 inicio = (armaInicio - transform.position) * factorEscala; Vector3 final = (posDisparo.Value - transform.position)*factorEscala;


            trailBala.positionCount = 2;
            trailBala.SetPositions(new Vector3[] { inicio, final });
            StopCoroutine(TiempoBalas());
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

    void DropAll(Vector3 direccionTiro = new Vector3(), bool customDir = false)
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
        for(int i = 0; i< cosasVistas.Count; i++)
        {
            string[] modulos = cosasVistas[i].Split("_");
            if (modulos[0] == "chkZone" && modulos[modulos.Length-1] == jug.playerID.ToString()) { j = i; break; }
            else if(modulos[0] == "verAJug" && modulos[modulos.Length - 1] == jug.playerID.ToString()) { return; }

        }
        if( j > -1)
        {

            string cosa = "chkZone_" + VectorAString(jug.transform.position) + "_" + jug.playerID.ToString();
            cosasVistas[j] = cosa;
            if(cosasTiempo.Count > j)
                cosasTiempo[j] = 0;
            if(cosasSteps.Count > j)
                cosasSteps[j] = 0;
        }
        else
        {
            string cosa = "chkZone_" + VectorAString(jug.transform.position) + "_" + jug.playerID.ToString();
            cosasVistas.Add(cosa);
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
        trailBala.positionCount = 0;

    }

    public void DesactivaVista()
    {
        vistaCol.SetActive(false);
    }

    public bool EnAlerta()
    {
        return cosasVistas[0].Contains("verAJug");
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

                    informacion.Add("The soldier patrols around the " + sitio);
                }
                else
                {

                    string[] vestimenta = FindObjectOfType<GameManager>().GetVest(infoClave);

                    if (vestimenta[1] == "-")
                    {
                        informacion.Add("The soldier doesn't wear a " + vestimenta[0]);
                    }
                    else
                    {
                        informacion.Add("The soldier wears a " + vestimenta[1] + " " + vestimenta[0]);
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

}
