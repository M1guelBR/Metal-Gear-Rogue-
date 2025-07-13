using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Objeto : ScriptableObject
{
    public string nombre;
    public string descripcion;

    public virtual int TipoObjeto()
    {
        return -2;
    }
    public virtual Arma arma()
    {
        return null;
    }
    public virtual ObjetoUsable objeto()
    {
        return null;
    }
    public virtual int balasArma()
    {
        return 0;
    }
    public virtual int cargador()
    {
        return 0;
    }
    public virtual int maxCantidad()
    {
        return 0;
    }
}
