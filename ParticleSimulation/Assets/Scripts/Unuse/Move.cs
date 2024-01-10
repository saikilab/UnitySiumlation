using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move : MonoBehaviour
{
    public Vector3 V;
    public bool Force, Impulce, VeloChan, Acceleration, Translate;
    Rigidbody rb;
    float dt;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (Impulce)
            rb.AddForce(V, ForceMode.Impulse);
    }

    private void FixedUpdate()
    {
        dt = Time.deltaTime;

        if (Force)
        {
            rb.AddForce(V);
            rb.AddForce(-rb.velocity);
        }
        else if (Translate)
            transform.Translate(V * dt);
        else if (VeloChan)
            rb.AddForce(V/dt, ForceMode.VelocityChange);
        else if (Acceleration)
            rb.AddForce(V, ForceMode.Acceleration);
    }
}
