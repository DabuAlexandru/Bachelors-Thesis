using UnityEngine;
using System.Collections.Generic;

using NoiseFunction = Constants.NoiseFunction;

public static class IslandGenerator
{
    private static int resolution;
    private static int mapChunkSize;
    private static GameObject island;
    
    private class IslandChunk
    {
        private GameObject chunkObject;
        public GameObject ChunkObject { get => chunkObject; set => chunkObject = value; }

        private ProceduralPlaneMesh planeObject;
        public ProceduralPlaneMesh PlaneObject { get => planeObject; set => planeObject = value; }

        private List<TreeEntity> trees;
        
        public void AddTree(TreeEntity tree) => trees.Add(tree);
        public TreeEntity GetTree(int i) => trees[i];

        public IslandChunk()
        {
            trees = new List<TreeEntity>();
        }

    }

    private static IslandChunk[,] islandChunks;

    public static void GenerateIsland(int resolution, int mapChunkSize, NoiseSettings noiseSettings, DistributionParams distributionParams,
        NoiseFunction noiseFunction, Material terrainMaterial, Material treeMaterial, Material leavesMaterial
    )
    {
        IslandGenerator.resolution = resolution;
        IslandGenerator.mapChunkSize = mapChunkSize;

        if(island == null)
            InitializeIslandChunks();

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
                islandChunks[i, j].PlaneObject = TerrainMeshGenerator.GenerateTerrainMesh(chunkHeightMap, 0, meshHeightMultiplier);
                islandChunks[i, j].PlaneObject.SetNormals(chunkVertexNormals);
            }
        }
        if(island == null)
            InitializeIslandObject(terrainMaterial, heightMap, meshHeightMultiplier);
        Vector2[] trees = TerrainMeshGenerator.GetTreesOnHeightMap(heightMap, distributionParams);
        AddTreesToIsland(trees, treeMaterial, leavesMaterial, heightMap, meshHeightMultiplier);
        UpdateChunkMeshes();
    }

    private static void InitializeIslandChunks()
    {
        islandChunks = new IslandChunk[resolution, resolution];
        for(int j = 0; j < resolution; j++)
        {
            for(int i = 0; i < resolution; i++)
            {
                islandChunks[i, j] = new IslandChunk();
            }
        }
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

    private static void InitializeIslandObject(Material terrainMaterial, float[,] heightMap, float meshHeightMultiplier)
    {
        island = new GameObject("Island");
        
        for(int j = 0; j < resolution; j++)
        {
            for(int i = 0; i < resolution; i++)
            {
                GameObject terrainChunk = new GameObject("TerrainChunk" + (j * resolution + i));
            
                terrainChunk.AddComponent<MeshFilter>();
                terrainChunk.AddComponent<MeshRenderer>();
                terrainChunk.AddComponent<MeshCollider>();
                terrainChunk.GetComponent<MeshCollider>();

                terrainChunk.GetComponent<MeshRenderer>().material = terrainMaterial;

                terrainChunk.transform.SetParent(island.transform);

                terrainChunk.transform.position = new Vector3((i + 0.5f) - resolution / 2, 0.0f, (j + 0.5f) - (resolution + 1) / 2);
                islandChunks[i, j].ChunkObject = terrainChunk;
            }
        }
        island.transform.localScale = new Vector3(mapChunkSize, 1.0f, mapChunkSize);
    }

    private static void AddTreesToIsland(Vector2[] trees, Material treeMaterial, Material leavesMaterial, float[,] heightMap, float meshHeightMultiplier)
    {
        int width = heightMap.GetLength(0), height = heightMap.GetLength(1);
        for(int ti = 0; ti < trees.Length; ti++)
        {
            int u = (int)trees[ti].x - width / 2, v = (int)trees[ti].y - height / 2;
            int i = (int)Mathf.Floor(trees[ti].x / mapChunkSize), j = (int)Mathf.Floor(trees[ti].y / mapChunkSize);

            TreeEntity tree = new TreeEntity(treeMaterial, leavesMaterial, true);
            islandChunks[i, j].AddTree(tree);
            tree.TreeObject.transform.position = new Vector3(u, heightMap[(int)trees[ti].x, (int)trees[ti].y] * meshHeightMultiplier, v);
            tree.TreeObject.transform.SetParent(islandChunks[i, j].ChunkObject.transform);
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
                GameObject terrainChunk = islandChunks[i, j].ChunkObject;
                Mesh terrainChunkMesh = terrainChunk.GetComponent<MeshFilter>().sharedMesh;
                terrainChunkMesh = islandChunks[i, j].PlaneObject.GetMesh();
                terrainChunkMesh.normals = islandChunks[i, j].PlaneObject.GetMesh().normals;
                terrainChunkMesh.RecalculateBounds();
                terrainChunk.GetComponent<MeshFilter>().sharedMesh = terrainChunkMesh;
                terrainChunk.GetComponent<MeshCollider>().sharedMesh = terrainChunkMesh;
            }
        }
    }
}