using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultipAux : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        if (FindObjectOfType<MultiplayerHandler>())
        {
            FindObjectOfType<MultiplayerHandler>().AsignaJugador(this);
        }
    }
}
