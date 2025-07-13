using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TechoChapuza : MonoBehaviour
{
    List<GameObject> collidersTecho = new List<GameObject>();
    [SerializeField] CharacterController controller;

    // Update is called once per frame
    void Update()
    {
        controller.stepOffset = collidersTecho.Count > 0 ? 0 : .1f;   
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("EscenarioColliders") && !collidersTecho.Contains(other.gameObject))
        {
            collidersTecho.Add(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("EscenarioColliders") && collidersTecho.Contains(other.gameObject))
        {
            collidersTecho.Remove(other.gameObject);
        }

    }
}
