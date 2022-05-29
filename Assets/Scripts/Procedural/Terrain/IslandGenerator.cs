using UnityEngine;

using NoiseFunction = Constants.NoiseFunction;

public static class IslandGenerator
{
    private static int resolution;
    private static int mapChunkSize;
    private static ProceduralPlaneMesh[,] terrainChunks;
    private static GameObject[,] islandChunkObjects;
    private static GameObject island;
    // private static 

    public static void GenerateIsland(int resolution, int mapChunkSize, NoiseSettings noiseSettings,
        NoiseFunction noiseFunction, Material terrainMaterial, Material treeMaterial
    )
    {
        IslandGenerator.resolution = resolution;
        IslandGenerator.mapChunkSize = mapChunkSize;
        IslandGenerator.terrainChunks = new ProceduralPlaneMesh[resolution, resolution];
        int seed = noiseSettings.GeneralNoiseParams.Seed;
        int mapResolution = resolution * mapChunkSize;

        float[,] heightMap = new float[mapResolution + 1, mapResolution + 1];
        
        if(noiseFunction == NoiseFunction.Perlin)
        {
            heightMap = Noise.GenerateHeightMap(mapResolution, mapResolution, seed, noiseSettings.PerlinNoiseParams);
        }
        else if(noiseFunction == NoiseFunction.DiamondSquare)
		{
			heightMap = Noise.GenerateHeightMap(mapResolution, mapResolution, seed, noiseSettings.DiamondSquareNoiseParams);
		}
		else if(noiseFunction == NoiseFunction.Voronoi)
		{
			heightMap = Noise.GenerateHeightMap(mapResolution, mapResolution, seed, noiseSettings.VoronoiDiagramParams);
		}
		else if(noiseFunction == NoiseFunction.Combined)
		{
			heightMap = Noise.GenerateHeightMap(mapResolution, mapResolution, seed, 
				noiseSettings.CombinedNoiseParams, noiseSettings.DiamondSquareNoiseParams, noiseSettings.VoronoiDiagramParams);
		}

        AnimationCurve meshHeightCurve = noiseSettings.GeneralNoiseParams.MeshHeightCurve;
        float meshHeightMultiplier = noiseSettings.GeneralNoiseParams.MeshHeightMultiplier;
        // modify the height map
        for(int v = 0; v <= mapResolution; v++)
        {
            for(int u = 0; u <= mapResolution; u++)
            {
                heightMap[u, v] = meshHeightCurve.Evaluate(heightMap[u, v]);
                heightMap[u, v] *= EvaluateShore((float) u / mapResolution, (float) v / mapResolution);
            }
        }

        Vector3[,] vertexNormals = TerrainMeshGenerator.GetNormalsFromHeightMap(heightMap, mapChunkSize, meshHeightMultiplier);

        float[,] chunkHeightMap = new float[mapChunkSize + 1, mapChunkSize + 1];
        for(int j = 0; j < resolution; j++)
        {
            for(int i = 0; i < resolution; i++)
            {
                Vector2 offset = new Vector2(i * mapChunkSize, j * mapChunkSize);
                Vector3[] chunkVertexNormals = new Vector3[(mapChunkSize + 1) * (mapChunkSize + 1)];
                int normalsIndex = 0;
                for(int v = 0; v <= mapChunkSize; v++)
                {
                    for(int u = 0; u <= mapChunkSize; u++)
                    {
                        int globalX = (int)offset.x + u;
                        int globalY = (int)offset.y + v;

                        chunkHeightMap[u, v] = heightMap[globalX, globalY];
                        chunkVertexNormals[normalsIndex] = vertexNormals[globalX, globalY];
                        normalsIndex++;
                    }
                }
                terrainChunks[i, j] = TerrainMeshGenerator.GenerateTerrainMesh(chunkHeightMap, 0, meshHeightMultiplier);
                terrainChunks[i, j].SetNormals(chunkVertexNormals);
            }
        }
        if(island == null) 
            InitializeIslandObject(terrainMaterial);
        UpdateChunkMeshes();
    }

    private static float EvaluateShore(float x, float y)
    {
        const float a = 0.25f, b = 0.7f;
        const float vMax = 0.4f, vMin = -0.15f;
        float circleValue = Mathf.Clamp(RoundedSquare(x, y, a, b), vMin, vMax);
        return Mathf.Pow((circleValue - vMin) / (vMax - vMin), 2f);
    }

    private static float RoundedSquare(float x, float y, float a, float b)
    {
        float u = Mathf.Pow(Mathf.Max(Mathf.Abs(2 * x - 1) - a, 0f), 2.0f);
        float v = Mathf.Pow(Mathf.Max(Mathf.Abs(2 * y - 1) - a, 0f), 2.0f);
        return b * b - (u + v);
    }

    private static void InitializeIslandObject(Material terrainMaterial)
    {
        island = new GameObject("Island");
        islandChunkObjects = new GameObject[resolution, resolution];
        
        for(int j = 0; j < resolution; j++)
        {
            for(int i = 0; i < resolution; i++)
            {
                GameObject terrainChunk = new GameObject("TerrainChunk" + (j * resolution + i));
            
                terrainChunk.AddComponent<MeshFilter>();
                terrainChunk.AddComponent<MeshRenderer>();

                terrainChunk.GetComponent<MeshRenderer>().material = terrainMaterial;

                terrainChunk.transform.SetParent(island.transform);

                terrainChunk.transform.position = new Vector3((i + 0.5f) - resolution / 2, 0.0f, (j + 0.5f) - (resolution + 1) / 2);
                islandChunkObjects[i, j] = terrainChunk;
            }
        }
        island.transform.localScale = new Vector3(mapChunkSize, 1.0f, mapChunkSize);
    }

    private static Vector3 FromHeightMapToWorldCoords(float px, float height, float pz) 
        => new Vector3((px - 0.5f) * resolution * mapChunkSize, height, (pz - 0.5f) * resolution * mapChunkSize);

    private static void UpdateChunkMeshes()
    {
        for(int j = 0; j < resolution; j++)
        {
            for(int i = 0; i < resolution; i++)
            {
                GameObject terrainChunk = islandChunkObjects[i, j];
                Mesh terrainChunkMesh = terrainChunk.GetComponent<MeshFilter>().sharedMesh;
                terrainChunkMesh = terrainChunks[i, j].GetMesh();
                terrainChunkMesh.normals = terrainChunks[i, j].GetMesh().normals;
                terrainChunkMesh.RecalculateBounds();
                terrainChunk.GetComponent<MeshFilter>().sharedMesh = terrainChunkMesh;
            }
        }
    }
}