using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MinijuegoCarga : MonoBehaviour
{
    public TMP_Text textoGolpes;
    int golpes = 0;
    Image thisIM;
    List<Sprite> imagenes = new List<Sprite>();
    [SerializeField, Range(1, 5)] float escala = 1;
    // Start is called before the first frame update
    void Start()
    {
        thisIM = this.GetComponent<Image>();

        //Hacer que carge las imágenes de resources
        //Hacer varios sets de imágenes, cada carpeta que se llame Set1, Set2, etc


        imagenes.AddRange(Resources.LoadAll<Sprite>("Imagenes/Load1"));
        thisIM.sprite = imagenes[golpes % 3];
        thisIM.rectTransform.localScale = new Vector3(thisIM.sprite.rect.width / thisIM.sprite.rect.height, 1, 1) * escala;

    }

    // Update is called once per frame
    void Update()
    {
        if (GetButtonDown("Interactuar"))
        {
            golpes += 1;
            if (golpes % 3 == 0)
                textoGolpes.text = (golpes / 3).ToString();

            thisIM.sprite = imagenes[golpes % imagenes.Count];
            thisIM.rectTransform.localScale = new Vector3(thisIM.sprite.rect.width / thisIM.sprite.rect.height, 1, 1) * escala;
        }
    }
    public bool GetButtonDown(string st)
    {

        return Keyboard.current.eKey.wasPressedThisFrame || (Gamepad.all.Count > 0 && Gamepad.current.buttonWest.wasPressedThisFrame);

    }
}
