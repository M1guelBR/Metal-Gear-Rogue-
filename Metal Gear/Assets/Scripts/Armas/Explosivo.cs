using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Explosivo : MonoBehaviour
{
    float tiempoExplosion = 3.5f;

    public enum Tipo
    {
        Granada,
        Stun,
        Chaff
    };
    public Tipo tipoExplosivo;
    public bool remoto;
    public int id = -1;
    public Transform tirador = null;
    Vector3 velIn;
    List<Transform> afectados = new List<Transform>();
    bool terminaExplotar = false;
    [SerializeField]bool remoteExplota = false;
    public float fuerza = 1;

    // Start is called before the first frame update
    void Start()
    {
        if(velIn == Vector3.zero)
        {
            velIn = transform.forward * 6 * fuerza;
        }
        this.GetComponent<Rigidbody>().velocity = velIn;
        if(tirador == null)
        {
            tirador = FindObjectsOfType<Snake>()[0].transform;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (!remoto && tiempoExplosion > 0)
        {
            tiempoExplosion -= Time.deltaTime;
            if (tiempoExplosion < 0)
                IniciaExplotar();
        }
        else if (remoteExplota)
        {
            remoteExplota = false;
            IniciaExplotar();
        }

    }
    private void LateUpdate()
    {

        if (tiempoExplosion == -1 && terminaExplotar)
        {
            //Debug.Break();
            Explotar();
            DestroyImmediate(this.gameObject);
        }
    }

    public void IniciaExplotar()
    {
        tiempoExplosion = 0;
        float radioExplosion = tipoExplosivo == Tipo.Chaff ? 2 : 3.5f;
        this.GetComponent<SphereCollider>().radius = radioExplosion;


        //dependiendo del tipo de explosivo que sea hace una cosa u otra
        /*switch (tipoExplosivo)
        {
            case Tipo.Granada:
                break;

            case Tipo.Stun:
                break;

            case Tipo.Chaff:
                break;

        }*/
    }

    public void Explotar()
    {
        for(int i = 0; i < afectados.Count; i++)
        {
            if(afectados[i].GetComponent<Soldier>())
            {
                Soldier sold = afectados[i].GetComponent<Soldier>();
                Vector3 direc = Vector3.zero;
                float mag = 0;
                switch (tipoExplosivo)
                {
                    case Tipo.Granada:

                        direc = Vector3.Scale(sold.transform.position - transform.position, new Vector3(1, 0, 1)).normalized;
                        mag = Mathf.Min(1, 1 / (sold.transform.position - transform.position).magnitude);

                        Snake tirador_ = tirador.GetComponent<Snake>();
                        if (tirador_ == null)
                            tirador_ = FindObjectOfType<Snake>();

                        sold.ThrowDirection(direc * mag * 2.15f, tirador_);


                        sold.QuitaVida(Mathf.Max((sold.Vida() - 0.01f) * mag * 3.5f,0));
                        //print("vida fuera");

                        break;

                    case Tipo.Stun:

                        direc = Vector3.Scale(sold.transform.position - transform.position, new Vector3(1, 0, 1)).normalized;
                        mag = Mathf.Min(1, 1 / (sold.transform.position - transform.position).magnitude);

                        sold.ThrowDirection(direc * mag * 2.15f, tirador.GetComponent<Snake>());

                        break;

                }
                //continue;
            }

            else if(afectados[i].GetComponent<Snake>())
            {
                Snake jug = afectados[i].GetComponent<Snake>();
                Vector3 direc = Vector3.zero;
                float mag = 0;
                switch (tipoExplosivo)
                {
                    case Tipo.Granada:

                        direc = (jug.transform.position - transform.position).normalized;
                        mag = Mathf.Min(3, 1 / (jug.transform.position - transform.position).magnitude);

                        //sold.ThrowDirection(direc * mag * 2.15f, tirador.GetComponent<Snake>());
                        jug.Tirate(direc * mag * 10f);

                        jug.QuitaVida(Mathf.Max((jug.vida - 0.01f) * mag * 1.5f, 0));

                        break;

                    case Tipo.Stun:

                        direc = (jug.transform.position - transform.position).normalized;
                        mag = Mathf.Min(3, 1 / (jug.transform.position - transform.position).magnitude);
                        //print(direc); Debug.Break();
                        //sold.ThrowDirection(direc * mag * 2.15f, tirador.GetComponent<Snake>());
                        jug.Tirate(direc * mag * 10f);
                        jug.StunG();

                        break;
                    case Tipo.Chaff:
                        jug.SetTJamming(10);
                        break;

                }
                //continue;
            }

            else if (afectados[i].GetComponent<Columna>() && tipoExplosivo == Tipo.Granada)
            {
                //Que reviente la columna
                afectados[i].GetComponent<Columna>().Destroy();
            }

        }


        //afectados = new List<Transform>();
        string chaffAdd = tipoExplosivo == Tipo.Chaff ? "Chaff" : "";
        GameObject expl = Instantiate(Resources.Load<GameObject>("Prefabs/Explosion" + chaffAdd + "SFX"));
        expl.transform.position = transform.position;
        tiempoExplosion = 3.5f;
        this.GetComponent<SphereCollider>().radius = 0.01f;
        terminaExplotar = false;
    }

    public void SetExplosivo(Transform jugador, int idExp)
    {
        tirador = jugador;
        id = idExp;
    }

    private void OnTriggerStay(Collider other)
    {
        if (tiempoExplosion == 0 && other.gameObject.name.Contains("InteraccionSoldado") && !afectados.Contains(other.transform.parent.parent))
        {

            Vector3 radioVec = other.transform.position - transform.position;
            float dist = Mathf.Max(Mathf.Abs(radioVec.x), Mathf.Abs(radioVec.z));
            print(Mathf.Abs(radioVec.y) + " Y dist");
            print(dist + " Radio XZ");

            if (Mathf.Abs(radioVec.y) <= 1 && dist < 0.3f)
            {
                afectados.Add(other.transform.parent.parent);
                return;
            }


            //Añade al soldado a la lista de afectados
            Soldier soldado = other.transform.parent.parent.GetComponent<Soldier>();
            //afectados.Add(soldado.transform);
            //return;
            //Rayo a cabeza y pies de soldado
            Vector3 soldPos = soldado.transform.position;
            Vector3 cabezaPos = soldado.transform.position + (soldado.hips.up * 0.35f);
            Vector3 piesPos = soldado.transform.position + (soldado.LLeg.up * 0.35f);
            RaycastHit rayoSold;


            //Debug.DrawLine(transform.position, soldPos);
            //Debug.DrawLine(transform.position, cabezaPos);
            //Debug.DrawLine(transform.position, piesPos );

            LayerMask soldLayer = (1 << LayerMask.NameToLayer("Interaccion")) | (1 << LayerMask.NameToLayer("EscenarioColliders"));

            if (Physics.Raycast(transform.position, soldPos - transform.position, out rayoSold, Mathf.Infinity, soldLayer))
            {
                if (rayoSold.collider.name.Contains("InteraccionSoldado"))
                {
                    afectados.Add(soldado.transform);
                    return;
                }
            }
            if (Physics.Raycast(transform.position, cabezaPos - transform.position, out rayoSold, Mathf.Infinity, soldLayer))
            {
                if (rayoSold.collider.name.Contains("InteraccionSoldado"))
                {
                    afectados.Add(soldado.transform);
                    return;
                }
            }
            if (Physics.Raycast(transform.position, piesPos - transform.position, out rayoSold, Mathf.Infinity, soldLayer))
            {

                if (rayoSold.collider.name.Contains("InteraccionSoldado"))
                {
                    afectados.Add(soldado.transform);
                    return;
                }

            }
            return;
        }

        else if (tiempoExplosion == 0 && other.gameObject.tag.Contains("JugCol") && !afectados.Contains(other.transform.parent.parent))
        {
            Vector3 radioVec = other.transform.position - transform.position;
            float dist = Mathf.Max(Mathf.Abs(radioVec.x), Mathf.Abs(radioVec.z));
            print(Mathf.Abs(radioVec.y) + " Y dist");
            print(dist + " Radio XZ");

            if (Mathf.Abs(radioVec.y) <= 1 && dist < 0.3f)
            {
                afectados.Add(other.transform.parent.parent);
                //Debug.Break();
                return;
            }


            //Añade al jugador a la lista de afectados

            //Cambiar cosas de soldado por cosas de jugador

            Snake snake = other.transform.parent.parent.GetComponent<Snake>();
            //afectados.Add(soldado.transform);
            //return;
            //Rayo a cabeza y pies de soldado
            Vector3 jugPos = snake.transform.position;
            Vector3 cabezaPos = snake.cabeza.transform.position;
            Vector3 piesPos = jugPos + (jugPos - cabezaPos);
            RaycastHit rayoSold;


            //Debug.DrawLine(transform.position, jugPos);
            //Debug.DrawLine(transform.position, cabezaPos);
            //Debug.DrawLine(transform.position, piesPos);

            LayerMask soldLayer = (1 << LayerMask.NameToLayer("InteraccionSnake")) | (1 << LayerMask.NameToLayer("EscenarioColliders"));

            if (Physics.Raycast(transform.position, jugPos - transform.position, out rayoSold, Mathf.Infinity, soldLayer))
            {
                if (rayoSold.collider.tag.Contains("JugCol"))
                {
                    afectados.Add(snake.transform);
                    return;
                }
            }
            if (Physics.Raycast(transform.position, cabezaPos - transform.position, out rayoSold, Mathf.Infinity, soldLayer))
            {
                if (rayoSold.collider.tag.Contains("JugCol"))
                {
                    afectados.Add(snake.transform);
                    return;
                }
            }
            if (Physics.Raycast(transform.position, piesPos - transform.position, out rayoSold, Mathf.Infinity, soldLayer))
            {

                if (rayoSold.collider.tag.Contains("JugCol"))
                {
                    afectados.Add(snake.transform);
                    return;
                }

            }

            return;
        }

        else if (tiempoExplosion == 0 && other.name.Contains("Columna") && !afectados.Contains(other.transform))
            afectados.Add(other.transform);

        if (remoto && tiempoExplosion > 0 && transform.parent == null)
        {
            ColliderDisparo colDisp = other.GetComponent<ColliderDisparo>();

            bool condA = other.gameObject.layer == LayerMask.NameToLayer("Explosivo");
            bool condB = (colDisp != null && colDisp.objetoPadre == tirador);
            bool condC = (other.gameObject.layer == LayerMask.NameToLayer("SnakeCollider") && other.transform.parent == tirador);
            bool condD = other.gameObject.layer == LayerMask.NameToLayer("InteraccionSnake");

            if (condA || condB || condC || condD)
                return;
            transform.parent = other.transform;
            this.GetComponent<Rigidbody>().isKinematic = true;
            //Debug.Break();
        }

    }
    private void FixedUpdate()
    {
        if (tiempoExplosion == 0 && terminaExplotar)
            tiempoExplosion = -1;
        else if (tiempoExplosion == 0 && !terminaExplotar)
        {
            terminaExplotar = true;
        }
    }

}
