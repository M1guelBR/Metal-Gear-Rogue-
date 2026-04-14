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

    int planta = 0;

    public Vector3 anchors = Vector3.zero;
    public Vector3 salaCentro = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        SacaAABB();
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
        Vector3 up = transform.rotation * Vector3.up; up = up.normalized * transform.localScale.y;
        Vector3 right = transform.rotation * Vector3.right; right = right.normalized * transform.localScale.x;
        Vector3 forward = transform.rotation * Vector3.forward; forward = forward.normalized * transform.localScale.z;
        i = i % centro.Length;
        Vector3[] vertices = new Vector3[8];


        vertices[0] = 0.5f * ((up * alto[i]) + (right * ancho[i]) + (forward * largo[i]));
        vertices[1] = 0.5f * (-(up * alto[i]) + (right * ancho[i]) + (forward * largo[i]));
        vertices[2] = .5f * (-(up * alto[i]) - (right * ancho[i]) + (forward * largo[i]));
        vertices[3] = .5f * (-(up * alto[i]) - (right * ancho[i]) - (forward * largo[i]));
        vertices[4] = .5f * ((up * alto[i]) - (right * ancho[i]) + (forward * largo[i]));
        vertices[5] = .5f * ((up * alto[i]) - (right * ancho[i]) - (forward * largo[i]));
        vertices[6] = .5f * ((up * alto[i]) + (right * ancho[i]) - (forward * largo[i]));
        vertices[7] = .5f * (-(up * alto[i]) + (right * ancho[i]) - (forward * largo[i]));

        Vector3 add = getCentro(i);
        for (int j = 0; j < vertices.Length; j++)
        {
            vertices[j] = add + vertices[j];
        }
        return vertices;
    }

    public Vector3 getCentro(int i)
    {

        i = i % centro.Length;
        Vector3 add = Vector3.Scale(centro[i], transform.localScale);
        add = (transform.right * add.x) + (transform.forward * add.z) + (transform.up * add.y);
        return add + transform.position;
    }

    //Las aristas tienen formato {punto, punto}
    public List<Vector3[]> getAristas(int i)
    {
        List<Vector3[]> aristas = new List<Vector3[]>();
        i = i % centro.Length;

        Vector3[] vertices = getVertices(i);

        aristas.Add(new Vector3[] { vertices[0], vertices[1] });
        aristas.Add(new Vector3[] { vertices[1], vertices[2] });//Debug.DrawLine(vertices[1], vertices[2], Color.green);
        aristas.Add(new Vector3[] { vertices[2], vertices[3] });//Debug.DrawLine(vertices[2], vertices[3], Color.green);
        aristas.Add(new Vector3[] { vertices[3], vertices[5] });//Debug.DrawLine(vertices[3], vertices[5], Color.green);
        aristas.Add(new Vector3[] { vertices[4], vertices[5] });//Debug.DrawLine(vertices[4], vertices[5], Color.green);
        aristas.Add(new Vector3[] { vertices[4], vertices[2] });//Debug.DrawLine(vertices[4], vertices[2], Color.green);
        aristas.Add(new Vector3[] { vertices[0], vertices[4] });//Debug.DrawLine(vertices[0], vertices[4], Color.green);
        aristas.Add(new Vector3[] { vertices[0], vertices[6] });//Debug.DrawLine(vertices[0], vertices[6], Color.green);
        aristas.Add(new Vector3[] { vertices[6], vertices[7] });//Debug.DrawLine(vertices[6], vertices[7], Color.green);
        aristas.Add(new Vector3[] { vertices[3], vertices[7] });//Debug.DrawLine(vertices[3], vertices[7], Color.green);
        aristas.Add(new Vector3[] { vertices[1], vertices[7] });//Debug.DrawLine(vertices[1], vertices[7], Color.green);
        aristas.Add(new Vector3[] { vertices[5], vertices[6] });//Debug.DrawLine(vertices[5], vertices[6], Color.green);


        return aristas;
    }

    //Las caras tienen formato {punto, normal}
    public List<Vector3[]> getCaras(int i)
    {
        i = i % centro.Length;
        List<Vector3[]> caras = new List<Vector3[]>();

        Vector3[] ejes = { transform.right, transform.up, transform.forward };
        for(int k = 0; k < ejes.Length; k++)
        {
            Vector3 normal = ejes[k];
            Vector3 offset = ejes[k] * sacaEscala(k, i) * 0.5f;

            //Ponemos la cara de delante
            caras.Add(new Vector3[] { getCentro(i) + offset, normal});
            //Y la de detras
            caras.Add(new Vector3[] { getCentro(i) - offset, -normal });

        }

        return caras;

    }

    public float sacaEscala(int i, int index)
    {
        float escala = ancho[index] * transform.localScale.x;
        if (i == 1)
            escala = alto[index] * transform.localScale.y;
        else if (i == 2)
            escala = largo[index] * transform.localScale.z;
        return escala;
    }

    public bool puntoEnCubo(Vector3 punto, int i)
    {
        i = i % centro.Length;
        Vector3 distancia = getCentro(i) - punto;
        float x = Mathf.Abs(Vector3.Dot(distancia, transform.right));
        float y = Mathf.Abs(Vector3.Dot(distancia, transform.up));
        float z = Mathf.Abs(Vector3.Dot(distancia, transform.forward));

        Vector3 anchor = getAnchor(i);
        if (x < anchor.x + 0.001f&& y < anchor.y + 0.001f && z < anchor.z + 0.001f)
            return true;

        return false;
    }

    public Vector3 getAnchor(int i)
    {
        i = i % centro.Length;
        Vector3 anchor = new Vector3(ancho[i], alto[i], largo[i]);
        anchor = Vector3.Scale(anchor, transform.localScale);
        anchor.x = Mathf.Abs(anchor.x); anchor.y = Mathf.Abs(anchor.y); anchor.z = Mathf.Abs(anchor.z);
        anchor /= 2;

        return anchor;
    }


    public bool puntosEnCubo(List<Vector3> puntos, int i)
    {
        foreach(Vector3 punto in puntos)
        {
            if (puntoEnCubo(punto, i))
                return true;
        }
        return false;
    }


    void SacaAABB()
    {
        Vector3 maxCoords = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity); ;
        Vector3 minCoords = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
        for(int i = 0; i < centro.Length; i++)
        {
            Vector3[] vertices = getVertices(i);
            for(int v = 0; v < vertices.Length; v++)
            {
                maxCoords = new Vector3(Mathf.Max(maxCoords.x, vertices[v].x), Mathf.Max(maxCoords.y, vertices[v].y), Mathf.Max(maxCoords.z, vertices[v].z));
                minCoords = new Vector3(Mathf.Min(minCoords.x, vertices[v].x), Mathf.Min(minCoords.y, vertices[v].y), Mathf.Min(minCoords.z, vertices[v].z));
            }

        }

        if(minCoords.x != Mathf.Infinity)
        {
            salaCentro = (maxCoords + minCoords) / 2;
            anchors = maxCoords - salaCentro;
        }

    }

    public bool TieneJugador(Vector3 jugPos)
    {
        Vector3 distASala = salaCentro - jugPos;
        return (Mathf.Abs(distASala.x) < anchors.x) && (Mathf.Abs(distASala.y) < anchors.y) && (Mathf.Abs(distASala.z) < anchors.z);
    }
}

