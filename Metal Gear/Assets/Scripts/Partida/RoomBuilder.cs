using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class RoomBuilder : MonoBehaviour
{

    List<GameObject> salasAUsar = new List<GameObject>();
    List<GameObject> salasUsadas = new List<GameObject>();
    List<GameObject> salasRef= new List<GameObject>();
    List<GameObject> pasillos = new List<GameObject>();
    [Range(.5f, 10)]
    public float escala = 3;

    List<Vector3> puertasDispPos = new List<Vector3>();
    List<Vector3> puertasDispFor = new List<Vector3>();

    List<Vector3> puertaEncajePos = new List<Vector3>();
    List<Vector3> puertaEncajeFor = new List<Vector3>();


    List<Vector3> backPPos = new List<Vector3>();
    List<Vector3> backPFor = new List<Vector3>();
    List<Vector3> backEPos = new List<Vector3>();
    List<Vector3> backEFor = new List<Vector3>();
    GameObject encajeBack = null, salaBack = null;
    List<GameObject> salasUsadasBack = new List<GameObject>();
    List<GameObject> pasillosBack = new List<GameObject>();
    int indAdd = -1;


    int maxSalas = 0;


    int numSala = 0;
    int repOverlap = 0;


    bool espera = false, esperaAux = false, overlap = false;
    bool terminadoSalas = false;
    Vector3 spawnPoint = Vector3.zero;
    float tiempoJug = 0.5f;
    [SerializeField]GameObject camLoad;
    List<int> vestSoldados = new List<int>();

    int mision, auxMisionData;
    [SerializeField] Transform cubeAux;

    // Start is called before the first frame update
    void Start()
    {

        //PruebaWhile.Write("", true);

        salasAUsar.AddRange(Resources.LoadAll<GameObject>("Prefabs/Salas"));

        maxSalas = Random.Range(7, 11);
        

    }

    // Update is called once per frame
    void Update()
    {
        
        //Crea las habitaciones y comprueba si encajan
        if (numSala < maxSalas && !espera)
        {
            //print("step 0");
            if (terminadoSalas)
            {
                numSala = maxSalas;
                //print("terminado");
                return;
            }
            terminadoSalas = haTerminadoSala();
            espera = true;
        }

        else if(numSala == maxSalas + 1)
        {
            //print("step 2");
            this.GetComponent<NavMeshSurface>().BuildNavMesh();


            //----------MISIONES

            //auxMisionData = (Random.Range(0, 100) % 3) + 2; // Datos auxiliares (número de ficheros a borrar, numero de soldados que conocen informacion, numero de columnas)
            
            //print("Mision tipo " + mision.ToString());


            int cantSoldados = 0; //La cantidad de soldados con informacion debe de ser <= a la de informacion disponible
            List<int> informacionDisp = new List<int>();
            int[] noAdd = { };
            int valor = (Random.Range(0, 100) % 2) * 2 - 1; //Si es 1, cada soldado sabe algo; Si es -1, un soldado puede saber varias cosas
            Soldier[] soldadosEnPartida = FindObjectsOfType<Soldier>();


            foreach (Soldier soldado in soldadosEnPartida)
            {
                soldado.GetComponent<Soldier>().enabled = true;
                soldado.GetComponent<NavMeshAgent>().enabled = true;
                GeneraVestimenta(soldado.GetComponent<Soldier>());
            }

            if (mision == 0)
            {
                //Seleccionamos al soldado al que vamos a eliminar
                auxMisionData = Random.Range(0, soldadosEnPartida.Length);

                //Ponemos el soldado a eliminar en la lista de soldados a eliminar. Posible modo de eliminar varios soldados?
                GetComponent<GameManager>().soldadoElim = soldadosEnPartida[auxMisionData].gameObject;


                //Buscar la ruta del soldado a eliminar
                Vector3 posInicial = GetComponent<GameManager>().soldadoElim.GetComponent<Soldier>().posiciones[0];
                foreach (RutaSoldado ruta in FindObjectsOfType<RutaSoldado>())
                {
                    if (ruta.GetPosition() == posInicial)
                    {
                        //Hacer que game manager obtenga la habitación en la que patrulla
                        GetComponent<GameManager>().patrolZone = ruta.transform.parent.name;
                        break;
                    }

                }




                //Cuantos soldados tienen informacion
                cantSoldados = 5;
                valor = 1;
                informacionDisp = new List<int>() { 0, 1, 2, 3, 4 }; //0 - 3 ropa de cabeza a pies, 4 donde patrulla
                noAdd = Enumerable.Repeat(-1, cantSoldados + 1).ToArray<int>(); noAdd[cantSoldados] = auxMisionData;


            }
            else if (mision == 1)
            {
                int L = FindObjectsOfType<Fichero>().Length;
                auxMisionData = Mathf.Min(L, Random.Range(1, 4)); //Cantidad de ficheros a recoger
                int[] ficherosARecInd = Enumerable.Repeat(-1, auxMisionData).ToArray();

                for (int i = 0; i < auxMisionData; i++)
                {
                    int seleccion = Random.Range(0, L);
                    while (ficherosARecInd.Contains<int>(seleccion))
                    {
                        seleccion = (seleccion + 1) % L;
                    }

                    //Ya tienes el indice a seleccionar, lo ponemos en la array de indices a usar
                    ficherosARecInd[i] = seleccion;

                }

                List<GameObject> aDest = new List<GameObject>();
                for (int i = 0; i < L; i++)
                {
                    //Si es fichero a recoger, se pone en la lista que usa el game manager
                    if (ficherosARecInd.Contains<int>(i))
                    {
                        GetComponent<GameManager>().ficheros.Add(FindObjectsOfType<Fichero>()[i].gameObject);
                    }
                    //Si no, se borra
                    else
                    {
                        aDest.Add(FindObjectsOfType<Fichero>()[i].gameObject);
                    }
                }

                for (int i = 0; i < aDest.Count; i++)
                    Destroy(aDest[i], 0);

                //Hacer que los guardias sepan donde están los ficheros
                cantSoldados = auxMisionData;
                informacionDisp = Enumerable.Range(0, auxMisionData).ToList();


            }
            else if (mision == 2)
            {

                //Eliminar columnas que no haya que destruir


                int L = FindObjectsOfType<Columna>().Length;
                auxMisionData = Mathf.Min(L, Random.Range(2, 5)); //Cantidad de ficheros a recoger
                int[] columsARecInd = Enumerable.Repeat(-1, auxMisionData).ToArray();
                //Peta en este while ¿Por que?

                //Creo que es porque L es menor que auxMissionData
                for (int i = 0; i < auxMisionData; i++)
                {
                    int seleccion = Random.Range(0, L);

                    while (columsARecInd.Contains(seleccion))
                    {
                        seleccion = (seleccion + 1) % L;
                    }

                    //Ya tienes el indice a seleccionar, lo ponemos en la array de indices a usar
                    columsARecInd[i] = seleccion;

                }
                List<GameObject> aDest = new List<GameObject>();


                for (int i = 0; i < L; i++)
                {
                    //Si es fichero a recoger, se pone en la lista que usa el game manager
                    if (columsARecInd.Contains(i))
                    {
                        GetComponent<GameManager>().columnas.Add(FindObjectsOfType<Columna>()[i].gameObject);
                    }
                    //Si no, se borra
                    else
                    {
                        aDest.Add(FindObjectsOfType<Columna>()[i].gameObject);
                    }
                }

                for (int i = 0; i < aDest.Count; i++)
                {
                    Destroy(aDest[i], 0);
                }


                //Si es mision de destrucción, que snake reciba el número de columnas de C4
                FindObjectOfType<Snake>().RecibObjeto(Resources.Load<Objeto>("Armas/C4"));
                FindObjectOfType<Snake>().RecibeBalas(Resources.Load<Objeto>("Armas/C4"), auxMisionData - 5);

                cantSoldados = auxMisionData;
                informacionDisp = Enumerable.Range(0, auxMisionData).ToList();
            }

            //Para que funcione el sistema de información de soldados
            if (noAdd.Length == 0)
                noAdd = Enumerable.Repeat(-1, cantSoldados).ToArray<int>();

            for (int i = 0; i < cantSoldados; i++)
            {

                int randomSelect = Random.Range(0, soldadosEnPartida.Length);
                int iter = 0;
                //print(randomSelect);
                while (valor > 0 && noAdd.Contains(randomSelect))
                {
                    //print("atrapado");
                    randomSelect = (randomSelect + 1) % soldadosEnPartida.Length;
                    iter += 1;
                    if (iter >= 15)
                        break;
                }
                noAdd[i] = randomSelect;


                int nuevSel = Random.Range(0, informacionDisp.Count);

                soldadosEnPartida[randomSelect].AddInfo(informacionDisp[nuevSel], mision);

                informacionDisp.RemoveAt(nuevSel);
            }

            //--------------------Activa lo que queda



            this.GetComponent<GameManager>().ActivaVistas();


            foreach (Puerta p in FindObjectsOfType<Puerta>())
                p.puerta_().SetActive(true);


            Destroy(camLoad, 0);

            GetComponent<SettingsManager>().LoadAll();
            GetComponent<GameManager>().IndicaMision(mision, auxMisionData);

            Destroy(this,0);
        }

        else if(numSala == maxSalas)
        {
            //print("step 1");
            //Si el mapa no vale (no hay suficientes salas y/o no hay posibles salidas, se resetea y vovlemos a empezar)
            //Habria que permitir salidas puntuales como el agua en el puerto o el ascensor del helipuerto

            if (salasUsadas.Count < 7 || (puertaEncajeFor.Count == 0 && puertasDispFor.Count == 0))
            {
                ResetSalas();
                return;
            }


            int spawnInd = Random.Range(0, salasUsadas.Count);
            try
            {
                spawnPoint = GameObject.Find(salasUsadas[spawnInd].name).transform.Find("SpawnPoint").position;// + Vector3.up;
            }
            catch
            {
                print(salasUsadas[spawnInd].name);
                Debug.Break();
            }

            //Crea el tipo de mision
            mision = Random.Range(0, 100) % 3; //0 => ELIMINACION || 1 => ARCHIVO || 2 => DESTRUCCION

            //Objetos a destruir
            List<GameObject> aDest = new List<GameObject>();

            //Ajusta y pone estáticas las salas
            foreach (GameObject sala in salasRef)
            {
                sala.isStatic = true;


                Destroy(sala.GetComponent<DibujosSala>(), 0);

                Destroy(sala.GetComponent<Sala>(), 0);
                List<RutaSoldado> rutasEnSala = new List<RutaSoldado>();
                aDest = new List<GameObject>();

                for (int i = 0; i < sala.transform.childCount; i++)
                {

                    if (sala.transform.GetChild(i).name == "TriggerOverlap")
                    {
                        aDest.Add(sala.transform.GetChild(i).gameObject);
                        continue;
                    }
                    else if (sala.transform.GetChild(i).GetComponent<RutaSoldado>())
                    {

                        //Si es solo una ruta, se puede borrar
                        if (sala.transform.GetChild(i).name.Contains("RutaSoldado"))
                        {

                            //aDest.Add(sala.transform.GetChild(i).gameObject);
                            rutasEnSala.Add(sala.transform.GetChild(i).GetComponent<RutaSoldado>());
                        }
                        //Si no, se elimina la componente del objeto
                        else if (!pasillos.Contains(sala.transform.GetChild(i).gameObject) && Random.Range(0, 101) > 25)
                        {
                            //75 % de que los pasillos tengan soldados
                            //Si el pasillo se va a borrar, que no se añada
                            rutasEnSala.Add(sala.transform.GetChild(i).GetComponent<RutaSoldado>());

                            //Destroy(sala.transform.GetChild(i).GetComponent<RutaSoldado>(), 0);

                        }

                        continue;

                    }
                    else if ((sala.transform.GetChild(i).GetComponent<Fichero>() && mision != 1) ||
                        (sala.transform.GetChild(i).GetComponent<Columna>() && mision != 2))
                    {
                        aDest.Add(sala.transform.GetChild(i).gameObject);
                    }

                    if (pasillos.Contains(sala.transform.GetChild(i).gameObject))
                        continue;



                    sala.transform.GetChild(i).gameObject.isStatic = true;
                }


                if (rutasEnSala.Count > 0)
                {
                    //Quitamos rutas aleatorias para variar los resultados
                    int cantRem = Random.Range(0, Mathf.Max(1, (int)rutasEnSala.Count / 2) + 1);
                    int cuenta = rutasEnSala.Count;
                    List<RutaSoldado> destruir = new List<RutaSoldado>();
                    for (int rem = 0; rem < cantRem; rem++)
                    {
                        int ind = Random.Range(0, cuenta);
                        //print(ind + sala.name);
                        destruir.Add(rutasEnSala[ind]);
                        rutasEnSala.RemoveAt(ind);
                        cuenta -= 1;
                    }
                    for (int rem = 0; rem < destruir.Count; rem++)
                        Destroy(destruir[rem], 0);
                }
                //print(cantRem);


                //Debug.Break();
                //Generamos soldados por cada ruta en la sala

                //Por si quiero que no aparezcan soldados
                //rutasEnSala = new List<RutaSoldado>();

                foreach(RutaSoldado ruta in rutasEnSala)
                {
                    GameObject soldado = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/NPCs/Soldier"));
                    //soldado.GetComponent<NavMeshAgent>().enabled = false;
                    //QuitaCloneNombre(soldado);
                    for(int i = 0; i < ruta.posiciones.Count; i++)
                    {
                        soldado.GetComponent<Soldier>().posiciones.Add(ruta.GetPosition(i));
                        soldado.GetComponent<Soldier>().tiempoEspera.Add(ruta.tiempos[i]);
                        soldado.GetComponent<Soldier>().rotacionesY.Add(ruta.GetAngulo(i));
                    }
                    soldado.transform.position = ruta.GetPosition(0);
                    soldado.GetComponent<Soldier>().enabled = false;
                }


                //Destruimos de manera segura los objetos no necesarios
                for(int i = 0; i < aDest.Count; i++)
                {
                    Destroy(aDest[i], 0);
                    aDest[i] = null;
                }

            }


            //Hacer salidas

            //Si no quedan encajes disponibles, lo cambia por un pasillo sin usar
            if(puertaEncajePos.Count == 0)
            {
                //Escogemos el pasillo que va a ser salida
                GameObject pasASal = pasillos[Random.Range(0, pasillos.Count)];
                pasillos.Remove(pasASal);


                //Saca el tipo de salida que es (A, B, C o D)
                string tipo = "A";
                if (pasASal.name.Contains("B"))
                    tipo = "B";
                else if (pasASal.name.Contains("C"))
                    tipo = "C";
                else if (pasASal.name.Contains("D"))
                    tipo = "D";

                //Creamos la salida según el tipo del pasillo y ponemos igual los tres transforms
                GameObject salida = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Pasillos/Salida" + tipo));
                salida.transform.position = pasASal.transform.position;
                salida.transform.rotation = pasASal.transform.rotation;
                salida.transform.localScale = escala * new Vector3(1, 1, 1);

                //No hace falta hacer el trigger de salida :D


                //Borramos el pasillo porque ya no sirve de nada
                Destroy(pasASal, 0);


            }
            //Si queda algún encaje, pone la salida de tipo A en el encaje
            else
            {
                //Vemos donde poner la salida
                int indiceDeSalida = Random.Range(0, puertaEncajePos.Count);

                //Creamos la salida
                GameObject salida = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Pasillos/SalidaA"));
                salida.transform.position = puertaEncajePos[indiceDeSalida];
                salida.transform.right = puertaEncajeFor[indiceDeSalida];
                salida.transform.localScale = escala * new Vector3(1, 1, 1);



                Transform puertaBloqueo = salida.transform.GetChild(0);
                Vector3 offset = (salida.transform.right * puertaBloqueo.localPosition.x) + (salida.transform.forward * puertaBloqueo.localPosition.z) + (salida.transform.up * puertaBloqueo.localPosition.y);
                //Hacer en cada axis

                offset += (puertaBloqueo.up * -0.24779f) + (puertaBloqueo.right * 0.147558f) + (puertaBloqueo.forward * 0.020022f);

                salida.transform.position -= (escala * offset);

                puertaEncajeFor.RemoveAt(indiceDeSalida);
                puertaEncajePos.RemoveAt(indiceDeSalida);

            }


            //Borra los pasillos sin usar
            {
                aDest = new List<GameObject>();

                foreach (GameObject pasillo in pasillos)
                    aDest.Add(pasillo);

                pasillos = new List<GameObject>(); pasillosBack = new List<GameObject>();
                for (int k = 0; k < aDest.Count; k++)
                {
                    //print(aDest[k].name);

                    //Tomamos la primera puerta del pasillo, que es la que conecta con la habitacion
                    //Y la reemplazamos con una puerta bloqueada del tipo del pasillo

                    if (!aDest[k].name.Contains("D"))
                    {
                        //Creamos la puerta bloqueada
                        Transform puertaBloqueo = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Pasillos/PuertaBlock")).transform;
                        //Pillamos cual es la primera puerta
                        Transform puertaSust = aDest[k].transform.GetChild(0);
                        //La ponemos en su lugar
                        puertaBloqueo.position = puertaSust.position;
                        puertaBloqueo.rotation = puertaSust.rotation;
                        puertaBloqueo.localScale = puertaSust.localScale * escala;

                    }
                    else
                    {
                        //Poner puerta que tape el pasillo calculando la posicion
                        Transform puertaBloqueo = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Pasillos/PortonBlock")).transform;


                        //La ponemos en su lugar
                        puertaBloqueo.position = aDest[k].transform.position;
                        puertaBloqueo.rotation = aDest[k].transform.rotation;
                        puertaBloqueo.localScale = aDest[k].transform.localScale * escala;


                    }

                    Destroy(aDest[k], 0);
                }

                //Añadimos bloqueos a los encajes que quedan sin bloquear
                for(int i = 0; i < puertaEncajePos.Count; i++)
                {
                    Transform puertaBloqueo = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Pasillos/PuertaBlock")).transform;
                    puertaBloqueo.position = puertaEncajePos[i];// - (escala * new Vector3(0.147558f, -0.019718f, 0.24779f));// Hay que sumarle el offset de la puerta
                    puertaBloqueo.forward = puertaEncajeFor[i];

                    Vector3 offset = (puertaBloqueo.up * -0.24779f) + (puertaBloqueo.right * 0.147558f) + (puertaBloqueo.forward* -0.019718f);
                    puertaBloqueo.position -= (escala * offset);

                    puertaBloqueo.localScale = escala * new Vector3(1, 1, 1);
                }
                aDest = new List<GameObject>();

            }


            //Quita las puertas para que pueda hacer bien el navMesh
            foreach (Puerta p in FindObjectsOfType<Puerta>())
                p.puerta_().SetActive(false);

            Physics.SyncTransforms();

            //Creamos el Game Manager 
            gameObject.AddComponent<GameManager>();

            //Hacemos que los armory crates seleccionen su objeto, cantidad y se añadan a la lista de crates
            foreach (ArmoryCrate crate in FindObjectsOfType<ArmoryCrate>())
            {
                crate.Inicia();
            }

            //Crear a Snake
            {

                GameObject auxSnake = Instantiate(Resources.Load<GameObject>("Prefabs/Player/SnakeAux"));
                GameObject snake = auxSnake.transform.GetChild(0).gameObject;
                snake.GetComponent<CharacterController>().enabled = false;

                GameObject camRadar = auxSnake.transform.GetChild(1).gameObject;
                GameObject camContainer = auxSnake.transform.GetChild(2).gameObject;
                GameObject UI = auxSnake.transform.GetChild(3).gameObject;
                snake.transform.parent = null;
                camRadar.transform.parent = null;
                camContainer.transform.parent = null;
                UI.transform.SetParent(null);
                
                snake.transform.position = spawnPoint;
                cubeAux.transform.position = spawnPoint;
                
                Destroy(auxSnake, 0);
                snake.GetComponent<CharacterController>().enabled = true;

            }
            numSala = maxSalas + 1;

            //Debug.Break();

        }
    }

    private void FixedUpdate()
    {

        //Comprobacion de si encajan con un frame de físicas de margen
        if (espera)
        {
            if (esperaAux )
            {
                espera = false;
                if (overlap)
                {
                    repOverlap += 1;
                    DestroyImmediate(salaBack);
                    if (encajeBack != null)
                        DestroyImmediate(encajeBack);

                    puertasDispPos = new List<Vector3>(); puertasDispPos.AddRange(backPPos);
                    puertasDispFor = new List<Vector3>(); puertasDispFor.AddRange(backPFor);
                    puertaEncajePos = new List<Vector3>(); puertaEncajePos.AddRange(backEPos);
                    puertaEncajeFor = new List<Vector3>(); puertaEncajeFor.AddRange(backEFor);
                    salasUsadas = new List<GameObject>(); salasUsadas.AddRange(salasUsadasBack);
                    pasillos = new List<GameObject>(); pasillos.AddRange(pasillosBack);
                    if(repOverlap > 10)
                    {
                        terminadoSalas = true;
                        return;
                    }
                    terminadoSalas = false;

                }
                else
                {
                    repOverlap = 0;
                    salasRef.Add(salaBack);
                    salasUsadas.Add(salasAUsar[indAdd]);
                    if (encajeBack != null)
                        salasRef.Add(encajeBack);
                    numSala += 1;
                }
            }
            else
            {
                esperaAux = true;
            }
        }
        if(numSala == maxSalas +1 && tiempoJug > 0)
        {
            tiempoJug -= Time.fixedDeltaTime;
            if(tiempoJug < 0)
            {
                tiempoJug = 0;
            }
        }
    }

    private void LateUpdate()
    {
    }

    public void CreaSala()
    {
        if (salasUsadas.Count >= maxSalas)
            return;
        Sala sala = null;


        //Primero guarda lo que lleva por si lo que va a hacer al final no encaja
        {
            backPPos = new List<Vector3>();
            backPFor = new List<Vector3>();
            backEPos = new List<Vector3>();
            backEFor = new List<Vector3>();
            encajeBack = null;
            salaBack = null;

            indAdd = -1;
            salasUsadasBack = new List<GameObject>();
            pasillosBack = new List<GameObject>();

            backPPos.AddRange(puertasDispPos); 
            backPFor.AddRange(puertasDispFor);
            backEPos.AddRange(puertaEncajePos);
            backEFor.AddRange(puertaEncajeFor);
            salasUsadasBack.AddRange(salasUsadas);
            pasillosBack.AddRange(pasillos);
        }


        //Encuentra una sala que tenga mas de una puerta si no hay muchas ya (si ya hay varias entonces da igual)
        //Que no esté usada
        //Y que tenga puertas (hay que quitar a futuro esa condicion, es para testear sin haber acabado las otras salas)
        int i = -1; int iterRand= 0;

        while (sala == null)
        {
            //Si no puede por metodo random, haz un apaño
            if (iterRand > 10)
            {
                i = Random.Range(0, salasAUsar.Count);
                while (salasUsadas.Contains(salasAUsar[i]))
                {
                    i = (i + 29) % salasAUsar.Count;
                }
                sala = salasAUsar[i].GetComponent<Sala>();
                break;
            }
            //Por algun motivo da fallo a veces con un NullReferenceException con maxSalas. try y arregaldo pero no se pq es
            try
            {
                i = Random.Range(0, salasAUsar.Count);
                //bool condA = (salasAUsar[i].GetComponent<Sala>().puertasPosiciones.Length < 2 != salasUsadas.Count < maxSalas - 2);

                bool condA = (salasAUsar[i].GetComponent<Sala>().puertasPosiciones.Length == 1) !=
                    (puertaEncajePos.Count == 1);


                bool condB = !salasUsadas.Contains(salasAUsar[i]);
                if (condA && condB)
                {
                    sala = salasAUsar[i].GetComponent<Sala>();
                }
            }
            catch
            {
            }
            //Debug.Break();
            iterRand += 1;
        }



        GameObject nuevaSala = GameObject.Instantiate(sala.gameObject);
        nuevaSala.name = sala.gameObject.name;
        nuevaSala.transform.position = Vector3.zero;
        nuevaSala.transform.localScale *= escala;
        salaBack = nuevaSala;


        //Ya hay mas salas, hay que acoplarla a alguna otra
        if (puertasDispFor.Count > 0 || puertaEncajePos.Count > 0)
        {

            int indPuertaNueva = Random.Range(0, sala.puertasPosiciones.Length);


            //Si se tiene que anclar al encaje, se ancla y ya
            if (puertaEncajePos.Count > 0)
            {
                int indPuertaAnclaje = Random.Range(0, puertaEncajePos.Count);
                //Calcula la rotacion a aplicar para acoplarse
                Quaternion rot = Quaternion.FromToRotation(sala.puertasFuera[indPuertaNueva], -puertaEncajeFor[indPuertaAnclaje]);
                rot = Quaternion.Euler(0,rot.eulerAngles.y,0);

                nuevaSala.transform.rotation = rot;
                //Se acopla
                Vector3 posAnc = calculaAnclaje(nuevaSala.transform, sala.puertasPosiciones[indPuertaNueva]);
                Vector3 movimiento = puertaEncajePos[indPuertaAnclaje] - posAnc;
                nuevaSala.transform.position += movimiento;

                //Quita las puertas usadas y añade las sin usar
                for (int k = 0; k < sala.puertasPosiciones.Length; k++)
                {
                    if (k == indPuertaNueva)
                        continue;
                    Vector3 posPuertaAdd = calculaAnclaje(nuevaSala.transform, sala.puertasPosiciones[k]);
                    Vector3 forPuertaAdd = rot * sala.puertasFuera[k];
                    puertasDispPos.Add(posPuertaAdd);
                    puertasDispFor.Add(forPuertaAdd);
                    pasillos.Add(nuevaSala.GetComponent<Sala>().pasillos[k].gameObject);


                }
                puertaEncajeFor.RemoveAt(indPuertaAnclaje);
                puertaEncajePos.RemoveAt(indPuertaAnclaje);
            }

            //Si se tiene que anclar a una puerta, primero añade un encaje y se ancla al encaje
            else
            {
                int indPuertaAnclaje = Random.Range(0, puertasDispPos.Count);


                //Creamos el encaje

                int numEncaje = Random.Range(1, 5);

                //Hay que hacer que haya mas puertas que las justas y que si hay dos habitaciones que se solapen, el encaje tiene que ser de tipo largo

                // num = 1 ó 2 -> 2 puertas
                // num = 3 -> 3 puertas
                // num = 4 -> 4 puertas
                if (sala.puertasPosiciones.Length == 1 && puertasDispPos.Count == 1 && numEncaje < 3)
                    numEncaje = Random.Range(3, 5);





                GameObject encaje = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/Encajes/Encaje" + numEncaje.ToString()));
                encajeBack = encaje;



                //Le aplicamos la escala
                encaje.transform.localScale *= escala;

                // estos son los índices de las puertas a acoplar a la habitación ya creada y a la habitación que ese está creando respectivamente
                int acopleAHecho = Random.Range(0, encaje.GetComponent<Sala>().puertasPosiciones.Length);
                int acopleANueva = (acopleAHecho + 1) % encaje.GetComponent<Sala>().puertasPosiciones.Length;


                //Acoplamos primero el encaje a la habitación
                Quaternion rotEncaje = Quaternion.FromToRotation(encaje.GetComponent<Sala>().puertasFuera[acopleAHecho], -puertasDispFor[indPuertaAnclaje]);
                rotEncaje = Quaternion.Euler(0, rotEncaje.eulerAngles.y, 0);

                encaje.transform.rotation = rotEncaje;
                Vector3 posAHecho = calculaAnclaje(encaje.transform, encaje.GetComponent<Sala>().puertasPosiciones[acopleAHecho]);
                Vector3 movimiento = puertasDispPos[indPuertaAnclaje] - posAHecho; encaje.transform.position += movimiento;


                //Acoplamos ahora la habitación nueva al propio encaje
                Vector3 encajeAcople = rotEncaje * encaje.GetComponent<Sala>().puertasFuera[acopleANueva];
                Quaternion rotSala = Quaternion.FromToRotation(sala.puertasFuera[indPuertaNueva], -encajeAcople);
                rotSala = Quaternion.Euler(0, rotSala.eulerAngles.y, 0);

                nuevaSala.transform.rotation = rotSala;

                Vector3 posInicioP = calculaAnclaje(nuevaSala.transform, sala.puertasPosiciones[indPuertaNueva]);
                Vector3 posFinalP = calculaAnclaje(encaje.transform, encaje.GetComponent<Sala>().puertasPosiciones[acopleANueva]);
                movimiento = posFinalP - posInicioP;


                nuevaSala.transform.position += movimiento;






                //Por último, añadimos las puertas sin usar de la habitación y del encaje y quitamos la puerta usada por la habitación anterior

                //Añadimos las puertas de la nueva sala creada
                for(int k = 0; k < sala.puertasPosiciones.Length; k++)
                {
                    if (k == indPuertaNueva)
                        continue;
                    Vector3 posPuertaAdd = calculaAnclaje(nuevaSala.transform, sala.puertasPosiciones[k]);
                    Vector3 forPuertaAdd = rotSala * sala.puertasFuera[k];
                    puertasDispPos.Add(posPuertaAdd);
                    puertasDispFor.Add(forPuertaAdd);
                    pasillos.Add(nuevaSala.GetComponent<Sala>().pasillos[k].gameObject);

                }

                //Añadimos las puertas del nuevo encaje
                for(int k = 0; k < encaje.GetComponent<Sala>().puertasPosiciones.Length; k++)
                {
                    if(k == acopleAHecho || k == acopleANueva)
                    {
                        continue;
                    }

                    Vector3 posPuertaAdd = calculaAnclaje(encaje.transform, encaje.GetComponent<Sala>().puertasPosiciones[k]);
                    Vector3 forPuertaAdd = rotEncaje * encaje.GetComponent<Sala>().puertasFuera[k];
                    puertaEncajePos.Add(posPuertaAdd);
                    puertaEncajeFor.Add(forPuertaAdd);
                }
                //print(puertaEncajePos.Count);

                //Quitamos la puerta usada de la habitacion ya creada
                puertasDispFor.RemoveAt(indPuertaAnclaje);
                puertasDispPos.RemoveAt(indPuertaAnclaje);
                pasillos.RemoveAt(indPuertaAnclaje);


            }


        }
        //Es la primera sala en crearse
        else
        {
            for (int k = 0; k < sala.puertasPosiciones.Length; k++)
            {
                Vector3 posNueva = calculaAnclaje(nuevaSala.transform, sala.puertasPosiciones[k]);
                puertasDispFor.Add(sala.puertasFuera[k]);
                puertasDispPos.Add(posNueva);
                pasillos.Add(nuevaSala.GetComponent<Sala>().pasillos[k].gameObject);
            }



        }

        indAdd = i;


    }

    public bool haTerminadoSala()
    {
        CreaSala();
        esperaAux = false;
        overlap = false;
        return puertasDispPos.Count == 0 && puertaEncajePos.Count == 0;
    }


    Vector3 calculaAnclaje(Transform tr, Vector3 inicio)
    {

        Vector3 direccionReal = Vector3.Scale(tr.localScale, inicio);

        direccionReal = (tr.forward * direccionReal.z) + (tr.right * direccionReal.x) + (tr.up * direccionReal.y);
        
        return tr.position + direccionReal;

    }

    public void overlap_()
    {
        //print("Se solapan habitaciones");
        overlap = true;
    }
    private void ResetSalas()
    {
        foreach(GameObject g in salasRef)
        {
            Destroy(g, 0);
        }
        salasUsadas = new List<GameObject>();
        salasRef = new List<GameObject>();

        puertasDispPos = new List<Vector3>();
        puertasDispFor = new List<Vector3>();

        puertaEncajePos = new List<Vector3>();
        puertaEncajeFor = new List<Vector3>();
        pasillos = new List<GameObject>();

        numSala = 0;

        terminadoSalas = false;

    }


    void GeneraVestimenta(Soldier soldado)
    {

        /*
          Hay 4 vestimentas con 6 posibilidades

        1 - Nada | -
        2 - Rojo | R
        3 - Verde | G
        4 - Amarillo | Y
        5 - Azul | B
        6 - Marrón | M
        

        Entonces hay 6 * 6 * 6 * 6 combinaciones de ropa
        
        Por optimización guardamos los indices de las 1296 combinaciones y sacamos que tipo de vestimenta es a partir del índice

        Suponemos que se ordenan así

        1) - - - - - -
        2) - - - - - R
        3) - - - - - A
        ....

        Es un número en base 6


        Solo podemos obtener índices que no estén en la lista de previamente usados


        */


        //El índice de la vestimenta a crear

        int index = Random.Range(0,1296);
        while (vestSoldados.Contains(index))
        {
            Debug.Log("while roñoso");
            index += 29;
            index = index % 1296;
        }

        vestSoldados.Add(index);


        //Sacamos la vestimenta del índice
        string[] vest = new string[4];

        string[] posVest = new string[6] { "-", "R", "G", "Y", "B", "M" };

        for(int i = 0; i < 4; i++)
        {
            int termino_i = index % 6;
            vest[i] = posVest[termino_i];
            index = index / 6;

        }

        string vestHash = vest[0] + vest[1] + vest[2] + vest[3];


        soldado.Vestirse(vestHash);



    }

    bool CustomContains(int[] arr, int value)
    {
        foreach (int valor in arr)
        {
            if (valor == value)
                return true;
        }
        return false;
    }
}

