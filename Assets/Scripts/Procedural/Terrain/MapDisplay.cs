using UnityEngine;
using System.Collections;

public class MapDisplay : MonoBehaviour {

	public Renderer textureRender;
	public MeshFilter meshFilter;
	public MeshRenderer meshRenderer;
	public MeshCollider meshCollider;

	public void DrawTexture(Texture2D texture) => textureRender.sharedMaterial.mainTexture = texture;

	public void DrawMesh(ProceduralPlaneMesh terrainObj) 
	{
		Mesh planeMesh = terrainObj.GetMesh();
		meshFilter.sharedMesh = terrainObj.GetMesh();
		SpawnTrees(terrainObj);
		meshCollider.sharedMesh = planeMesh;
	}

	public void SpawnTrees(ProceduralPlaneMesh terrainObj)
	{
		// float[,] heightMap = terrainObj.ExtractHeightMap();
		// Mesh planeMesh = terrainObj.GetMesh();
		// Vector3[] vertices = planeMesh.vertices;
		// int n = vertices.Length;
		// for(int i = 0; i < n; i++)
		// {
		// 	if(i % 4 == 0)
		// 	{

		// 	}
		// }
	}

}
