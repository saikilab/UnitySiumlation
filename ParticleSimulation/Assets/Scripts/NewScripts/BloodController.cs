using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodController : MonoBehaviour
{
    public Vector3 movePow;
    public float maxSpeed;

    public bool moveBlood;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (moveBlood)
        {
            if(rb.velocity.magnitude < maxSpeed)
                rb.AddForce(movePow);
        }
    }
}
