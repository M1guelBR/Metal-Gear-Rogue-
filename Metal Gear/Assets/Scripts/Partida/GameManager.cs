using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class GameManager : MonoBehaviour
{
    NavMeshSurface surface;
    Vector2 r = Vector2.zero;
    [Range(0, 1)] public float timeSc = 1;
    [SerializeField] bool autoBake = false;
    public List<GameObject> crates = new List<GameObject>();

    //Sistema de alertas
    List<Soldier> soldadosEnAlerta = new List<Soldier>(), soldadosBuscando = new List<Soldier>();
    int numAlertas = 0; int antSAlert = 0; bool pantallaAlerta = false;
    //Slots de soldados que pueden disparar
    static int huecosDisparar = 3; 
    int huecosQuedan = huecosDisparar;
    static float tiempoHuecos = 5;
    float actTiempoHuecos = 0;

    int cantMuertes = 0;
    public int tipoMision = 0;

    public GameObject soldadoElim;
    //public RutaSoldado ruta;
    public string patrolZone;
    public List<GameObject> ficheros = new List<GameObject>();
    public List<GameObject> columnas = new List<GameObject>();


    //Lista que no debemos tocar para saber cual es el indice de cada columna y fichero en la partida
    //Util para la informacion suministrada al jugador
    List<GameObject> fichRef = new List<GameObject>();
    List<GameObject> colRef = new List<GameObject>();

    float tAlerta = 0;
    float tiempoMision = 0;

    List<int> infoAdq = new List<int>(); //Informacion ya adquirida
    List<int> infoRed= new List<int>();  //Informacion redundante

    public List<Snake> jugadores = new List<Snake>();
    List<bool> pausa = new List<bool>();
    bool pausaGeneral = false;
    GameObject gameOver;
    List<Snake> jugadoresMuertos = new List<Snake>();
    int cantJugs = 1;
    [SerializeField] bool cambiaInfo = true;
    [SerializeField] bool autoServer = true;


    //Minimapa
    int curPlanta = 1;
    [SerializeField]float[] alturasPlantas = new float[0];
    //[SerializeField] Camera camMiniMapa;

    //Post-Procesado
    [SerializeField] UniversalRendererData[] renderers;

    //Scripts auxiliares de control que indican cual es cada jugador y su mando
    PlayerInput[] mandos = new PlayerInput[0];
    Vector2 relAspecto = new Vector2(1, 1);


    //Guardamos las salas en las que está el jugador/los jugadores
    public List<GameObject> salasEnJuego = new List<GameObject>();
    List<Sala> salasConJug = new List<Sala>();

    void Start()
    {
        //BuscaControles();
        //Crea la pantalla de Game Over
        gameOver = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/GameOverScreen"));
        gameOver.name = "GameOverScreen";

        if (!GetComponent<NavMeshSurface>())
        {
            NavMeshSurface surf = gameObject.AddComponent<NavMeshSurface>();
            surf.agentTypeID = 0;
            surf.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            surf.layerMask = (1 << LayerMask.NameToLayer("EscenarioColliders")) | (1 << LayerMask.NameToLayer("CollidersEscAux"));

        }

        surface = GetComponent<NavMeshSurface>();
        //print(surface.agentTypeID);

        if (autoBake)
        {
            surface.BuildNavMesh();
            ActivaVistas();
            PonRenderers();
            for (int i = 0; i < jugadores.Count; i++)
                pausa.Add(false);
        }

        Soldier[] soldadosEnPartida = FindObjectsOfType<Soldier>();
        for(int i = 0; i < soldadosEnPartida.Length; i++)
        {
            soldadosEnPartida[i].ID = i;
        }
        actTiempoHuecos = tiempoHuecos;
    }

    // Update is called once per frame
    void Update()
    {
        //Ponemos la camara del minimapa
        if (autoBake)
        {
            IndicaSalas(surface.navMeshData.sourceBounds.center, surface.navMeshData.sourceBounds.size, alturasPlantas, 2.4f);
            autoBake = false;
        }

        if ((colRef.Count == 0 != (columnas.Count == 0)) ||
            (fichRef.Count == 0 != (ficheros.Count == 0)))
        {

            colRef.AddRange(columnas);
            fichRef.AddRange(ficheros);

        }


        foreach(GameObject salaGO in salasEnJuego)
        {
            Sala sala = salaGO.GetComponent<Sala>();

            bool tieneJugador = false;
            for(int i = 0; i < jugadores.Count; i++)
            {
                if (sala.TieneJugador(jugadores[i].transform.position))
                {
                    tieneJugador = true;
                    break;
                }
            }

            if (tieneJugador)
            {

            }
            else
            {

            }

        }

        if (gameOver.activeInHierarchy)
            return;


        if(Time.timeScale != 0)
            Time.timeScale = timeSc;


        if (r.x != Screen.currentResolution.width || r.y != Screen.currentResolution.height)
            ActualizaEscala();

        float max = -1; List<Soldier> aRem = new List<Soldier>();
        bool exp = false;

        actTiempoHuecos -= Time.deltaTime;

        foreach (Soldier sold in soldadosEnAlerta)
        {
            //QUITAMOS A LOS SOLDADOS NO CONSCIENTES O QUE NO PERSIGUEN AL JUGADOR
            if (!sold.Consciente() || !sold.EstaVivo() || 
                (sold.pensamientos[0].tipo != Pensamiento.JUGADOR && sold.pensamientos[0].tipo != Pensamiento.EXPLOSIVO_CERCA))
            {
                aRem.Add(sold);
                continue;
            }

            if (sold.pensamientos[0].tipo != Pensamiento.EXPLOSIVO_CERCA)
            {
                max = Mathf.Max(max, sold.pensamientos[0].tiempo);
                //Si le toca seleccionar otra vez, pone que ninguno en un principio puede disparar
                if(actTiempoHuecos < 0)
                {
                    sold.puedeDisparar = false;
                    //Si quedan huecos disponibles, entonces puede disparar
                    //CAMBIAR POR TACTICAS
                    if(huecosQuedan > 0)
                    {
                        sold.puedeDisparar = true;
                        huecosQuedan -= 1;
                    }
                }

            }
            else
                exp = true;
        }

        if(actTiempoHuecos < 0)
        {
            //Reordenar lista de soldados en alerta
            DesordenaSoldados();
            //Ponemos que quedan los huecos iniciales para la proxima
            //HACER LAS TACTICAS
            huecosQuedan = huecosDisparar;
            //Ponemos el tiempo donde estaba
            actTiempoHuecos = tiempoHuecos;
        }

        for (int i = 0; i < aRem.Count; i++)
        {
            soldadosEnAlerta.Remove(aRem[i]);

        }

        if (max > 0)
        {
            FindObjectOfType<UI>().TAlerta(max * 100 / 3);
            tAlerta = max * 100 / 3;
        }

        else if(tAlerta > 0 && !exp)
        {
            tAlerta -= 50f * Time.deltaTime;
            FindObjectOfType<UI>().TAlerta(tAlerta);
            //Si no hay tiempo de alerta es porque no hay alerta
            if (tAlerta <= 0)
            {
                tAlerta = 0;
                FindObjectOfType<UI>().Alerta(false);
                FindObjectOfType<Musica>().ResetMusicaCola();
                FindObjectOfType<Musica>().RepMusica();
            }
        }

        if((tAlerta == 0 || soldadosEnAlerta.Count == 0) && pantallaAlerta)
        {
            pantallaAlerta = false;
            FindObjectOfType<UI>().Alerta(false);
        }

    }

    private void LateUpdate()
    {
        antSAlert = soldadosEnAlerta.Count;
        if(Time.timeScale > 0)
            tiempoMision += Time.unscaledDeltaTime;
    }

    public void ActualizaEscala()
    {
        foreach(Snake jugador in jugadores)
        {
            CanvasScaler interfaz = jugador.interfaz.GetComponent<CanvasScaler>();
            Vector2 res = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);
            //Vector2 escala = jugador.Cam.GetComponent<Camera>().rect.size;
            r = res;
            res = interfaz.GetComponent<RectTransform>().sizeDelta;
            //Como vemos la relacion de aspecto?
            float aspect = (res.x / res.y) * (relAspecto.x / relAspecto.y);

            Vector2 min = interfaz.referenceResolution;
            float minAspect = min.x / min.y;

            interfaz.matchWidthOrHeight = aspect < minAspect ? 0 : 1;

        }

    }


    public void AddAlerta(Soldier sold, bool remove = false, bool busc = false, bool overlook = false)
    {
        if (!overlook && !remove && sold.pensamientos.Count > 0 && sold.pensamientos[0].tipo != Pensamiento.JUGADOR)
        //(!overlook && !remove && sold.cosasVistas.Count > 0 && !sold.cosasVistas[0].Contains("verAJug"))
        {
            return;
        }

        if (!busc)
        {
            //Si no había nadie en alerta ni buscando pero se añaden, hay una nueva alerta
            if (soldadosEnAlerta.Count == 0 && !remove )//&& soldadosBuscando.Count == 0)
            {
                numAlertas += 1;


                FindObjectOfType<Musica>().AlertaMusica();

                //Hacemos que tenga que ponerlo como activo
                actTiempoHuecos = 0;

                //Si los soldados se alertan 6 veces distintas, entran en modo precaución
                if (numAlertas == 6)
                    Caution();

            }


            if (!remove && !soldadosEnAlerta.Contains(sold))
                soldadosEnAlerta.Add(sold);

            else if (remove && soldadosEnAlerta.Contains(sold))
            {
                soldadosEnAlerta.Remove(sold);
                if(soldadosEnAlerta.Count == 0)
                {
                    GetComponent<AudioSource>().Stop();
                }
            }
        }
        else
        {

            if (!remove && !soldadosBuscando.Contains(sold))
                soldadosBuscando.Add(sold);

            else if (remove && soldadosBuscando.Contains(sold))
                soldadosBuscando.Remove(sold);
        }

        FindObjectOfType<UI>().Alerta(soldadosEnAlerta.Count != 0);
        pantallaAlerta = soldadosEnAlerta.Count != 0;
    }

    public bool haIncrementado()
    {
        return antSAlert < soldadosEnAlerta.Count; 
    }

    public void cambiaAlertas(int input)
    {
        if(numAlertas + input < 6 && numAlertas >= 6)
        {
            DesactivaCaution();
        }
        numAlertas += input;
    }

    void Caution()
    {
        foreach(Soldier soldado in FindObjectsOfType<Soldier>())
        {
            soldado.ActivaCaution();
        }

        FindObjectOfType<UI>().ActivaCaution();

    }

    void DesactivaCaution()
    {
        foreach (Soldier soldado in FindObjectsOfType<Soldier>())
        {
            soldado.DesactivaCaution();
        }

        FindObjectOfType<UI>().DesactivaCaution();


    }

    public void ActivaVistas(bool activar = true)
    {
        foreach(Soldier soldado in FindObjectsOfType<Soldier>())
        {
            soldado.vistaCol.SetActive(activar);
        }
    }

    //Cambiar texto info
    public void RecogeFichero(Fichero fichero)
    {
        int indRem = ficheros.IndexOf(fichero.gameObject);
        

        ficheros.Remove(fichero.gameObject);
        //Actualizar información de la mision
        if (!cambiaInfo)
            return;

        if(ficheros.Count > 1)
        {
            //Poner en la pantalla de informacion que la mision es escapar
            foreach (Snake jugador in jugadores)
            {
                string informacion = jugador.SacaInfo();
                string primerElemento = informacion.Split("\n")[0];
                primerElemento = informacion.Substring(primerElemento.Length, informacion.Length - primerElemento.Length);

                jugador.RecibeInformacion("RETRIEVE THE " + ficheros.Count.ToString() + " CLASSIFIED FILES" + primerElemento,0,true);

            }

        }
        else if(ficheros.Count == 1)
        {
            //Poner en la pantalla de informacion que la mision es escapar

            foreach (Snake jugador in jugadores)
            {
                string informacion = jugador.SacaInfo();
                string primerElemento = informacion.Split("\n")[0];
                primerElemento = informacion.Substring(primerElemento.Length, informacion.Length - primerElemento.Length);

                jugador.RecibeInformacion("RETRIEVE THE CLASSIFIED FILE" + primerElemento,0, true);

            }

        }
        else
        {
            //Poner en la pantalla de informacion que la mision es escapar
            foreach (Snake jugador in jugadores)
            {
                jugador.QuitaInfo(0, true);
                jugador.RecibeInformacion("ESCAPE THE BASE", 0, true);

            }

        }


        int indTach = fichRef.IndexOf(fichero.gameObject);
        if (infoAdq.Contains(indTach))
        {
            //Rehacer con el tachado
            foreach (Snake jugador in jugadores)
            {
                jugador.interfaz.TachaTexto(indTach + 1);
            }
        }
        infoRed.Add(indRem);


    }

    //Cambiar texto info
    public void QuitaColumna(Columna columna)
    {
        int indRem = columnas.IndexOf(columna.gameObject);
        

        columnas.Remove(columna.gameObject);
        if (!cambiaInfo)
            return;

        if (columnas.Count > 1)
        {
            //Poner en la pantalla de informacion que la mision es escapar
            foreach (Snake jugador in jugadores)
            {
                string informacion = jugador.SacaInfo();
                string primerElemento = informacion.Split("\n")[0];
                primerElemento = informacion.Substring(primerElemento.Length, informacion.Length - primerElemento.Length);

                jugador.RecibeInformacion("DESTROY THE " + columnas.Count.ToString() + " MAIN PILLARS THAT HOLD THE BASE" + primerElemento, 0, true);

            }

        }
        else if (columnas.Count == 1)
        {
            //Poner en la pantalla de informacion que la mision es escapar
            foreach (Snake jugador in jugadores)
            {
                string informacion = jugador.SacaInfo();
                string primerElemento = informacion.Split("\n")[0];
                primerElemento = informacion.Substring(primerElemento.Length, informacion.Length - primerElemento.Length);

                jugador.RecibeInformacion("DESTROY THE REMAINING PILLAR THAT HOLDS THE BASE" + primerElemento, 0, true);

            }

        }
        else
        {
            //Poner en la pantalla de informacion que la mision es escapar
            foreach (Snake jugador in jugadores)
            {
                jugador.QuitaInfo(0, true);
                jugador.RecibeInformacion("ESCAPE THE BASE", 0, true);

            }

        }

        int indTach = colRef.IndexOf(columna.gameObject);
        if (infoAdq.Contains(indTach))
        {
            //Rehacer con el tachado
            foreach(Snake jugador in jugadores)
            {
                jugador.interfaz.TachaTexto(indTach+1);
            }
        }
        infoRed.Add(indRem);

    }

    public void MataSoldado(GameObject sold, Transform culpable)
    {
        if (sold == soldadoElim && cambiaInfo)
        {
            soldadoElim = null;

            //Poner en la pantalla de informacion que la mision es escapar
            foreach(Snake jugador in jugadores)
            {
                jugador.QuitaInfo(0,true);
                jugador.RecibeInformacion("ESCAPE THE BASE", 0, true);

            }

        }
        if(culpable.GetComponent<Snake>())
            cantMuertes += 1;

    }

    public string[] GetVest(int infoClave)
    {
        string[] vestimentas = new string[2];

        vestimentas[1] = "-";

        if (infoClave == 0)
        {
            vestimentas[0] = "cap";

        }
        else if (infoClave == 1)
        {
            vestimentas[0] = "scarf";
        }
        else if (infoClave == 2)
        {
            vestimentas[0] = "kerchief in their arm";

        }
        else if(infoClave == 3)
        {
            vestimentas[0] = "kerchief in their leg";
        }

        if (soldadoElim.GetComponent<Soldier>().ropa(infoClave) != null)
            vestimentas[1] = soldadoElim.GetComponent<Soldier>().ropa(infoClave).sharedMaterial.name.ToLower();

        return vestimentas;
    }

    public bool RecibeInformacion(string info)
    {
        //Rehacer con nuevo sistema de menu informacion

        string[] modulos = info.Split(";");
        int linea = -1;
        //Si es informacion que depende de un objeto y no es redundante, se añade
        if (modulos.Length > 1 && !infoRed.Contains(int.Parse(modulos[1])))
        {
            linea = (int.Parse(modulos[1]));
        }

        //Si depende de un objeto pero es redundante, no se hace nada y el soldado no habla
        else if (modulos.Length > 1 && infoRed.Contains(int.Parse(modulos[1])))
            return false;

        //Para cada jugador, añade la informacion a su pantalla de información
        //La linea a añadir es la ultima a la añadida a la informacion adquirida
        foreach (Snake jugador in jugadores)
            jugador.RecibeInformacion(modulos[0], linea+1);

        infoAdq.Add(linea);
        return true;
    }

    public void IndicaMision(int modo, int auxData = 0)
    {
        string instruccion = "";
        switch (modo)
        {
            case 0:
                instruccion = "Kill the specialized soldier";
                break;

            case 1:
                instruccion = "Retrieve the " + (auxData > 1 ? auxData.ToString() : "") + " classified file" + (auxData > 1 ? "s" : "");
                break;

            case 2:
                instruccion = "Destroy the " + auxData.ToString() + " main pillars that hold the base";
                break;
        }



        foreach (Snake jugador in jugadores)
        {
            jugador.RecibeInformacion(instruccion,0);
        }

        tipoMision = modo;
    }

    public void LoadScene(string nombre)
    {

        SceneManager.LoadScene(nombre);
    }
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MataJugador(Snake jugador, bool morir = false)
    {
        print("matar");
        jugadores.Remove(jugador);

        if (morir)
            jugadoresMuertos.Add(jugador);

        if(jugadores.Count == 0)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            //Activa la pantalla de game over y quita el resto de la musica
            this.GetComponent<AudioListener>().enabled = true;
            this.GetComponent<Musica>().enabled = false;
            this.GetComponent<AudioSource>().Stop();
            gameOver.SetActive(true);


            //Puntuación de la partida
            bool sinPistolas = true;
            {
                foreach(Snake jug in FindObjectsOfType<Snake>())
                {
                    if (jug.tienePist)
                    {
                        sinPistolas = false;
                        break;
                    }
                }

            }



            //Destruye TODOS los objetos
            Destroy(GetComponent<NavMeshSurface>(), 0);
            Destroy(GetComponent<SettingsManager>(), 0);
            GameObject[] gameObjects = FindObjectsOfType<GameObject>();
            for(int i = 0; i < gameObjects.Length; i++)
            {
                //Los auxiliares de multijugador no los borra
                if (gameObjects[i].transform.parent != null ||
                    gameObjects[i].name == "Manager" || gameObjects[i].name == "CamContainer" ||
                    gameObjects[i].name == "GameOverScreen" || gameObjects[i].name == "EventSystem"||
                    gameObjects[i].name == "GameManager"|| gameObjects[i].GetComponent<MultipAux>())
                    continue;

                Destroy(gameObjects[i], 0);
            }

            //Mision completada y no han muerto todos
            //Game over mal?
            if(soldadoElim == null && columnas.Count == 0 && ficheros.Count == 0 && jugadoresMuertos.Count < cantJugs)
            {
                gameOver.transform.GetChild(2).GetComponent<TMP_Text>().text = "MISSION COMPLETED!";

                //Le manda la info de la mision, si es mision de eliminacion, el soldado a eliminar no lo tiene en cuenta
                gameOver.GetComponent<GameOverScreen>().SetPoints(true && cambiaInfo, (int)tiempoMision, numAlertas, cantMuertes - (tipoMision == 0 ? 1 : 0), sinPistolas);

                //Desbloquea el modo big boss
                if(numAlertas == 0 && cantMuertes - (tipoMision == 0 ? 1 : 0) == 0 && PlayerPrefs.GetInt("NSnake", -1) == -1)
                {
                    PlayerPrefs.SetInt("NSnake", 0);
                }

            }
            //Mision sin completar
            else
            {
                string textoMision = "MISSION FAILED";
                if (jugadoresMuertos.Count < cantJugs)
                    textoMision = "YOU ESCAPED WITHOUT COMPLETING THE MISSION";
                gameOver.transform.GetChild(2).GetComponent<TMP_Text>().text = textoMision;
                gameOver.GetComponent<GameOverScreen>().SetPoints(false);
            }

            //Hacer lista de puntuacion?
        }
    }

    public void IndicaSalas(Vector3 centroSalas, Vector3 tamSalas, float[] alturas, float alturaPlanta)
    {
        //camMiniMapa = cam;
        alturasPlantas = alturas;

        float maxAnchor = Mathf.Max(tamSalas.x, tamSalas.z);

        //Ahora hay que hacer que la camara
        //Se centre en la planta en la que spawnea el jugador
        for (int j = 0; j < jugadores.Count; j++)
        {
            Camera cam = jugadores[j].interfaz.miniMapaCam;
            //La camara enfoca todo el mapa
            cam.transform.position = centroSalas;
            cam.orthographicSize = (maxAnchor / 2) * 1.25f;
            float ySpawn = jugadores[j].transform.position.y;

            //Primero hay que hallar la planta
            int planta = 0; float dist = Mathf.Infinity;
            for(int i = 0; i < alturas.Length; i++)
            {
                float alturaAnalizar = alturas[i];
                if(dist > Mathf.Abs(ySpawn - alturaAnalizar))
                {
                    planta = i;
                    dist = Mathf.Abs(ySpawn - alturaAnalizar);
                }
            }

            curPlanta = planta + 1;
            //Ponemos el plano del far lo justo para que renderize el minimapa
            cam.farClipPlane = alturaPlanta * 0.75f;
        
            jugadores[j].interfaz.BotonPlanta(0);
        }

    }

    public int ActualizaCamaraMinimapa(Camera camMiniMapa, int plantaSent = 0)
    {
        curPlanta += plantaSent;
        while (curPlanta < 1)
            curPlanta += alturasPlantas.Length;
        while (curPlanta > alturasPlantas.Length)
            curPlanta -= alturasPlantas.Length;
        int planta = curPlanta-1;

        Vector3 posY = camMiniMapa.transform.position;
        //Situamos la camara justo en la planta en la que está Snake
        //Y la subimos un poco para que pueda renderizar
        posY.y = alturasPlantas[planta] + (0.2f * camMiniMapa.farClipPlane);
        //Ponemos el plano del far lo justo para que renderize el minimapa
        camMiniMapa.transform.position = posY;

        return planta;

    }

    void PonRenderers()
    {
        for (int i = 0; i < jugadores.Count; i++)
        {

            jugadores[i].SetCameraRenderData(renderers[i], i);

        }
    }

    public void MeteRenderers(UniversalRendererData[] input)
    {
        renderers = input;
        PonRenderers();
    }

    public void PonJugadorEnLista(Snake jugador)
    {
        jugador.playerID = jugadores.Count;
        jugadores.Add(jugador);
        pausa.Add(true);
        //Si hay varios jugadores en partida
        if (jugadores.Count == 2)
            relAspecto = new Vector2(.5f,1);
        else
            relAspecto = new Vector2(1, 1);
    }

    public void PonMultiplayer(RenderTexture[] tex, Material[] mats)
    {
        for(int i = 0; i < jugadores.Count; i++)
        {
            jugadores[i].AjustaCamMarco(i, jugadores.Count);
            jugadores[i].interfaz.opsMenu.SetId(i);

            //Ponerle input a cada jugador
            //Se asigna el jugador al mando en específico
            if (mandos.Length > 0)
            {
                Snake jugador = jugadores[i];
                InputDevice[] devices_ = mandos[i].devices.ToArray();
                jugador.playerInput.actions.devices = devices_;
                jugador.interfaz.SetRadarRendTex(tex[i], mats[i]);
            }

        }
    }

    public void BuscaControles()
    {
        //Buscamos los indicadores
        MultipAux[] indicadores = FindObjectsOfType<MultipAux>();
        mandos = new PlayerInput[indicadores.Length];
        for(int i = 0; i < indicadores.Length; i++)
        {
            mandos[i] = indicadores[i].GetComponent<PlayerInput>();
        }

    }

    public void SetSensitivity(float sensitivity, int id)
    {
        if (id > jugadores.Count)
            return;
        jugadores[id].SetSensitivity(sensitivity);
    }
    public void SetInversiones(int pos,bool value, int id)
    {
        if (id > jugadores.Count)
            return;
        jugadores[id].SetInversiones(pos, value);
    }
    public void SetFov(int fov, bool fps, int id)
    {
        if (id > jugadores.Count)
            return;
        jugadores[id].SetFov(fov, fps);
    }

    public bool Pausa(bool valor, int ind = 0)
    {
        if (!pausaGeneral && valor)
        {
            pausaGeneral = true;
            pausa[ind] = valor;
            Time.timeScale = 0;
            foreach (Snake jug in jugadores)
            {
                jug.pausa = true;
                jug.interfaz.pausaFondo.SetActive(true);
            }
            return true;
        }
        else if(pausaGeneral && valor)
        {
            return true;
        }
        else if(pausaGeneral && !valor && pausa[ind])
        {
            pausaGeneral = false;
            pausa[ind] = false;
            Time.timeScale = 1;
            foreach (Snake jug in jugadores)
            {
                jug.pausa = false;
                jug.interfaz.pausaFondo.SetActive(false);
            }

        }

        return false;

    }

    public void SonidoFisico(AudioSource source, AudioClip clip, float minVolume, float maxVolume, float volumeMult, bool overlap = true)
    {
        //Hacemos el sonido lineal
        float cercaDist = Mathf.Infinity;
        float minDist = source.minDistance * source.minDistance;

        //Buscamos el que está mas cerca
        foreach(Snake jugador in jugadores)
        {
            float distComp = Vector3.SqrMagnitude(jugador.cabeza.transform.position - source.transform.position);
            if (distComp < cercaDist)
                cercaDist = distComp;
            if (cercaDist < minDist)
            {
                cercaDist = source.minDistance;
                break;
            }
        }
        //Si no ha encontrado jugadores es porque han muerto todos, en tal caso, buscamos entre los muertos
        if (cercaDist == Mathf.Infinity)
        {
            foreach (Snake jugador in jugadoresMuertos)
            {
                float distComp = Vector3.SqrMagnitude(jugador.cabeza.transform.position - source.transform.position);
                if (distComp < cercaDist)
                    cercaDist = distComp;
                if (cercaDist < minDist)
                {
                    cercaDist = source.minDistance;
                    break;
                }
            }
        }

        //Cuando lo hemos encontrado, calculamos el volumen
        minDist = source.minDistance;
        float maxDist = source.maxDistance;

        //Si no es la minima, hay que hacer la raiz cuadrada
        if(cercaDist != minDist)
            cercaDist = Mathf.Sqrt(cercaDist);

        //Si está demasiado lejos que no suene
        if (cercaDist > maxDist)
            return;

        float lerpValor = (minDist - cercaDist)/(minDist - maxDist);
        float volume = Mathf.Lerp(minVolume, maxVolume, lerpValor);

        source.pitch *= Time.timeScale;
        //Por ultimo, lo reproducimos en 2D 
        if(overlap)
            source.PlayOneShot(clip, volume * volumeMult);
        else
        {
            source.volume = volume * volumeMult;
            source.clip = clip;
            source.Play();
        }
    }

    public void DesordenaSoldados()
    {
        Soldier[] nuevoEnAlerta = new Soldier[soldadosEnAlerta.Count];

        for(int i = 0; i < nuevoEnAlerta.Length; i++)
        {
            //Cogemos un indice aleatorio de la lista y metemos el soldado en esa posicion al array
            int indCoger = Random.Range(0, soldadosEnAlerta.Count);
            Soldier aSacar = soldadosEnAlerta[indCoger];

            nuevoEnAlerta[i] = aSacar;
            //Luego, lo quitamos de la lista
            soldadosEnAlerta.RemoveAt(indCoger);
            //Acaba quedando un array desordenado y una lista vacia
        }
        //Por ultimo metemos el array en la lista
        soldadosEnAlerta.AddRange(nuevoEnAlerta);
    }

}
