using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Text;
using System;

public class ParticleController : MonoBehaviour
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
    public GameObject InfoPanel;
    public Button ON_MagButton, OFF_MagButton;
    public SetParticle setParticle;
    public GameObject[] MagneticParticle;
    public int step;
    public int ChangeStep; //変化Step
    public float diameter;
    [HideInInspector] public int particleNumber;

    //noise value
    private const float eta = 8.9e-4f; //水の粘性係数 8.9*10^-4
    private const float T = 300; //絶対温度 K
    private const float kb = 1.38e-23f; //ボルツマン定数 1.38*10^-23
    private float gamma; //粘性抵抗
    private float D; //拡散係数
    private Vector3 V; //速度
    private Vector3 nowPos; //前の位置との比較用
    private Vector3[] brownF; //ブラウン運動の力
    private Vector3[] BeforPosition; //前ステップの位置を保存
    //public float randomForce; //ランダム力（実験から算出）

    //mag value
    private const float u0 = 1.26e-6f; //真空の透磁率 約1.26 10^-6 N/A^2
    private float q; //磁荷　kai * H_pow  SI→Wb CGS→emu
    private float shita_x; //回転方向(x軸基準)に対する鎖の角度
    private float shita_y; //回転軸(z軸)に対する鎖の角度
    private float dist;
    private Vector3 PosVect; //粒子間の差分ベクトル
    private Vector3 E; //単位ベクトル
    private Vector3 M1, M2; //磁気双極子モーメント Wb*m
    private Vector3[] H, dH; // 磁界 = m/(4πμ0r^2) = 1/mr^2?
    public float default_H_pow; //外部磁場の強さの初期値 60
    [HideInInspector] public float kai; //単位質量磁化率　Dynabeadsの場合 50 (emu/g)/G = (A*m^2/kg)/G
    public float thDist; //閾値（磁場影響範囲）
    public float rotationSpeed; //回転速度
    public float ChangeMag; //時間変化磁場の変化量
    public bool onStartMag; //初期磁場条件 切り替え
    public bool useRotation; //回転磁場 切り替え
    public bool useTimeChangeMag; //時間変化磁場 切り替え
    [HideInInspector] public float H_pow; //外部磁場の強さ(G) 70 A/m =  約1 G

    //Save value
    public bool saveParticlePosition; //磁場保存 切り替え
    [HideInInspector] public string[,] AllParticlePosition;

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
        brownF = new Vector3[particleNumber];
        H = new Vector3[particleNumber];
        dH = new Vector3[particleNumber];
        MagneticParticleRB = new Rigidbody[particleNumber];
        MagneticParticleTrans = new Transform[particleNumber];
        BeforPosition = new Vector3[particleNumber];
        AllParticlePosition = new string[SimulationController.MaxStep, particleNumber];

        //MPRBを取得, 初期位置を代入
        for (i = 0; i < particleNumber; i++)
        {
            MagneticParticleRB[i] = MagneticParticle[i].GetComponent<Rigidbody>();
            MagneticParticleTrans[i] = MagneticParticle[i].GetComponent<Transform>();
            BeforPosition[i] = MagneticParticle[i].transform.position;
        }

        //回転用 角度設定（30度）
        //shita_y = 30 * Pi / 180;

        //パラメータ設定
        diameter = diameter * MCoefficient;
        gamma = 6 * Pi * (diameter / 2) * eta;
        D = kb * T / gamma;
        kai = 50 * 1 * KgCoefficient;//単位質量磁化率→磁化率(emu/G)へ（粒子質量1kg*質量係数）

        //磁場の初期条件
        if (onStartMag)
            ONMag();
        else
            OFFMag();
    }

    private void FixedUpdate()
    {
        step = SimulationController.Step;

        if (saveParticlePosition) //粒子位置を保存
        {
            SaveParticlePosition();
        }

        if (useRotation) //回転磁場
            RatationMagneticField();

        if (useTimeChangeMag) //時間変化磁場
        {
            if (step <= ChangeStep) //ChangeStepまでChangeMagずつ増加
                H_pow = (float)step*ChangeMag;
            else
                H_pow = 0.2f - (float)step*ChangeMag;
            ChangeMagneticField();
        }

        stepTime = Time.deltaTime; //1ステップの時間
        Noise(); //ブラウン運動
        Interactive(); //磁気相互作用
    }

    private void Update()
    {
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
            shita_y = 90f;
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
            V = (nowPos - BeforPosition[n]) * MCoefficient / stepTime;
            BeforPosition[n] = nowPos;

            //ボックスミュラー法（一様分布の乱数を標準正規分布へ 
            x = Mathf.Sqrt(-2.0f * Mathf.Log(UnityEngine.Random.Range(0.00001f, 1.0f))) * Mathf.Cos(2.0f * Pi * UnityEngine.Random.Range(0f, 1.0f));
            y = Mathf.Sqrt(-2.0f * Mathf.Log(UnityEngine.Random.Range(0.00001f, 1.0f))) * Mathf.Cos(2.0f * Pi * UnityEngine.Random.Range(0f, 1.0f));
            z = Mathf.Sqrt(-2.0f * Mathf.Log(UnityEngine.Random.Range(0.00001f, 1.0f))) * Mathf.Cos(2.0f * Pi * UnityEngine.Random.Range(0f, 1.0f));

            //ランジュバン方程式 F=ma=-βv+η(t)
            //x = -gamma * V.x + x * randomForce;
            //y = -gamma * V.y + y * randomForce;
            //z = -gamma * V.z + z * randomForce;

            //ランジュバン方程式の解 〈(𝑥(𝑡) − 𝑥(0))^2〉 = 2𝐷|𝑡|
            //上記の式をもとに変位を算出（質量に依存しない）　係数で割ってUnity単位へ　変位を力へ
            //x = Mathf.Sqrt(stepTime * 2 * D) / MCoefficient * x * (KgCoefficient / (stepTime * stepTime));
            //y = Mathf.Sqrt(stepTime * 2 * D) / MCoefficient * y * (KgCoefficient / (stepTime * stepTime));
            //z = Mathf.Sqrt(stepTime * 2 * D) / MCoefficient * z * (KgCoefficient / (stepTime * stepTime));

            //ランジュバン方程式の解 〈(𝑥(𝑡) − 𝑥(0))^2〉 = 2𝐷|𝑡|
            //Translateで直接制御　stepを変えてもうまくいった
            //x = Mathf.Sqrt(stepTime * 2 * D) / MCoefficient * x;
            //y = Mathf.Sqrt(stepTime * 2 * D) / MCoefficient * y;
            //z = Mathf.Sqrt(stepTime * 2 * D) / MCoefficient * z;

            //ランジュバン方程式の解 〈(𝑥(𝑡) − 𝑥(0))^2〉 = 2𝐷|𝑡|
            //forceで制御するため上記の式をtで2回微分　失敗
            //x = 1f / 2f * Mathf.Sqrt(D / (2f * stepTime * stepTime * stepTime)) * x / MCoefficient;
            //y = 1f / 2f * Mathf.Sqrt(D / (2f * stepTime * stepTime * stepTime)) * y / MCoefficient;
            //z = 1f / 2f * Mathf.Sqrt(D / (2f * stepTime * stepTime * stepTime)) * z / MCoefficient;

            //ランジュバン方程式の解 〈(𝑥(𝑡) − 𝑥(0))^2〉 = 2𝐷|𝑡|
            //速度で制御
            x = Mathf.Sqrt(stepTime * 2 * D) / MCoefficient * x;
            y = Mathf.Sqrt(stepTime * 2 * D) / MCoefficient * y;
            z = Mathf.Sqrt(stepTime * 2 * D) / MCoefficient * z;
            brownF[n] = new Vector3(x, y, z);
            MagneticParticleRB[n].AddForce(brownF[n], ForceMode.VelocityChange);
        }
    }

    public void Interactive() //磁気相互作用
    {
        for (i = 0; i < particleNumber; i++) //Hを初期化
        {
            H[i] = new Vector3(0f, 0f, 0f);
        }
        for (i = 0; i < particleNumber; i++)
        {
            for (j = i + 1; j < particleNumber; j++)
            {
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
        }
    }

    public void ChangeMagneticField()
    {
        q = kai * H_pow;

        shita_y = Mathf.Deg2Rad * shita_y;
        shita_x = Mathf.Deg2Rad * shita_x;

        x = (q * diameter / u0) * Mathf.Sin(shita_y) * Mathf.Cos(shita_x);
        y = (q * diameter / u0) *                      Mathf.Sin(shita_x);
        z = (q * diameter / u0) * Mathf.Cos(shita_y);

        M1 = new Vector3(x, y, z);
        M2 = M1;
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
        shita_x += rotationSpeed * stepTime * Pi / 180;
        x = (q * diameter / u0) * Mathf.Sin(shita_y) * Mathf.Cos(shita_x);
        y = (q * diameter / u0) * Mathf.Cos(shita_y);
        z = (q * diameter / u0) * Mathf.Sin(shita_y) * Mathf.Sin(shita_x);

        M1 = new Vector3(x, y, z);
        M2 = M1;
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
