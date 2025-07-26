using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arma : Objeto
{

    public Mesh modeloArma;
    public Material materialArma;
    public AudioClip sonidoDisparo;
    public Sprite UIArma;
    public AudioClip recargaClip;

    public override int TipoObjeto()
    {
        return -1;
    }
    public override Arma arma()
    {
        return this;
    }
    public virtual Vector3 posParticulas()
    {
        return Vector3.zero;
    }
    public virtual Pistola_Fusil pistolaFusil()
    {
        return null;
    }
    public virtual Granada granada()
    {
        return null;
    }

    public virtual Vector3? Disparar(Vector3 posicion, Vector3 direccion, Vector3 up, Transform disparador, bool esJug)
    {
        return null;
    }
    public virtual AudioClip sonido()
    {
        return null;
    }
    public virtual float Cadencia()
    {
        return 0;
    }
    public virtual float remoto()
    {
        return 0;
    }
}
