using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetParticle : MonoBehaviour
{
    public bool set;
    public GameObject Wall;
    public float diameter;
    private int i, j, k, n, totalParticle;
    public GameObject MagneticParticlePrefab;
    public GameObject[] MagneticParticle;

    void Awake()
    {
        if (set)
        {
            SetFullParticle();
        }
    }

    private void SetFullParticle()
    {
        if(Wall.tag == "Square")
        {
            GameObject X_Wall_1 = Wall.transform.Find("X_Wall1").gameObject;
            GameObject X_Wall_2 = Wall.transform.Find("X_Wall2").gameObject;
            GameObject Y_Wall_1 = Wall.transform.Find("Y_Wall1").gameObject;
            GameObject Y_Wall_2 = Wall.transform.Find("Y_Wall2").gameObject;
            float width, height;
            width = Mathf.Abs(X_Wall_1.transform.position.x - X_Wall_2.transform.position.x);
            height = Mathf.Abs(Y_Wall_1.transform.position.y - Y_Wall_2.transform.position.y);
            totalParticle = (int)((height / diameter) * (width / diameter));
            MagneticParticle = new GameObject[totalParticle];
            n = 0;
            for (i = 0; i < (int)(height / diameter); i++)
            {
                for (j = 0; j < (int)(width / diameter); j++)
                {
                    MagneticParticle[n] = Instantiate(MagneticParticlePrefab);
                    MagneticParticle[n].transform.SetParent(this.transform);
                    MagneticParticle[n].transform.position = new Vector3(X_Wall_2.transform.position.x + (float)i * diameter + diameter / 2, Y_Wall_2.transform.position.y + (float)j * diameter + diameter / 2, 0);
                    n++;
                }
            }
        }
        if(Wall.tag == "Sphere")
        {
            //int size = (int)(Wall.transform.localScale.x / (diameter / 2)); ;
            //float x, y, z;
            //Vector3 setPos;
            //for (i = -size; i <= size; i++) //z方向
            //{
            //    for (j = -size; j <= size; j++) //y方向
            //    {
            //        for (k = -size; k <= size; k++) //x方向
            //        {
            //            MagneticParticle[n] = Instantiate(MagneticParticlePrefab);
            //            MagneticParticle[n].transform.SetParent(this.transform);
            //            setPos = new Vector3(x*k, y*j, z*i);
            //            MagneticParticle[n].transform.position = setPos;
            //        }
            //    }
            //}

        }
    }
}
