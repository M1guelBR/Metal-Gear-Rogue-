using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activador : MonoBehaviour
{
    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "JugColButton")
        {
            this.transform.parent.GetComponent<Terminal>().Activar();
            Destroy(this.gameObject, 0);
        }
    }
}
