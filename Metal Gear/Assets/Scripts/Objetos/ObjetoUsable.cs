using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]

public class ObjetoUsable : Objeto
{
    public Sprite UIObj;
    public enum Parent
    {
        Ninguno,
        Pecho,
        Cabeza
    }
    public Parent parent;

    [Range(1, 15)] public int maxCant=1;
    public bool esDef;
    public Mesh objetoVisual;
    public Material material;
    public Vector3 escala = new Vector3(1,1,1);
    public Vector3 localPos = Vector3.zero;
    public Vector3 localEuler = Vector3.zero;
     
    public override ObjetoUsable objeto()
    {
        return this;
    }
    public override int maxCantidad()
    {
        return maxCant;
    }

}
