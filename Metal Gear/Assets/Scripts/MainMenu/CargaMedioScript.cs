using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CargaMedioScript : MonoBehaviour
{
    public string escena = "";
    private void Awake()
    {
        DontDestroyOnLoad(this);
    }
}
