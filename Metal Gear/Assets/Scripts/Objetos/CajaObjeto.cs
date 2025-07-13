using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CajaObjeto : MonoBehaviour
{

    public Objeto objeto;
    [SerializeField] bool soloBalas;
    [SerializeField] int balas;
    public bool activo = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!activo)
            return;
        if(other.tag.Contains("JugCol") && !soloBalas)
        {
            bool haRecibido = other.transform.parent.parent.GetComponent<Snake>().RecibObjeto(objeto);
            bool aux = true;
            if (balas != 0 && haRecibido)
                aux = other.transform.parent.parent.GetComponent<Snake>().RecibeBalas(objeto, balas, false);
            if (haRecibido)
                Destroy(gameObject, 0);
        }
        else if(other.tag.Contains("JugCol") && soloBalas)
        {
            bool haRecibido = other.transform.parent.parent.GetComponent<Snake>().RecibeBalas(objeto, balas);
            if (haRecibido)
                Destroy(gameObject, 0);
        }
        else if (other.tag.Contains("SoldCol") && !other.name.Contains("Inc") && soloBalas && objeto.TipoObjeto() >= 0)
        {
            bool haRecibido = other.transform.parent.parent.GetComponent<Soldier>().RecibeBalas(objeto.arma(), objeto.arma().balasArma() + balas);
            if (haRecibido)
            {
                print(haRecibido);
                Destroy(gameObject, 0);
            }
        }
        else if(other.tag.Contains("SoldCol") && !other.name.Contains("Inc") && !soloBalas && objeto.TipoObjeto() >= 0)
        {
            bool haRecibido = other.transform.parent.parent.GetComponent<Soldier>().RecibeArma(objeto.arma(), objeto.arma().balasArma() + balas);
            if (haRecibido)
            {
                print(haRecibido);
                Destroy(gameObject, 0);
                gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (GetComponent<Rigidbody>() && GetComponent<Rigidbody>().velocity.magnitude < .1f)
        {
            activo = true;
            Destroy(GetComponent<Rigidbody>(), 0);
        }

    }

    public void Crear(Objeto obj, bool modoBalas, int balasCaja, bool activo_ = false)
    {
        objeto = obj;
        soloBalas = modoBalas;
        balas = balasCaja;
        activo = activo_; 
    }

}
