using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Globalization;


public class VistaSoldado : MonoBehaviour
{

    public List<Collider> collidersALaVista;

    List<Transform> jugadoresPerseguidos;

    [SerializeField]Soldier soldado;
    [SerializeField] List<Snake> jugadoresAsumidos;

    float distPerder = 4f;

// Start is called before the first frame update
    void Start()
    {
        jugadoresPerseguidos = new List<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        


    }

    private void LateUpdate()
    {
        List<int> indicesRem = new List<int>();
        for(int i = 0; i < collidersALaVista.Count; i++)
        {
            if (collidersALaVista[i] == null || !collidersALaVista[i].gameObject.activeInHierarchy)
            {
                indicesRem.Add(i);
                continue;

            }

        }

        //Solo ve su entorno si piensa
        if (soldado.EstaPensando())
        {
            for (int i = 0; i < collidersALaVista.Count; i++)
            {
                if (collidersALaVista[i].tag.Contains("JugCol"))
                {
                    //Checkear si es jugador y dar alerta a Soldier.cs
                    Transform jugador = collidersALaVista[i].transform.parent.parent;
                    Snake snakeJ = jugador.GetComponent<Snake>();
                    bool caja = snakeJ.caja;
                    bool cajaCamuf = !snakeJ.enMovimiento();
                    bool hold = snakeJ.Holding();
                    Vector3 lookDir = snakeJ.Rig.forward;

                    //Vector3 jugPos = jugador.GetComponent<CharacterController>().center + jugador.position;
                    Vector3 jugPos = snakeJ.ActualPos();
                    Vector3 cabezaPos = snakeJ.cabeza.transform.position;
                    Vector3 piesPos = jugPos + 2 * (jugPos - cabezaPos);

                    RaycastHit hitPlayer;
                    LayerMask layer = (1 << LayerMask.NameToLayer("EscenarioColliders")) | (1 << LayerMask.NameToLayer("ColliderDisparo"));
                    bool detectado = false;
                    if (Physics.Raycast(transform.position, cabezaPos - transform.position, out hitPlayer, Mathf.Infinity, layer))
                    {
                        //Alerta
                        if (EsColDeSnake(hitPlayer.collider, collidersALaVista[i].gameObject))
                        {
                            Alerta(jugador, hold, lookDir, caja, cajaCamuf, snakeJ.playerID);
                            //print("visto");
                            detectado = true;
                        }
                        Debug.DrawLine(transform.position, hitPlayer.point, Color.white);
                    }
                    if (!detectado && Physics.Raycast(transform.position, jugPos - transform.position, out hitPlayer, Mathf.Infinity, layer))
                    {
                        //Alerta
                        if (EsColDeSnake(hitPlayer.collider, collidersALaVista[i].gameObject))
                        {
                            Alerta(jugador, hold, lookDir, caja, cajaCamuf, snakeJ.playerID);
                            //print("visto");
                            detectado = true;
                        }
                        Debug.DrawLine(transform.position, hitPlayer.point, Color.white);
                    }
                    if (!detectado && Physics.Raycast(transform.position, piesPos - transform.position, out hitPlayer, Mathf.Infinity, layer))
                    {
                        //Alerta
                        if (EsColDeSnake(hitPlayer.collider, collidersALaVista[i].gameObject))
                        {
                            Alerta(jugador, hold, lookDir, caja, cajaCamuf, snakeJ.playerID);
                            //print("visto");
                        }
                        Debug.DrawLine(transform.position, hitPlayer.point, Color.white);
                    }


                }

                else if (collidersALaVista[i].tag == "SoldCol")
                {
                    // hacer que cuando deje de verlo se olvide que está ahí
                    if (collidersALaVista[i].name != "InteraccionSoldadoInc" && collidersALaVista[i].name != "InteraccionSoldadoRehen")
                    {
                        indicesRem.Add(i);
                        continue;
                    }


                    //Ver a soldado inconsciente
                    Soldier soldInc = collidersALaVista[i].transform.parent.parent.GetComponent<Soldier>();
                    Vector3 soldPos = soldInc.Rig.transform.position;
                    Vector3 cabezaPos = soldInc.Rig.transform.position + (soldInc.hips.up * 0.35f);
                    Vector3 piesPos = soldInc.Rig.transform.position + (soldInc.LLeg.up * 0.35f);

                    Debug.DrawRay(transform.position, cabezaPos - transform.position, Color.white);
                    Debug.DrawRay(transform.position, soldPos - transform.position, Color.white);
                    Debug.DrawRay(transform.position, piesPos - transform.position, Color.white);

                    RaycastHit hitSoldier;
                    bool detectado = false;
                    LayerMask layer = (1 << LayerMask.NameToLayer("EscenarioColliders")) | (1 << LayerMask.NameToLayer("ColliderDisparo"));
                    if (Physics.Raycast(transform.position, cabezaPos - transform.position, out hitSoldier, Mathf.Infinity, layer))
                    {
                        //Ve a soldado inconsciente
                        //print(hitSoldier.collider.name);
                        if (EsColDeSoldado(hitSoldier.collider, collidersALaVista[i].gameObject))
                        {
                            if (collidersALaVista[i].name == "InteraccionSoldadoInc")
                            {
                                CompInc(soldInc);
                            }
                            else if (collidersALaVista[i].name == "InteraccionSoldadoRehen")
                            {
                                CheckZone(soldPos);
                            }
                            detectado = true;
                        }
                    }
                    if (!detectado && Physics.Raycast(transform.position, soldPos - transform.position, out hitSoldier, Mathf.Infinity, layer))
                    {
                        //Ve a soldado inconsciente
                        //print(hitSoldier.collider.name);

                        if (EsColDeSoldado(hitSoldier.collider, collidersALaVista[i].gameObject))
                        {
                            if (collidersALaVista[i].name == "InteraccionSoldadoInc")
                            {
                                CompInc(soldInc);
                            }
                            else if (collidersALaVista[i].name == "InteraccionSoldadoRehen")
                            {
                                CheckZone(soldPos);
                            }
                            detectado = true;
                        }

                    }
                    if (!detectado && Physics.Raycast(transform.position, piesPos - transform.position, out hitSoldier, Mathf.Infinity, layer))
                    {
                        //Ve a soldado inconsciente
                        //print(hitSoldier.collider.name);
                        if (EsColDeSoldado(hitSoldier.collider, collidersALaVista[i].gameObject))
                        {
                            if (collidersALaVista[0].name == "InteraccionSoldadoInc")
                            {
                                CompInc(soldInc);
                            }
                            else if (collidersALaVista[0].name == "InteraccionSoldadoRehen")
                            {
                                CheckZone(soldPos);
                            }
                        }
                    }
                }

                else if (collidersALaVista[i].tag == "Objeto" && collidersALaVista[i].gameObject.layer == LayerMask.NameToLayer("Explosivo"))
                {
                    RaycastHit rayo; LayerMask layer = (1 << LayerMask.NameToLayer("EscenarioColliders")) | (1 << LayerMask.NameToLayer("Explosivo"));
                    if (Physics.Raycast(transform.position, collidersALaVista[i].transform.position - transform.position, out rayo, Mathf.Infinity, layer, QueryTriggerInteraction.Ignore))
                    {
                        if (rayo.collider == collidersALaVista[i])
                            Granada(rayo.collider.transform);
                    }
                }
            }
        }


        for (int j = 0; j < indicesRem.Count; j++)
        {
            try
            {
                collidersALaVista.RemoveAt(indicesRem[j]);
            }
            catch
            {
                //Hacer el remove de null;
            }
        }

        indicesRem = new List<int>();

        for (int i = 0; i < jugadoresAsumidos.Count; i++)
        {
            if (jugadoresPerseguidos.Contains(jugadoresAsumidos[i].transform))
                continue;
            Snake snakeJ = jugadoresAsumidos[i];
            bool caja = snakeJ.caja;
            bool cajaCamuf = snakeJ.caja && !snakeJ.enMovimiento();
            bool hold = snakeJ.Holding();
            Vector3 lookDir = snakeJ.Rig.forward;
            Alerta(snakeJ.transform, hold, lookDir, caja, cajaCamuf, snakeJ.playerID, false);
            print("asumo");
            if (Vector3.Distance(snakeJ.transform.position, transform.position) > distPerder)
            {
                indicesRem.Add(i);
            }
        }

        for (int j = 0; j < indicesRem.Count; j++)
        {
            try
            {
                jugadoresPerseguidos.Remove(jugadoresAsumidos[indicesRem[j]].transform);
                jugadoresAsumidos.RemoveAt(indicesRem[j]);
            }
            catch
            {
                //Hacer el remove de null;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        //Añadir soldados inconscientes
        if (other.tag.Contains("JugCol") && !collidersALaVista.Contains(other))
        {
            collidersALaVista.Add(other);
        }
        else if (other.name.Contains("InteraccionSoldado") && other.tag == "SoldCol" && !collidersALaVista.Contains(other))
        {
            collidersALaVista.Add(other);
        }
        else if (other.tag == "Objeto" && other.gameObject.layer == LayerMask.NameToLayer("Explosivo") && other.GetType() != typeof(SphereCollider))
            collidersALaVista.Add(other);



    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Contains("JugCol") && collidersALaVista.Contains(other))
        {
            collidersALaVista.Remove(other);
            PierdeJugador(other);
        }

        // hacer que cuando deje de verlo se olvide que está ahí
        else if ((other.name.Contains("InteraccionSoldado") && other.tag == "SoldCol" && collidersALaVista.Contains(other)))
        {
            collidersALaVista.Remove(other);
        }
    }

    void PierdeJugador(Collider other)
    {
        //HAY QUE VER PRIMERO SI LO ESTABA SIGUIENDO
        if (jugadoresPerseguidos.Count == 0)
            return;

        //Primero que se levante si esta agachado
        soldado.Agachate(false);
        //Luego, vemos la distancia a la que esta el collider que ha salido del cono de vision es menor a cierto valor
        float distancia_jug = Vector3.Distance(transform.position, other.transform.position);
        //Si se ha salido muy lejos, entonces le pierde la pista
        if (distancia_jug > distPerder)
        {
            return;
        }
        //Si no, lo mete a una lista de asumidos
        else
        {
            //Aqui guarda al collider, y los valores de hold, normalHold, caja, cajaCamuf, index
            Snake jug = other.transform.parent.parent.GetComponent<Snake>();
            if(!jugadoresPerseguidos.Contains(jug.transform))
                return;
            if (!jugadoresAsumidos.Contains(jug))
                jugadoresAsumidos.Add(jug);
        }


    }
    public void Alerta(Transform snake, bool hold = false, Vector3 normalHold = new Vector3(), 
        bool caja = false, bool cajaCamuf = false, int indexPlayer = 0, bool setsTime = true)
    {
        //REHACER CAJA

        //Si esta en una caja
        

        Vector3 snakePos = snake.position;
        bool yaDetectado = false;
        int indJug = -1;
        for(int i = 0; i < soldado.pensamientos.Count; i++)
        {
            if (soldado.pensamientos[i].tipo == Pensamiento.JUGADOR || soldado.pensamientos[i].tipo == Pensamiento.CHECK_ZONA)
            {

                //string[] modulos = soldado.cosasVistas[i].Split("_");
                //Intenta parsear el final a ver si pone la id del jugador. Si no, es que no hay id y por tanto no busca al jugador
                int id = soldado.pensamientos[i].ultimoDato().GetInt(); //Si no es int, devuelve -1
                if(id == indexPlayer || id == -1)
                {
                    yaDetectado = true;
                    indJug = i;
                    break;

                }
            }
        }

        if (caja)
        {
            //Si se supone que está camuflado (no se mueve)
            //Entonces es porque el soldado no le ha visto ponersela (es decir, le ha perdido de vista, es decir -> t < 2.8 o le ha perdido o no le habia detectado)
            //Y porque no busca cajas (es decir, caja en pensamiento es false y caution es false)
            bool leVePonersela = false;
            bool buscaCajas = false;
            if (yaDetectado)
            {
                leVePonersela = soldado.pensamientos[indJug].tipo == Pensamiento.JUGADOR && //Si busca al jugador
                    soldado.pensamientos[indJug].tiempo > 2.8f; //Si le ha visto ponersela
                buscaCajas = (soldado.pensamientos[indJug].tipo == Pensamiento.JUGADOR && // Si busca al jugador
                    soldado.pensamientos[indJug].datos[1].GetInt() == 1 ) || //Si el dato asociado a la caja es true
                    soldado.Caution(); //O si esta en caution
            }
            else
            {
                leVePonersela = false;
                buscaCajas = soldado.Caution();
            }

            cajaCamuf = cajaCamuf && !leVePonersela && !buscaCajas;
        }

        int estadoRehen = hold ? 1 : 0; 
        int estadoCaja = (caja && !cajaCamuf) ? 1 : 0; //Solo cuenta la caja si la lleva Y no está camuflado
        Dato[] datos = new Dato[5];
        datos[0] = new Dato(snakePos);
        datos[1] = new Dato(estadoCaja);
        datos[2] = new Dato(estadoRehen);
        datos[3] = new Dato(normalHold);
        datos[4] = new Dato(indexPlayer);


        Pensamiento verAJug = new Pensamiento(Pensamiento.JUGADOR, datos);
        if (!yaDetectado)
        {
            if (caja && cajaCamuf && !soldado.Caution())
            {
                return;
            }
            
            verAJug.step = -3;
            verAJug.tiempo = 3;

            soldado.pensamientos.Add(verAJug);


            if (soldado.EstaVivo() && soldado.Consciente() && soldado.controller.enabled)
            {
                soldado.SonidoAlerta();
            }
            //FindObjectOfType<GameManager>().AddAlerta(soldado, overlook:true);
        }
        else
        {

            if (soldado.pensamientos[indJug].tipo == Pensamiento.CHECK_ZONA)
            {
                //Le quitamos de buscar
                FindObjectOfType<GameManager>().AddAlerta(soldado, true, true);
                soldado.pensamientos[indJug].step = -3;
                soldado.pensamientos[indJug].tiempo = 3;

                if (soldado.EstaVivo() && soldado.Consciente())
                    soldado.SonidoAlerta();
               
            }

            //Si esta en alerta, busca el tercer valor del pensamiento para ver si busca incluyendo caja o no
            else
            {
            }


            soldado.pensamientos[indJug].tipo = Pensamiento.JUGADOR;
            soldado.pensamientos[indJug].datos = datos;
            

            //Para el caso en el que se salga de vista pero asume donde está
            if (setsTime)
            {
                soldado.pensamientos[indJug].tiempo = 3;
            }

        }

        if(!jugadoresPerseguidos.Contains(snake))
            jugadoresPerseguidos.Add(snake);
    }

    public void CheckZone(Vector3 zona)
    {
        /*
        string chk = "chkZone_" + VectorAString(zona);
        if (soldado.cosasVistas.Contains(chk))
            return;
        soldado.cosasVistas.Add(chk);
        */
        if (soldado.ContieneExacPensamiento(Pensamiento.CHECK_ZONA, zona))
            return;

        Dato[] datos = new Dato[1];
        datos[0] = new Dato(zona);
        Pensamiento checkZona = new Pensamiento(Pensamiento.CHECK_ZONA, datos);
        soldado.pensamientos.Add(checkZona);

    }

    public bool CheckZoneEx(Vector3 zona)
    {
        if (!soldado.EstaVivo() || !soldado.Consciente() || soldado.jugadorAg != null)
            return true;
        bool yaChk = false;

        for(int i = 0; i < soldado.pensamientos.Count; i++)
        {
            if (soldado.pensamientos[i].tipo == Pensamiento.CHECK_ZONA)
            {
                string chk = "chkZone_" + VectorAString(zona);

                soldado.pensamientos[i].datos[0] = new Dato(zona);

                soldado.pensamientos[i].tiempo = 0;
                soldado.pensamientos[i].step = 0;

                yaChk = true;
                break;
            }
        }

        if(!yaChk)
            CheckZone(zona);

        return yaChk;
    }


    void CompInc(Soldier comp)
    {

        if (comp.Consciente())
        {
            return;
        }


        //Hacer que la deteccion dependa del ID de cada soldado
        bool yaDetectado = false;
        int indJug = -1;
        for (int i = 0; i < soldado.pensamientos.Count; i++)
        {
            if (soldado.pensamientos[i].tipo == Pensamiento.COMP_INC)//(soldado.cosasVistas[i].Contains("compInc"))
            {

                int id = soldado.pensamientos[i].ultimoDato().GetInt();//int.Parse(modulos[modulos.Length - 1]);

                if (id == comp.ID && id != -1)
                {
                    yaDetectado = true;
                    indJug = i;
                    break;
                }
            }
            else if (soldado.pensamientos[i].tipo == Pensamiento.JUGADOR)//(soldado.cosasVistas[i].Contains("verAJug"))
                return;
        }

        if (!yaDetectado)
        {
            //string visto = "compInc_" + VectorAString(comp.transform.position) + "_" + comp.ID.ToString();
            //soldado.cosasVistas.Add(visto);
            Dato[] datos = new Dato[2];
            datos[0] = new Dato(comp.transform.position);
            datos[1] = new Dato(comp.ID);

            Pensamiento compInc = new Pensamiento(Pensamiento.COMP_INC, datos);

            soldado.pensamientos.Add(compInc);

        }

        else
        {
            //string visto = "compInc_" + VectorAString(comp.transform.position) + "_" + comp.ID.ToString();
            //soldado.cosasVistas[indJug] = visto;
            //Dato[] datos = new Dato[2];
            soldado.pensamientos[indJug].datos[0] = new Dato(comp.transform.position);
            soldado.pensamientos[indJug].datos[1] = new Dato(comp.ID);

        }
    }

    void Granada(Transform granada)
    {
        Explosivo explosivo = granada.GetComponent<Explosivo>();

        //string granadaCosa = "explCer_" + VectorAString(granada.position) + "_" + explosivo.id.ToString() + "_" + (explosivo.tirador == soldado.transform).ToString();
        //Busca a ver si ya ha visto la granada
        for (int i = 0; i < soldado.pensamientos.Count; i++)
        {
            if (soldado.pensamientos[i].tipo == Pensamiento.EXPLOSIVO_CERCA)//(soldado.cosasVistas[i].Contains("explCer"))
            {
                //string[] modulos = soldado.cosasVistas[i].Split("_");
                //int id = -1;
                int id = soldado.pensamientos[i].datos[soldado.pensamientos[i].datos.Length - 2].GetInt();


                //id = int.Parse(modulos[modulos.Length - 2]);
                if (id == explosivo.id) 
                {
                    //soldado.cosasVistas[i] = granadaCosa; 
                    soldado.pensamientos[i].datos[0] = new Dato(granada.position);
                    soldado.pensamientos[i].datos[1] = new Dato(explosivo.id);
                    soldado.pensamientos[i].datos[2] = new Dato(explosivo.tirador == soldado.transform ? 1 : 0);

                    return;
                }

            }
        }
        Dato[] datos = new Dato[3];

        datos[0] = new Dato(granada.position);
        datos[1] = new Dato(explosivo.id);
        datos[2] = new Dato(explosivo.tirador == soldado.transform ? 1 : 0);

        Pensamiento granadaPensamiento = new Pensamiento(Pensamiento.EXPLOSIVO_CERCA, datos);

        soldado.pensamientos.Add(granadaPensamiento);

        //soldado.cosasVistas.Add(granadaCosa);

    }

    string VectorAString(Vector3 input, string separador = ";")
    {

        return input.x.ToString(CultureInfo.InvariantCulture.NumberFormat) + separador + 
            input.y.ToString(CultureInfo.InvariantCulture.NumberFormat) + separador + 
            input.z.ToString(CultureInfo.InvariantCulture.NumberFormat);

    }

    bool EsColDeSoldado(Collider col, GameObject interac)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("ColliderDisparo") && col.tag == "Soldado")
        {
            return col.GetComponent<ColliderDisparo>().objetoPadre = interac.transform.parent.parent;
        }
        return false;
             
    }
    bool EsColDeSnake(Collider col, GameObject interac)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("ColliderDisparo") && col.tag == "Snake")
        {
            return col.GetComponent<ColliderDisparo>().objetoPadre = interac.transform.parent.parent;
        }

        return false;
    }
    public void Sonido()
    {
        if (soldado.EnAlerta() || !soldado.Consciente() || !soldado.EstaVivo())
            return;
        soldado.sonidoSoldado.PlayOneShot(Resources.Load<AudioClip>("Audio/Soldiers/WhosThat" + soldado.terminoGenero));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 1, 1, .1f);
        Gizmos.DrawSphere(transform.position, distPerder);
    }

    public void QuitaAsumido(int id)
    {
        int j = -1;
        //Buscamos el jugador que tenga el id especificado
        for(int i = 0; i < jugadoresAsumidos.Count; i++)
        {
            if(jugadoresAsumidos[i].playerID == id)
            {
                j = i;
                break;
            }
        }

        //Si lo hemos encontrado, lo quitamos
        if (j > -1)
            jugadoresAsumidos.RemoveAt(j);
    }

    public void QuitaAsumidos()
    {
        jugadoresAsumidos = new List<Snake>();
    }

    public void QuitaPerseguido(int id)
    {
        int j = -1;
        //Buscamos el jugador que tenga el id especificado
        for (int i = 0; i < jugadoresPerseguidos.Count; i++)
        {
            if (jugadoresPerseguidos[i].GetComponent<Snake>().playerID == id)
            {
                j = i;
                break;
            }
        }

        //Si lo hemos encontrado, lo quitamos
        if (j > -1)
            jugadoresPerseguidos.RemoveAt(j);
    }

    public void QuitaPerseguidos()
    {
        jugadoresPerseguidos = new List<Transform>();
    }
}
