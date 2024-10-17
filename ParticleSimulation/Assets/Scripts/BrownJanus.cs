using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class BrownJanus : MonoBehaviour
{
    //Janus value
    public float pow;
    private Vector3 RandomPow;

    //no dimention value
    private const float KgCoefficient = 1e-14f;
    private const float MCoefficient = 1e-6f;

    //Brown value
    private const float eta = 0.001f; //水の粘性係数 8.9*10^-4
    private const float T = 300; //絶対温度 K
    private const float kb = 1.38e-23f; //ボルツマン定数 1.38*10^-23
    private float gamma; //粘性抵抗
    private float D; //拡散係数

    float x, y, z;
    Rigidbody rb;
    float dt, Pi, diameter;

    private void Start()
    {
        Pi = Mathf.PI;
        rb = this.GetComponent<Rigidbody>();

        diameter = this.transform.localScale.x * MCoefficient;
        gamma = 6 * Pi * (diameter / 2) * eta;
        D = kb * T / gamma;
    }

    void FixedUpdate()
    {
        dt = Time.deltaTime;

        //ボックスミュラー法（一様分布の乱数を標準正規分布へ 
        x = Mathf.Sqrt(-2.0f * Mathf.Log(UnityEngine.Random.Range(0.00001f, 1.0f))) * Mathf.Cos(2.0f * Pi * UnityEngine.Random.Range(0f, 1.0f));
        y = Mathf.Sqrt(-2.0f * Mathf.Log(UnityEngine.Random.Range(0.00001f, 1.0f))) * Mathf.Cos(2.0f * Pi * UnityEngine.Random.Range(0f, 1.0f));
        z = Mathf.Sqrt(-2.0f * Mathf.Log(UnityEngine.Random.Range(0.00001f, 1.0f))) * Mathf.Cos(2.0f * Pi * UnityEngine.Random.Range(0f, 1.0f));

        //ランジュバン方程式の解 〈(𝑥(𝑡) − 𝑥(0))^2〉 = 2𝐷|𝑡|
        x = Mathf.Sqrt(dt * 2 * D) * x / MCoefficient;
        y = Mathf.Sqrt(dt * 2 * D) * y / MCoefficient;
        z = Mathf.Sqrt(dt * 2 * D) * z / MCoefficient;
        //transform.Translate(x, y, z);

        RandomPow = new Vector3(UnityEngine.Random.Range(-pow, pow), UnityEngine.Random.Range(-pow, pow), UnityEngine.Random.Range(-pow, pow));
        rb.AddForce(RandomPow);

        //rb.AddForce(new Vector3(x, y, z) / KgCoefficient);
    }
}
