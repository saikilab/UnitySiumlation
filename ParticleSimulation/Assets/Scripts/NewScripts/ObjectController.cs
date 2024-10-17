using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
    public float Pow;
    Vector3 nowPow;
    Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Input.anyKey)
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    nowPow = new Vector3(nowPow.x, nowPow.y, Pow);
                }

                if (Input.GetKey(KeyCode.DownArrow))
                {
                    nowPow = new Vector3(nowPow.x, nowPow.y, -Pow);
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    nowPow = new Vector3(nowPow.x, Pow, nowPow.z);
                }

                if (Input.GetKey(KeyCode.DownArrow))
                {
                    nowPow = new Vector3(nowPow.x, -Pow, nowPow.z);
                }
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                nowPow = new Vector3(Pow, nowPow.y, nowPow.z);
            }

            if (Input.GetKey(KeyCode.LeftArrow))
            {
                nowPow = new Vector3(-Pow, nowPow.y, nowPow.z);
            }
        } else
        {
            nowPow = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        rb.AddForce(nowPow);
    }
}
