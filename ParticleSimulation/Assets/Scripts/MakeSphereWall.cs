using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class MakeSphereWall : MonoBehaviour
{
    public int grade;
    public float[] verticesMargin, objMargin;
    const int baseNum = 20;
    int[] triangleNum;
    int[] verticesNum;
    int[] sidesNum;

    public GameObject TrianglePrefab;
    GameObject nowTriangleObj;
    Vector3[] trianglePos;
    [HideInInspector] public GameObject[] triangleObjcts;

    Mesh mesh;
    MeshCollider meshCollider;
    public Vector3[] BaseVerticesPos;
    Vector3[] VerticesPos;
    Vector3[] newVectors = new Vector3[4];
    Vector3 centralPosition;
    float topPos_a;
    bool[,] AdjacentVertices;

    int[] newTriangles = new int[24];

    int i, j, k;
    public float diameter;
    float radius;

    public bool useGravity, useSpringJoint, useHingeJoint, useHingeSpring, enableCollision, enablePreprocessing;
    public float mass, spring, damper, tolerance;

    public Vector3 setPos;

    NewParticleController newParticleController;

    private void Awake()
    {
        radius = diameter / 2f;

        triangleNum = new int[grade + 1];
        verticesNum = new int[grade + 1];
        sidesNum = new int[grade + 1];

        triangleNum[0] = baseNum;
        verticesNum[0] = BaseVerticesPos.Length;
        sidesNum[0] = baseNum * 3 / 2;
        for (i = 1; i <= grade; i++)
        {
            triangleNum[i] = triangleNum[i - 1] * 4;
            sidesNum[i] = triangleNum[i] * 3 / 2;
            verticesNum[i] = verticesNum[i - 1] + sidesNum[i - 1];
        }

        trianglePos = new Vector3[triangleNum[grade]];
        triangleObjcts = new GameObject[triangleNum[grade]];

        newTriangles[0] = 0;
        newTriangles[1] = 1;
        newTriangles[2] = 2;

        newTriangles[3] = 0;
        newTriangles[4] = 2;
        newTriangles[5] = 1;

        newTriangles[6] = 0;
        newTriangles[7] = 1;
        newTriangles[8] = 3;

        newTriangles[9] = 0;
        newTriangles[10] = 3;
        newTriangles[11] = 1;

        newTriangles[12] = 1;
        newTriangles[13] = 2;
        newTriangles[14] = 3;

        newTriangles[15] = 2;
        newTriangles[16] = 1;
        newTriangles[17] = 3;

        newTriangles[18] = 2;
        newTriangles[19] = 0;
        newTriangles[20] = 3;

        newTriangles[21] = 2;
        newTriangles[22] = 3;
        newTriangles[23] = 0;

        //頂点の分割
        SetVertices();

        //三角形の設定
        SetTriangle();

        //ジョイントの設定
        //tolerance = radius / (grade * 4); //細かいほど誤差を許容する
        if (useHingeJoint)
            SetHingeJoint();
        if (useSpringJoint)
            SetSpringJoint();

        transform.position = setPos;
    }

    private void Start()
    {
        newParticleController = GameObject.Find("ParticleController").GetComponent<NewParticleController>();
        if (newParticleController.addBeadsByWallPos)
        {
            newParticleController.WallObjects = new GameObject[triangleNum[grade]];

            int i;
            for(i = 0; i < triangleNum[grade]; i++)
            {
                newParticleController.WallObjects[i] = triangleObjcts[i];
            }
        }
    }

    void SetVertices()
    {
        AdjacentVertices = new bool[verticesNum[grade], verticesNum[grade]];
        VerticesPos = new Vector3[verticesNum[grade]];
        for (i = 0; i < BaseVerticesPos.Length; i++)
        {
            VerticesPos[i] = BaseVerticesPos[i] * radius;
        }

        int l, nowVerticesCount = BaseVerticesPos.Length;

        for(l = 0; l <= grade; l++)
        {
            //int count = 0;

            //新頂点計算
            for (i = 0; i < verticesNum[l]; i++)
            {
                for(j = i + 1; j < verticesNum[l]; j++)
                {
                    if (AdjacentVertices[i, j])
                    {
                        //隣接頂点の中点を計算
                        Vector3 centralPos = (VerticesPos[i] + VerticesPos[j]) / 2f;

                        //補正係数を計算
                        float a = Mathf.Sqrt(radius * radius / Vector3.Dot(centralPos, centralPos));

                        //新頂点を計算
                        VerticesPos[nowVerticesCount] = a * centralPos;
                        nowVerticesCount++;
                    }
                }
            }

            //隣接頂点の設定
            for (i = 0; i < verticesNum[l]; i++)
            {
                //頂点iについて、最短頂点間距離を計算
                float nowDist, minDist = Vector3.Distance(VerticesPos[0], VerticesPos[1]);
                float[] Dists = new float[verticesNum[l]];
                for (j = 0; j < verticesNum[l]; j++)
                {
                    if(i != j) //自身を除く全ての頂点との距離を計算
                    {
                        nowDist = Vector3.Distance(VerticesPos[i], VerticesPos[j]);
                        Dists[j] = nowDist;

                        if (nowDist < minDist)
                        {
                            minDist = nowDist;
                        }
                    }
                }

                //Debug.Log("最小値：" + minDist);

                //隣接関係を設定
                float nowMargin = verticesMargin[0];
                for (j = i + 1; j < verticesNum[l]; j++)
                {
                    //マージンの設定
                    for (k = grade; k >= 0; k--)
                    {
                        //より小さな頂点番号のマージンに更新
                        if (i < verticesNum[k] || j < verticesNum[k])
                        {
                            nowMargin = verticesMargin[k];
                        }
                    }
                    if (Dists[j] <= minDist * nowMargin) //マージン付き
                    {
                        //count++;
                        AdjacentVertices[i, j] = true;
                        AdjacentVertices[j, i] = true;
                    }
                    else
                    {
                        AdjacentVertices[i, j] = false;
                        AdjacentVertices[j, i] = false;
                    }
                }
            }
            //Debug.Log(l);
            //Debug.Log(count);
        }
    }

    void SetTriangle()
    {
        int n = 0;

        //三角形の設定
        for (i = 0; i < verticesNum[grade]; i++)
        {
            for (j = i + 1; j < verticesNum[grade]; j++)
            {
                //第一隣接を検索
                if (AdjacentVertices[i, j])
                {
                    for (k = j + 1; k < verticesNum[grade]; k++)
                    {
                        //第二隣接を検索
                        if (AdjacentVertices[j, k])
                        {
                            //第三隣接を確認
                            if (AdjacentVertices[i, k])
                            {
                                //３点が隣接しているため、三角形を設定
                                nowTriangleObj = Instantiate(TrianglePrefab);
                                nowTriangleObj.transform.SetParent(this.gameObject.transform);
                                triangleObjcts[n] = nowTriangleObj;
                                nowTriangleObj.GetComponent<Rigidbody>().useGravity = useGravity;

                                mesh = nowTriangleObj.GetComponent<MeshFilter>().mesh;
                                meshCollider = nowTriangleObj.GetComponent<MeshCollider>();

                                //三角形の頂点代入
                                newVectors[0] = VerticesPos[i];
                                newVectors[1] = VerticesPos[j];
                                newVectors[2] = VerticesPos[k];

                                //位置の補正
                                centralPosition = (newVectors[0] + newVectors[1] + newVectors[2]) / 3f;
                                nowTriangleObj.transform.position = centralPosition;
                                trianglePos[n] = centralPosition;
                                newVectors[0] = newVectors[0] - centralPosition;
                                newVectors[1] = newVectors[1] - centralPosition;
                                newVectors[2] = newVectors[2] - centralPosition;

                                //三角錐の頂点
                                topPos_a = radius / centralPosition.magnitude;
                                newVectors[3] = centralPosition * topPos_a - centralPosition;

                                mesh.SetVertices(newVectors);
                                mesh.SetTriangles(newTriangles, 0);
                                meshCollider.sharedMesh = mesh;

                                n++;
                            }
                        }
                    }
                }
            }
        }

        for (i = 0; i < triangleNum[grade]; i++)
        {
            triangleObjcts[i].GetComponent<Rigidbody>().mass = mass / triangleNum[grade];
        }
    }

    void SetHingeJoint()
    {
        float nowDist, minDist = Vector3.Distance(trianglePos[0], trianglePos[1]);

        //三角錐の位置から最短距離を測定
        for (i = 0; i < triangleNum[grade]; i++)
        {
            for (j = 0; j < triangleNum[grade]; j++)
            {
                if(i != j)
                {
                    nowDist = Vector3.Distance(trianglePos[i], trianglePos[j]);

                    if (nowDist < minDist)
                    {
                        minDist = nowDist;
                    }
                }
            }
        }

        //三角錐の位置から隣接の有無を判定
        //int count = 0;
        for (i = 0; i < triangleNum[grade]; i++)
        {
            for (j = 0; j < triangleNum[grade]; j++)
            {
                if (i != j)
                {
                    //マージンの設定
                    float nowMargin = objMargin[0];
                    for (k = grade; k >= 0; k--)
                    {
                        //より小さな頂点番号のマージンに更新
                        if (i < verticesNum[k] || j < verticesNum[k])
                        {
                            nowMargin = objMargin[k];
                        }
                    }

                    nowDist = Vector3.Distance(trianglePos[i], trianglePos[j]);
                    if (nowDist < minDist * nowMargin)
                    {
                        //count++;
                        //隣接関係にある三角錐をHingeJointで結合
                        HingeJoint nowJoint;
                        nowJoint = triangleObjcts[i].AddComponent<HingeJoint>();
                        nowJoint.connectedBody = triangleObjcts[j].GetComponent<Rigidbody>();

                        //回転軸の設定
                        //中点に近い頂点を取得
                        Vector3 centerPos = (trianglePos[i] + trianglePos[j]) / 2f;
                        float minDist_1 = 0, minDist_2 = 0;
                        int k, minVertexNum_1 = 0, minVertexNum_2 = 0;
                        for (k = 0; k < verticesNum[grade]; k++)
                        {
                            nowDist = Vector3.Distance(centerPos, VerticesPos[k]);
                            if (k == 0)
                            {
                                minVertexNum_1 = k;
                                minDist_1 = nowDist;
                            }
                            else if (k == 1)
                            {
                                if (nowDist < minDist_1)
                                {
                                    minDist_2 = minDist_1;
                                    minVertexNum_2 = minVertexNum_1;
                                    minDist_1 = nowDist;
                                    minVertexNum_1 = k;
                                }
                                else
                                {
                                    minDist_2 = nowDist;
                                    minVertexNum_2 = k;
                                }
                            }
                            else if (nowDist < minDist_1)
                            {
                                minDist_2 = minDist_1;
                                minVertexNum_2 = minVertexNum_1;
                                minDist_1 = nowDist;
                                minVertexNum_1 = k;
                            }
                            else if (nowDist < minDist_2)
                            {
                                minDist_2 = nowDist;
                                minVertexNum_2 = k;
                            }
                        }

                        nowJoint.anchor = (VerticesPos[minVertexNum_1] + VerticesPos[minVertexNum_2]) / 2.0f - trianglePos[i];
                        nowJoint.autoConfigureConnectedAnchor = true;
                        nowJoint.enableCollision = enableCollision;
                        nowJoint.enablePreprocessing = enablePreprocessing;
                        nowJoint.useSpring = useHingeSpring;
                        JointSpring hingeSpring = nowJoint.spring;
                        hingeSpring.spring = spring;
                        hingeSpring.damper = damper;
                        nowJoint.spring = hingeSpring;
                        nowJoint.axis = VerticesPos[minVertexNum_1] - VerticesPos[minVertexNum_2];
                    }
                }
            }
        }
        //Debug.Log(count);
    }


    void SetSpringJoint()
    {
        float nowDist, minDist = Vector3.Distance(trianglePos[0], trianglePos[1]);

        //三角錐の位置から最短距離を測定
        for (i = 0; i < triangleNum[grade]; i++)
        {
            for (j = 0; j < triangleNum[grade]; j++)
            {
                if (i != j)
                {
                    nowDist = Vector3.Distance(trianglePos[i], trianglePos[j]);

                    if (nowDist < minDist)
                    {
                        minDist = nowDist;
                    }
                }
            }
        }

        //三角錐の位置から隣接の有無を判定
        //int count = 0;
        for (i = 0; i < triangleNum[grade]; i++)
        {
            for (j = 0; j < triangleNum[grade]; j++)
            {
                if (i != j)
                {
                    //マージンの設定
                    float nowMargin = objMargin[0];
                    for (k = grade; k >= 0; k--)
                    {
                        //より小さな頂点番号のマージンに更新
                        if (i < verticesNum[k] || j < verticesNum[k])
                        {
                            nowMargin = objMargin[k];
                        }
                    }

                    nowDist = Vector3.Distance(trianglePos[i], trianglePos[j]);
                    if (nowDist < minDist * nowMargin)
                    {
                        //count++;

                        //回転軸の設定
                        //中点に近い頂点を取得
                        Vector3 centerPos = (trianglePos[i] + trianglePos[j]) / 2f;
                        float minDist_1 = 0, minDist_2 = 0;
                        int k, minVertexNum_1 = 0, minVertexNum_2 = 0;
                        for (k = 0; k < verticesNum[grade]; k++)
                        {
                            nowDist = Vector3.Distance(centerPos, VerticesPos[k]);
                            if (k == 0)
                            {
                                minVertexNum_1 = k;
                                minDist_1 = nowDist;
                            }
                            else if (k == 1)
                            {
                                if (nowDist < minDist_1)
                                {
                                    minDist_2 = minDist_1;
                                    minVertexNum_2 = minVertexNum_1;
                                    minDist_1 = nowDist;
                                    minVertexNum_1 = k;
                                }
                                else
                                {
                                    minDist_2 = nowDist;
                                    minVertexNum_2 = k;
                                }
                            }
                            else if (nowDist < minDist_1)
                            {
                                minDist_2 = minDist_1;
                                minVertexNum_2 = minVertexNum_1;
                                minDist_1 = nowDist;
                                minVertexNum_1 = k;
                            }
                            else if (nowDist < minDist_2)
                            {
                                minDist_2 = nowDist;
                                minVertexNum_2 = k;
                            }
                        }

                        //隣接関係にある三角錐をSpringJointで結合
                        SpringJoint nowJoint1;
                        nowJoint1 = triangleObjcts[i].AddComponent<SpringJoint>();
                        nowJoint1.connectedBody = triangleObjcts[j].GetComponent<Rigidbody>();
                        nowJoint1.anchor = VerticesPos[minVertexNum_1] - trianglePos[i];
                        nowJoint1.autoConfigureConnectedAnchor = true;
                        nowJoint1.spring = spring;
                        nowJoint1.damper = damper;
                        nowJoint1.tolerance = tolerance;
                        nowJoint1.enableCollision = enableCollision;
                        nowJoint1.enablePreprocessing = enablePreprocessing;

                        SpringJoint nowJoint2;
                        nowJoint2 = triangleObjcts[i].AddComponent<SpringJoint>();
                        nowJoint2.connectedBody = triangleObjcts[j].GetComponent<Rigidbody>();
                        nowJoint2.anchor = VerticesPos[minVertexNum_2] - trianglePos[i];
                        nowJoint2.autoConfigureConnectedAnchor = true;
                        nowJoint2.spring = spring;
                        nowJoint2.damper = damper;
                        nowJoint1.tolerance = tolerance;
                        nowJoint2.enableCollision = enableCollision;
                        nowJoint2.enablePreprocessing = enablePreprocessing;

                        SpringJoint nowJoint3;
                        nowJoint3 = triangleObjcts[i].AddComponent<SpringJoint>();
                        nowJoint3.connectedBody = triangleObjcts[j].GetComponent<Rigidbody>();
                        nowJoint3.anchor = trianglePos[j] - trianglePos[i];
                        nowJoint3.autoConfigureConnectedAnchor = true;
                        nowJoint3.spring = spring;
                        nowJoint3.damper = damper;
                        nowJoint1.tolerance = tolerance;
                        nowJoint3.enableCollision = enableCollision;
                        nowJoint3.enablePreprocessing = enablePreprocessing;
                    }
                }
            }
        }
        //Debug.Log(count);
    }



}