using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeParticleColor : MonoBehaviour
{
    void Update()
    {
        if(this.transform.position.z < 0)
        {
            this.GetComponent<Renderer>().material.color = Color.cyan;
        } else
        {
            this.GetComponent<Renderer>().material.color = Color.blue;
        }
    }
}
