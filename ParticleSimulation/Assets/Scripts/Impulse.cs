using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Impulse : MonoBehaviour
{
    public Vector3 Pow;
    public bool Add;
    Rigidbody rb;

    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Add)
        {
            Add = false;
            rb.AddForce(Pow, ForceMode.Impulse);
        }
    }
}
