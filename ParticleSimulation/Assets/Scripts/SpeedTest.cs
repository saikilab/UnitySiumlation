using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedTest : MonoBehaviour
{
    Rigidbody rb;
    
    private void Start()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    private void OnCollisionStay(Collision collision)
    {
        Debug.Log(collision.impulse / Time.deltaTime);
    }
}
