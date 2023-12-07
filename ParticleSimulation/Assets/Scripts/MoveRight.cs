using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveRight : MonoBehaviour
{
    public Vector3 Pow;
    Rigidbody rb;
    bool isMoving;
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (isMoving)
            rb.AddForce(Pow);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            isMoving = false;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isMoving = true;
        }
    }
}
