using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilmPresser : MonoBehaviour
{
    public float PW;
    Rigidbody rb;
    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            rb.AddForce(PW, 0, 0);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rb.AddForce(-PW, 0, 0);
        }
    }
}
