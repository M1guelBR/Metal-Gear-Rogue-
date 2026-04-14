using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class MultiplayerHandler : MonoBehaviour
{
    public int cantJugadores = 0;
    public TMP_Text textoJugs;
    [SerializeField] MultiplayerRepresentator[] representators;

    public void OnJoin()
    {
        cantJugadores += 1;
        textoJugs.text = cantJugadores.ToString();
    }
    public void OnDisconnect()
    {
        cantJugadores -= 1;
        textoJugs.text = cantJugadores.ToString();
        representators[cantJugadores].gameObject.SetActive(false);
    }
    public void DisconnectAll()
    {
        MultipAux[] multiplObjs = FindObjectsOfType<MultipAux>();
        for(int i = 0; i < multiplObjs.Length; i++)
        {
            Destroy(multiplObjs[i].gameObject, 0);
            OnDisconnect();
        }
        this.GetComponent<PlayerInputManager>().EnableJoining();
    }

    public void AsignaJugador(MultipAux jug)
    {
        representators[cantJugadores-1].gameObject.SetActive(true);
        representators[cantJugadores-1].aux = jug.GetComponent<PlayerInput>();
    }

    public void Empieza()
    {
        if(cantJugadores > 1)
        {
            FindObjectOfType<MainMenu>().AbrirEscena("SinglePlayerMode");
            this.gameObject.SetActive(false);

        }
    }

}
