using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetHeight : MonoBehaviour
{
    public NewParticleController newParticleController;
    public float thickness;
    public GameObject Z_Wall_front, Z_Wall_back;

    float diameter;

    void Awake()
    {
        diameter = newParticleController.diameter;
        Z_Wall_front.transform.localPosition = new Vector3(0, 0, -(thickness * diameter + Z_Wall_front.transform.localScale.z) / 2f);
        Z_Wall_back.transform.localPosition = new Vector3(0, 0, (thickness * diameter + Z_Wall_back.transform.localScale.z) / 2f);
    }
}
