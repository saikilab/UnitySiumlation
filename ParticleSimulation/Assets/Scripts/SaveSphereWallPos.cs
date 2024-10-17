using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SaveSphereWallPos : MonoBehaviour
{
    bool calc;

    [Tooltip("保存の最大ステップ")]
    public int maxStep;
    int step;
    string dirN;

    [Tooltip("移動距離を保存する間隔")]
    public float saveLenStep;
    [Tooltip("軌跡を保存する間隔")]
    public float savePosStep;

    public GameObject[] Walls;
    int WallsLen;
    Vector3[] WallAVEPos;
    Vector3 DefaultWallAVEPos, nowWallAVEPos, nowWallAVEPos_Sum;

    int n;
    public MakeSphereWall makeSphereWall;

    private void Start()
    {
        Walls = makeSphereWall.triangleObjcts;
        WallsLen = Walls.Length;
        WallAVEPos = new Vector3[maxStep + 1];
    }

    private void FixedUpdate()
    {
        if (calc)
        {
            CalcWallAVEPos();
            if (step >= maxStep)
            {
                calc = false;
                SaveWall();
            }

            step++;
        }
    }

    public void OnCalc()
    {
        Initialize();
        calc = true;
        Debug.Log(dirN + "へ壁の位置の保存を開始しました");
    }

    void Initialize()
    {
        dirN = DateTime.Now.ToString("MMdd") + "_" + DateTime.Now.ToString("HHmmss");
        Directory.CreateDirectory(dirN);

        step = 0;
    }

    void CalcWallAVEPos()
    {
        nowWallAVEPos_Sum = Vector3.zero;
        for (n = 0; n < WallsLen; n++)
        {
            nowWallAVEPos_Sum += Walls[n].transform.position;
        }
        nowWallAVEPos = nowWallAVEPos_Sum / WallsLen;
        WallAVEPos[step] = nowWallAVEPos;
    }

    public void SaveWall()
    {
        int i, s;
        float t = 0, dt = Time.deltaTime;
        string fileName = dirN + "_Wall_MoveLength.csv";
        StreamWriter swW = new StreamWriter(dirN + "/" + fileName);
        DefaultWallAVEPos = WallAVEPos[0];
        s = (int)(saveLenStep / dt);
        for (i = s; i <= maxStep; i += s)
        {
            nowWallAVEPos = WallAVEPos[i] - DefaultWallAVEPos;
            t = dt * i;
            swW.WriteLine(t.ToString("F3") + ", " + Vector3.SqrMagnitude(nowWallAVEPos));
        }
        swW.Close();

        t = 0;
        fileName = dirN + "_Wall_AVEPosition.csv";
        swW = new StreamWriter(dirN + "/" + fileName);
        DefaultWallAVEPos = WallAVEPos[0];
        s = (int)(savePosStep / dt);
        for (i = s; i <= maxStep; i += s)
        {
            nowWallAVEPos = WallAVEPos[i] - DefaultWallAVEPos;
            t = dt * i;
            swW.WriteLine(t.ToString("F3") + ", " + nowWallAVEPos.x + ", " + nowWallAVEPos.y + ", " + nowWallAVEPos.z);
        }
        swW.Close();

        Debug.Log(dirN + "への保存を完了しました");
    }
}
