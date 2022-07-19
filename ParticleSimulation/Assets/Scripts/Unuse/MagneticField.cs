using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagneticField : MonoBehaviour
{
    public SetParticle setParticle;

    //横x縦y奥行きz, S極z+ N極z-
    public GameObject[] MagneticParticle;
    public Rigidbody[] MagneticParticleRB;
    public int particleNumber;
    public float radius; //半径
    public float kai; //磁化定数 100
    public float H_ext; //外部磁場 60
    public float m; //磁荷
    public float k; //定数　k = 1/4πμ0 = 0.8
    public float randomForce;

    private int i, j;
    private Vector3 thisParticlePosition, thatParticlePosition, thisPositionS, thisPositionN, thatPositionS, thatPositionN;
    private Vector3 H; //粒子の作る磁場
    private Vector3[] F; //はたらく力の合計

    private void Start()
    {
        m = kai * H_ext;

        if (MagneticParticle.Length == 0)
        {
            particleNumber = setParticle.MagneticParticle.Length;
            MagneticParticle = new GameObject[particleNumber];
            for (i = 0; i < particleNumber; i++)
            {
                MagneticParticle[i] = setParticle.MagneticParticle[i];
            }
        }
        else
        {
            particleNumber = MagneticParticle.Length;
        }

        MagneticParticleRB = new Rigidbody[particleNumber];
        for (i = 0; i < particleNumber; i++)
        {
            MagneticParticleRB[i] = MagneticParticle[i].GetComponent<Rigidbody>();
        }
    }

    private void Update()
    {
        Noise();
        CalcInteractive();
    }

    private void Noise()
    {
        float x, y, z;
        int n;
        Vector3 brown;

        for(n = 0; n < particleNumber; n++)
        {
            x = Random.Range(-1.0f, 1.0f) * randomForce;
            y = Random.Range(-1.0f, 1.0f) * randomForce;
            z = Random.Range(-1.0f, 1.0f) * randomForce;
            brown = new Vector3(x, y, z);
            MagneticParticleRB[n].AddForce(brown);
        }
    }

    public void CalcInteractive()
    {
        F = new Vector3[particleNumber];
        for (i = 0; i < particleNumber; i++)
        {
            thisParticlePosition = MagneticParticle[i].transform.position;
            thisPositionS = thisParticlePosition;
            thisPositionS.z = thisPositionS.z + radius;
            thisPositionN = thisParticlePosition;
            thisPositionN.z = thisPositionN.z - radius;
            for (j = i + 1; j < particleNumber; j++)
            {
                thatParticlePosition = MagneticParticle[j].transform.position;
                thatPositionS = thatParticlePosition;
                thatPositionS.z = thatPositionS.z + radius;
                thatPositionN = thatParticlePosition;
                thatPositionN.z = thatPositionN.z - radius;

                CalcH(thisPositionN, thatPositionN);
                F[i] -= m*H;
                F[j] += m*H;
                CalcH(thisPositionN, thatPositionS);
                F[i] += m*H;
                F[j] -= m*H;
                CalcH(thisPositionS, thatPositionN);
                F[i] += m*H;
                F[j] -= m*H;
                CalcH(thisPositionS, thatPositionS);
                F[i] -= m*H;
                F[j] += m*H;
            }
            MagneticParticleRB[i].AddForce(F[i]);
        }
    }

    public void CalcH(Vector3 thisVector, Vector3 thatVector)
    {
        Vector3 Vector = thatVector - thisVector;
        float dist = Vector3.Dot(Vector, Vector);

        H = (k * m) / (dist * dist * dist) * Vector;
    }
}
