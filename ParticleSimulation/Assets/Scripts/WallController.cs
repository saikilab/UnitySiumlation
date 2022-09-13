using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class WallController : MonoBehaviour
{
    public Vector3 F, change_Z_Wall;
    public GameObject X_Wall1, X_Wall2, Y_Wall1, Y_Wall2, Z_Wall1, Z_Wall2;
    public float thickness;
    public float MoveSpeed;

    private float DPX1, DPX2, DPY1, DPY2; //defaultPosition
    private Rigidbody X1, X2, Y1, Y2;
    public bool timeWall, pistonWall;

    //Save
    public bool switch_save_wall;
    [HideInInspector]
    public string dirN;
    public StreamWriter sw;
    public string[] WallPosition;
    public bool didSave;

    //step
    public int step;
    public int ChangeStep;

    private void Start()
    {
        DPX1 = X_Wall1.transform.position.x;
        DPX2 = X_Wall2.transform.position.x;
        DPY1 = Y_Wall1.transform.position.y;
        DPY2 = Y_Wall2.transform.position.y;
        Z_Wall1.transform.localPosition = new Vector3(0, 0, thickness / 2f);
        Z_Wall2.transform.localPosition = new Vector3(0, 0, -thickness / 2f);
        X1 = X_Wall1.GetComponent<Rigidbody>();
        X2 = X_Wall2.GetComponent<Rigidbody>();
        Y1 = Y_Wall1.GetComponent<Rigidbody>();
        Y2 = Y_Wall2.GetComponent<Rigidbody>();

        if (timeWall)
            step = 0;

        if (switch_save_wall)
        {
            dirN = DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString();
            Directory.CreateDirectory(@dirN);

            string fileName = dirN + "/Wall_Position.cdv";
            sw = new StreamWriter(@fileName);
        }

        WallPosition = new string[10000];
        didSave = false;
    }

    void FixedUpdate()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            X1.isKinematic = true;
            X_Wall1.transform.Translate(-MoveSpeed, 0, 0);
            X_Wall2.transform.Translate(MoveSpeed, 0, 0);
            Y_Wall1.transform.Translate(0, -MoveSpeed, 0);
            Y_Wall2.transform.Translate(0, MoveSpeed, 0);

            change_Z_Wall = Z_Wall1.transform.localScale;
            change_Z_Wall.x -= 2 * MoveSpeed;
            change_Z_Wall.y -= 2 * MoveSpeed;
            Z_Wall1.transform.localScale = change_Z_Wall;
            change_Z_Wall = Z_Wall2.transform.localScale;
            change_Z_Wall.x -= 2 * MoveSpeed;
            change_Z_Wall.y -= 2 * MoveSpeed;
            Z_Wall2.transform.localScale = change_Z_Wall;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (X_Wall1.transform.position.x < DPX1 && DPX2 < X_Wall2.transform.position.x)
            {
                X1.isKinematic = true;
                X_Wall1.transform.Translate(MoveSpeed, 0, 0);
                X_Wall2.transform.Translate(-MoveSpeed, 0, 0);
                Y_Wall1.transform.Translate(0, MoveSpeed, 0);
                Y_Wall2.transform.Translate(0, -MoveSpeed, 0);

                change_Z_Wall = Z_Wall1.transform.localScale;
                change_Z_Wall.x += 2 * MoveSpeed;
                change_Z_Wall.y += 2 * MoveSpeed;
                Z_Wall1.transform.localScale = change_Z_Wall;
                change_Z_Wall = Z_Wall2.transform.localScale;
                change_Z_Wall.x += 2 * MoveSpeed;
                change_Z_Wall.y += 2 * MoveSpeed;
                Z_Wall2.transform.localScale = change_Z_Wall;
            }
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            //上ボタンで縮小
            //移動可能へ
            X1.isKinematic = false;
            X2.isKinematic = false;
            Y1.isKinematic = false;
            Y2.isKinematic = false;

            X1.AddForce(-F);
            X2.AddForce(F);
            Y1.AddForce(-F);
            Y2.AddForce(F);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            //下ボタンで拡大
            //初期値を超える場合は停止
            if (DPX1 < X_Wall1.transform.position.x)
                X1.isKinematic = true;
            else
                X1.isKinematic = false;
            if (DPX2 > X_Wall2.transform.position.x)
                X2.isKinematic = true;
            else
                X2.isKinematic = false;
            if (DPY1 < Y_Wall1.transform.position.y)
                Y1.isKinematic = true;
            else
                Y1.isKinematic = false;
            if (DPY2 > Y_Wall2.transform.position.y)
                Y2.isKinematic = true;
            else
                Y2.isKinematic = false;

            X1.AddForce(F);
            X2.AddForce(-F);
            Y1.AddForce(F);
            Y2.AddForce(-F);
        }
        //else
        //{
        //    //ボタンを押していない時は動かない
        //    X1.isKinematic = true;
        //    X2.isKinematic = true;
        //    Y1.isKinematic = true;
        //    Y2.isKinematic = true;
        //}

        if (timeWall)
        {
            if (switch_save_wall)
            {
                SaveWall();
            }

            TimeWall();
        }
        else if (pistonWall)
        {
            if (switch_save_wall)
            {
                SaveWall();
            }

            PistonWall();
        }
    }

    public void SaveWall()
    {
        string[] s1 = {string.Format("{0,5}", step.ToString()), string.Format("{0,8}", X_Wall1.transform.position.x.ToString("F4")), string.Format("{0,8}", X_Wall2.transform.position.x.ToString("F4")), string.Format("{0,8}", Y_Wall1.transform.position.y.ToString("F4")), string.Format("{0,8}", Y_Wall2.transform.position.y.ToString("F4")) };
        string s2 = string.Join(" ", s1);
        WallPosition[step] = s2;
    }

    public void TimeWall()
    {
        step++;
        if (step <= ChangeStep)
        {
            X1.isKinematic = false;
            X2.isKinematic = false;
            Y1.isKinematic = false;
            Y2.isKinematic = false;

            X1.AddForce(-F);
            X2.AddForce(F);
            Y1.AddForce(-F);
            Y2.AddForce(F);
        }
        else
        {
            if (DPX1 < X_Wall1.transform.position.x)
                X1.isKinematic = true;
            else
                X1.isKinematic = false;
            if (DPX2 > X_Wall2.transform.position.x)
                X2.isKinematic = true;
            else
                X2.isKinematic = false;
            if (DPY1 < Y_Wall1.transform.position.y)
                Y1.isKinematic = true;
            else
                Y1.isKinematic = false;
            if (DPY2 > Y_Wall2.transform.position.y)
                Y2.isKinematic = true;
            else
                Y2.isKinematic = false;

            X1.AddForce(F);
            X2.AddForce(-F);
            Y1.AddForce(F);
            Y2.AddForce(-F);
        }

        if (DPX1 < X_Wall1.transform.position.x && DPX2 > X_Wall2.transform.position.x && DPY1 < Y_Wall1.transform.position.y && DPY2 > Y_Wall2.transform.position.y)
        {
            int s;
            if (!didSave)
            {
                for (s = 0; s < step; s++)
                    sw.WriteLine(WallPosition[s]);

                sw.Close();
                didSave = true;
            }

            Debug.Log("壁が初期位置へ戻ったため停止しました");
            Debug.Break();
        }
    }

    public void PistonWall()
    {
        step++;
        if (step <= ChangeStep) //指定ステップまで押し込み
        {
            X1.isKinematic = false;

            X1.AddForce(-F);
        }
        else //指定ステップ以降は放置
        {
            X1.isKinematic = false;

            if (DPX1 < X_Wall1.transform.position.x) //初期位置を超えたら停止
            {
                if (!didSave)
                {
                    int s;
                    for (s = 0; s < step; s++)
                        sw.WriteLine(WallPosition[s]);

                    sw.Close();
                    didSave = true;
                }

                Debug.Log("壁が初期位置へ戻ったため停止しました");
                Debug.Break();
            }
        }
    }
}
