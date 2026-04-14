using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Columna : MonoBehaviour
{
    [SerializeField] Mesh destruida;
    [SerializeField] Material matDestruida;


    public void Destroy()
    {
        //----------Cambiamos el mesh al destruido
        GetComponent<MeshFilter>().mesh = destruida;
        GetComponent<MeshRenderer>().material = matDestruida;

        //---------Cambiamos las colisiones al destruido
        GetComponent<MeshCollider>().enabled = true;
        GetComponent<BoxCollider>().enabled = false;


        //--------------Quitamos la columna del game manager
        FindObjectOfType<GameManager>().QuitaColumna(this);


    }
}
