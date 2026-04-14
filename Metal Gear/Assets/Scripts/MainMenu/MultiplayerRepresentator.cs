using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiplayerRepresentator : MonoBehaviour
{
    public PlayerInput aux;
    AudioSource bonk;
    // Start is called before the first frame update
    void Start()
    {
        bonk = this.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (aux.currentActionMap.FindAction("Bonk").WasPressedThisFrame())
        {
            bonk.Stop();
            bonk.pitch = Random.Range(.9f, 1.1f);
            bonk.Play();
        }
    }
}
