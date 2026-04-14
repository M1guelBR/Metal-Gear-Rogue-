using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NieveSetTr : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Vector3 parentScale = transform.parent.localScale;
        ParticleSystem particleSystem = this.GetComponent<ParticleSystem>();
        var shape = particleSystem.shape;
        shape.position = Vector3.Scale(shape.position, parentScale);
        shape.scale = Vector3.Scale(shape.scale, parentScale);

        var mainSpeed = particleSystem.main;
        mainSpeed.startSpeed = parentScale.y;
    }

}
