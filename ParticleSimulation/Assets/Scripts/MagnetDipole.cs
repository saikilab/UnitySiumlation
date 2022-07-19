using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MagnetDipole : MonoBehaviour
{
    //Canvas
    public Button ON_MagButton, OFF_MagButton;
    public SetParticle setParticle;
    public GameObject InfoPanel;

    //simulation, particle value
    public GameObject[] MagneticParticle;
    public Rigidbody[] MagneticParticleRB;
    public int particleNumber;
    public float diameter, time, timeScale;

    //noise value
    public float randomForce;
    public float beta; //粘性抵抗
    private Vector3 V; //速度
    private Vector3[] BeforPosition;

    //mag value
    private Vector3[] H, dH; // 磁界 = m/(4πμ0r^2) = 1/mr^2?
    private float u0; //透磁率
    public float H_pow; //外部磁場の強さ
    public float default_H_pow; //外部磁場の強さの初期値 60
    public float kai; //磁化率 100
    private float q; //磁荷　kai * H_pow
    private Vector3 M1, M2; //磁気双極子（ベクトル）
    private float shita_x, shita_y;
    public float ratationSpeed;
    public bool Rotation;

    //Math value
    private float pi;
    private int i, j;

    private void Start()
    {
        if (MagneticParticle.Length == 0)
        {
            particleNumber = setParticle.MagneticParticle.Length;
            MagneticParticle = new GameObject[particleNumber];
            for (i = 0; i < particleNumber; i++)
            {
                MagneticParticle[i] = setParticle.MagneticParticle[i];
            }
        } else
        {
            particleNumber = MagneticParticle.Length;
        }

        pi = Mathf.PI;
        H = new Vector3[particleNumber];
        dH = new Vector3[particleNumber];
        u0 = 0.1f;
        H_pow = default_H_pow;
        q = kai * H_pow;

        MagneticParticleRB = new Rigidbody[particleNumber];
        for (i = 0; i < particleNumber; i++)
        {
            MagneticParticleRB[i] = MagneticParticle[i].GetComponent<Rigidbody>();
        }
        BeforPosition = new Vector3[particleNumber];
        OFFMag();
        shita_y = 30 * pi / 180;
    }

    private void Update()
    {
        time = Time.deltaTime * timeScale;
        Noise();
        Interactive();
        if (Rotation)
            RatationMagneticField();
    }

    private void Noise()
    {
        float x, y, z, sigma=1;
        int n;
        Vector3 brown, nowPosition;
        for (n = 0; n < particleNumber; n++)
        {
            nowPosition = MagneticParticle[n].transform.position;
            V = (nowPosition - BeforPosition[n]) / time;
            x = sigma * Mathf.Sqrt(-2.0f * Mathf.Log(Random.Range(0.00001f, 1.0f))) * Mathf.Cos(2.0f * pi * Random.Range(0f, 1.0f));
            y = sigma * Mathf.Sqrt(-2.0f * Mathf.Log(Random.Range(0.00001f, 1.0f))) * Mathf.Cos(2.0f * pi * Random.Range(0f, 1.0f));
            z = sigma * Mathf.Sqrt(-2.0f * Mathf.Log(Random.Range(0.00001f, 1.0f))) * Mathf.Cos(2.0f * pi * Random.Range(0f, 1.0f));
            x = -beta * V.x + x * randomForce * time;
            y = -beta * V.y + y * randomForce * time;
            z = -beta * V.z + z * randomForce * time;
            brown = new Vector3(x, y, z);
            MagneticParticleRB[n].AddForce(brown);
            BeforPosition[n] = nowPosition;
        }
    }

    public void Interactive()
    {
        Vector3 posVector;
        for (i = 0; i < particleNumber; i++)
        {
            H[i] = new Vector3(0f, 0f, 0f);
            dH[i] = new Vector3(0f, 0f, 0f);
        }
        for (i = 0; i < particleNumber; i++)
        {
            for (j = i + 1; j < particleNumber; j++)
            {
                posVector = MagneticParticle[i].transform.position - MagneticParticle[j].transform.position;
                Vector3 E; //単ベクトル
                float dist = Mathf.Sqrt(Vector3.Dot(posVector, posVector));
                E = posVector / dist;

                dH[i] = (3f * u0 / (4f * pi * Mathf.Pow(dist, 4f))) * (Vector3.Dot(M1, E) * M2
                      + Vector3.Dot(M2, E) * M1 + Vector3.Dot(M1, M2) * E
                      - 5f * Vector3.Dot(M1, E) * Vector3.Dot(M2, E) * E);
                H[i] += dH[i];
                H[j] -= dH[i];
            }
            MagneticParticleRB[i].AddForce(H[i]*time);
        }
    }

    public void ChangeMagneticField()
    {
        q = kai * H_pow;
        M1 = new Vector3(0f, 0f, q * diameter / u0);
        M2 = new Vector3(0f, 0f, q * diameter / u0);
        InfoPanel.GetComponent<GetInfo>().UpdateMagInfo();
    }

    public void ONMag()
    {
        ON_MagButton.interactable = false;
        OFF_MagButton.interactable = true;
        H_pow = default_H_pow;
        ChangeMagneticField();
    }

    public void OFFMag()
    {
        ON_MagButton.interactable = true;
        OFF_MagButton.interactable = false;
        H_pow = 0;
        ChangeMagneticField();
    }

    private void RatationMagneticField()
    {
        float x, y, z;
        shita_x += ratationSpeed * time * pi / 180;
        x = (q * diameter / u0) * Mathf.Sin(shita_y) * Mathf.Cos(shita_x);
        y = (q * diameter / u0) * Mathf.Cos(shita_y);
        z = (q * diameter / u0) * Mathf.Sin(shita_y) * Mathf.Sin(shita_x);

        M1 = new Vector3(x, y, z);
        M2 = M1;
    }
}
