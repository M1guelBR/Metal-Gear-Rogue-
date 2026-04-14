using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SonidoAmbiente : MonoBehaviour
{
    [SerializeField] AudioSource sonidoAmbiente;
    int volumenTarget = 0;
    
    // Update is called once per frame
    void Update()
    {
        sonidoAmbiente.volume = Mathf.MoveTowards(sonidoAmbiente.volume, volumenTarget, 3*Time.deltaTime);
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag.Contains("JugCol"))
            volumenTarget = 1;
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Contains("JugCol"))
            volumenTarget = 0;
    }
}
