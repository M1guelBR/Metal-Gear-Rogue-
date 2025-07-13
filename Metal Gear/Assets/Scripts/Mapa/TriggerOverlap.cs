using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerOverlap : MonoBehaviour
{

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("TriggerOverlap") && FindObjectOfType<RoomBuilder>() != null)
            FindObjectOfType<RoomBuilder>().overlap_();
    }

}
