using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    NavMeshSurface surface;
    Vector2 r = Vector2.zero;
    [Range(0, 1)] public float timeSc = 1;
    [SerializeField] bool autoBake = false;
    public List<GameObject> crates = new List<GameObject>();
    List<Soldier> soldadosEnAlerta = new List<Soldier>(), soldadosBuscando = new List<Soldier>();
    int numAlertas = 0; int antSAlert = 0;
    int cantMuertes = 0;
    public int tipoMision = 0;

    public GameObject soldadoElim;
    //public RutaSoldado ruta;
    public string patrolZone;
    public List<GameObject> ficheros = new List<GameObject>();
    public List<GameObject> columnas = new List<GameObject>();

    float tAlerta = 0;
    float tiempoMision = 0;

    List<int> infoAdq = new List<int>();
    List<int> infoRed= new List<int>();

    public List<Snake> jugadores = new List<Snake>();
    GameObject gameOver;
    List<Snake> jugadoresMuertos = new List<Snake>();
    int cantJugs = 1;


    // Start is called before the first frame update
    void Start()
    {
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
        }

        Soldier[] soldadosEnPartida = FindObjectsOfType<Soldier>();
        for(int i = 0; i < soldadosEnPartida.Length; i++)
        {
            soldadosEnPartida[i].ID = i;
        }

        ActualizaEscala();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameOver.activeInHierarchy)
            return;


        if(Time.timeScale != 0)
            Time.timeScale = timeSc;


        if (r.x != Screen.currentResolution.width || r.y != Screen.currentResolution.height)
            ActualizaEscala();

        float max = -1; List<Soldier> aRem = new List<Soldier>();
        bool exp = false;
        foreach (Soldier sold in soldadosEnAlerta)
        {
            if (!sold.Consciente() || !sold.EstaVivo() || (!sold.cosasVistas[0].Contains("verAJug") && !sold.cosasVistas[0].Contains("explCer")))
            {
                aRem.Add(sold);
                continue;
            }
            if (!sold.cosasVistas[0].Contains("explCer"))
                max = Mathf.Max(max, sold.cosasTiempo[0]);
            else
                exp = true;
        }

        for(int i = 0; i < aRem.Count; i++)
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
    }

    private void LateUpdate()
    {
        antSAlert = soldadosEnAlerta.Count;
        tiempoMision += Time.unscaledDeltaTime;
    }

    void ActualizaEscala()
    {
        CanvasScaler interfaz = FindObjectOfType<CanvasScaler>();
        Vector2 res = new Vector2(Screen.currentResolution.width, Screen.currentResolution.height);

        r = res;
        interfaz.matchWidthOrHeight = res.x / res.y;

        if (res.y < res.x)
            interfaz.matchWidthOrHeight = res.y / res.x;


    }


    public void AddAlerta(Soldier sold, bool remove = false, bool busc = false, bool overlook = false)
    {
        print("add");
        if (!overlook && !remove && sold.cosasVistas.Count > 0 && !sold.cosasVistas[0].Contains("verAJug"))
        {
            print("no add");
            return;
        }

        if (!busc)
        {
            //Si no había nadie en alerta ni buscando pero se añaden, hay una nueva alerta
            if (soldadosEnAlerta.Count == 0 && !remove )//&& soldadosBuscando.Count == 0)
            {
                numAlertas += 1;

                FindObjectOfType<Musica>().AlertaMusica();

                //Si los soldados se alertan 6 veces distintas, entran en modo precaución
                if (numAlertas == 6)
                    Caution();
            }


            if (!remove && !soldadosEnAlerta.Contains(sold))
                soldadosEnAlerta.Add(sold);

            else if (remove && soldadosEnAlerta.Contains(sold))
                soldadosEnAlerta.Remove(sold);
        }
        else
        {
            if (!remove && !soldadosBuscando.Contains(sold))
                soldadosBuscando.Add(sold);

            else if (remove && soldadosBuscando.Contains(sold))
                soldadosBuscando.Remove(sold);
        }

        FindObjectOfType<UI>().Alerta(soldadosEnAlerta.Count != 0);
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

    public void RecogeFichero(Fichero fichero)
    {
        int indRem = ficheros.IndexOf(fichero.gameObject);
        

        ficheros.Remove(fichero.gameObject);
        //Actualizar información de la mision

        if(ficheros.Count > 1)
        {
            //Poner en la pantalla de informacion que la mision es escapar
            foreach (Snake jugador in jugadores)
            {
                string informacion = jugador.SacaInfo();
                string primerElemento = informacion.Split("\n")[0];
                primerElemento = informacion.Substring(primerElemento.Length, informacion.Length - primerElemento.Length);

                jugador.RecibeInformacion("RETRIEVE THE " + ficheros.Count.ToString() + " CLASSIFIED FILES" + primerElemento, false, true);

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

                jugador.RecibeInformacion("RETRIEVE THE CLASSIFIED FILE" + primerElemento, false, true);

            }

        }
        else
        {
            //Poner en la pantalla de informacion que la mision es escapar
            foreach (Snake jugador in jugadores)
            {

                jugador.RecibeInformacion("ESCAPE THE BASE", false, true);

            }

        }
        if (infoAdq.Contains(indRem))
        {
            indRem = infoAdq.IndexOf(indRem);
            foreach (Snake jugador in jugadores)
            {
                jugador.QuitaInfo(indRem);
            }
            infoAdq.Remove(indRem);
        }
        infoRed.Add(indRem);


    }

    public void QuitaColumna(Columna columna)
    {
        int indRem = columnas.IndexOf(columna.gameObject);
        

        columnas.Remove(columna.gameObject);

        if (columnas.Count > 1)
        {
            //Poner en la pantalla de informacion que la mision es escapar
            foreach (Snake jugador in jugadores)
            {
                string informacion = jugador.SacaInfo();
                string primerElemento = informacion.Split("\n")[0];
                primerElemento = informacion.Substring(primerElemento.Length, informacion.Length - primerElemento.Length);

                jugador.RecibeInformacion("DESTROY THE " + columnas.Count.ToString() + " MAIN PILLARS THAT HOLD THE BASE" + primerElemento, false, true);

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

                jugador.RecibeInformacion("DESTROY THE REMAINING PILLAR THAT HOLDS THE BASE" + primerElemento, false, true);

            }

        }
        else
        {
            //Poner en la pantalla de informacion que la mision es escapar
            foreach (Snake jugador in jugadores)
            {
                jugador.RecibeInformacion("ESCAPE THE BASE", false, true);

            }

        }
        if (infoAdq.Contains(indRem))
        {
            indRem = infoAdq.IndexOf(indRem);
            foreach (Snake jugador in jugadores)
            {
                jugador.QuitaInfo(indRem);
            }
            indRem = columnas.IndexOf(columna.gameObject);
            infoAdq.Remove(indRem);
        }
        infoRed.Add(indRem);

    }

    public void MataSoldado(GameObject sold)
    {
        if (sold == soldadoElim)
        {
            soldadoElim = null;

            //Poner en la pantalla de informacion que la mision es escapar
            foreach(Snake jugador in jugadores)
            {
                jugador.RecibeInformacion("ESCAPE THE BASE", false, true);

            }


        }
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

        string[] modulos = info.Split(";");

        //Si es informacion que depende de un objeto y no es redundante, se añade
        if (modulos.Length > 1 && !infoRed.Contains(int.Parse(modulos[1])))
        {
            infoAdq.Add(int.Parse(modulos[1]));
        }
        //Si depende de un objeto pero es redundante, no se hace nada y el soldado no habla
        else if (modulos.Length > 1 && infoRed.Contains(int.Parse(modulos[1])))
            return false;

        //Para cada jugador, añade la informacion a su pantalla de información

        foreach (Snake jugador in jugadores)
            jugador.RecibeInformacion(modulos[0]);


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
            jugador.RecibeInformacion(instruccion, false);

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
                if (gameObjects[i].transform.parent != null ||
                    gameObjects[i].name == "Manager" || gameObjects[i].name == "CamContainer" ||
                    gameObjects[i].name == "GameOverScreen" || gameObjects[i].name == "EventSystem"||
                    gameObjects[i].name == "GameManager")
                    continue;

                Destroy(gameObjects[i], 0);
            }

            //Mision completada y no han muerto todos
            if(soldadoElim == null && columnas.Count == 0 && ficheros.Count == 0 && jugadoresMuertos.Count < cantJugs)
            {
                gameOver.transform.GetChild(2).GetComponent<TMP_Text>().text = "MISSION COMPLETED!";

                //Le manda la info de la mision, si es mision de eliminacion, el soldado a eliminar no lo tiene en cuenta
                gameOver.GetComponent<GameOverScreen>().SetPoints(true, (int)tiempoMision, numAlertas, cantMuertes - (tipoMision == 0 ? 1 : 0), sinPistolas);

                //Desbloquea el modo big boss
                if(numAlertas == 0 && cantMuertes - (tipoMision == 0 ? 1 : 0) == 0 && PlayerPrefs.GetInt("NSnake", -1) == -1)
                {
                    PlayerPrefs.SetInt("NSnake", -1);
                }

            }
            //Mision sin completar
            else
            {
                gameOver.transform.GetChild(2).GetComponent<TMP_Text>().text = "MISSION FAILED";
                gameOver.GetComponent<GameOverScreen>().SetPoints(false);
            }

            //Hacer lista de puntuacion?
        }
    }

}
