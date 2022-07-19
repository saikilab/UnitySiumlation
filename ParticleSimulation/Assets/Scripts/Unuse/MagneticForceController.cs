using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticForceController : MonoBehaviour
{
    //磁性粒子（電荷）
    public float randomForce;
    public float magneticForce;

    private Vector3 brown;
    private Rigidbody particle;
    private float x, y, z;

    private float dist, x2, y2, z2;
    private Vector3 interactive;

    private int particleNumber;
    private int pole;

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
        x = Random.Range(-1.0f, 1.0f) * randomForce;
        y = Random.Range(-1.0f, 1.0f) * randomForce;
        z = Random.Range(-1.0f, 1.0f) * randomForce;
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
        pole = 1;

        //極の調整
        if(this.tag == "S")
        {
            pole = -1;
        }
        foreach (GameObject nowParticle in GameObject.FindGameObjectsWithTag("N"))
        {
            particleNumber++;
            dist = Vector3.Distance(nowParticle.transform.position, this.transform.position);
            if (dist != 0)
            {
                x2 = x2 - pole * (nowParticle.transform.position.x - this.transform.position.x) / (dist * dist);
                y2 = y2 - pole * (nowParticle.transform.position.y - this.transform.position.y) / (dist * dist);
                z2 = z2 - pole * (nowParticle.transform.position.z - this.transform.position.z) / (dist * dist);
            }
        }
        foreach (GameObject nowParticle in GameObject.FindGameObjectsWithTag("S"))
        {
            particleNumber++;
            dist = Vector3.Distance(nowParticle.transform.position, this.transform.position);
            if (dist != 0)
            {
                x2 = x2 + pole * (nowParticle.transform.position.x - this.transform.position.x) / (dist * dist);
                y2 = y2 + pole * (nowParticle.transform.position.y - this.transform.position.y) / (dist * dist);
                z2 = z2 + pole * (nowParticle.transform.position.z - this.transform.position.z) / (dist * dist);
            }
        }

        //１粒子との比較のため、粒子数に応じて力を減少、距離減衰が強いためmagneticForceを大きくする
        x2 = x2 * magneticForce / particleNumber;
        y2 = y2 * magneticForce / particleNumber;
        z2 = z2 * magneticForce / particleNumber;

        interactive = new Vector3(x2, y2, z2);
        particle.AddForce(interactive);
    }
}
