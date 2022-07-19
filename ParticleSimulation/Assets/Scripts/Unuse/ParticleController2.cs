using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController2 : MonoBehaviour
{
    //全粒子から斥力を計算（重い？）
    public float force;
    public float repulsion;

    private Vector3 brown;
    private Rigidbody particle;
    private float x, y, z;

    private float dist, x2, y2, z2;
    private Vector3 interactive;

    private int particleNumber;

    private void Start()
    {
        x = 0;
        y = 0;
        z = 0;
        x2 = 0;
        y2 = 0;
        z2 = 0;
        particleNumber = 0;
        particle = this.transform.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        AllInteractive();
        Noise();
    }

    private void Noise()
    {
        x = Random.Range(-1.0f, 1.0f) * force;
        y = Random.Range(-1.0f, 1.0f) * force;
        z = Random.Range(-1.0f, 1.0f) * force;
        brown = new Vector3(x, y, z);
        particle.AddForce(brown);
    }

    private void AllInteractive()
    {
        dist = 0;
        particleNumber = 0;
        x2 = 0;
        y2 = 0;
        z2 = 0;

        foreach (GameObject nowParticle in GameObject.FindGameObjectsWithTag("particle"))
        {
            particleNumber++;
            dist = Vector3.Distance(nowParticle.transform.position, this.transform.position);
            if (dist != 0)
            {
                x2 = x2 + ((nowParticle.transform.position.x - this.transform.position.x) / dist);
                y2 = y2 + ((nowParticle.transform.position.y - this.transform.position.y) / dist);
                z2 = z2 + ((nowParticle.transform.position.z - this.transform.position.z) / dist);
            }
        }

        //１粒子との比較のため、粒子数に応じて力を減少
        x2 = x2 * repulsion / particleNumber;
        y2 = y2 * repulsion / particleNumber;
        z2 = z2 * repulsion / particleNumber;

        // 斥力なので負
        interactive = new Vector3(-x2, -y2, -z2);
        particle.AddForce(interactive);
    }
}
