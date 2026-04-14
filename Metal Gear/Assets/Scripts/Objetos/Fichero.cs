using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fichero : MonoBehaviour
{

    private void OnTriggerStay(Collider other)
    {
        //Si es un juegador interactuando con el fichero
        if (other.tag.Contains("JugColBut"))
        {
            FindObjectOfType<GameManager>().RecogeFichero(this);
            Destroy(this.gameObject, 0);
        }
    }

}
