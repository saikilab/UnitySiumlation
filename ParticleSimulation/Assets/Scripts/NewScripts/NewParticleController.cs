using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using System;

public class NewParticleController : MonoBehaviour
{
    //no dimention value
    private const float KgCoefficient = 0.00000000000001f;
    private const float MCoefficient = 0.000001f;

    //simulation value
    private const float Pi = Mathf.PI;
    private int n, i, j;
    private float stepTime, x, y, z;
    private Rigidbody[] MagneticParticleRB;
    private Transform[] MagneticParticleTrans;
    public Button ON_MagButton, OFF_MagButton, TimeMagButton;
    public SetParticle setParticle;
    public GameObject[] MagneticParticle;
    public int step;
    public int ChangeStep; //変化Step
    public float diameter;
    [HideInInspector] public int particleNumber;

    //noise value
    public bool useTrans, useBrown, useRandPow;
    public float pow;
    private const float eta = 0.001f; //水の粘性係数 8.9*10^-4
    private const float T = 300; //絶対温度 K
    private const float kb = 0.00000000000000000000001f; //ボルツマン定数 1.38*10^-23
    private float gamma; //粘性抵抗
    private float D; //拡散係数
    private Vector3 V; //速度
    private Vector3 nowPos; //前の位置との比較用
    private Vector3[] brownX; //ブラウン運動の変位
    private Vector3[] BeforePosition; //前ステップの位置を保存
    private Vector3[] brownX_before;

    //mag value
    public bool useMagnet;
    private const float u0 = 0.000001f; //真空の透磁率 約1.26 10^-6 N/A^2
    private float q; //磁荷　kai * H_pow  SI→Wb CGS→emu
    private float shita_x; //回転方向(x軸基準)に対する鎖の角度
    private float shita_y; //回転軸(z軸)に対する鎖の角度
    private float dist;
    private Vector3 PosVect; //粒子間の差分ベクトル
    private Vector3 E; //単位ベクトル
    private Vector3 M0, M1, M2; //磁気双極子モーメント Wb*m
    private Vector3[] H, dH; // 磁界 = m/(4πμ0r^2) = 1/mr^2?
    public float default_H_pow; //外部磁場の強さの初期値 60
    [HideInInspector] public float kai; //単位質量磁化率　Dynabeadsの場合 50 (emu/g)/G = (A*m^2/kg)/G
    public float thDist; //閾値（磁場影響範囲）
    public float tooNearDist;
    public float rotationSpeed; //回転速度
    public float TargetMag, ChangeMag; //時間変化磁場の変化量
    public bool onStartMag; //初期磁場条件 切り替え
    public bool useRotation; //回転磁場 切り替え
    public bool useTimeChangeMag; //時間変化磁場 切り替え
    [HideInInspector] public float H_pow; //外部磁場の強さ(G) 70 A/m =  約1 G
    public GameObject MagnetOBJ;
    public Transform MagnetOBJTrans;
    public bool useMagnetOBJ;
    Vector3[] M_ofParticles;

    //Save value
    public bool saveParticlePosition; //磁場保存 切り替え
    [HideInInspector] public string[,] AllParticlePosition;
    public bool saveBrown;
    string dirN;
    int stepN;
    StreamWriter swP;

