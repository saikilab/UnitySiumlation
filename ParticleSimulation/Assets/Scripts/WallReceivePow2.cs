using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallReceivePow2: MonoBehaviour
{
    public string name;
    private Vector3 Fp;
    private int count;

    private void OnCollisionStay(Collision collision)
    {
        //Fp += collision.impulse;
        Fp = collision.impulse;
        count++;
        Debug.Log(name + Fp / Time.deltaTime + "Num:" + count);
        Debug.Log("X" + Fp.x);
        Debug.Log("Y" + Fp.y);
        Debug.Log("Z" + Fp.z);
    }
    private void FixedUpdate()
    {
        //Debug.Log(name + Fp / Time.deltaTime + "Num:" + count);
        //Debug.Log("X" + Fp.x);
        //Debug.Log("Y" + Fp.y);
        //Debug.Log("Z" + Fp.z);

        Fp = new Vector3(0,0,0);
        count = 0;
    }
}
