using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewGetInfo : MonoBehaviour
{
    NewParticleController newParticleController;
    SetHeight setHeight;
    public GameObject ParticleController, Wall, ZWall;
    //public Rigidbody RightWall;
    public Text WallTypeText, ParticleNumberText, MagneticPowerText, WallSpeedText, StepText, RepeatNumberText, TimeText;
    private float size, F, dt;
    bool delay;

    private void Start()
    {
        newParticleController = ParticleController.GetComponent<NewParticleController>();
        setHeight = ZWall.GetComponent<SetHeight>();
        delay = true;
        WallTypeText.text = "";
        ParticleNumberText.text = "";
        MagneticPowerText.text = "";
        WallSpeedText.text = "";
        StepText.text = "";
        RepeatNumberText.text = "";
    }

    private void FixedUpdate()
    {
        if (delay)
        {
            delay = false;
            if (Wall.tag == "Square")
            {
                size = setHeight.thickness;
                WallTypeText.text = "形状：正方形\n厚さ：" + size;
            }
            if (Wall.tag == "Circle")
            {
                size = Wall.transform.localScale.x;
                WallTypeText.text = "形状：円形\n直径：" + size;
            }
            ParticleNumberText.text = "粒子数：" + newParticleController.particleNumber;
        }

        MagneticPowerText.text = "磁場：" + newParticleController.H_pow;

        TimeText.text = "経過時間:" + SimulationController.Step * Time.deltaTime;

        //Step.text = "ステップ：" + SimulationController.Step;

        //WallSpeedText.text = "右壁の速度：" + RightWall.velocity.x;

        //RepeatNumber.text = "繰り返し回数：" + wallController.repeatCounter;
    }
}
