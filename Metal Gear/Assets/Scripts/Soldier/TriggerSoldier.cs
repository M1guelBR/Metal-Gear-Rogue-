using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSoldier : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        Soldier soldado = transform.parent.parent.GetComponent<Soldier>();
        soldado.TriggerEnter(other);
    }
    private void OnTriggerExit(Collider other)
    {
        Soldier soldado = transform.parent.parent.GetComponent<Soldier>();
        soldado.TriggerExit(other);
    }
    private void OnTriggerStay(Collider other)
    {
        Soldier soldado = transform.parent.parent.GetComponent<Soldier>();
        soldado.TriggerStay(other);
    }

}
