using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressController : MonoBehaviour
{
    public GameObject PressWall;
    Rigidbody PressWall_rb;
    public bool usePress;
    public Vector3 V0;
    public float ChangeStep;
    float step;
    Vector3 PW_DefaultPosition;

    private void Start()
    {
        step = 0;
        PressWall_rb = PressWall.GetComponent<Rigidbody>();
        PW_DefaultPosition = PressWall.transform.position;
    }

    private void FixedUpdate()
    {
        if (usePress)
        {
            step++;
            ElasticForceMeasure();
        }
    }

    public void ElasticForceMeasure() //弾性力測定
    {
        //PressWall_rb.isKinematic = false;
        if(step == 0) //初速
        {
            PressWall.transform.Translate(V0);
            //PressWall_rb.AddForce(V0, ForceMode.VelocityChange);
        }

        if (step < ChangeStep) //圧縮
        {
            PressWall.transform.Translate(V0);
            //Vector3 V1 = V0 - PressWall_rb.velocity;
            //PressWall_rb.AddForce(V1, ForceMode.VelocityChange);
            //PressWall_rb.AddForce(WallReceivePow.Fp, ForceMode.Impulse);
            //Debug.Log("V" + PressWall_rb.velocity);
            //Debug.Log(V1);
            //Debug.Log(WallReceivePow.Fp);
        }
        else //放置
        {
            PressWall_rb.isKinematic = false;
            //右壁が初期位置を超えたら停止
            if (PW_DefaultPosition.x < PressWall.transform.position.x)
            {
                Debug.Log("壁が初期位置へ戻ったため停止しました");
                //SimulationController.endSimulation = true;
                usePress = false;
                step = 0;
                PressWall.transform.position = PW_DefaultPosition;
                PressWall_rb.isKinematic = true;
                //useSaveWall = false;
                //SetDP();
            }
        }
    }
}
