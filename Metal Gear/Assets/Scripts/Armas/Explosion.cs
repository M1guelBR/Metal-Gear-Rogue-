using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    float tiempo = 0;
    float scale = 1;
    [Range(1, 5), SerializeField] float multEsc = 1;
    // Start is called before the first frame update
    private void Start()
    {
        this.GetComponent<AudioSource>().pitch = Random.Range(.9f, 1.1f);
        scale = transform.localScale.x;
    }
    // Update is called once per frame
    void Update()
    {
        if (tiempo < 1)
        {
            tiempo += Time.deltaTime;
            transform.localScale = new Vector3(1, 1, 1) * multEsc * scale * aux(tiempo);
        }
        else
            DestroyImmediate(this.gameObject);
    }

    float aux(float t)
    {
        t = Mathf.Clamp01(t);
        float out_ = 0;
        if (t < 0.25f)
            out_ = 4 * t;
        else if (t < 0.75f)
            out_ = 1;
        else
            out_ = (1 - t) * 4;


        return out_;
    }
}
