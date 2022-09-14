using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using System;

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
    public float diameter, time;

    //noise value
    public float randomForce;
    public float beta; //粘性抵抗
    private Vector3 V; //速度
    private Vector3[] BeforPosition;

    //mag value
    private Vector3[] H, dH; // 磁界 = m/(4πμ0r^2) = 1/mr^2?
    private float u0; //透磁率 約1.26 10^-6 N/A^2
    public float H_pow; //外部磁場の強さ
    public float default_H_pow; //外部磁場の強さの初期値 60
    public float kai; //体積磁化率
    private float q; //磁荷　kai * H_pow
    private Vector3 M1, M2; //磁気双極子（ベクトル）
    private float shita_x, shita_y;
    public float ratationSpeed;
    public bool Rotation;
    public float thDist;

    //Math value
    public bool switch_start_mag;
    private float pi;
    private int i, j;

    //Time Change Mag
    public bool timeMag;

    //Save
    public bool switch_save_mag;
    public int step;
    [HideInInspector]
    public string dirN;

    //step
    public int ChangeStep;
    public float ChangeMag;

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
        u0 = 0.000001f;
        H_pow = default_H_pow;
        q = kai * H_pow;

        MagneticParticleRB = new Rigidbody[particleNumber];
        for (i = 0; i < particleNumber; i++)
        {
            MagneticParticleRB[i] = MagneticParticle[i].GetComponent<Rigidbody>();
        }
        BeforPosition = new Vector3[particleNumber];
        if (switch_start_mag)
            ONMag();
        else
            OFFMag();
        shita_y = 30 * pi / 180;

        if (switch_save_mag)
        {
            dirN = DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString();
            Directory.CreateDirectory(@dirN);
            dirN = dirN + "/particles";
            Directory.CreateDirectory(@dirN);
        }
    }

    private void FixedUpdate()
    {
        if (switch_save_mag) //ファイル出力切り替え
        {
            SaveMag();
            step++;
        }

        time = Time.deltaTime; //ステップ時間
        Noise(); //ブラウン運動
        Interactive(); //磁気相互作用

        if (Rotation) //回転磁場切り替え
            RatationMagneticField();

        if (timeMag) //時間変化磁場切り替え
        {
            if (step <= ChangeStep) //ChangeStepまでChangeMagずつ増加
                H_pow = (float)step*ChangeMag;
            else
                H_pow = 0.2f - (float)step*ChangeMag;
            ChangeMagneticField();
        }
        //if (2*ChangeStep < step)//2*ChangeStepで停止
        //{
        //    Debug.Log("指定のステップ数実行が完了したため停止しました");
        //    Debug.Break();
        //}         
    }

    private void Noise()
    {
        float x, y, z;
        int n;
        Vector3 brown, nowPosition;

        for (n = 0; n < particleNumber; n++)
        {
            nowPosition = MagneticParticle[n].transform.position;
            V = (nowPosition - BeforPosition[n]) / time;
            x = Mathf.Sqrt(-2.0f * Mathf.Log(UnityEngine.Random.Range(0.00001f, 1.0f))) * Mathf.Cos(2.0f * pi * UnityEngine.Random.Range(0f, 1.0f));
            y = Mathf.Sqrt(-2.0f * Mathf.Log(UnityEngine.Random.Range(0.00001f, 1.0f))) * Mathf.Cos(2.0f * pi * UnityEngine.Random.Range(0f, 1.0f));
            z = Mathf.Sqrt(-2.0f * Mathf.Log(UnityEngine.Random.Range(0.00001f, 1.0f))) * Mathf.Cos(2.0f * pi * UnityEngine.Random.Range(0f, 1.0f));

            //ランジュバン方程式
            x = -beta * V.x + x * randomForce * time;
            y = -beta * V.y + y * randomForce * time;
            z = -beta * V.z + z * randomForce * time;
            brown = new Vector3(x, y, z);
            MagneticParticleRB[n].AddForce(brown, ForceMode.Impulse);
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
                if(dist < thDist)
                {
                    E = posVector / dist;

                    dH[i] = (3f * u0 / (4f * pi * Mathf.Pow(dist, 4f))) * (Vector3.Dot(M1, E) * M2
                          + Vector3.Dot(M2, E) * M1 + Vector3.Dot(M1, M2) * E
                          - 5f * Vector3.Dot(M1, E) * Vector3.Dot(M2, E) * E);
                    H[i] += dH[i];
                    H[j] -= dH[i];
                }
            }
            MagneticParticleRB[i].AddForce(H[i]*time);
            //Debug.Log(H[i]);
        }
    }

    public void ChangeMagneticField()
    {
        q = (4/3) * pi * Mathf.Pow((diameter/2), 3) * kai * H_pow / u0;
        M1 = new Vector3(0f, 0f, q);
        M2 = new Vector3(0f, 0f, q);
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

    public void SaveMag()
    {
        string fileName = dirN + "/particle_colloid_" + step.ToString("d5") + ".cdv";
        StreamWriter sw = new StreamWriter(@fileName);

        int s;
        for (s=0; s < particleNumber; s++)
        {
            string z;
            if (MagneticParticle[s].transform.position.z < 0)
                z = "2";
            else
                z = "3";
            string[] s1 = { string.Format("{0, 4}", s.ToString()), z, string.Format("{0,7}", MagneticParticle[s].transform.position.z.ToString("F4")), string.Format("{0,7}", MagneticParticle[s].transform.position.x.ToString("F4")), string.Format("{0,7}", MagneticParticle[s].transform.position.y.ToString("F4")), };
            string s2 = string.Join(" ", s1);
            sw.WriteLine(s2);
        }
        sw.Close();
    }
}
