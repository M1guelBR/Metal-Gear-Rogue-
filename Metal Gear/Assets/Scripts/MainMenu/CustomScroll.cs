using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CustomScroll : MonoBehaviour
{

    [SerializeField] float scrollAm = 0;
    [SerializeField] float scrollTop = 500;
    [SerializeField] float scrollBottom = 200;
    [SerializeField] EventSystem eventSystem;
    [SerializeField] Slider barra;
    [SerializeField] GameObject topScreen, bottomScreen;
    float length = 1, topDist = 1;
    private void Start()
    {
        eventSystem = FindObjectOfType<EventSystem>();
        //Obtenemos longitud de pantalla
        topDist = Vector3.Dot(topScreen.transform.position - transform.position, transform.up);
        float bottomDist = Vector3.Dot(bottomScreen.transform.position - transform.position, transform.up);
        length = topDist - bottomDist;
    }

    // Update is called once per frame
    void Update()
    {
        float scrollChange = Mouse.current.scroll.ReadValue().y / 120; //Dividimos por 120 que es la unidad
        //print(scrollChange);


        print(length);

        scrollAm += scrollChange * .1f;
        //Si tiene un objeto seleccionado y es de la zona scrolleable, intentamos centrar la vista en el
        if(eventSystem.currentSelectedGameObject != null &&
            eventSystem.currentSelectedGameObject.transform.parent.parent == transform)
        {
            Transform rectChild = eventSystem.currentSelectedGameObject.transform;
            float dist = (topDist - Vector3.Dot(rectChild.position - transform.position, transform.up))/ length;
            scrollAm = Mathf.MoveTowards(scrollAm, 1 - dist, Time.unscaledDeltaTime);

        }
        scrollAm = Mathf.Clamp01(scrollAm);
        RectTransform rect = this.GetComponent<RectTransform>();
        float yPos = (scrollAm * scrollTop) + ((1 - scrollAm) * scrollBottom);
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, yPos);
        barra.value = 1 - scrollAm;

    }

    public void SetValorBarra()
    {
        scrollAm = (1 - barra.value);
    }
}
