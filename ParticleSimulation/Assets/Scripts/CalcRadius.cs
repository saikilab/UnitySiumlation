using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalcRadius : MonoBehaviour
{
    public Transform[] WallDotTrans;

    public void ClickedCalcRadius()
    {
        int i = 0;
        Vector3 SumPos = new Vector3(0, 0, 0);
        Vector3 AvePos = new Vector3(0, 0, 0);
        for (i = 0; i < WallDotTrans.Length; i++)
        {
            SumPos += WallDotTrans[i].position;
        }
        AvePos = SumPos / i;

        float Sum = 0f;
        for (i = 0; i < WallDotTrans.Length; i++)
        {
            Sum += (WallDotTrans[i].position - AvePos).magnitude;
        }
        float Radius = Sum / i;

        Debug.Log("半径：" + Radius);
    }
}
