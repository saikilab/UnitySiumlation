using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallReceivePow : MonoBehaviour
{
    public static Vector3 Fp;

    private void OnCollisionStay(Collision collision)
    {
        Fp += collision.impulse;
    }
}
