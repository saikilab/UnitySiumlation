using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpY : MonoBehaviour
{
    private const float KgCoefficient = 0.00000000000001f;
    private const float MCoefficient = 0.000001f;

    public int SaveStep;
    public static Vector3[] Fp;
    Vector3 nowFp;

    private void Start()
    {
        Fp = new Vector3[SaveStep * 2];
    }

    private void FixedUpdate()
    {
        Fp[SimulationController.Step] = nowFp / Time.deltaTime * (KgCoefficient / MCoefficient);
        nowFp = Vector3.zero;
    }

    private void OnCollisionStay(Collision collision)
    {
        nowFp = collision.impulse;
    }
}
