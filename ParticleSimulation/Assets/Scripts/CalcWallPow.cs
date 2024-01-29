using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalcWallPow : MonoBehaviour
{
    public string name;
    Vector3 Fp;
    int count, countE;
    float dt;

    private void OnCollisionEnter(Collision collision)
    {
        Fp += collision.impulse;
    }

    private void OnCollisionStay(Collision collision)
    {
        Fp += collision.impulse;
    }
    private void FixedUpdate()
    {
        Fp = new Vector3(0, 0, 0);
        dt = Time.deltaTime;
    }

    public void Log()
    {
        Debug.Log(name + Fp / dt);
    }
}
