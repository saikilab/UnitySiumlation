using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallReceivePow2: MonoBehaviour
{
    public static Vector3 Fp;

    private void OnCollisionStay(Collision collision)
    {
        Fp += collision.impulse;
    }
    private void FixedUpdate()
    {
        Debug.Log(Fp / Time.deltaTime);
        Fp = new Vector3(0,0,0);
    }
}
