using ProceduralMeshes;
using ProceduralMeshes.Generators;
using ProceduralMeshes.Streams;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ProceduralMesh : MonoBehaviour {

    static MeshJobScheduleDelegate[] jobs = {
		MeshJob<SquareGrid, SingleStream>.ScheduleParallel,
		MeshJob<SharedSquareGrid, SingleStream>.ScheduleParallel,
		MeshJob<UVSphere, SingleStream>.ScheduleParallel
	};

	public enum MeshType {
		SquareGrid, SharedSquareGrid, SharedTriangleGrid, 
		PointyHexagonGrid, FlatHexagonGrid, UVSphere
	};

	[SerializeField]
	MeshType meshType;

    [SerializeField, Range(1, 50)]
	int resolution = 1;

    Mesh mesh;

	Vector3[] vertices, normals;

	Vector4[] tangents;

	[System.Flags]
	public enum GizmoMode { Nothing = 0, Vertices = 1, Normals = 0b10, Tangents = 0b100 }

	[SerializeField]
	GizmoMode gizmos;

	void Awake () {
		mesh = new Mesh {
			name = "Procedural Mesh"
		};
		// GenerateMesh();
		GetComponent<MeshFilter>().mesh = mesh;
	}

    void OnValidate () => enabled = true;

	void Update () {
		GenerateMesh();
		enabled = false;

		vertices = null;
		normals = null;
		tangents = null;
	}

	void OnDrawGizmos () {
		if (gizmos == GizmoMode.Nothing || mesh == null) {
			return;
		}

		bool drawVertices = (gizmos & GizmoMode.Vertices) != 0;
		bool drawNormals = (gizmos & GizmoMode.Normals) != 0;
		bool drawTangents = (gizmos & GizmoMode.Tangents) != 0;

		if (vertices == null) {
			vertices = mesh.vertices;
		}
		if (drawNormals && normals == null) {
			normals = mesh.normals;
		}
		if (drawTangents && tangents == null) {
			tangents = mesh.tangents;
		}

		Transform t = transform;
		for (int i = 0; i < vertices.Length; i++) {
			Vector3 position = t.TransformPoint(vertices[i]);
			if (drawVertices) {
				Gizmos.color = Color.cyan;
				Gizmos.DrawSphere(position, 0.02f);
			}
			if (drawNormals) {
				Gizmos.color = Color.green;
				Gizmos.DrawRay(position, t.TransformDirection(normals[i]) * 0.2f);
			}
			if (drawTangents) {
				Gizmos.color = Color.red;
				Gizmos.DrawRay(position, t.TransformDirection(tangents[i]) * 0.2f);
			}
		}
	}
	
	void GenerateMesh () {
		Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
		Mesh.MeshData meshData = meshDataArray[0];

        // Multistream aproach
        // MeshJob<SquareGrid, MultiStream>.ScheduleParallel(
		
        // Singlestream aproach
        // MeshJob<SquareGrid, SingleStream>.ScheduleParallel(
		// 	mesh, meshData, resolution, default
		// ).Complete();

        jobs[(int)meshType](mesh, meshData, resolution, default).Complete();

		Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
	}
}