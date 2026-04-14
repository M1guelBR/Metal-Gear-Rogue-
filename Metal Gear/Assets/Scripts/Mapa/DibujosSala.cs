using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class DibujosSala : MonoBehaviour
{
    Sala this_;
    [SerializeField]bool dibujaEnUpdate = false;
    public void Start()
    {
        this_ = this.GetComponent<Sala>();
    }
    public void Update()
    {
        if (this_ == null)
            this_ = this.GetComponent<Sala>();

        if (!dibujaEnUpdate)
            return;



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
            List<Vector3[]> aristas = this_.getAristas(i);
            List<Vector3[]> caras = this_.getCaras(i);

            foreach (Vector3[] arista in aristas)
                Debug.DrawLine(arista[0], arista[1], Color.green);

        }

    }

    private void OnDrawGizmosSelected()
    {
        if (dibujaEnUpdate)
            return;

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
            List<Vector3[]> aristas = this_.getAristas(i);
            List<Vector3[]> caras = this_.getCaras(i);

            foreach (Vector3[] arista in aristas)
                Debug.DrawLine(arista[0], arista[1], Color.green);

        }

        Gizmos.color = Color.yellow;
        //DIBUJAMOS EL AABB
        Gizmos.DrawCube(this_.salaCentro, this_.anchors*2);
    }

}
