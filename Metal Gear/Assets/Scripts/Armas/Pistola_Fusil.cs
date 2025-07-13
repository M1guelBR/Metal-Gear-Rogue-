using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Pistola_Fusil : Arma
{

    [Range(0, 2)] public int tipoArma;

    //BALAS POR SEGUNDO
    [Range(0, 20)] public float cadencia; //B/S * (60S/1Min)

    //DAÑO
    [Range(0, 20)] public float daño;
    public bool tranq;

    //BALAS POR CARGADOR
    [Range(0, 60)] public int balas;
    [Range(0, 300)] public int maxCargador;

    //NUMERO DE RONDAS
    [Range(1, 5)] public int rondas;
    [Range(0, 2)] public float tiempoRondas;
    public bool automatica;

    //PARTICULAS Y SONIDO
    public Vector3 posPart;
    public bool esDef;


    public override int TipoObjeto()
    {
        return tipoArma+1;
    }
    public override int balasArma()
    {
        return balas;
    }
    public override int cargador()
    {
        return maxCargador;
    }
    public override Vector3 posParticulas()
    {
        return posPart;
    }

    public override Pistola_Fusil pistolaFusil()
    {
        return this;
    }

    public override Vector3? Disparar(Vector3 posicion, Vector3 direccion, Vector3 up, Transform disparador, bool esJug)
    {

        RaycastHit[] rayoDisparo;

        float cantidadRandom = 0;
        if (esJug)
        {
            bool agach = disparador.GetComponent<Snake>().agach; bool arrast = disparador.GetComponent<Snake>().arrast;
            cantidadRandom = (1 - cadencia) * .05f * (agach ? 0.5f : 1) * (arrast ? 0 : 1);
        }
        else
        {
            bool agach = disparador.GetComponent<Soldier>().Agachado();
            cantidadRandom = (1 - cadencia) * .05f * (agach ? 0.5f : 1);

        }
        //print(cantidadRandom);
        cantidadRandom *= 0.35f;
        Vector3 randR = Vector3.Cross(direccion, up).normalized * cantidadRandom * Random.Range(-1.0f, 1.0f);
        Vector3 randU = up * cantidadRandom * Random.Range(-1.0f, 1.0f);
        LayerMask layers = (1 << LayerMask.NameToLayer("EscenarioColliders")) | (1 << LayerMask.NameToLayer("ColliderDisparo")) | (1 << LayerMask.NameToLayer("Explosivo"));

        rayoDisparo = Physics.RaycastAll(posicion, direccion + randR + randU, Mathf.Infinity, layers);
        List<RaycastHit> rayos = new List<RaycastHit>();

        //Ordena los rayos por cercania
        for(int i = 0; i < rayoDisparo.Length; i++)
        {
            int k = 0;
            for(int j = 0; j < rayos.Count; j++)
            {
                if (Vector3.Distance(rayoDisparo[i].point, posicion) <= Vector3.Distance(rayos[j].point, posicion))
                {
                    break;
                }
                k += 1;
            }
            rayos.Insert(k, rayoDisparo[i]);
            //rayos = insertaEn(k, rayos, rayoDisparo[i]);
        }
        //Debug.Log(rayos.Count);
        //Debug.Log(rayoDisparo.Length);

        for (int i = 0; i < rayos.Count; i++)
        {
            if (rayos[i].collider.gameObject.layer == 6)
            {
                return rayos[i].point;
            }

            if(rayos[i].collider.GetComponent<ColliderDisparo>())
            {
                ColliderDisparo colDisp = rayos[i].collider.GetComponent<ColliderDisparo>();
                if (colDisp.objetoPadre == disparador)
                    continue;

                if (colDisp.objetoPadre.GetComponent<Soldier>() && colDisp.objetoPadre.GetComponent<Soldier>().EstaVivo())
                {
                    if (colDisp.dañoDisp != 100)
                    {
                        colDisp.objetoPadre.GetComponent<Soldier>().QuitaVida(colDisp.dañoDisp * daño / 15);
                        colDisp.objetoPadre.GetComponent<Soldier>().BajaOxigeno(-100);
                    }

                    else
                        colDisp.objetoPadre.GetComponent<Soldier>().QuitaVida(100);

                    if (!colDisp.objetoPadre.GetComponent<Soldier>().EstaVivo())
                    {
                        colDisp.objetoPadre.GetComponent<Soldier>().ThrowDirection(colDisp.transform.forward * 0.1f, FindObjectOfType<Snake>());
                    }
                    else if (esJug)
                    {
                        colDisp.objetoPadre.GetComponent<Soldier>().BuscaJug(disparador.GetComponent<Snake>());
                    }
                    
                }
                else if (colDisp.objetoPadre.GetComponent<Snake>() && colDisp.objetoPadre.GetComponent<Snake>().EstaVivo())
                {
                    colDisp.objetoPadre.GetComponent<Snake>().QuitaVida(colDisp.dañoDisp * daño / 15);
                }
                return rayos[i].point;
            }

            else if(rayos[i].collider.GetComponent<Explosivo>())
            {
                Explosivo explosivo = rayos[i].collider.GetComponent<Explosivo>();
                explosivo.IniciaExplotar();
                return rayos[i].point;
            }

        }
        return posicion + 100 * direccion;
    }
    public override AudioClip sonido()
    {
        return sonidoDisparo;
    }
    public override float Cadencia()
    {
        return cadencia;
    }

    List<RaycastHit> insertaEn(int k, List<RaycastHit> lista, RaycastHit hit)
    {
        List<RaycastHit> ret = new List<RaycastHit>();
        if (k > lista.Count)
        {
            ret = lista;
            ret.Add(hit);
        }
        else
        {

            for (int i = 0; i < k; i++)
            {
                ret.Add(lista[i]);
            }
            ret.Add(hit);
            for(int i = k; i < lista.Count; i++)
            {
                ret.Add(lista[i]);
            }

        }

        return ret;
    }
}
