using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Salida : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.tag.Contains("JugCol"))
        {
            Snake jugador = other.transform.parent.parent.GetComponent<Snake>();
            FindObjectOfType<GameManager>().MataJugador(jugador);
        }
    }
}
