using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighPos : MonoBehaviour
{
    Transform trn;
    float y, y_before;
    bool isFall;
    void Start()
    {
        trn = this.transform;
    }

    //void FixedUpdate()
    //{
    //    float y = trn.position.y;

    //    if(y < y_before & !isFall) //落下初めに最高到達点を表示
    //    {
    //        Debug.Log(Time.deltaTime + "：" +y_before);
    //        isFall = true;
    //    } else if (y_before < y) //上昇中はisFallをfalseへ
    //    {
    //        isFall = false;
    //    }
    //    y_before = y;
    //}

    void FixedUpdate()
    {
        float y = trn.position.y;

        if (y_before < y & isFall) //上昇初めに最高到達点を表示
        {
            Debug.Log(Time.deltaTime + "：" + y_before);
            isFall = false;
        }
        else if (y < y_before) //落下中はisFallをtrueeへ
        {
            isFall = true;
        }
        y_before = y;
    }
}
