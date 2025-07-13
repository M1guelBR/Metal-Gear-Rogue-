using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteraccionSnake : MonoBehaviour
{
    public Soldier soldadoCQC;
    List<Collider> colliders = new List<Collider>();
    public bool comprueba;
    public Collider masProximo;
    private void OnTriggerStay(Collider other)
    {
        if(other.name.Contains("InteraccionSoldado") && !colliders.Contains(other))
        {
            colliders.Add(other);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.name.Contains("InteraccionSoldado") && colliders.Contains(other))
        {
            colliders.Remove(other);
        }
    }

    private void LateUpdate()
    {
        if (comprueba)
        {
            float dot = -1.1f;
            List<int> indAQuitar = new List<int>();

            masProximo = null;

            for (int i = 0; i < colliders.Count; i++)
            {
                if (!colliders[i].name.Contains("InteraccionSoldado") || colliders[i].gameObject.activeInHierarchy == false) 
                {
                    indAQuitar.Add(i);
                    continue;
                }

                Vector3 colliderDist = colliders[i].transform.position - transform.position;
                float nuevoDot = Vector3.Dot(colliderDist.normalized, transform.forward);
                if (nuevoDot > dot)
                {
                    masProximo = colliders[i];
                    dot = nuevoDot;
                }



            }

            if (masProximo != null)
            {
                soldadoCQC = masProximo.transform.parent.parent.GetComponent<Soldier>();
                if ((soldadoCQC.pillado && soldadoCQC.jugadorAg != transform.parent.parent) || masProximo.name == "InteraccionSoldadoInc")
                    soldadoCQC = null;
            }
            else
                soldadoCQC = null;

            for (int j = 0; j < indAQuitar.Count; j++)
            {
                colliders.RemoveAt(indAQuitar[j]);
            }
        }
    }


}
