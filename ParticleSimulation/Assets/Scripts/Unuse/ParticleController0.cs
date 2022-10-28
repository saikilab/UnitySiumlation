using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController0 : MonoBehaviour
{
    public float force;
    public float repulsion;

    private Vector3 brown;
    private Rigidbody particle;
    private float x, y, z;

    private GameObject nearParticle;
    private float dist, nearDist, x2, y2, z2;
    private Vector3 interactive;

    private void Start()
    {
        x = 0;
        y = 0;
        z = 0;
        x2 = 0;
        y2 = 0;
        z2 = 0;
        particle = this.transform.GetComponent<Rigidbody>();
    }

    private void Update()
    {
        Interactive();
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

    private void Interactive()
    {
        nearDist = 0;
        dist = 0;

        foreach(GameObject nowParticle in GameObject.FindGameObjectsWithTag("particle"))
        {
            dist = Vector3.Distance(nowParticle.transform.position, this.transform.position);
            if (dist != 0 && (nearDist == 0.0f ||  nearDist > dist))
            {
                nearDist = dist;
                nearParticle = nowParticle;
            }
        }

        //粒子が複数の場合実行
        if(nearDist != 0)
        {
            x2 = (nearParticle.transform.position.x - this.transform.position.x) * repulsion / nearDist;
            y2 = (nearParticle.transform.position.y - this.transform.position.y) * repulsion / nearDist;
            z2 = (nearParticle.transform.position.z - this.transform.position.z) * repulsion / nearDist;
            // 斥力なので負
            interactive = new Vector3(-x2, -y2, -z2);
            particle.AddForce(interactive);
        }
    }
}
