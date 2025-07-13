using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecibeSonidos : MonoBehaviour
{
    public VistaSoldado vista;
    public bool disabled = false;

    private void OnTriggerEnter(Collider other)
    {
        if (disabled)
            return;
        if (other.gameObject.layer == LayerMask.NameToLayer("Sonido") &&
            (!other.GetComponent<SonidoSoldado>() || (other.GetComponent<SonidoSoldado>().jug == null)))
        {
            if(!vista.CheckZoneEx(other.transform.position))
                vista.Sonido();

            //Debug.Break();
        }
        else if(other.gameObject.layer == LayerMask.NameToLayer("Sonido") && other.GetComponent<SonidoSoldado>().jug != null)
        {
            //Acomodar a las cajas

            vista.Alerta(other.GetComponent<SonidoSoldado>().jug.position, false, false, other.GetComponent<SonidoSoldado>().jug.GetComponent<Snake>().playerID);
        }

    }

}