    private void Start()
    {
        step = 0;
        //セットされた粒子オブジェクトとその数を取得
        if (setParticle.set)
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

        //粒子数をもとに磁場などの配列サイズを定義
        brownX = new Vector3[particleNumber];
        H = new Vector3[particleNumber];
        dH = new Vector3[particleNumber];
        MagneticParticleRB = new Rigidbody[particleNumber];
        MagneticParticleTrans = new Transform[particleNumber];
        BeforePosition = new Vector3[particleNumber];
        brownX_before = new Vector3[particleNumber];
        AllParticlePosition = new string[SimulationController.MaxStep, particleNumber];

        //MPRBを取得, 初期位置を代入
        for (i = 0; i < particleNumber; i++)
        {
            MagneticParticleRB[i] = MagneticParticle[i].GetComponent<Rigidbody>();
            MagneticParticleTrans[i] = MagneticParticle[i].GetComponent<Transform>();
            BeforePosition[i] = MagneticParticle[i].transform.position;
        }

        //回転用 角度設定（30度）
        //shita_y = 30 * Pi / 180;

        //パラメータ設定
        thDist = thDist * diameter;
        diameter = diameter * MCoefficient;
        gamma = 6 * Pi * (diameter / 2) * eta;
        D = kb * T / gamma;
        kai = 50 * 1 * KgCoefficient;//単位質量磁化率→磁化率(emu/G)へ（粒子質量1kg*質量係数）

        //磁場の初期条件
        if (onStartMag)
            ONMag();
        else
            OFFMag();

        if (saveBrown)
        {
            stepN = 0;
            dirN = DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString();
            Directory.CreateDirectory(dirN);
            swP = new StreamWriter(dirN + "/particle_position_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + ".csv");
        }

        M_ofParticles = new Vector3[particleNumber];
    }

    private void FixedUpdate()
    {
        step = SimulationController.Step;
        stepTime = Time.deltaTime; //1ステップの時間

        if (saveBrown)
        {
            string s = (stepN * stepTime).ToString("F3");
            s += ",";
            float r = 0;
            for (j = 0; j < particleNumber; j++)
            {
                //s += ",";
                //s += j;
                //s += ",";
                //s += MagneticParticleTrans[j].position.x.ToString("F3");
                //s += ",";
                //s += MagneticParticleTrans[j].position.y.ToString("F3");
                //s += ",";
                //s += MagneticParticleTrans[j].position.z.ToString("F3");
                r += Vector3.SqrMagnitude(MagneticParticleTrans[j].position);
            }
            s += (r/particleNumber).ToString("F5");
            swP.WriteLine(s);

            if (stepN * stepTime >= 5)
            {
                saveBrown = false;
                swP.Close();
                Debug.Log("endSave");
            }

            stepN++;
        }

        if (saveParticlePosition) //粒子位置を保存
        {
            SaveParticlePosition();
        }

        if (useTimeChangeMag) //時間変化磁場
        {
            if (H_pow <= TargetMag) //ChangeStepまでChangeMagずつ増加
                H_pow += stepTime * ChangeMag;
            else
            {
                H_pow = TargetMag;
                useTimeChangeMag = false;
            }
            ChangeMagneticField();
        }

        if (useBrown || useTrans)
        {
            Noise(); //ブラウン運動
        }

        if (useRandPow)
        {
            RandomPow();
        }

        if (useMagnet)
        {
            Interactive(); //磁気相互作用
        }
    }

    private void Update()
    {
        ////磁場方向切り替え
        //if (Input.GetKeyDown(KeyCode.X))
        //{
        //    shita_x = 0f;
        //    shita_y = 90f;
        //    ChangeMagneticField();
        //}
        //if (Input.GetKeyDown(KeyCode.Y))
        //{
        //    shita_x = 90f;
        //    shita_y = 90f;
        //    ChangeMagneticField();
        //}
        //if (Input.GetKeyDown(KeyCode.Z))
        //{
        //    shita_x = 0f;
        //    shita_y = 0f;
        //    ChangeMagneticField();
        //}

        //磁場方向切り替え
        if (Input.GetKeyDown(KeyCode.X))
        {
            shita_x = 0f;
            shita_y = 90f;
            ChangeMagneticField();
        }
        if (Input.GetKeyDown(KeyCode.Y))
        {
            shita_x = 90f;
            shita_y = 0f;
            ChangeMagneticField();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            shita_x = 0f;
            shita_y = 0f;
            ChangeMagneticField();
        }
    }

    private void Noise() //ブラウン運動
    {
        for (n = 0; n < particleNumber; n++)
        {
            nowPos = MagneticParticleTrans[n].position;
            V = (nowPos - BeforePosition[n]) * MCoefficient / stepTime;
            BeforePosition[n] = nowPos;

            //ボックスミュラー法（一様分布の乱数を標準正規分布へ 
            x = Mathf.Sqrt(-2.0f * Mathf.Log(UnityEngine.Random.Range(0.00001f, 1.0f))) * Mathf.Cos(2.0f * Pi * UnityEngine.Random.Range(0f, 1.0f));
            y = Mathf.Sqrt(-2.0f * Mathf.Log(UnityEngine.Random.Range(0.00001f, 1.0f))) * Mathf.Cos(2.0f * Pi * UnityEngine.Random.Range(0f, 1.0f));
            z = Mathf.Sqrt(-2.0f * Mathf.Log(UnityEngine.Random.Range(0.00001f, 1.0f))) * Mathf.Cos(2.0f * Pi * UnityEngine.Random.Range(0f, 1.0f));

            //ランジュバン方程式の解 〈(𝑥(𝑡) − 𝑥(0))^2〉 = 2𝐷|𝑡|
            x = Mathf.Sqrt(stepTime * 2 * D) / MCoefficient * x;
            y = Mathf.Sqrt(stepTime * 2 * D) / MCoefficient * y;
            z = Mathf.Sqrt(stepTime * 2 * D) / MCoefficient * z;
            brownX[n] = new Vector3(x, y, z);

            //位置制御
            if (useTrans)
            {
                MagneticParticleTrans[n].transform.Translate(brownX[n]);
            }
            else
            {
                MagneticParticleRB[n].AddForce((brownX[n]) / stepTime, ForceMode.VelocityChange);
            }

            //速度制御
            //pow += x;
            //pow += y;
            //pow += z;
            //MagneticParticleRB[n].AddForce((brownX[n]) / stepTime, ForceMode.VelocityChange);
            //MagneticParticleRB[n].AddForce((brownX[n] - brownX_before[n]) / stepTime, ForceMode.VelocityChange);

            //brownX_before[n] = brownX[n];
        }
    }

    private void RandomPow()
    {
        for (n = 0; n < particleNumber; n++)
        {
            Vector3 RandomPow = new Vector3(UnityEngine.Random.Range(-pow, pow), UnityEngine.Random.Range(-pow, pow), UnityEngine.Random.Range(-pow, pow));

            MagneticParticleRB[n].AddForce(RandomPow);
        }
    }

    public void Interactive() //磁気相互作用
    {
        for (i = 0; i < particleNumber; i++) //Hを初期化
        {
            H[i] = new Vector3(0f, 0f, 0f);

            //磁石-粒子間相互作用 追加23/9/28
            if (useMagnetOBJ)
            {
                PosVect = MagneticParticleTrans[i].position - MagnetOBJTrans.position;
                dist = Mathf.Sqrt(Vector3.Dot(PosVect, PosVect));
                E = PosVect / dist;
                dist = dist * MCoefficient;
                M_ofParticles[i] = kai * H_pow * (-1f / 4f * Pi * u0) * ((M0/dist*dist*dist)-(3*Vector3.Dot(M0, PosVect)*PosVect/dist * dist * dist * dist * dist));
                M1 = M_ofParticles[i];
                H[i] = (3f * u0 / (4f * Pi * dist * dist * dist * dist)) * (Vector3.Dot(M1, E) * M0
                     + Vector3.Dot(M0, E) * M1 + Vector3.Dot(M1, M0) * E
                     - 5f * Vector3.Dot(M1, E) * Vector3.Dot(M0, E) * E);
            }
        }

        //粒子間相互作用
        for (i = 0; i < particleNumber; i++)
        {
            for (j = i + 1; j < particleNumber; j++)
            {
                if (useMagnetOBJ)
                {
                    M1 = M_ofParticles[i];
                    M2 = M_ofParticles[j];
                }               

                PosVect = MagneticParticleTrans[i].position - MagneticParticleTrans[j].position;
                dist = Mathf.Sqrt(Vector3.Dot(PosVect, PosVect));
                if (dist < thDist)
                {
                    E = PosVect / dist;
                    dist = dist * MCoefficient;

                    dH[i] = (3f * u0 / (4f * Pi * dist * dist * dist * dist)) * (Vector3.Dot(M1, E) * M2
                          + Vector3.Dot(M2, E) * M1 + Vector3.Dot(M1, M2) * E
                          - 5f * Vector3.Dot(M1, E) * Vector3.Dot(M2, E) * E);

                    H[i] += dH[i];
                    H[j] -= dH[i];
                }
            }
            MagneticParticleRB[i].AddForce(H[i] / KgCoefficient);
            //Debug.Log(H[i]/KgCoefficient);
        }
    }

    public void ChangeMagneticField()
    {
        //均一磁場
        //q = kai * H_pow;

        //shita_y = Mathf.Deg2Rad * shita_y;
        //shita_x = Mathf.Deg2Rad * shita_x;

        //x = (q * diameter / u0) * Mathf.Sin(shita_y) * Mathf.Cos(shita_x);
        //y = (q * diameter / u0) *                      Mathf.Sin(shita_x);
        //z = (q * diameter / u0) * Mathf.Cos(shita_y);

        //M1 = new Vector3(x, y, z);
        //M2 = M1;

        if (!useMagnetOBJ)
        {
            //均一磁場
            q = kai * H_pow;

            shita_y = Mathf.Deg2Rad * shita_y;
            shita_x = Mathf.Deg2Rad * shita_x;

            x = (q * diameter / u0) * Mathf.Sin(shita_y);
            y = (q * diameter / u0) * Mathf.Sin(shita_x);
            z = (q * diameter / u0) * Mathf.Cos(shita_y) * Mathf.Cos(shita_x);

            M1 = new Vector3(x, y, z);
            M2 = M1;
        } else
        {
            //不均一磁場
            Vector3 MagnetRotation = MagnetOBJ.GetComponent<Transform>().rotation.eulerAngles;
            shita_y = MagnetRotation.y;
            shita_x = MagnetRotation.x;

            M0 = new Vector3(Mathf.Sin(shita_y), Mathf.Sin(shita_x), Mathf.Cos(shita_y) * Mathf.Cos(shita_x)) * H_pow;
        }
    }

    public void ONMag()
    {
        ON_MagButton.interactable = false;
        OFF_MagButton.interactable = true;
        TimeMagButton.interactable = false;
        H_pow = default_H_pow;
        useTimeChangeMag = false;
        ChangeMagneticField();
    }

    public void OFFMag()
    {
        ON_MagButton.interactable = true;
        OFF_MagButton.interactable = false;
        TimeMagButton.interactable = true;
        H_pow = 0;
        useTimeChangeMag = false;
        ChangeMagneticField();
    }

    public void UseTimeChangeMag()
    {
        TimeMagButton.interactable = false;
        useTimeChangeMag = true;
        H_pow = 0;
    }

    public void SaveParticlePosition()
    {
        int s;
        for (s = 0; s < particleNumber; s++)
        {
            string z;
            if (MagneticParticle[s].transform.position.z < 0)
                z = "2";
            else
                z = "3";
            string[] s1 = { string.Format("{0, 4}", s.ToString()), z, string.Format("{0,7}", MagneticParticle[s].transform.position.z.ToString("F4")), string.Format("{0,7}", MagneticParticle[s].transform.position.x.ToString("F4")), string.Format("{0,7}", MagneticParticle[s].transform.position.y.ToString("F4")), };
            string s2 = string.Join(" ", s1);
            AllParticlePosition[step, s] = s2;
        }
    }
}
