using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class DibujosSala : MonoBehaviour
{
    Sala this_;
    public void Start()
    {
        this_ = this.GetComponent<Sala>();
    }
    public void Update()
    {
        if(this_ == null)
            this_ = this.GetComponent<Sala>();



        if (this_.puertasFuera.Count == this_.puertasPosiciones.Length)
        {
            for (int i = 0; i < this_.puertasPosiciones.Length; i++)
            {
                Vector3 direccionReal = Vector3.Scale(transform.localScale, this_.puertasPosiciones[i]);

                direccionReal = (transform.forward * direccionReal.z) + (transform.right * direccionReal.x) + (transform.up * direccionReal.y);

                Vector3 posReal = transform.position + direccionReal;

                Vector3 fueraReal = (transform.forward * this_.puertasFuera[i].z) + (transform.right * this_.puertasFuera[i].x) + (transform.up * this_.puertasFuera[i].y);


                Debug.DrawRay(posReal, fueraReal, Color.red);
            }
            //print("hola");
        }

        for (int i = 0; i < this_.centro.Length; i++)
        {
            Vector3[] vertices = this_.getVertices(i);


            Debug.DrawLine(vertices[0], vertices[1], Color.green);
            Debug.DrawLine(vertices[1], vertices[2], Color.green);
            Debug.DrawLine(vertices[2], vertices[3], Color.green);
            Debug.DrawLine(vertices[3], vertices[5], Color.green);
            Debug.DrawLine(vertices[4], vertices[5], Color.green);
            Debug.DrawLine(vertices[4], vertices[2], Color.green);
            Debug.DrawLine(vertices[0], vertices[4], Color.green);
            Debug.DrawLine(vertices[0], vertices[6], Color.green);
            Debug.DrawLine(vertices[6], vertices[7], Color.green);
            Debug.DrawLine(vertices[3], vertices[7], Color.green);
            Debug.DrawLine(vertices[1], vertices[7], Color.green);
            Debug.DrawLine(vertices[5], vertices[6], Color.green);
        }

    }

}
