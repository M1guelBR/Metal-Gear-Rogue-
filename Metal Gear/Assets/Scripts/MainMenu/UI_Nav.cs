using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Nav : MonoBehaviour
{
    EventSystem eventSystem;
    public enum Modo
    {
        Boton,
        Toggle
    };
    public Modo modo = Modo.Boton;
    public Button buttonDef;
    public Toggle toggleDef;
    [SerializeField] Button volverButton;
    [SerializeField] bool autoSelect = false;
    // Start is called before the first frame update
    void Start()
    {
        eventSystem = FindObjectOfType<EventSystem>();
        if (autoSelect)
            SelectDef();
    }

    // Update is called once per frame
    void Update()
    {

        bool gamepad = false;
        if (Gamepad.all.Count > 0)
            gamepad = Gamepad.current.IsActuated();
        if (eventSystem.currentSelectedGameObject == null && (Keyboard.current.IsActuated() || gamepad))
        {

            if (modo == Modo.Boton)
                buttonDef.Select();
            if (modo == Modo.Toggle)
                toggleDef.Select();

        }
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
            Volver();

    }
    public void Volver()
    {
        if (volverButton == null)
            return;
        volverButton.onClick.Invoke();
    }
    public void SetVolverButton(Button volver)
    {
        volverButton = volver;
    }

    public void SetDefaultButton(GameObject def)
    {


        if (def.GetComponent<Button>())
        {
            buttonDef = def.GetComponent<Button>();
            toggleDef = null;
            modo = Modo.Boton;
        }
        else if (def.GetComponent<Toggle>())
        {
            toggleDef = def.GetComponent<Toggle>();
            buttonDef = null;
            modo = Modo.Toggle;
        }



    }

    public void SelectNone()
    {
        eventSystem.SetSelectedGameObject(null);
    }
    public void SelectDef()
    {
        if (modo == Modo.Boton)
            buttonDef.Select();

        else
            toggleDef.Select();
    }
}
