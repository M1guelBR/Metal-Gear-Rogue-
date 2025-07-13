using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class ArmoryCrate : MonoBehaviour
{
    [SerializeField]Objeto obj;
    int cant = 0;
    bool recoge = false;
    [SerializeField] TMP_Text textFront, textBack;
    [SerializeField] bool autoIn = false;
    // Start is called before the first frame update
    void Start()
    {
        //SeleccionaObjeto();

    }

    // Update is called once per frame
    void Update()
    {
        if (autoIn)
        {
            Inicia();
            autoIn = false;
        }
    }
    public void Inicia()
    {

        FindObjectOfType<GameManager>().crates.Add(this.gameObject);
        SeleccionaObjeto();
        cant = Random.Range(1, 6);
        textFront.text = cant.ToString();
        textBack.text = cant.ToString();
    }

    void SeleccionaObjeto()
    {
        int arma_obj = Random.Range(1, 101);

        List<Objeto> objetosPosibles = new List<Objeto>();
        //Si es un arma
        if(arma_obj < 50)
        {

            //Cargamos todas las armas posibles
            objetosPosibles = Resources.LoadAll<Objeto>("Armas").ToList<Objeto>();

            //Quitamos la vacía, todas las cajas tienen algo
            objetosPosibles.Remove(Resources.Load<Objeto>("Armas/EMPTY"));


        }
        //Si es un objeto
        else
        {
            //Cargamos todos los objetos posibles
            objetosPosibles = Resources.LoadAll<Objeto>("Objetos").ToList<Objeto>();

            //Quitamos el vacío, todas las cajas tienen algo
            objetosPosibles.Remove(Resources.Load<Objeto>("Objetos/EMPTY"));

        }


        //Los pesos de probabilidad de que se cada arma
        int[] pesos = new int[objetosPosibles.Count];

        //La "suma" de los pesos
        int total = pesos.Length * 10;

        int probAsig = 0; int cantAsig = 0;
        //Asignamos ciertos pesos especiales a ciertas armas : COLT, C4 (Si luego añado más armas, las puedo poner aquí)

        for (int i = 0; i < objetosPosibles.Count; i++)
        {
            if (objetosPosibles[i].nombre == "COLT SAA")
            {
                pesos[i] = (int)(total * 0.1f);
                probAsig += pesos[i];
                cantAsig += 1;
            }

            else if (objetosPosibles[i].nombre == "C4")
            {
                pesos[i] = (int)(total * 0.1f);
                probAsig += pesos[i];
                cantAsig += 1;
            }
            else if (objetosPosibles[i].nombre == "RATION")
            {
                pesos[i] = (int)(total * 0.25f);
                probAsig += pesos[i];
                cantAsig += 1;
            }
            else if (objetosPosibles[i].nombre == "N.V.G.")
            {
                pesos[i] = (int)(total * 0.25f);
                probAsig += pesos[i];
                cantAsig += 1;
            }
            else if (objetosPosibles[i].nombre == "C. BOX")
            {
                pesos[i] = (int)(total * 0.25f);
                probAsig += pesos[i];
                cantAsig += 1;
            }
        }

        //Al resto, le asignamos probabilidad uniforme de la que queda sin asignar
        for (int i = 0; i < objetosPosibles.Count; i++)
        {
            if (pesos[i] != 0)
                continue;
            else
            {
                //Asignamos lo que queda dividido por el número de pesos que quedan por asignar
                pesos[i] = (total - probAsig) / (pesos.Length - cantAsig);
                probAsig += pesos[i];
                cantAsig += 1;
            }
        }

        cantAsig = Random.Range(0, total); //El número aleatorio del que se saca el arma a seleccionar
        probAsig = 0;
        for (int i = 0; i < objetosPosibles.Count; i++) //Otro for, que nunca está mal
        {

            //Si el número está ente la probabilidad acumulada por el arma i y la siguiente (siempre que haya siguiente)
            //Entonces este es nuestro objeto
            if (cantAsig >= probAsig && cantAsig < probAsig + pesos[i])
            {
                obj = objetosPosibles[i];
                return;
            }
            probAsig += pesos[i];

        }

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag.Contains("JugColBut") && !recoge && cant > 0)
        {
            recoge = true;
            if (other.transform.parent.parent.GetComponent<Snake>().RecibObjeto(obj))
            {
                cant -= 1;
                textFront.text = cant.ToString();
                textBack.text = cant.ToString();
            }
        }
        else if (recoge && other.tag == "JugCol")
        {
            recoge = false;
            print("sale");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Contains("JugCol"))
            recoge = false;
    }
}
