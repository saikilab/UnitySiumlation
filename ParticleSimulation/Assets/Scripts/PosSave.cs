using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PosSave : MonoBehaviour
{
    bool calc;
    public int maxStep;
    int step;
    string dirN;
    public float saveLenTime, savePosTime;

    Vector3[] Pos;
    Vector3 DefaultPos, nowPos;

    int n;


    private void Start()
    {
        Pos = new Vector3[maxStep + 1];
    }

    private void FixedUpdate()
    {
        if (calc)
        {
            Pos[step] = transform.position; 
            if (step >= maxStep)
            {
                calc = false;
                SavePos();
            }

            step++;
        }
    }

    public void SaveStart()
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

    void SavePos()
    {
        int i, s;
        float t = 0, dt = Time.deltaTime;
        string fileName = dirN + "_MoveLength.csv";
        StreamWriter swW = new StreamWriter(dirN + "/" + fileName);
        DefaultPos = Pos[0];
        s = (int)(saveLenTime / dt);
        for (i = s; i <= maxStep; i += s)
        {
            nowPos = Pos[i] - DefaultPos;
            t = dt * i;
            swW.WriteLine(t.ToString("F3") + ", " + Vector3.SqrMagnitude(nowPos));
        }
        swW.Close();

        t = 0;
        fileName = dirN + "_Pos.csv";
        swW = new StreamWriter(dirN + "/" + fileName);
        DefaultPos = Pos[0];
        s = (int)(savePosTime / dt);
        for (i = s; i <= maxStep; i += s)
        {
            nowPos = Pos[i] - DefaultPos;
            t = dt * i;
            swW.WriteLine(t.ToString("F3") + ", " + nowPos.x + ", " + nowPos.y + ", " + nowPos.z);
        }
        swW.Close();

        Debug.Log(dirN + "への保存を完了しました");
    }
}
