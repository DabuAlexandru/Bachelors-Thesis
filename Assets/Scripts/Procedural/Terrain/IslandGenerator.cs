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

    public static void GenerateIsland(int resolution, int mapChunkSize, NoiseSettings noiseSettings, DistributionParams distributionParams,
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
            InitializeIslandObject(distributionParams, terrainMaterial, treeMaterial, heightMap, meshHeightMultiplier);
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

    private static void InitializeIslandObject(DistributionParams distributionParams, Material terrainMaterial, Material treeMaterial, float[,] heightMap, float meshHeightMultiplier)
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

        Vector2[] trees = TerrainMeshGenerator.GetTreesOnHeightMap(heightMap, distributionParams);
        AddTreesToIsland(islandChunkObjects, trees, treeMaterial, heightMap, meshHeightMultiplier);
    }

    private static void AddTreesToIsland(GameObject[,] islandChunkObjects, Vector2[] trees, Material treeMaterial, float[,] heightMap, float meshHeightMultiplier)
    {
        int width = heightMap.GetLength(0), height = heightMap.GetLength(1);
        for(int ti = 0; ti < trees.Length; ti++)
        {
            int u = (int)trees[ti].x - width / 2, v = (int)trees[ti].y - height / 2;
            int i = (int)Mathf.Floor(trees[ti].x / mapChunkSize), j = (int)Mathf.Floor(trees[ti].y / mapChunkSize);

            GameObject tree = new TreeEntity(treeMaterial).TreeObject;
            tree.transform.position = new Vector3(u, heightMap[(int)trees[ti].x, (int)trees[ti].y] * meshHeightMultiplier, v);
            tree.transform.SetParent(islandChunkObjects[i, j].transform);
        }
    }

    private static void AddTreesToObject(GameObject parent, Material treeMaterial, int i, int j, float[,] heightMap, float meshHeightMultiplier)
    {
        for(int u = 1; u < 5; u++)
        {
            for(int v = 1; v < 5; v++)
            {
                int gx = (int)Mathf.Floor((0.1f * u + i - resolution / 2) * mapChunkSize);
                int gy = (int)Mathf.Floor((0.1f * v + j - resolution / 2) * mapChunkSize);

                int hx = (int)Mathf.Floor((0.1f * u + i) * mapChunkSize);
                int hy = (int)Mathf.Floor((0.1f * v + j) * mapChunkSize);

                GameObject tree = new TreeEntity(treeMaterial).TreeObject;
                tree.transform.position = new Vector3(gx, heightMap[hx, hy] * meshHeightMultiplier, gy);
                tree.transform.SetParent(parent.transform);
            }
        }
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