using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Granada : Arma
{

    public enum Tipo
    {
        Granada, 
        Chaff,
        Stun
    };
    public Tipo tipo;
    [Range(0, 1)] public float impulso = 1;
    public bool remote = false;
    public bool esDef = false;
    public Vector3 posPart;
    public override int TipoObjeto()
    {
        return 0;
    }
    public override Granada granada()
    {
        return this;
    }
    public override int cargador()
    {
        return 10;
    }
    public override int balasArma()
    {
        return 5;
    }

    public override Vector3? Disparar(Vector3 posicion, Vector3 direccion, Vector3 up, Transform disparador, bool esJug)
    {

        if (remote)
        {
            if (disparador.GetComponent<Snake>().GetButton("Apuntar"))
                PonerRemoto(posicion, direccion, disparador.GetComponent<Snake>());
            else
                ActivarRemoto(disparador.GetComponent<Snake>());

            return null;
        }

        GameObject explosivo = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/" + this.nombre));
        explosivo.transform.forward = direccion;
        explosivo.transform.position = posicion  + (direccion.normalized * 0.15f)+ (Vector3.up * 0.15f);
        Explosivo exp = explosivo.GetComponent<Explosivo>();
        exp.id = FindObjectsOfType<Explosivo>().Length;
        exp.tirador = disparador;
        exp.fuerza = this.impulso;

        return null;
    }


    public void PonerRemoto(Vector3 posicion, Vector3 direccion, Snake jugador)
    {
        //Vemos primero si se puede poner
        int indexRemote = jugador.armasInv.IndexOf(this);


        //Hallamos el ID a añadir
        int idAdd = -1;
        foreach(Explosivo explosivo in FindObjectsOfType<Explosivo>())
        {
            if (explosivo.name == this.nombre && explosivo.id > idAdd)
                idAdd = explosivo.id;

        }

        //El id a añadir es el siguiente al más grande que haya
        idAdd += 1;

        //Creamos el dispositivo remoto
        {
            GameObject explosivo = GameObject.Instantiate(Resources.Load<GameObject>("Prefabs/" + this.nombre)); explosivo.name = this.nombre;
            explosivo.transform.forward = direccion;
            explosivo.transform.position = posicion + (direccion.normalized * 0.15f) + (Vector3.up * 0.15f);
            Explosivo exp = explosivo.GetComponent<Explosivo>();
            exp.id = FindObjectsOfType<Explosivo>().Length;
            exp.tirador = jugador.transform;
            exp.fuerza = .05f;
            exp.id = idAdd;
        }

        //Añadimos el id al front del jugador
        jugador.SetFrontID(indexRemote, idAdd, false);

    }
    public void ActivarRemoto(Snake jugador)
    {
        //Buscamos el id del explosivo a detonar
        int indexRemote = jugador.armasInv.IndexOf(this);

        int idADetonar = jugador.GetFirstFrontID(indexRemote);

        //Si no hay nada a detonar, que vuelva
        if (idADetonar == -1)
            return;

        //Buscamos el explosivo a detonar
        Explosivo expADetonar = null;
        foreach(Explosivo exp in FindObjectsOfType<Explosivo>())
        {
            if (exp.name != this.nombre || exp.id != idADetonar)
                continue;
            expADetonar = exp;
            break;
        }


        //Detonamos el explosivo y borramos el ID del front
        expADetonar.IniciaExplotar();
        jugador.SetFrontID(indexRemote, idADetonar, true);

    }

    public override float remoto()
    {
        return remote ? .05f : 0;
    }
}
