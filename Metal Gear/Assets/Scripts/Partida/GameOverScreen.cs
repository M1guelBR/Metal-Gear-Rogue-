using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameOverScreen : MonoBehaviour
{
    public TMP_Text textoLeaderBoard;
    public void Return()
    {
        FindObjectOfType<GameManager>().LoadScene("MainMenu");
    }
    public void Retry()
    {
        FindObjectOfType<GameManager>().ReloadScene();
    }

    public void SetPoints(bool activo, int tiempoMision = 0, int cantAlertas = 0, int cantMuertes = 0, bool sinPistolas = false)
    {
        if (!activo)
        {
            textoLeaderBoard.transform.parent.gameObject.SetActive(false);
            return;
        }
        textoLeaderBoard.text = tiempoMision.ToString() + "\n" + cantAlertas.ToString() + "\n" + cantMuertes + "\n" + (sinPistolas ? "NO" : "YES");

        int puntos = 100;
        if (!sinPistolas)
            puntos -= 15;
        puntos -= cantMuertes * 2;
        puntos -= cantAlertas * 3;
        puntos = Mathf.Max(puntos, 0);

        string rango = "S";
        if (puntos <= 20)
            rango = "D";
        else if (puntos <= 40)
            rango = "C";
        else if (puntos <= 60)
            rango = "B";
        else if(puntos <= 80)
            rango = "A";

        textoLeaderBoard.text += "\n" + rango;

    }
}
