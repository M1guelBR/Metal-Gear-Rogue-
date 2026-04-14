using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RutaSoldado : MonoBehaviour
{
    public List<Vector3> posiciones;
    public List<float> angulos;
    public List<float> tiempos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        int m = Mathf.Min(posiciones.Count, angulos.Count, tiempos.Count);
        for (int i = 0; i < m; i++)
        {
            Vector3 posI = GetPosition(i);
            Vector3 posF = GetPosition((i + 1) % m);
            Debug.DrawLine(posI, posF, Color.blue);

            Vector3 rotado = Quaternion.Euler(0, GetAngulo((i + 1)%m), 0) * Vector3.forward;

            Debug.DrawRay(posI, rotado, Color.cyan);
        }


    }
    public Vector3 GetPosition(int index = 0)
    {
        if (posiciones.Count < index)
            return Vector3.zero;
        Vector3 fac = Vector3.Scale(posiciones[index], transform.parent.localScale);
        return transform.position + (transform.right * fac.x) + (transform.up * fac.y) + (transform.forward * fac.z);
    }
    public float GetAngulo(int index = 0)
    {
        if (posiciones.Count < index)
            return 0;
        return angulos[index] + transform.eulerAngles.y;

    }
}
