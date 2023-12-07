using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleCollision : MonoBehaviour
{
    public bool particleCollision;

    private void OnCollisionStay(Collision collision)
    {
        particleCollision = true;
    }

    private void OnCollisionExit(Collision collision)
    {
        particleCollision = false;
    }
}
