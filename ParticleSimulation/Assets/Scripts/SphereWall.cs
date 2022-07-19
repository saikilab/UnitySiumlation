using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SphereWall : MonoBehaviour
{
    void Start()
    {
        //球の内側に当たり判定を配置
        Mesh mesh = GetComponent<MeshFilter>().mesh;
        mesh.triangles = mesh.triangles.Reverse().ToArray();
        gameObject.AddComponent<MeshCollider>();
    }
}
