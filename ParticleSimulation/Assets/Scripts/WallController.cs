using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class WallController : MonoBehaviour
{
    //no dimention value
    private const float KgCoefficient = 0.00000000000001f;
    private const float MCoefficient = 0.000001f;

    //simulation value
    private float DPX1, DPX2, DPY1, DPY2, dt; //defaultPosition
    private Rigidbody X1, X2, Y1, Y2;
    public GameObject X_Wall1, X_Wall2, Y_Wall1, Y_Wall2, Z_Wall1, Z_Wall2;
    public GameObject PressM;
    private Rigidbody PM;
    public int step;
    public int ChangeStep;
    public int repeatNumber;
    [HideInInspector] public int repeatCounter;
    public float thickness;
    public float MoveSpeed;
    private Vector3 RecPow;
    public Vector3 MovePower, V0;
    public bool useTimeWall, usePistonWall, useRepeatWall, useEFM, usePressMachine; //壁の挙動 切り替え

    //Save
    //public bool useSaveWall; //壁の保存　切り替え
    [HideInInspector] public string[] WallPosition;
    [HideInInspector] public string[] Fp;

    private void Start()
    {
        DPX1 = X_Wall1.transform.position.x;
        DPX2 = X_Wall2.transform.position.x;
        DPY1 = Y_Wall1.transform.position.y;
        DPY2 = Y_Wall2.transform.position.y;
        Z_Wall1.transform.localPosition = new Vector3(0, 0, (thickness+Z_Wall1.transform.localScale.z) / 2f);
        Z_Wall2.transform.localPosition = new Vector3(0, 0, -(thickness+Z_Wall2.transform.localScale.z) / 2f);
        if(this.tag != "Film")
        {
            X1 = X_Wall1.GetComponent<Rigidbody>();
            X2 = X_Wall2.GetComponent<Rigidbody>();
            Y1 = Y_Wall1.GetComponent<Rigidbody>();
            Y2 = Y_Wall2.GetComponent<Rigidbody>();
        } else {
            PM = PressM.GetComponent<Rigidbody>();
            X_Wall1 = PressM;
            DPX1 = PM.transform.position.x;
        }
        step = 0;

        WallPosition = new string[SimulationController.MaxStep]; //最大ステップを10000と仮置き
        Fp = new string[SimulationController.MaxStep];
    }

    void FixedUpdate()
    {
        dt = Time.deltaTime;

        step = SimulationController.Step;

        if (this.tag != "Film")
        {
            StopOverWall();

            //左右で位置制御
            if (Input.GetKey(KeyCode.RightArrow))
            {
                MoveWallPos(MoveSpeed);
            }
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                MoveWallPos(-MoveSpeed);
            }

            //上下で力制御
            if (Input.GetKey(KeyCode.UpArrow))
            {
                AdmitMoveWall();
                MoveWallF(MovePower);
            }
            else if (Input.GetKey(KeyCode.DownArrow))
            {
                MoveWallF(-MovePower);
            }

            //保存処理
            //右壁が受ける力を取得、初期化
            RecPow = WallReceivePow.Fp;
            WallReceivePow.Fp = new Vector3(0, 0, 0);
            if (useTimeWall)
            {
                //if (useSaveWall)
                SaveWall();
                SaveFp();
                TimeWall();
            }
            else if (usePistonWall)
            {
                //if (useSaveWall)
                SaveWall();
                SaveFp();
                PistonWall();
            }
            else if (useRepeatWall)
            {
                //if (useSaveWall)
                SaveWall();
                SaveFp();
                RepeatPosWall();
            }
            else if (useEFM)
            {
                //if (useSaveWall)
                SaveWall();
                SaveFp();
                ElasticForceMeasure();
            }
        } else
        {
            RecPow = WallReceivePow.Fp;
            WallReceivePow.Fp = new Vector3(0, 0, 0);
            if (usePressMachine)
            {
                SaveWall();
                SaveFp();
                Press();
            } else
            {
                PM.isKinematic = true;
            }
        }
    }

    public void MoveWallPos(float moveSpeed) //位置制御
    {
        X_Wall1.transform.Translate(-moveSpeed, 0, 0);
        X_Wall2.transform.Translate(moveSpeed, 0, 0);
        Y_Wall1.transform.Translate(0, -moveSpeed, 0);
        Y_Wall2.transform.Translate(0, moveSpeed, 0);
    }

    public void MoveWallF(Vector3 movePower) //力制御
    {
        X1.AddForce(-movePower, ForceMode.VelocityChange);
        X2.AddForce(movePower, ForceMode.VelocityChange);
        Y1.AddForce(-movePower, ForceMode.VelocityChange);
        Y2.AddForce(movePower, ForceMode.VelocityChange);
    }

    public void StopOverWall() //初期位置を超える場合は壁を停止
    {
        if (DPX1 <= X_Wall1.transform.position.x)
            X1.isKinematic = true;
        if (DPX2 >= X_Wall2.transform.position.x)
            X2.isKinematic = true;
        if (DPY1 <= Y_Wall1.transform.position.y)
            Y1.isKinematic = true;
        if (DPY2 >= Y_Wall2.transform.position.y)
            Y2.isKinematic = true;
    }

    public void AdmitMoveWall() //壁の移動を許可
    {
        X1.isKinematic = false;
        X2.isKinematic = false;
        Y1.isKinematic = false;
        Y2.isKinematic = false;
    }

    public void SaveWall()
    {
        string[] s1 = {string.Format("{0,5}", step.ToString()), string.Format("{0,8}", X_Wall1.transform.position.x.ToString("F4")), string.Format("{0,8}", X_Wall2.transform.position.x.ToString("F4")), string.Format("{0,8}", Y_Wall1.transform.position.y.ToString("F4")), string.Format("{0,8}", Y_Wall2.transform.position.y.ToString("F4")) };
        string s2 = string.Join(" ", s1);
        WallPosition[step] = s2;
    }

    public void SaveFp()
    {
        string[] s1 = {(step*dt).ToString("F4"), ((-RecPow.x/dt)*(KgCoefficient/MCoefficient)).ToString()};
        string s2 = string.Join(",", s1);
        Fp[step] = s2;
    }

    public void TimeWall() //指定ステップまで力で圧縮、拡大
    {
        if (step == ChangeStep) //切り替えの瞬間に一度力を停止
        {
            X1.isKinematic = true;
            X2.isKinematic = true;
            Y1.isKinematic = true;
            Y2.isKinematic = true;
        }

        if (step < ChangeStep)
        {
            AdmitMoveWall();
            MoveWallF(MovePower);
        }
        else
        {
            AdmitMoveWall();
            MoveWallF(-MovePower);
            StopOverWall();

            //全ての壁が初期位置を超えたら停止
            if (DPX1 <= X_Wall1.transform.position.x && DPX2 >= X_Wall2.transform.position.x && DPY1 <= Y_Wall1.transform.position.y && DPY2 >= Y_Wall2.transform.position.y)
            {
                Debug.Log("壁が初期位置へ戻ったため停止しました");
                SimulationController.endSimulation = true;
                useTimeWall = false;
                //useSaveWall = false;
                SetDP();
            }
        }
    }


    public void PistonWall() //指定ステップまで力で圧縮、放置
    {
        X1.isKinematic = false;
        if (step < ChangeStep) //圧縮
        {
            X1.AddForce(-MovePower);
        }
        else //放置
        {
            //右壁が初期位置を超えたら停止
            if (DPX1 < X_Wall1.transform.position.x)
            {
                Debug.Log("壁が初期位置へ戻ったため停止しました");
                SimulationController.endSimulation = true;
                usePistonWall = false;
                //useSaveWall = false;
                SetDP();
            }
        }
    }

    public void ElasticForceMeasure() //弾性力測定
    {
        X1.isKinematic = false;
        if(step == 0) //初速
        {
            X1.AddForce(V0, ForceMode.VelocityChange);
        }

        if (step < ChangeStep) //圧縮
        {
            Vector3 V1 = V0 - X1.velocity;
            X1.AddForce(V1, ForceMode.VelocityChange);
            X1.AddForce(RecPow, ForceMode.Impulse);
        }
        else //放置
        {
            //右壁が初期位置を超えたら停止
            if (DPX1 < X_Wall1.transform.position.x)
            {
                Debug.Log("壁が初期位置へ戻ったため停止しました");
                SimulationController.endSimulation = true;
                useEFM = false;
                //useSaveWall = false;
                SetDP();
            }
        }
    }

    public void RepeatPosWall() //指定回数
    {
        repeatCounter = step / (2 * ChangeStep);

        if (step - ChangeStep * repeatCounter * 2 < ChangeStep)
            MoveWallPos(MoveSpeed);
        else
            MoveWallPos(-MoveSpeed);

        //指定回数実行したら停止
        if(repeatCounter == repeatNumber)
        {
            Debug.Log("指定回数実行したため停止しました");
            SimulationController.endSimulation = true;
            useRepeatWall = false;
            //useSaveWall = false;
            SetDP();
        }
    }

    public void Press()
    {
        PM.isKinematic = false;
        if (step == 0) //初速
        {
            PM.AddForce(V0, ForceMode.VelocityChange);
        }

        if (step < ChangeStep) //圧縮
        {
            Vector3 V1 = V0 - PM.velocity;
            PM.AddForce(V1, ForceMode.VelocityChange);
            PM.AddForce(RecPow, ForceMode.Impulse);
        }
        else if(step == ChangeStep)
        {
            PM.isKinematic = true;
        }
        else //放置
        {
            //右壁が初期位置を超えたら停止
            if (DPX1 < X_Wall1.transform.position.x)
            {
                Debug.Log("壁が初期位置へ戻ったため停止しました");
                SimulationController.endSimulation = true;
                usePressMachine = false;
                //useSaveWall = false;
                SetDP();
                repeatCounter++;
            }
        }
    }

    void SetDP()
    {
        X_Wall1.transform.position = new Vector3(DPX1, 0, 0);
        X_Wall2.transform.position = new Vector3(DPX2, 0, 0);
        Y_Wall1.transform.position = new Vector3(0, DPY1, 0);
        Y_Wall2.transform.position = new Vector3(0, DPY2, 0);
    }
}