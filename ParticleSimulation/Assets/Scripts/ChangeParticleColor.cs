using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeParticleColor : MonoBehaviour
{
    private Renderer R;

    private void Start()
    {
        R = this.GetComponent<Renderer>();
    }

    void Update()
    {
        if(this.transform.position.z < 0)
        {
            R.material.color = Color.cyan;
        } else
        {
            R.material.color = Color.blue;
        }
    }
}
