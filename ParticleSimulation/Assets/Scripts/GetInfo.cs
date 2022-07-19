using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetInfo : MonoBehaviour
{
    public GameObject Wall;
    public Text WallInfo, ParticleN, MagneticField;
    private float size;
    bool delay;

    private void Start()
    {
        delay = true;
    }

    private void Update()
    {
        if (delay)
        {
            delay = false;
            if (Wall.tag == "Square")
            {
                size = Wall.GetComponent<WallController>().thickness;
                WallInfo.text = "形状：正方形\n厚さ：" + size;
            }
            if (Wall.tag == "Sphere")
            {
                size = Wall.transform.localScale.x;
                WallInfo.text = "形状：球体\n直径：" + size;
            }
            ParticleN.text = "粒子数：" + Wall.GetComponent<MagnetDipole>().particleNumber;
            UpdateMagInfo();
        }
    }

    public void UpdateMagInfo()
    {
        MagneticField.text = "磁場：" + Wall.GetComponent<MagnetDipole>().H_pow;
    }
}
