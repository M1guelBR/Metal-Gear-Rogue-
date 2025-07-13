using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sala : MonoBehaviour
{
    public Vector3[] puertasPosiciones = new Vector3[0];
    public List<Vector3> puertasFuera = new List<Vector3>();

    public List<float> ancho = new List<float>{ 1 };
    public List<float> largo = new List<float> { 1 };
    public List<float> alto = new List<float> { 1 };
    public Vector3[] centro;
    public Transform[] pasillos;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnValidate()
    {

        
        if (puertasFuera.Count < puertasPosiciones.Length)
        {

            for (int i = puertasFuera.Count; i < puertasPosiciones.Length; i++)
                puertasFuera.Add(Vector3.forward);
        }
        else
        {
            int max = puertasFuera.Count;
            for (int i = puertasPosiciones.Length; i < max; i++)
                puertasFuera.RemoveAt(puertasFuera.Count - 1);
        }
    }

    public Vector3[] getVertices(int i)
    {
        i = i % centro.Length;
        Vector3[] vertices = new Vector3[8];
        vertices[0] = 0.5f * ((transform.up * alto[i]) + (transform.right * ancho[i]) + (transform.forward * largo[i]));
        vertices[1] = 0.5f * (-(transform.up * alto[i]) + (transform.right * ancho[i]) + (transform.forward * largo[i]));
        vertices[2] = .5f * (-(transform.up * alto[i]) - (transform.right * ancho[i]) + (transform.forward * largo[i]));
        vertices[3] = .5f * (-(transform.up * alto[i]) - (transform.right * ancho[i]) - (transform.forward * largo[i]));
        vertices[4] = .5f * ((transform.up * alto[i]) - (transform.right * ancho[i]) + (transform.forward * largo[i]));
        vertices[5] = .5f * ((transform.up * alto[i]) - (transform.right * ancho[i]) - (transform.forward * largo[i]));
        vertices[6] = .5f * ((transform.up * alto[i]) + (transform.right * ancho[i]) - (transform.forward * largo[i]));
        vertices[7] = .5f * (-(transform.up * alto[i]) + (transform.right * ancho[i]) - (transform.forward * largo[i]));


        Vector3 add = Vector3.Scale(centro[i], transform.localScale);
        add = (transform.right * add.x) + (transform.forward * add.z) + (transform.up * add.y);
        for (int j = 0; j < vertices.Length; j++)
        {
            vertices[j] = add + transform.position + Vector3.Scale(vertices[j], transform.localScale);
        }
        return vertices;
    }
}

