using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CargaEscenaReal : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

        CargaMedioScript cargaMedio = FindObjectOfType<CargaMedioScript>();
        if (cargaMedio == null)
        {
            print("No se ha podido cargar");
            return;
        }

        SceneManager.LoadSceneAsync(cargaMedio.escena);
        Destroy(cargaMedio.gameObject, 0);
    }
}
