using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puerta : MonoBehaviour
{
    List<GameObject> colliders = new List<GameObject>();
    [SerializeField]Transform puerta;
    float escala = 1;
    [SerializeField] AudioSource sonidoPuerta;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int target = colliders.Count > 0 ? 0 : 1;
        escala = Mathf.MoveTowards(escala, target, Time.deltaTime * 10);
        puerta.localScale = new Vector3(escala, 1, 1);



    }

    private void OnTriggerEnter(Collider other)
    {
        if (LayerMask.LayerToName(other.gameObject.layer).Contains("Interaccion") && !colliders.Contains(other.gameObject))
        {
            colliders.Add(other.gameObject);
            if (colliders.Count == 1)
                sonidoPuerta.Play();

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (LayerMask.LayerToName(other.gameObject.layer).Contains("Interaccion") && colliders.Contains(other.gameObject))
        {
            colliders.Remove(other.gameObject);
            if (colliders.Count == 0)
                sonidoPuerta.Play();
        }
    }
    
    public GameObject puerta_()
    {
        return puerta.gameObject;
    }

}
