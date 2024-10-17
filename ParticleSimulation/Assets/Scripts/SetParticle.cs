using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetParticle : MonoBehaviour
{
    public NewParticleController newParticleController;

    public int setParticleN;
    public bool set, set2, dimer, x, y, z;
    public GameObject Wall;
    float diameter;
    private int i, j, k, l, n, maxParticle;
    public float XBorder, YBorder;
    public GameObject MagneticParticlePrefab;
    [HideInInspector] public GameObject[] MagneticParticle;
    public float circleWall_Radius;

    public Vector3 setPos;

    void Awake()
    {
        if (set)
        {
            diameter = newParticleController.diameter;
            if (set2)
                SetParticleN2(setParticleN);
            else
                SetParticleN(setParticleN);
            if (x)
            {
                transform.Rotate(0, 0, 90);
            } else if (y)
            {
                transform.Rotate(0, 0, 0);
            }
            else if (z)
            {
                transform.Rotate(90, 0, 0);
            }

            transform.position = setPos;
        }
    }

    private void SetParticleN(int N)
    {
        if (Wall.tag == "Square")
        {
            //GameObject X_Wall1 = Wall.transform.Find("X_Wall1").gameObject;
            //GameObject X_Wall2 = Wall.transform.Find("X_Wall2").gameObject;
            //GameObject Y_Wall1 = Wall.transform.Find("Y_Wall1").gameObject;
            //GameObject Y_Wall2 = Wall.transform.Find("Y_Wall2").gameObject;
            //XBorder = X_Wall1.transform.position.x - X_Wall1.transform.localScale.x / 2;
            //YBorder = Y_Wall1.transform.position.y - Y_Wall1.transform.localScale.x / 2;
            float width, height;
            
            //密着
            //width = Mathf.Abs(X_Wall1.transform.position.x - X_Wall1.transform.localScale.x / 2 - X_Wall2.transform.position.x + X_Wall2.transform.localScale.x);
            //height = Mathf.Abs(Y_Wall1.transform.position.y - Y_Wall1.transform.localScale.y / 2 - Y_Wall2.transform.position.y + Y_Wall2.transform.localScale.y);

            //マージンあり?
            width = XBorder * 2f;
            height = YBorder * 2f;

            maxParticle = (int)(height / diameter) * (int)(width / diameter);
            if (maxParticle < N)
            {
                N = maxParticle;
                Debug.Log("粒子数が上限を超えているので粒子数を" + N + "へ自動的に変更しました");
            }
            MagneticParticle = new GameObject[N];

            int sideSize = Mathf.FloorToInt(Mathf.Sqrt(N * diameter * diameter));
            float pseudoBorder = sideSize / 2f;

            //n = 0;
            //for (i = 0; i < (int)(width / diameter); i++)
            //{
            //    for (j = 0; j < (int)(height / diameter); j++)
            //    {
            //        if (n < N)
            //        {
            //            MagneticParticle[n] = Instantiate(MagneticParticlePrefab);
            //            MagneticParticle[n].transform.SetParent(this.transform);
            //            MagneticParticle[n].transform.position = new Vector3(-XBorder + (float)i * diameter + diameter / 2, -YBorder + (float)j * diameter + diameter / 2, 0);
            //            n++;
            //        }
            //    }
            //}

            n = 0;
            for (i = 0; i < Mathf.CeilToInt(sideSize / diameter); i++)
            {
                for (j = 0; j < Mathf.CeilToInt(sideSize / diameter); j++)
                {
                    if (n < N)
                    {
                        MagneticParticle[n] = Instantiate(MagneticParticlePrefab);
                        MagneticParticle[n].transform.localScale = new Vector3(diameter, diameter, diameter);
                        MagneticParticle[n].transform.SetParent(this.transform);
                        MagneticParticle[n].transform.position = new Vector3(-pseudoBorder + (float)i * diameter + diameter / 2, 0, -pseudoBorder + (float)j * diameter + diameter / 2);
                        n++;
                    }
                }
            }
        }
        if (Wall.tag == "Circle")
        {
            int maxSetNumber = (int)(Mathf.Sqrt(circleWall_Radius*circleWall_Radius - diameter*diameter) / diameter);
            int tmpSetNumber;
            Vector3 setPos = new Vector3(0,0,0);

            for (i = 0; i < maxSetNumber; i++)
            {
                tmpSetNumber = (int)(Mathf.Sqrt(circleWall_Radius * circleWall_Radius - (i + 1) * diameter * (i + 1) * diameter) / diameter);

                for (j = 0; j < tmpSetNumber; j++)
                {
                    for (k = 0; k < 4; k++)
                    {
                        n++;
                    }
                }
            }

            if (n < N)
            {
                MagneticParticle = new GameObject[n];
                Debug.Log("粒子数が上限を超えているので粒子数を" + n + "へ自動的に変更しました");
            } else if(0 <= N)
            {
                MagneticParticle = new GameObject[N];
            }

            n = 0;
            for (i = 0; i < maxSetNumber; i++)
            {
                tmpSetNumber = (int)(Mathf.Sqrt(circleWall_Radius * circleWall_Radius - (i + 1) * diameter * (i + 1) * diameter) / diameter);

                for (j = 0; j < tmpSetNumber; j++)
                {
                    for(k = 0; k < 4; k++)
                    {
                        if (n < N)
                        {
                            if (k == 0)
                                setPos = new Vector3(diameter / 2 + i * diameter, 0, diameter / 2 + j * diameter);
                            if (k == 1)
                                setPos = new Vector3(diameter / 2 + i * diameter, 0, -diameter / 2 + -j * diameter);
                            if (k == 2)
                                setPos = new Vector3(-diameter / 2 + -i * diameter, 0, diameter / 2 + j * diameter);
                            if (k == 3)
                                setPos = new Vector3(-diameter / 2 + -i * diameter, 0, -diameter / 2 + -j * diameter);

                            MagneticParticle[n] = Instantiate(MagneticParticlePrefab);
                            MagneticParticle[n].transform.localScale = new Vector3(diameter, diameter, diameter);
                            MagneticParticle[n].transform.SetParent(this.transform);
                            MagneticParticle[n].transform.position = setPos;
                            n++;
                        }
                        else
                        {
                            return;
                        }
                    }                    
                }
            }
        }
    }

    void SetParticleN2(int N)
    {
        int maxSetNumber = (int)(Mathf.Sqrt(circleWall_Radius * circleWall_Radius - diameter * diameter) / diameter);
        int tmpSetNumber;
        Vector3 setPos = new Vector3(0, 0, 0);

        for (i = 0; i < maxSetNumber; i++)
        {
            tmpSetNumber = (int)(Mathf.Sqrt(circleWall_Radius * circleWall_Radius - (i + 1) * diameter * (i + 1) * diameter) / diameter);

            for (j = 0; j < tmpSetNumber; j++)
            {
                for (k = 0; k < 4; k++)
                {
                    for(l = -1; l < 2; l += 2)
                        n++;
                }
            }
        }

        if (n < N)
        {
            MagneticParticle = new GameObject[n];
            Debug.Log("粒子数が上限を超えているので粒子数を" + n + "へ自動的に変更しました");
        }
        else if (0 <= N)
        {
            MagneticParticle = new GameObject[N];
        }

        n = 0;
        for (i = 0; i < maxSetNumber; i++)
        {
            tmpSetNumber = (int)(Mathf.Sqrt(circleWall_Radius * circleWall_Radius - (i + 1) * diameter * (i + 1) * diameter) / diameter);

            for (j = 0; j < tmpSetNumber; j++)
            {
                for (k = 0; k < 4; k++)
                {
                    for(l = -1; l < 2; l += 2)
                    {
                        if (n < N)
                        {
                            if (k == 0)
                                setPos = new Vector3(diameter / 2 + i * diameter, l * diameter / 2, diameter / 2 + j * diameter);
                            if (k == 1)
                                setPos = new Vector3(diameter / 2 + i * diameter, l * diameter / 2, -diameter / 2 + -j * diameter);
                            if (k == 2)
                                setPos = new Vector3(-diameter / 2 + -i * diameter, l * diameter / 2, diameter / 2 + j * diameter);
                            if (k == 3)
                                setPos = new Vector3(-diameter / 2 + -i * diameter, l * diameter / 2, -diameter / 2 + -j * diameter);

                            MagneticParticle[n] = Instantiate(MagneticParticlePrefab);
                            MagneticParticle[n].transform.localScale = new Vector3(diameter, diameter, diameter);
                            MagneticParticle[n].transform.SetParent(this.transform);
                            MagneticParticle[n].transform.position = setPos;
                            if (dimer && l > 0) //dimerオンで２個目なら結合
                            {
                                MagneticParticle[n].AddComponent<FixedJoint>().connectedBody = MagneticParticle[n-1].GetComponent<Rigidbody>();
                            }
                            n++;
                        }
                        else
                        {
                            return;
                        }
                    }
                }
            }
        }
    }
}
