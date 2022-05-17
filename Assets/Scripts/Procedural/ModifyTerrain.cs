using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifyTerrain : MonoBehaviour
{
    SingleStreamPlaneProceduralMesh terrainObject;
    Mesh terrainMesh;
    
    [SerializeField, Range(1.0f, 5.0f)]
    float Lacunarity = 2.0f;

    [SerializeField, Range(0.1f, 1.0f)]
    float Persistance = 0.5f;

    [SerializeField, Range(1, 5)]
    int OctavesCount = 3;

    [SerializeField]
    Vector2 NoiseOffset = new Vector2(0.0f, 0.0f);

    [SerializeField]
    float NoiseScale = 2.0f;

    [SerializeField]
    int Seed = 0;

    [SerializeField]
    float TerrainAmplitude = 1.0f;

    private const int resolutionU = Constants.terrainResolutionU;
    private const int resolutionV = Constants.terrainResolutionV;

    void OnEnable()
    {
        terrainObject = new SingleStreamPlaneProceduralMesh(GetComponent<MeshFilter>());
        MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = GetComponent<MeshFilter>().mesh;
        terrainMesh = terrainObject.GetMeshFilter().mesh;
        ApplyPerlinNoise();
    }

    // void Start()
    // {
    //     ApplyPerlinNoise();
    // }

    void ApplyPerlinNoise()
    {
        Vector3[] myVertices = terrainMesh.vertices;

        float[,] noiseMap = PerlinNoise.GenerateNoiseMap(resolutionU, resolutionV, Seed, NoiseScale, OctavesCount, Persistance, Lacunarity, NoiseOffset);
        int vi = 0;
        for(int v = 0; v <= resolutionV; v++)
        {
            for(int u = 0; u <= resolutionU; u++)
            {
                myVertices[vi].y = TerrainAmplitude * noiseMap[u,v];
                vi++;
            }
        }

        terrainMesh.vertices = myVertices;
        terrainMesh.RecalculateNormals();
        terrainMesh.RecalculateBounds();
    }
}
