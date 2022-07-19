using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticForceController2 : MonoBehaviour
{
    //磁性粒子
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

        float ax, ay, az, bx, by, bz;

        foreach (GameObject nowParticle in GameObject.FindGameObjectsWithTag("M"))
        {
            particleNumber++;

            ax = this.transform.position.x;
            ay = this.transform.position.y;
            az = this.transform.position.z;
            bx = nowParticle.transform.position.x;
            by = nowParticle.transform.position.y;
            bz = nowParticle.transform.position.z;

            Vector3 thisDirection = this.transform.forward; //(0, 0, 1) z軸方向
            Vector3 thatDirection = nowParticle.transform.forward; //(0, 0, 1)
            Vector3 Vector = new Vector3(bx - ax, by - ay, bz - az);

            dist = Vector3.Distance(nowParticle.transform.position, this.transform.position);

            if (dist != 0)
            {
                //cos(angle)

                float thisSize = Mathf.Sqrt(thisDirection.x * thisDirection.x + thisDirection.y * thisDirection.y + thisDirection.z + thisDirection.z);
                float thatSize = Mathf.Sqrt(thatDirection.x * thatDirection.x + thatDirection.y * thatDirection.y + thatDirection.z + thatDirection.z);
                float VectorSize = Mathf.Sqrt(Vector.x * Vector.x + Vector.y * Vector.y + Vector.z * Vector.z);
                float thisAngle = (thisDirection.x * Vector.x + thisDirection.y * Vector.y + thisDirection.z * Vector.z) / (thisSize * VectorSize);
                float thatAngle = (thatDirection.x * -Vector.x + thatDirection.y * -Vector.y + thatDirection.z * -Vector.z) / (thatSize * VectorSize);

                if ((0 < thisAngle && 0 < thatAngle) || (0 > thisAngle && 0 > thisAngle))
                {
                    //斥力
                    pole = -1;
                } else
                {
                    //引力
                    pole = 1;
                }

                x2 = x2 + pole * Vector.x / (dist * dist);
                y2 = y2 + pole * Vector.y / (dist * dist);
                z2 = z2 + pole * Vector.z / (dist * dist);
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
