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
		meshCollider.sharedMesh = planeMesh;
	}

}
