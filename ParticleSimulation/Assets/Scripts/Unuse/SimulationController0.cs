using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationController0 : MonoBehaviour
{
    public GameObject[] ParticlePrefab;
    public Transform ParticleBox;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GameObject particle = Instantiate(ParticlePrefab[0]);
            particle.transform.SetParent(ParticleBox);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GameObject particle = Instantiate(ParticlePrefab[1]);
            particle.transform.SetParent(ParticleBox);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            GameObject particle = Instantiate(ParticlePrefab[2]);
            particle.transform.SetParent(ParticleBox);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            GameObject particle = Instantiate(ParticlePrefab[3]);
            particle.transform.SetParent(ParticleBox);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            GameObject particle = Instantiate(ParticlePrefab[4]);
            particle.transform.SetParent(ParticleBox);
        }
    }
}
