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
    private const float KgCoefficient = 1e-14f;
    private const float MCoefficient = 1e-6f;

    //simulation value
    private const float Pi = Mathf.PI;
    private int n, i, j;
    private float stepTime, x, y, z;
    private Rigidbody[] MagneticParticle_rb;
    private Transform[] MagneticParticle_trans;
    public Button ON_MagButton, OFF_MagButton, TimeMagButton;
    public SetParticle setParticle;
    public GameObject[] MagneticParticle;
    public int step;
    public int ChangeStep; //変化Step
    public float diameter;
    [HideInInspector] public int particleNumber;
    public bool addBeadsByWallPos;
    public GameObject[] WallObjects;
    private Vector3[] defaultPos;

    //noise value
    public bool useTrans, useBrown, useRandPow;
    public float brownPow, pow;
    private const float eta = 0.001f; //水の粘性係数 8.9*10^-4
    private const float T = 300; //絶対温度 K
    private const float kb = 1.38e-23f; //ボルツマン定数 1.38*10^-23
    private float gamma; //粘性抵抗
    private float D; //拡散係数
    private Vector3 V; //速度
    private Vector3 nowPos; //前の位置との比較用
    private Vector3[] brownX; //ブラウン運動の変位
    //private Vector3[] BeforePosition; //前ステップの位置を保存
    //private Vector3[] brownX_before;

    //mag value
    public bool useMagnet;
    private const float u0 = 1.26e-6f; //真空の透磁率 約1.26 10^-6 N/A^2
    private float q; //磁荷　kai * H_pow  SI→Wb CGS→emu
    public float shita_x; //回転方向(x軸基準)に対する鎖の角度
    public float shita_y; //回転軸(z軸)に対する鎖の角度
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
    public bool useStartMag; //初期磁場条件 切り替え
    public bool useRotation; //回転磁場 切り替え
    public bool useTimeChangeMag; //時間変化磁場 切り替え
    [HideInInspector] public float H_pow; //外部磁場の強さ(G) 70 A/m =  約1 G
    public GameObject MagneticParticlePrefab, MagnetOBJ;
    public Transform MagnetOBJTrans;
    public bool useMagnetOBJ, useLine, useLineHit, useSumLine;
    public float lineCoefficient;
    public Material LineMaterial;
    Vector3[] M_ofParticles;
    LineRenderer[] lineRenderer_Sum;
    LineRenderer[,] lineRenderer;
    GameObject[,] lineChilds;

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

            if (useLine)
            {
                if (useSumLine)
                {
                    InitSumLineRenderer();
                }else
                {
                    InitLineRenderer();
                }
            }
        } else
        {
            particleNumber = MagneticParticle.Length;
        }

        //粒子数をもとに磁場などの配列サイズを定義
        brownX = new Vector3[particleNumber];
        H = new Vector3[particleNumber];
        dH = new Vector3[particleNumber];
        MagneticParticle_rb = new Rigidbody[particleNumber];
        MagneticParticle_trans = new Transform[particleNumber];
        M_ofParticles = new Vector3[particleNumber];
        //BeforePosition = new Vector3[particleNumber];
        //brownX_before = new Vector3[particleNumber];
        AllParticlePosition = new string[SimulationController.MaxStep, particleNumber];
        defaultPos = new Vector3[particleNumber];

        //MPRBを取得, 初期位置を代入
        for (i = 0; i < particleNumber; i++)
        {
            MagneticParticle_rb[i] = MagneticParticle[i].GetComponent<Rigidbody>();
            MagneticParticle_trans[i] = MagneticParticle[i].GetComponent<Transform>();
            //BeforePosition[i] = MagneticParticle[i].transform.position;
            defaultPos[i] = MagneticParticle_trans[i].position;
        }

        //回転用 角度設定（30度）
        shita_y = 30 * Pi / 180;

        //パラメータ設定
        kai = KgCoefficient * diameter * diameter * diameter / (2.8f * 2.8f * 2.8f);//unity粒子質量1kg*質量係数*単位質量磁化率(emu/g)→磁化率へ（Dynabeadsの場合は磁場により変化する）
        thDist = thDist * diameter;
        diameter = diameter * MCoefficient;
        gamma = 6 * Pi * (diameter / 2) * eta;
        D = kb * T / gamma;

        //磁場の初期条件
        if (useStartMag)
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
    }

    void AddMagParticle()
    {
        GameObject newMagneticParticle = Instantiate(MagneticParticlePrefab);
        newMagneticParticle.transform.SetParent(this.transform);
        newMagneticParticle.transform.localScale = new Vector3(diameter, diameter, diameter) / MCoefficient;
        Vector3 newPos = Vector3.zero;
        int i;
        if (addBeadsByWallPos)
        {
            for(i = 0; i < WallObjects.Length; i++)
            {
                newPos += WallObjects[i].transform.position;
            }
            newMagneticParticle.transform.position = newPos / WallObjects.Length;
        } else
        {
            for (i = 0; i < particleNumber; i++)
            {
                newPos += MagneticParticle_trans[i].position;
            }
            newMagneticParticle.transform.position = newPos / particleNumber;
        }

        particleNumber += 1;
        Array.Resize(ref MagneticParticle, particleNumber);
        Array.Resize(ref MagneticParticle_rb, particleNumber);
        Array.Resize(ref MagneticParticle_trans, particleNumber);

        MagneticParticle[particleNumber - 1] = newMagneticParticle;
        MagneticParticle_rb[particleNumber - 1] = newMagneticParticle.GetComponent<Rigidbody>();
        MagneticParticle_trans[particleNumber - 1] = newMagneticParticle.GetComponent<Transform>();

        if (useLine)
        {
            if (useSumLine)
            {
                InitSumLineRenderer();
            } else
            {
                InitLineRenderer();
            }
            ////Array.Resize(ref lineRenderer, particleNumber);
            //lineRenderer = new LineRenderer[particleNumber, particleNumber]; //二次元配列に変更
            //lineChilds = new GameObject[particleNumber, particleNumber];
            //for (j = 0; j < particleNumber; j++)
            //{
            //    lineChilds[particleNumber - 1, j] = new GameObject(j.ToString());
            //    lineChilds[particleNumber - 1, j].transform.parent = MagneticParticle[i].transform;
            //}
            //SetLineRenderer(particleNumber - 1);
        }
        brownX = new Vector3[particleNumber];
        H = new Vector3[particleNumber];
        dH = new Vector3[particleNumber];
        AllParticlePosition = new string[SimulationController.MaxStep, particleNumber];
        M_ofParticles = new Vector3[particleNumber];

        Debug.Log("粒子数を" + particleNumber + "へ変更しました");
    }

    void InitSumLineRenderer()
    {
        if(lineRenderer_Sum == null)
        {
            lineRenderer_Sum = new LineRenderer[particleNumber];

            for (i = 0; i < particleNumber; i++)
            {
                lineRenderer_Sum[i] = MagneticParticle[i].AddComponent<LineRenderer>();
                lineRenderer_Sum[i].startWidth = 1f;
                lineRenderer_Sum[i].endWidth = 0f;
                lineRenderer_Sum[i].material = LineMaterial;
            }
        } else
        {
            var newLineRenderer = new LineRenderer[particleNumber];

            for (i = 0; i < particleNumber - 1; i++)
            {
                newLineRenderer[i] = lineRenderer_Sum[i];
            }

            newLineRenderer[particleNumber - 1] = MagneticParticle[i].AddComponent<LineRenderer>();
            newLineRenderer[particleNumber - 1].startWidth = 1f;
            newLineRenderer[particleNumber - 1].endWidth = 0f;
            newLineRenderer[particleNumber - 1].material = LineMaterial;

            lineRenderer_Sum = new LineRenderer[particleNumber];

            for (i = 0; i < particleNumber; i++)
            {
                lineRenderer_Sum[i] = newLineRenderer[i];
            }
        }
    }

    void InitLineRenderer()
    {
        var newlineRenderer = new LineRenderer[particleNumber, particleNumber];
        var newLineChilds = new GameObject[particleNumber, particleNumber];
        int j;

        if (lineChilds == null) //初期化
        {
            for (i = 0; i < particleNumber; i++)
            {
                for (j = 0; j < particleNumber; j++)
                {

                    newLineChilds[i, j] = new GameObject(j.ToString());
                    newLineChilds[i, j].transform.parent = MagneticParticle[i].transform;
                }
            }

            for (i = 0; i < particleNumber; i++)
            {
                for (j = 0; j < particleNumber; j++)
                {
                    newlineRenderer[i, j] = newLineChilds[i, j].AddComponent<LineRenderer>();
                    newlineRenderer[i, j].startWidth = 1f;
                    newlineRenderer[i, j].endWidth = 0f;
                    newlineRenderer[i, j].material = LineMaterial;
                }
            }
        }
        else //追加
        {
            for (i = 0; i < particleNumber; i++)
            {
                for (j = 0; j < particleNumber; j++)
                {
                    if(i == particleNumber - 1 || j == particleNumber - 1)
                    {
                        newLineChilds[i, j] = new GameObject(j.ToString());
                        newLineChilds[i, j].transform.parent = MagneticParticle[i].transform;

                        newlineRenderer[i, j] = newLineChilds[i, j].AddComponent<LineRenderer>();
                        newlineRenderer[i, j].startWidth = 1f;
                        newlineRenderer[i, j].endWidth = 0f;
                        newlineRenderer[i, j].material = LineMaterial;
                    }
                    else
                    {
                        newLineChilds[i, j] = lineChilds[i, j];
                        newlineRenderer[i, j] = lineRenderer[i, j];
                    }
                }
            }
        }

        lineChilds = new GameObject[particleNumber, particleNumber];
        lineRenderer = new LineRenderer[particleNumber, particleNumber];

        for (i = 0; i < particleNumber; i++)
        {
            for (j = 0; j < particleNumber; j++)
            {
                lineChilds[i, j] = newLineChilds[i, j];
                lineRenderer[i, j] = newlineRenderer[i, j];
            }
        }
    }

    //void SetLineRenderer(int i)
    //{
    //    //lineRenderer[i] = MagneticParticle[i].AddComponent<LineRenderer>();
    //    //lineRenderer[i].startWidth = 1f;
    //    //lineRenderer[i].endWidth = 0f;
    //    //lineRenderer[i].material = LineMaterial;

    //    Debug.Log(lineRenderer.Length);
    //    Debug.Log(lineChilds.Length);

    //    //二次元配列に変更
    //    int j;
    //    for(j = 0; j < particleNumber; j++)
    //    {
    //        Debug.Log(j);
    //        lineRenderer[i, j] = lineChilds[i, j].AddComponent<LineRenderer>();
    //        lineRenderer[i, j].startWidth = 1f;
    //        lineRenderer[i, j].endWidth = 0f;
    //        lineRenderer[i, j].material = LineMaterial;
    //    }
    //}

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
                r += Vector3.SqrMagnitude(MagneticParticle_trans[j].position - defaultPos[j]);
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

        if (useRotation)
        {
            RotationMagneticField();
        }
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
            shita_y = 0f;
            ChangeMagneticField();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            shita_x = 0f;
            shita_y = 0f;
            ChangeMagneticField();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            ChangeMagneticField();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            AddMagParticle();
        }
    }

    private void Noise() //ブラウン運動
    {
        for (n = 0; n < particleNumber; n++)
        {
            nowPos = MagneticParticle_trans[n].position;
            //V = (nowPos - BeforePosition[n]) * MCoefficient / stepTime;
            //BeforePosition[n] = nowPos;

            //ボックスミュラー法（一様分布の乱数を標準正規分布へ 
            x = Mathf.Sqrt(-2.0f * Mathf.Log(UnityEngine.Random.Range(0.00001f, 1.0f))) * Mathf.Cos(2.0f * Pi * UnityEngine.Random.Range(0f, 1.0f));
            y = Mathf.Sqrt(-2.0f * Mathf.Log(UnityEngine.Random.Range(0.00001f, 1.0f))) * Mathf.Cos(2.0f * Pi * UnityEngine.Random.Range(0f, 1.0f));
            z = Mathf.Sqrt(-2.0f * Mathf.Log(UnityEngine.Random.Range(0.00001f, 1.0f))) * Mathf.Cos(2.0f * Pi * UnityEngine.Random.Range(0f, 1.0f));

            //ランジュバン方程式の解 〈(𝑥(𝑡) − 𝑥(0))^2〉 = 2𝐷|𝑡|
            x = Mathf.Sqrt(stepTime * 2 * D) * x;
            y = Mathf.Sqrt(stepTime * 2 * D) * y;
            z = Mathf.Sqrt(stepTime * 2 * D) * z;
            brownX[n] = new Vector3(x, y, z);

            //位置制御
            if (useTrans)
            {
                MagneticParticle_trans[n].Translate(brownPow * brownX[n]/ MCoefficient);
            }
            else
            {
                MagneticParticle_rb[n].AddForce(brownX[n] / MCoefficient, ForceMode.VelocityChange);
                //MagneticParticle_rb[n].AddForce((brownX[n]) / KgCoefficient);
            }

            //速度制御
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

            MagneticParticle_rb[n].AddForce(RandomPow);
        }
    }

    public void Interactive() //磁気相互作用
    {
        for (i = 0; i < particleNumber; i++) //Hを初期化
        {
            H[i] = new Vector3(0f, 0f, 0f);

            //磁石-粒子間相互作用
            if (useMagnetOBJ)
            {
                PosVect = MagneticParticle_trans[i].position - MagnetOBJTrans.position;
                dist = Mathf.Sqrt(Vector3.Dot(PosVect, PosVect));
                E = PosVect / dist;
                dist = dist * MCoefficient;

                ////磁石が粒子iの位置に作り出す磁場・・・10^-23の磁力で作動（距離が近いため値が大きくなり過ぎている）
                PosVect = PosVect * MCoefficient;
                M_ofParticles[i] = (1f / (4f * Pi * u0)) * (- (M0 / (dist * dist * dist)) + (3 * Vector3.Dot(M0, PosVect) * PosVect / (dist * dist * dist * dist * dist)));
                //Debug.Log("M0:" + M0);
                //Debug.Log("dist:" + dist);
                //Debug.Log("PosVect:" + PosVect);
                //Debug.Log(H_pow);
                //Debug.Log(M_ofParticles[i].magnitude);
                //Debug.Log("磁石" + M_ofParticles[i].z);


                //粒子iの磁荷
                q = kai * Mathf.Pow(H_pow, 0.383f);
                //q = kai * Mathf.Pow(H_pow / (4 * Pi * u0 * dist * dist), 0.383f);
                //Debug.Log(q);

                //磁気モーメントM = 1/u0 * q * d ・・・　A/m = emu(A/m^2) * d(m)
                //x = (q * diameter / u0) * Mathf.Sin(Mathf.Deg2Rad * shita_y);
                //y = (q * diameter / u0) * Mathf.Sin(Mathf.Deg2Rad * shita_x);
                //z = (q * diameter / u0) * Mathf.Cos(Mathf.Deg2Rad * shita_y) * Mathf.Cos(Mathf.Deg2Rad * shita_x);
                //M1 = new Vector3(x, y, z);

                //磁石-粒子間相互作用・・・元の値とかなり近い磁力で作動
                H[i] = (3f * u0 / (4f * Pi * dist * dist * dist * dist)) * (Vector3.Dot(M1, E) * M0
                     + Vector3.Dot(M0, E) * M1 + Vector3.Dot(M1, M0) * E
                     - 5f * Vector3.Dot(M1, E) * Vector3.Dot(M0, E) * E);
            }
        }

        //粒子間相互作用
        Vector3 sum = Vector3.zero;
        for (i = 0; i < particleNumber; i++)
        {
            for (j = i + 1; j < particleNumber; j++)
            {
                if (useMagnetOBJ)
                {
                    M1 = M_ofParticles[i];
                    M2 = M_ofParticles[j];
                }

                PosVect = MagneticParticle_trans[i].position - MagneticParticle_trans[j].position;
                dist = Mathf.Sqrt(Vector3.Dot(PosVect, PosVect));
                if (dist < thDist)
                {
                    E = PosVect / dist;
                    dist = dist * MCoefficient;

                    dH[i] = (3f * u0 / (4f * Pi * dist * dist * dist * dist)) * (Vector3.Dot(M1, E) * M2
                          + Vector3.Dot(M2, E) * M1 + Vector3.Dot(M1, M2) * E
                          - 5f * Vector3.Dot(M1, E) * Vector3.Dot(M2, E) * E);


                    //dH[i] = (3f * u0 / (4f * Pi * dist * dist * dist * dist))
                    //      * (Vector3.Cross (Vector3.Cross(E, M1), M2)
                    //      + Vector3.Cross(Vector3.Cross(E, M2), M1)
                    //      - 2 * Vector3.Dot(M1, M2) * E
                    //      + 5f * Vector3.Dot(Vector3.Cross(E, M1), Vector3.Cross(E, M2)) * E);

                    H[i] += dH[i];
                    H[j] -= dH[i];

                    //二次元配列用
                    if (useLine) {
                        if(!useSumLine)
                        {
                            Vector3 lineLength = lineCoefficient * dH[i] / KgCoefficient;
                            Vector3[] lineVects_i = { MagneticParticle_trans[i].position, lineLength + MagneticParticle_trans[i].position };
                            Vector3[] lineVects_j = { MagneticParticle_trans[j].position, -lineLength + MagneticParticle_trans[j].position };

                            //障害物の位置でラインを止める
                            if (useLineHit)
                            {
                                RaycastHit hit_i;
                                Ray ray_i = new Ray(MagneticParticle_trans[i].position, lineLength); //rayの始点と向きを指定
                                if (Physics.Raycast(ray_i, out hit_i, lineLength.magnitude)) //rayの距離を指定して発射
                                {
                                    if (hit_i.collider.gameObject.tag == "Particle")
                                    {
                                        //Debug.Log("hit");
                                        lineVects_i = new Vector3[] { MagneticParticle_trans[i].position, hit_i.point };
                                    }
                                }
                                else
                                {
                                    //Debug.Log("miss");
                                }

                                RaycastHit hit_j;
                                Ray ray_j = new Ray(MagneticParticle_trans[j].position, -lineLength); //rayの始点と向きを指定
                                if (Physics.Raycast(ray_j, out hit_j, lineLength.magnitude)) //rayの距離を指定して発射
                                {
                                    if (hit_j.collider.gameObject.tag == "Particle")
                                    {
                                        //Debug.Log("hit");
                                        lineVects_j = new Vector3[] { MagneticParticle_trans[j].position, hit_j.point };
                                    }
                                }
                                else
                                {
                                    //Debug.Log("miss");
                                }
                            }

                            lineRenderer[i, j].SetPositions(lineVects_i);
                            lineRenderer[j, i].SetPositions(lineVects_j);
                        }
                    }
                }
            }
            if (useLine)
            {
                if (useSumLine)
                {
                    Vector3 lineLength = lineCoefficient * H[i] / KgCoefficient;
                    Vector3[] lineVects_i = { MagneticParticle_trans[i].position, lineLength + MagneticParticle_trans[i].position };

                    //障害物の位置でラインを止める
                    if (useLineHit)
                    {
                        RaycastHit hit_i;
                        Ray ray_i = new Ray(MagneticParticle_trans[i].position, lineLength); //rayの始点と向きを指定
                        if (Physics.Raycast(ray_i, out hit_i, lineLength.magnitude)) //rayの距離を指定して発射
                        {
                            if (hit_i.collider.gameObject.tag == "Particle")
                            {
                                //Debug.Log("hit");
                                lineVects_i = new Vector3[] { MagneticParticle_trans[i].position, hit_i.point };
                            }
                        }
                        else
                        {
                            //Debug.Log("miss");
                        }
                    }

                    lineRenderer_Sum[i].SetPositions(lineVects_i);
                }
            }
            MagneticParticle_rb[i].AddForce(H[i] / KgCoefficient);
            //Debug.Log(H[i] / KgCoefficient);

            //if (useLine)
            //{
            //    Vector3 lineLength = lineCoefficient * H[i] / KgCoefficient;
            //    Vector3[] lineVects = { MagneticParticle_trans[i].position, lineLength + MagneticParticle_trans[i].position };

            //    //障害物の位置でラインを止める
            //    if (useLineHit)
            //    {
            //        RaycastHit hit;
            //        Ray ray = new Ray(MagneticParticle_trans[i].position, lineLength); //rayの始点と向きを指定
            //        if (Physics.Raycast(ray, out hit, lineLength.magnitude)) //rayの距離を指定して発射
            //        {
            //            if(hit.collider.gameObject.tag == "Particle")
            //            {
            //                //Debug.Log("hit");
            //                lineVects = new Vector3[] { MagneticParticle_trans[i].position, hit.point };
            //            }
            //        }
            //        else
            //        {
            //            //Debug.Log("miss");
            //        }
            //    }

            //    lineRenderer[i].SetPositions(lineVects);
            //}
            //Debug.Log(i + ":" + H[i] / KgCoefficient);
            //Debug.Log("X:" + H[i].x / KgCoefficient);
            //Debug.Log("Y:" + H[i].y / KgCoefficient);
            //Debug.Log("Z:" + H[i].z / KgCoefficient);
            //sum += H[i];
        }
        //Debug.Log("sum:" + sum);
        //Debug.Log("x:" + sum.x);
        //Debug.Log("y:" + sum.y);
        //Debug.Log("z:" + sum.z);
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

        if (useMagnetOBJ)
        {
            //不均一磁場(磁石)
            Vector3 MagnetRotation = MagnetOBJ.GetComponent<Transform>().rotation.eulerAngles;
            shita_y = MagnetRotation.y;
            shita_x = MagnetRotation.x;

            //
            //
            //
            //1 G = 1000/4π A/m → A/m = H_pow(G) / 79.58
            //磁石の長さがパラメータにないので入れる！！！！！！！！！！！！！！
            //
            //            
            M0 = H_pow / 79.58f * new Vector3(Mathf.Sin(shita_y), Mathf.Sin(shita_x), Mathf.Cos(shita_y) * Mathf.Cos(shita_x));
        }
        //Excelより、y=X^0.383 q 磁荷 Wb, emu
        q = kai * Mathf.Pow(H_pow, 0.383f);
        //q = kai * Mathf.Pow(H_pow, 0.383f) * KgCoefficient;
        Debug.Log(q);

        //磁気モーメントM = 1/u0 * q * d ・・・　A/m = emu(A/m^2) * d(m)
        x = (q * diameter / u0) * Mathf.Sin(Mathf.Deg2Rad * shita_y);
        z = (q * diameter / u0) * Mathf.Sin(Mathf.Deg2Rad * shita_x);
        y = (q * diameter / u0) * Mathf.Cos(Mathf.Deg2Rad * shita_y) * Mathf.Cos(Mathf.Deg2Rad * shita_x);

        M1 = new Vector3(x, y, z);
        M2 = M1;
        //Debug.Log("M:" + M1 / KgCoefficient);

        //Debug.Log(x);
        //Debug.Log(y);
        //Debug.Log(z);
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

    private void RotationMagneticField()
    {
        //shita_x += rotationSpeed * stepTime * Pi / 180;
        shita_x += rotationSpeed * stepTime;
        x = (q * diameter / u0) * Mathf.Sin(Mathf.Deg2Rad * shita_y);
        z = (q * diameter / u0) * Mathf.Sin(Mathf.Deg2Rad * shita_x);
        y = (q * diameter / u0) * Mathf.Cos(Mathf.Deg2Rad * shita_y) * Mathf.Cos(Mathf.Deg2Rad * shita_x);

        M1 = new Vector3(x, y, z);
        M2 = M1;
    }
}