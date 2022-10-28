using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetParticle : MonoBehaviour
{
    public int setParticleN;
    public bool set;
    public GameObject Wall;
    public float diameter;
    private int i, j, k, n, maxParticle;
    private float XBorder, YBorder;
    public GameObject MagneticParticlePrefab;
    public GameObject[] MagneticParticle;

    void Awake()
    {
        if (set)
        {
            SetParticleN(setParticleN);
        }
    }

    private void SetParticleN(int N)
    {
        if(Wall.tag == "Square")
        {
            GameObject X_Wall1 = Wall.transform.Find("X_Wall1").gameObject;
            GameObject X_Wall2 = Wall.transform.Find("X_Wall2").gameObject;
            GameObject Y_Wall1 = Wall.transform.Find("Y_Wall1").gameObject;
            GameObject Y_Wall2 = Wall.transform.Find("Y_Wall2").gameObject;
            float width, height;
            XBorder = X_Wall1.transform.position.x - X_Wall1.transform.localScale.x / 2;
            YBorder = Y_Wall1.transform.position.y - Y_Wall1.transform.localScale.y / 2;
            //width = Mathf.Abs(X_Wall1.transform.position.x - X_Wall1.transform.localScale.x / 2 - X_Wall2.transform.position.x + X_Wall2.transform.localScale.x);
            //height = Mathf.Abs(Y_Wall1.transform.position.y - Y_Wall1.transform.localScale.y / 2 - Y_Wall2.transform.position.y + Y_Wall2.transform.localScale.y);
            width = XBorder * 2;
            height = YBorder * 2;
            maxParticle = (int)(height / diameter) * (int)(width / diameter);
            if (maxParticle < N)
            {
                N = maxParticle;
                Debug.Log("粒子数が上限を超えているので粒子数を" + N + "へ自動的に変更しました");
            }
            MagneticParticle = new GameObject[N];

            n = 0;
            for (i = 0; i < (int)(height / diameter); i++)
            {
                for (j = 0; j < (int)(width / diameter); j++)
                {
                    if (n < N)
                    {
                        MagneticParticle[n] = Instantiate(MagneticParticlePrefab);
                        MagneticParticle[n].transform.SetParent(this.transform);
                        MagneticParticle[n].transform.position = new Vector3(-XBorder + (float)i * diameter + diameter / 2, -YBorder + (float)j * diameter + diameter / 2, 0);
                        n++;
                    }
                }
            }
        }
        //if(Wall.tag == "Sphere")
        //{
        //    //int size = (int)(Wall.transform.localScale.x / (diameter / 2)); ;
        //    //float x, y, z;
        //    //Vector3 setPos;
        //    //for (i = -size; i <= size; i++) //z方向
        //    //{
        //    //    for (j = -size; j <= size; j++) //y方向
        //    //    {
        //    //        for (k = -size; k <= size; k++) //x方向
        //    //        {
        //    //            MagneticParticle[n] = Instantiate(MagneticParticlePrefab);
        //    //            MagneticParticle[n].transform.SetParent(this.transform);
        //    //            setPos = new Vector3(x*k, y*j, z*i);
        //    //            MagneticParticle[n].transform.position = setPos;
        //    //        }
        //    //    }
        //    //}

        //}
    }
}
