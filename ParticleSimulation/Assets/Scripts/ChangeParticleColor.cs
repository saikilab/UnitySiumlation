using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeParticleColor : MonoBehaviour
{
    public bool x, y, z;
    private Renderer R;

    private void Start()
    {
        R = this.GetComponent<Renderer>();
    }

    void Update()
    {
        if (x)
        {
            if (this.transform.position.x > 0.01)
            {
                R.material.color = Color.cyan;
            }
            else
            {
                R.material.color = Color.blue;
            }
        }
        else if (y)
        {
            if (this.transform.position.y > 0.01)
            {
                R.material.color = Color.cyan;
            }
            else
            {
                R.material.color = Color.blue;
            }
        } else if (z)
        {
            if (this.transform.position.z > 0.01)
            {
                R.material.color = Color.cyan;
            }
            else
            {
                R.material.color = Color.blue;
            }
        }
    }
}
