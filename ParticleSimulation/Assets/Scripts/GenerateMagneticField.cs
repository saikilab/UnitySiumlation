using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMagneticField : MonoBehaviour
{
    public float roomSize; //1辺のサイズ[m]
    public int meshNum; //1方向あたりのの分割数
    int dotNum; //部屋分割数
    float meshSize; //1ドットのサイズ

    public GameObject roomDot; //描画用オブジェクト
    Transform[] DotTrans;
    Renderer[] DotRenderer; //描画システム
    public Transform HeatMap; //描画用オブジェクトの入れ物

    int i, j;

    public float H_pow, height;
    Vector3 MagnetPos, ParticlePos;
    public bool generate, calc;
    private const float KgCoefficient = 0.00000000000001f;
    private const float MCoefficient = 0.000001f;

    Vector3 PosVect, E, M0, M1;
    float dist, Pi, kai, u0;

    public bool addY, addZ;

    private void Start()
    {
        Pi = Mathf.PI;
        kai = 50 * 1 * KgCoefficient;
        u0 = 0.000001f;
    }

    public void Update()
    {
        if (generate)
        {
            generate = false;
            GenerateMagneticFieldView();
        }

        if (calc)
        {
            calc = false;
            CalcPowToDist();
        }
    }

    public void GenerateMagneticFieldView()
    {
        //初期化
        dotNum = meshNum * meshNum;
        DotTrans = new Transform[dotNum];
        DotRenderer = new Renderer[dotNum];
        meshSize = roomSize / meshNum;


        //ヒートマップ描画用オブジェクトの生成
        int x, y;
        int n = 0;
        for (x = meshNum; x > 0; x--)
        {
            for (y = 0; y < meshNum; y++)
            {
                GameObject tmp = Instantiate(roomDot, new Vector3(y * meshSize + meshSize / 2, x * meshSize - meshSize / 2, (height - 1f)), Quaternion.identity, HeatMap);
                DotTrans[n] = tmp.GetComponent<Transform>();
                DotTrans[n].localScale = new Vector3(meshSize, meshSize, meshSize);
                DotRenderer[n] = tmp.GetComponent<Renderer>();
                n++;
            }
        }


        //中心粒子による描画オブジェクト位置における力の計算
        Vector3[] H = new Vector3[dotNum];
        for (i = 0; i < dotNum; i++)
        {
            ParticlePos = DotTrans[i].position;
            PosVect = ParticlePos - MagnetPos;
            dist = Mathf.Sqrt(Vector3.Dot(PosVect, PosVect));
            E = PosVect / dist;
            dist = dist * MCoefficient;
            SetM0();
            M1 = M0;
            //M1 = kai * H_pow * (-1f / 4f * Pi * u0) * ((M0 / dist * dist * dist) - (3 * Vector3.Dot(M0, PosVect) * PosVect / dist * dist * dist * dist * dist));
            H[i] = (3f * u0 / (4f * Pi * dist * dist * dist * dist)) * (Vector3.Dot(M1, E) * M0
                 + Vector3.Dot(M0, E) * M1 + Vector3.Dot(M1, M0) * E
                 - 5f * Vector3.Dot(M1, E) * Vector3.Dot(M0, E) * E);
        }
        //ParticlePos = new Vector3(1f, 0, height - 1f);
        //PosVect = ParticlePos - MagnetPos;
        //dist = Mathf.Sqrt(Vector3.Dot(PosVect, PosVect));
        //E = PosVect / dist;
        //dist = dist * MCoefficient;
        //M0 = new Vector3(0, 0, H_pow);
        //M1 = kai * H_pow * (-1f / 4f * Pi * u0) * ((M0 / dist * dist * dist) - (3 * Vector3.Dot(M0, PosVect) * PosVect / dist * dist * dist * dist * dist));
        //Vector3 H1 = (3f * u0 / (4f * Pi * dist * dist * dist * dist)) * (Vector3.Dot(M1, E) * M0
        //     + Vector3.Dot(M0, E) * M1 + Vector3.Dot(M1, M0) * E
        //     - 5f * Vector3.Dot(M1, E) * Vector3.Dot(M0, E) * E);
        //Debug.Log(H1.x + H1.y);

        //ヒートマップの描画
        float pow  = 0;
        float red, blue;
        float max = 0, min = 0;
        //float max = 0.0000002683f, min = -0.0000000043f;
        //float max = 5 * Mathf.Pow(10, 15), min = -8 * Mathf.Pow(10, 17); //厚さ1.786の境界値
        if (addZ)
        {
            //厚さ1.786の適正表示値
            max = 1 * Mathf.Pow(10, 16);
            min = -1 * Mathf.Pow(10, 16); 
        }
        if (addY)
        {
            //縦方向　厚さ1.0の適正表示値
            max = 1 * Mathf.Pow(10, 17);
            min = -1 * Mathf.Pow(10, 17); 
        }
        //float max = 0, min = 0;
        for (i = 0; i < dotNum; i++)
        {
            if (addY)
            {
                pow = H[i].x;
            }
            if (addZ)
            {
                pow = H[i].x + H[i].y;
            }
            if (pow > 0f) //斥力
            {
                red = 0;
                blue = pow / max;
                if (blue > 1)
                {
                    blue = 1;
                }
                //if(max < pow) //max計算用
                //{
                //    max = pow;
                //}
            }
            else if (pow < 0f) //引力
            {
                red = pow / min;
                if (red > 1)
                {
                    red = 1;
                }
                blue = 0;
                //if (min > pow) //min計算用
                //{
                //    min = pow;
                //}
            }
            else
            {
                Debug.Log("no pow");
                red = 0;
                blue = 0;
            }
            Color color = new Color(1 - blue, 1 - red - blue, 1 - red);//ここで色を指定
            DotRenderer[i].material.color = color;
        }
        //Debug.Log(max);
        //Debug.Log(min);
    }

    public void CalcPowToDist()
    {
        //引力：斥力の切り替わる点の計算
        float[] thDist = new float[100];
        for (j = 0; j < 100; j++)
        {
            Vector3[] H = new Vector3[meshNum];
            for (i = 0; i < meshNum; i++)
            {
                ParticlePos = new Vector3(roomSize * (float)i / meshNum, 0, j * 0.01f);
                PosVect = ParticlePos - MagnetPos;
                dist = Mathf.Sqrt(Vector3.Dot(PosVect, PosVect));
                E = PosVect / dist;
                dist = dist * MCoefficient;
                SetM0();
                M1 = M0;
                //M1 = kai * H_pow * (-1f / 4f * Pi * u0) * ((M0 / dist * dist * dist) - (3 * Vector3.Dot(M0, PosVect) * PosVect / dist * dist * dist * dist * dist));
                H[i] = (3f * u0 / (4f * Pi * dist * dist * dist * dist)) * (Vector3.Dot(M1, E) * M0
                     + Vector3.Dot(M0, E) * M1 + Vector3.Dot(M1, M0) * E
                     - 5f * Vector3.Dot(M1, E) * Vector3.Dot(M0, E) * E);
                if (H[i].x + H[i].y > 0) //水平成分が斥力へ切り替わった点を出力
                {
                    Debug.Log(ParticlePos);
                    thDist[j] = ParticlePos.magnitude;
                    break;
                }

                //if (H[i].z < 0) //厚さ成分
                //{
                //    thDist[j] = ParticlePos.magnitude;
                //    break;
                //}

                if (i == meshNum - 1)
                {
                    thDist[j] = 0f;
                }
            }
            float hn = ParticlePos.z + 1f;
            Debug.Log("厚さ：" + hn);
            Debug.Log("距離：" + thDist[j]);
        }
    }

    void SetM0()
    {
        if (addZ) //奥行方向
        {
            M0 = new Vector3(0, 0, H_pow);
        }
        if (addY) //縦方向
        {
            M0 = new Vector3(0, H_pow, 0);
        }
    }
}
