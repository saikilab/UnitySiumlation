using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Sphere : MonoBehaviour
{
    private MeshRenderer _renderer;
    private MeshRenderer Renderer => _renderer != null ? _renderer : (_renderer = GetComponent<MeshRenderer>());

    private MeshFilter _filter;
    private MeshFilter Filter => _filter != null ? _filter : (_filter = GetComponent<MeshFilter>());

    private Mesh _mesh;

    void Start()
    {
        Create();
    }

    public Vector2Int divide;

    void Create()
    {
        int divideX = divide.x;
        int divideY = divide.y;

        var data = CreateSphere(divideX, divideY);
        if (_mesh == null)
            _mesh = new Mesh();
        _mesh.SetVertices(data.vertices);
        _mesh.SetIndices(data.indices, MeshTopology.Triangles, 0);
        Filter.mesh = _mesh;
        _mesh.RecalculateNormals();
    }

    struct MeshData
    {
        public Vector3[] vertices;
        public int[] indices;
    }

    MeshData CreateSphere(int divideX, int divideY, float size = 1f)
    {
        divideX = divideX < 4 ? 4 : divideX;
        divideY = divideY < 4 ? 4 : divideY;

        // =============================  
        // 頂点座標作成  
        // =============================  

        // 半径  
        float r = size * 0.5f;
        int cnt = 0;
        int vertCount = divideX * (divideY - 1) + 2;
        var vertices = new Vector3[vertCount];

        // 中心角  
        float centerRadianX = 2f * Mathf.PI / (float)divideX;
        float centerRadianY = 2f * Mathf.PI / (float)divideY;

        // 天面  
        vertices[cnt++] = new Vector3(0, r, 0);
        for (int vy = 0; vy < divideY - 1; vy++)
        {
            var yRadian = (float)(vy + 1) * centerRadianY / 2f;

            // 1辺の長さ  
            var tmpLen = Mathf.Abs(Mathf.Sin(yRadian));

            var y = Mathf.Cos(yRadian);
            for (int vx = 0; vx < divideX; vx++)
            {
                var pos = new Vector3(
                    tmpLen * Mathf.Sin((float)vx * centerRadianX),
                    y,
                    tmpLen * Mathf.Cos((float)vx * centerRadianX)
                );
                // サイズ反映  
                vertices[cnt++] = pos * r;
            }
        }

        // 底面  
        vertices[cnt] = new Vector3(0, -r, 0);

        // =============================  
        // 頂点インデックス作成  
        // =============================  

        int topAndBottomTriCount = divideX * 2;
        // 側面三角形の数  
        int aspectTriCount = divideX * (divideY - 2) * 2;

        int[] indices = new int[(topAndBottomTriCount + aspectTriCount) * 3];

        //天面  
        int offsetIndex = 0;
        cnt = 0;
        for (int i = 0; i < divideX * 3; i++)
        {
            if (i % 3 == 0)
            {
                indices[cnt++] = 0;
            }
            else if (i % 3 == 1)
            {
                indices[cnt++] = 1 + offsetIndex;
            }
            else if (i % 3 == 2)
            {
                var index = 2 + offsetIndex++;
                // 蓋をする  
                index = index > divideX ? indices[1] : index;
                indices[cnt++] = index;
            }
        }

        // 側面Index  

        /* 頂点を繋ぐイメージ  
         * 1 - 2  
         * |   |  
         * 0 - 3  
         *  
         * 0, 1, 2  
         * 0, 2, 3  
         *  
         * 注意 : 1周した時にループするのを忘れないように。  
         */

        // 開始Index番号  
        int startIndex = indices[1];

        // 天面、底面を除いたIndex要素数  
        int sideIndexLen = divideX * (divideY - 2) * 2 * 3;

        // ループ時に使用するIndex  
        int loop1stIndex = 0;
        int loop2ndIndex = 0;

        // 一周したときのindex数  
        int lapDiv = divideX * 2 * 3;

        int createSquareFaceCount = 0;

        for (int i = 0; i < sideIndexLen; i++)
        {
            // 一周の頂点数を超えたら更新(初回も含む)  
            if (i % lapDiv == 0)
            {
                loop1stIndex = startIndex;
                loop2ndIndex = startIndex + divideX;
                createSquareFaceCount++;
            }

            if (i % 6 == 0 || i % 6 == 3)
            {
                indices[cnt++] = startIndex;
            }
            else if (i % 6 == 1)
            {
                indices[cnt++] = startIndex + divideX;
            }
            else if (i % 6 == 2 || i % 6 == 4)
            {
                if (i > 0 &&
                    (i % (lapDiv * createSquareFaceCount - 2) == 0 ||
                     i % (lapDiv * createSquareFaceCount - 4) == 0)
                )
                {
                    // 1周したときのループ処理  
                    // 周回ポリゴンの最後から2番目のIndex  
                    indices[cnt++] = loop2ndIndex;
                }
                else
                {
                    indices[cnt++] = startIndex + divideX + 1;
                }
            }
            else if (i % 6 == 5)
            {
                if (i > 0 && i % (lapDiv * createSquareFaceCount - 1) == 0)
                {
                    // 1周したときのループ処理  
                    // 周回ポリゴンの最後のIndex  
                    indices[cnt++] = loop1stIndex;
                }
                else
                {
                    indices[cnt++] = startIndex + 1;
                }

                // 開始Indexの更新  
                startIndex++;
            }
            else
            {
                Debug.LogError("Invalid : " + i);
            }
        }


        // 底面Index  
        offsetIndex = vertices.Length - 1 - divideX;
        var loopIndex = offsetIndex;

        for (int i = divideX * 3 - 1; i >= 0; i--)
        {
            if (i % 3 == 0)
            {
                // 底面の先頂点  
                indices[cnt++] = vertices.Length - 1;
                offsetIndex++;
            }
            else if (i % 3 == 1)
            {
                indices[cnt++] = offsetIndex;
            }
            else if (i % 3 == 2)
            {
                var value = 1 + offsetIndex;
                if (value >= vertices.Length - 1)
                {
                    value = loopIndex;
                }

                indices[cnt++] = value;
            }
        }


        return new MeshData()
        {
            indices = indices,
            vertices = vertices
        };
    }
}