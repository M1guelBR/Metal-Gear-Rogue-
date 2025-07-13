using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Globalization;

public class VistaSoldado : MonoBehaviour
{

    public List<Collider> collidersALaVista;

    [SerializeField]Soldier soldado;

// Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        List<int> indicesRem = new List<int>();
        for(int i = 0; i < collidersALaVista.Count; i++)
        {
            if (collidersALaVista[i] == null || !collidersALaVista[i].gameObject.activeInHierarchy)
            {
                indicesRem.Add(i);
                continue;

            }
            if (collidersALaVista[i].tag.Contains("JugCol"))
            {
                //Checkear si es jugador y dar alerta a Soldier.cs
                Transform jugador = collidersALaVista[i].transform.parent.parent;
                Snake snakeJ = jugador.GetComponent<Snake>();
                bool caja = snakeJ.caja;
                bool cajaCamuf = snakeJ.caja && !snakeJ.enMovimiento();

                //Vector3 jugPos = jugador.GetComponent<CharacterController>().center + jugador.position;
                Vector3 jugPos = jugador.GetComponent<CapsuleCollider>().center + jugador.position;
                Vector3 cabezaPos = snakeJ.cabeza.transform.position;
                Vector3 piesPos = jugPos + (jugPos - cabezaPos);
                Debug.DrawRay(transform.position, cabezaPos - transform.position, Color.white);
                Debug.DrawRay(transform.position, jugPos - transform.position, Color.white);
                Debug.DrawRay(transform.position, piesPos - transform.position, Color.white);

                RaycastHit hitPlayer; 
                LayerMask layer = (1 << LayerMask.NameToLayer("EscenarioColliders")) | (1 << LayerMask.NameToLayer("ColliderDisparo"));
                bool detectado = false;
                if (Physics.Raycast(transform.position, cabezaPos - transform.position, out hitPlayer, Mathf.Infinity, layer))
                {
                    //Alerta
                    if (EsColDeSnake(hitPlayer.collider, collidersALaVista[i].gameObject))
                    {
                        Alerta(jugPos, caja, cajaCamuf, snakeJ.playerID);
                        detectado = true;
                    }
                }
                if (!detectado && Physics.Raycast(transform.position, jugPos - transform.position, out hitPlayer, Mathf.Infinity, layer))
                {
                    //Alerta
                    if (EsColDeSnake(hitPlayer.collider, collidersALaVista[i].gameObject))
                    {
                        Alerta(jugPos, caja, cajaCamuf, snakeJ.playerID);
                        detectado = true;
                    }
                }
                if (!detectado && Physics.Raycast(transform.position, piesPos - transform.position, out hitPlayer, Mathf.Infinity, layer))
                {
                    //Alerta
                    if (EsColDeSnake(hitPlayer.collider, collidersALaVista[i].gameObject))
                    {
                        Alerta(jugPos, caja, cajaCamuf, snakeJ.playerID);
                    }
                }


            }

            else if(collidersALaVista[i].tag == "SoldCol")
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
                Vector3 cabezaPos = soldInc.Rig.transform.position + (soldInc.hips.up*0.35f);
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
                    if(EsColDeSoldado(hitSoldier.collider, collidersALaVista[i].gameObject))
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

            else if(collidersALaVista[i].tag == "Objeto" && collidersALaVista[i].gameObject.layer == LayerMask.NameToLayer("Explosivo"))
            {
                RaycastHit rayo;LayerMask layer = (1 << LayerMask.NameToLayer("EscenarioColliders")) | (1 << LayerMask.NameToLayer("Explosivo"));
                if (Physics.Raycast(transform.position, collidersALaVista[i].transform.position - transform.position, out rayo, Mathf.Infinity, layer, QueryTriggerInteraction.Ignore))
                {
                    if (rayo.collider == collidersALaVista[i])
                        Granada(rayo.collider.transform);
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
            //PierdeJugador(other);
        }

        // hacer que cuando deje de verlo se olvide que está ahí
        else if ((other.name.Contains("InteraccionSoldado") && other.tag == "SoldCol" && collidersALaVista.Contains(other)))
        {
            collidersALaVista.Remove(other);
        }
    }

    void PierdeJugador(Collider other)
    {
        soldado.Agachate(false);
        for (int i = 0; i < soldado.cosasVistas.Count; i++)
        {
            if (!soldado.cosasVistas[i].Contains("verAJug"))
                continue;

            Snake snakeJ = other.transform.parent.parent.GetComponent<Snake>();
            if (!soldado.cosasVistas[i].EndsWith(snakeJ.playerID.ToString()))
                continue;

            //Hacer que vaya en direccion a donde va el jugador cuando lo pierde de vista

            Vector3 nPos = snakeJ.transform.position + snakeJ.Direccion();
            soldado.cosasVistas[i] = "chkZone_" + VectorAString(nPos) + "_" + snakeJ.playerID.ToString();
            if (soldado.cosasSteps.Count <= i)
            {
                soldado.cosasSteps.Add(0);
            }
            else
            {
                soldado.cosasSteps[i] = 0;
            }

            if (soldado.cosasTiempo.Count <= i)
            {
                soldado.cosasTiempo.Add(0);
            }
            else
            {
                soldado.cosasTiempo[i] = 0;
            }

            FindObjectOfType<GameManager>().AddAlerta(soldado, true);
            FindObjectOfType<GameManager>().AddAlerta(soldado, false, true);
            break;

        }
    }
    public void Alerta(Vector3 snakePos, bool caja = false, bool cajaCamuf = false, int indexPlayer = 0)
    {

        bool yaDetectado = false;
        int indJug = -1;
        for(int i = 0; i < soldado.cosasVistas.Count; i++)
        {
            if ((soldado.cosasVistas[i].Contains("verAJug") || soldado.cosasVistas[i].Contains("chkZone")))
            {

                string[] modulos = soldado.cosasVistas[i].Split("_");
                int id = -1;
                //Intenta parsear el final a ver si pone la id del jugador. Si no, es que no hay id y por tanto no busca al jugador
                try
                {
                    id = int.Parse(modulos[modulos.Length - 1]);
                }
                catch
                {

                }
                if(id == indexPlayer || id == -1)
                {
                    yaDetectado = true;
                    indJug = i;
                    break;

                }
            }
        }

        if (!yaDetectado)
        {
            if (caja && cajaCamuf && !soldado.Caution())
            {
                return;
            }

            string visto = "verAJug_" + VectorAString(snakePos) + "_" + (caja ? "t" : "f") + "_" + indexPlayer.ToString();
            soldado.cosasVistas.Add(visto);
            soldado.cosasSteps.Add(-2);
            soldado.cosasTiempo.Add(3);
            soldado.SonidoAlerta();
            //FindObjectOfType<GameManager>().AddAlerta(soldado);

        }
        else
        {
            bool valorCaja = false;

            if (soldado.cosasVistas[indJug].Contains("chkZone"))
            {

                FindObjectOfType<GameManager>().AddAlerta(soldado, true, true);
                soldado.cosasSteps[indJug] = -2;
                soldado.cosasTiempo[indJug] = 3;
                soldado.SonidoAlerta();
                //EditorApplication.isPaused = true;
            }

            //Si esta en alerta, busca el tercer valor del pensamiento para ver si busca incluyendo caja o no
            else
            {
                valorCaja = soldado.cosasVistas[indJug].Split("_")[2] == "t";

            }

            if (caja && cajaCamuf)
            {
                if (!valorCaja && !soldado.Caution())
                    return;

                //Si no vuelve es porque checkea las cajas, en tal caso, el valor de caja es true
                valorCaja = true;


            }
            else if(caja && !cajaCamuf)
            {
                //Ahora esta en una caja y no está camuflado así que automáticamente lo busca en cajas

                //Si no le buscaba, reproduce el sonido de alertarse
                if(!valorCaja)
                    soldado.SonidoAlerta();

                valorCaja = true;
            }



            string visto = "verAJug_" + VectorAString(snakePos) + "_" + (valorCaja ? "t" : "f") + "_" + indexPlayer.ToString();


            soldado.cosasVistas[indJug] = visto;

            if (soldado.cosasSteps[0] != -2 && soldado.cosasTiempo.Count == soldado.cosasVistas.Count)
                soldado.cosasTiempo[indJug] = 3;

            else if (soldado.cosasTiempo.Count != soldado.cosasVistas.Count) 
                soldado.cosasTiempo.Add(3);

        }



    }

    public void CheckZone(Vector3 zona)
    {
        string chk = "chkZone_" + VectorAString(zona);
        if (soldado.cosasVistas.Contains(chk))
            return;
        soldado.cosasVistas.Add(chk);
    }

    public bool CheckZoneEx(Vector3 zona)
    {
        if (!soldado.EstaVivo() || !soldado.Consciente() || soldado.jugadorAg != null)
            return true;
        bool yaChk = false;

        for(int i = 0; i < soldado.cosasVistas.Count; i++)
        {
            if (soldado.cosasVistas[i].Contains("chkZone"))
            {
                string chk = "chkZone_" + VectorAString(zona);
                soldado.cosasVistas[i] = chk;
                soldado.cosasTiempo[i] = 0;
                soldado.cosasSteps[i] = 0;
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
        for (int i = 0; i < soldado.cosasVistas.Count; i++)
        {
            if (soldado.cosasVistas[i].Contains("compInc"))
            {

                string[] modulos = soldado.cosasVistas[i].Split("_");
                int id = int.Parse(modulos[modulos.Length - 1]);

                if (id == comp.ID && id != -1)
                {
                    yaDetectado = true;
                    indJug = i;
                    break;
                }
            }
            else if (soldado.cosasVistas[i].Contains("verAJug"))
                return;
        }


        if (!yaDetectado)
        {
            string visto = "compInc_" + VectorAString(comp.transform.position) + "_" + comp.ID.ToString();
            soldado.cosasVistas.Add(visto);

        }

        else
        {
            string visto = "compInc_" + VectorAString(comp.transform.position) + "_" + comp.ID.ToString();
            soldado.cosasVistas[indJug] = visto;
        }
    }

    void Granada(Transform granada)
    {
        Explosivo explosivo = granada.GetComponent<Explosivo>();

        string granadaCosa = "explCer_" + VectorAString(granada.position) + "_" + explosivo.id.ToString() + "_" + (explosivo.tirador == soldado.transform).ToString();
        //Busca a ver si ya ha visto la granada
        for (int i = 0; i < soldado.cosasVistas.Count; i++)
        {
            if (soldado.cosasVistas[i].Contains("explCer"))
            {
                string[] modulos = soldado.cosasVistas[i].Split("_");
                int id = -1;


                id = int.Parse(modulos[modulos.Length - 2]);
                if (id == explosivo.id) { soldado.cosasVistas[i] = granadaCosa; return; }

            }
        }

        soldado.cosasVistas.Add(granadaCosa);

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
        soldado.sonidoSoldado.PlayOneShot(Resources.Load<AudioClip>("Audio/Soldiers/WhosThat"));
    }

}
