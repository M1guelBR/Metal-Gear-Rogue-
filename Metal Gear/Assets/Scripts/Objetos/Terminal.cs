using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terminal : MonoBehaviour
{
    public bool activado = false;
    [SerializeField] GameObject tapadera, radar;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void Activar()
    {
        activado = true;
    }

    private void OnTriggerStay(Collider other)
    {

        if (other.tag.Contains("JugCol"))
        {
            tapadera.SetActive(false);
            if (activado)
                other.transform.parent.parent.GetComponent<Snake>().SetRadar(radar, tapadera.transform.position.y + .05f);
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag.Contains("JugCol"))
        {
            tapadera.SetActive(true);
            other.transform.parent.parent.GetComponent<Snake>().SetRadar(null);
        }
    }


}
