using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshManager : MonoBehaviour
{
    public Transform nextWallDot;
    private Vector3 Dist;

    private MeshFilter meshFilter;
    private Mesh myMesh;
    private MeshCollider meshCollider;
    private Vector3[] myVertices = new Vector3[4];
    private int[] myTriangles = new int[6];
    private float height = 1;

    void Start()
    {
        meshFilter = gameObject.GetComponent<MeshFilter>();
        myMesh = new Mesh();
        meshCollider = gameObject.GetComponent<MeshCollider>();

        //[0]自分の位置奥、[1]隣の位置奥、[2]自分の位置前、[3]隣の位置前
        myVertices[0] = new Vector3(transform.position.x, transform.position.y, height);
        myVertices[1] = new Vector3(nextWallDot.position.x, nextWallDot.position.y, height);
        myVertices[2] = new Vector3(transform.position.x, transform.position.y, -height);
        myVertices[3] = new Vector3(nextWallDot.position.x, nextWallDot.position.y, -height);


        myMesh.SetVertices(myVertices);

        //三角形配置用頂点番号決定
        myTriangles[0] = 0;
        myTriangles[1] = 2;
        myTriangles[2] = 1;
        myTriangles[3] = 2;
        myTriangles[4] = 3;
        myTriangles[5] = 1;

        myMesh.SetTriangles(myTriangles, 0);
       
        meshFilter.mesh = myMesh;
        meshCollider.sharedMesh = myMesh;
    }

    void FixedUpdate()
    {
        Dist = nextWallDot.localPosition - transform.localPosition;
        myVertices[0] = new Vector3(0, 0, height);
        myVertices[1] = new Vector3(Dist.x, Dist.y, height);
        myVertices[2] = new Vector3(0, 0, -height);
        myVertices[3] = new Vector3(Dist.x, Dist.y, -height);
        myMesh.SetVertices(myVertices);
        myMesh.SetTriangles(myTriangles, 0);
        meshFilter.mesh = myMesh;
        meshCollider.sharedMesh = myMesh;
    }
}
