using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TubeReceivePow : MonoBehaviour
{
    private const float KgCoefficient = 0.00000000000001f;
    private const float MCoefficient = 0.000001f;
    float f;

    private void FixedUpdate()
    {
        Debug.Log(f / Time.deltaTime * KgCoefficient  + " N"); //f= UnityN/step → Nへ変換
        f = 0;
    }

    private void OnCollisionStay(Collision collision)
    {
        f += collision.impulse.magnitude;
        //　↑　単位は？
    }
}
