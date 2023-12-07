using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetInfo : MonoBehaviour
{
    ParticleController particleController;
    WallController wallController;
    public GameObject Wall;
    public Rigidbody RightWall;
    public Text WallInfo, ParticleN, MagneticField, WallSpeed, Step, RepeatNumber, TimeText;
    private float size, F, dt;
    bool delay;

    private void Start()
    {
        particleController = Wall.GetComponent<ParticleController>();
        wallController = Wall.GetComponent<WallController>();
        delay = true;
        dt = Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (delay)
        {
            delay = false;
            if (Wall.tag == "Square")
            {
                size = wallController.thickness;
                WallInfo.text = "形状：正方形\n厚さ：" + size;
            }
            if (Wall.tag == "Film")
            {
                size = wallController.thickness;
                WallInfo.text = "形状：膜\n厚さ：" + size;
            }
            if (Wall.tag == "Sphere")
            {
                size = Wall.transform.localScale.x;
                WallInfo.text = "形状：球体\n直径：" + size;
            }
            ParticleN.text = "粒子数：" + particleController.particleNumber;
        }

        MagneticField.text = "磁場：" + particleController.H_pow;

        WallSpeed.text = "右壁の速度：" + RightWall.velocity.x;

        Step.text = "ステップ：" + SimulationController.Step;

        RepeatNumber.text = "繰り返し回数：" + wallController.repeatCounter;
    }
}
