using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChanger1 : MonoBehaviour
{
    public Terrain terrain;
    TerrainData terrainData;
    public float x, y;

    private void Start()
    {
        terrainData = terrain.terrainData;
    }

    void Update()
    {
        int hx = Mathf.FloorToInt(x * terrainData.heightmapResolution);
        int hy = Mathf.FloorToInt(y * terrainData.heightmapResolution);
        var heights = terrainData.GetHeights(hx, hy, 1, 1); // クリック箇所のヘイトマップを取得[1x1]
        heights[0, 0] = 0; // 高さを0にする
        terrainData.SetHeightsDelayLOD(hx, hy, heights);
        //terrainData.SetHeights(x, y, );
    }
}
