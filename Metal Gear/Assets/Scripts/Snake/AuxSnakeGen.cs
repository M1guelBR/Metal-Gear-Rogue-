using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AuxSnakeGen : MonoBehaviour
{
    [SerializeField] bool delete = false;
    // Start is called before the first frame update
    void Start()
    {
        Transform[] children = new Transform[transform.childCount];

        for(int i = 0; i < children.Length; i++)
        {
            children[i] = transform.GetChild(i);
        }
        for (int i = 0; i < children.Length; i++)
        {
            children[i].parent = null;
        }
        Destroy(this.gameObject,0);



    }

}
