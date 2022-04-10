using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;
using System.Runtime.InteropServices;

using static Unity.Mathematics.math;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class SingleStreamCylindricalProceduralMesh : MonoBehaviour
{

    [StructLayout(LayoutKind.Sequential)]
    struct Vertex
    {
        public float3 position, normal;
        public half4 tangent;
        public half2 texCoord0;
    }
    // number of sections
    const int resolutionU = 30;
    const int resolutionV = 30;
    // dimensions of the generated shape
    private const float radius = 0.5f;
    private const float height = 2.0f;
    // specifications of the mesh
    private Bounds bounds = new Bounds(Vector3.zero, new Vector3(radius * 2f, height, radius * 2f));
    private const int vertexCount = (resolutionU + 1) * (resolutionV + 1);
    private const int indexCount = 6 * resolutionU * resolutionV;
    private const int vertexAttributeCount = 4; // four attributes: a position, a normal, a tangent, and a set of texture coordinates

    Vector3[] verticesInitialPos;
    Mesh myMesh;

    [System.Flags]
    public enum GizmoMode { Nothing = 0, Vertices = 1, Normals = 0b10, Tangents = 0b100 }

    [SerializeField]
    GizmoMode gizmos;
    // the offset of the mouse away from the center of this gameObject
    private Vector3 mouseOffset;
    // the chosen z coordinate to put the mouse on (in order for the camera to track it)
    private float chosenZCoordinate;
    // the ring hovered over by the mouse
    int ringLevel;
    private float mousePosX;

    const float minRadiusPercentage = 0.5f, maxRadiusPercentage = 1.5f;
    float[] ringRadiusPercentages = new float[resolutionV + 1];
    const float radiusModifyRate = 0.5f;
    const int numberOfAffectedNeighbourPairs = 4;

    void OnEnable()
    {
        Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
        Mesh.MeshData meshData = meshDataArray[0];

        var vertexAttributes = new NativeArray<VertexAttributeDescriptor>(
            vertexAttributeCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory
        );
        vertexAttributes[0] = new VertexAttributeDescriptor(dimension: 3);
        vertexAttributes[1] = new VertexAttributeDescriptor(
            VertexAttribute.Normal, dimension: 3
        );
        vertexAttributes[2] = new VertexAttributeDescriptor(
            VertexAttribute.Tangent, VertexAttributeFormat.Float16, 4
        );
        vertexAttributes[3] = new VertexAttributeDescriptor(
            VertexAttribute.TexCoord0, VertexAttributeFormat.Float16, 2
        );
        meshData.SetVertexBufferParams(vertexCount, vertexAttributes);
        vertexAttributes.Dispose();
        NativeArray<Vertex> vertices = meshData.GetVertexData<Vertex>();

        meshData.SetIndexBufferParams(indexCount, IndexFormat.UInt16);
        NativeArray<ushort> triangleIndices = meshData.GetIndexData<ushort>();

        var vertex = new Vertex();
        int vi = 0, ti = 0;
        for (int v = 0; v <= resolutionV; v++)
        {
            for (int u = 0; u <= resolutionU; u++)
            {
                vertex.position.x = radius * cos(2 * PI * u / resolutionU);
                vertex.position.z = radius * sin(2 * PI * u / resolutionU);

                vertex.normal.xz = vertex.position.xz;
                vertex.normal.y = 0.0f;

                vertex.tangent.x = half(-vertex.position.z);
                vertex.tangent.y = half(0.0f);
                vertex.tangent.z = half(vertex.position.x);

                vertex.position.y = height * ((float)v / resolutionV) - height / 2;
                vertex.texCoord0.x = half((float)u / resolutionU);
                vertex.texCoord0.y = half((float)v / resolutionV);

                vertices[vi] = vertex;
                // Debug.Log("VERTEX: " + vi.ToString() + ' ' + vertex.position.ToString());
                vi++;

                if (v < resolutionV && u < resolutionU)
                {
                    int currentIndex = v * (resolutionU + 1) + u;
                    int rightIndex = v * (resolutionU + 1) + (u + 1);
                    int topRightIndex = (v + 1) * (resolutionU + 1) + (u + 1);
                    int topIndex = (v + 1) * (resolutionU + 1) + u;
                    // Debug.Log("SQUARE: " + currentIndex.ToString() + ' ' + rightIndex.ToString() + ' ' + topRightIndex.ToString() + ' ' + topIndex.ToString());

                    triangleIndices[ti] = (ushort)currentIndex;
                    triangleIndices[ti + 1] = (ushort)topRightIndex;
                    triangleIndices[ti + 2] = (ushort)rightIndex;
                    ti += 3;

                    triangleIndices[ti] = (ushort)currentIndex;
                    triangleIndices[ti + 1] = (ushort)topIndex;
                    triangleIndices[ti + 2] = (ushort)topRightIndex;
                    ti += 3;
                }
            }
        }

        meshData.subMeshCount = 1;
        meshData.SetSubMesh(0, new SubMeshDescriptor(0, indexCount)
        {
            bounds = bounds,
            vertexCount = vertexCount
        }, MeshUpdateFlags.DontRecalculateBounds);

        var mesh = new Mesh
        {
            bounds = bounds,
            name = "Procedural Mesh"
        };

        vertices.Dispose();
        triangleIndices.Dispose();

        Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh);
        GetComponent<MeshFilter>().mesh = mesh;
        myMesh = GetComponent<MeshFilter>().mesh;
        verticesInitialPos = myMesh.vertices;
    }

    void OnDrawGizmos()
    {
        if (gizmos == GizmoMode.Nothing || myMesh == null)
        {
            return;
        }

        bool drawVertices = (gizmos & GizmoMode.Vertices) != 0;
        bool drawNormals = (gizmos & GizmoMode.Normals) != 0;
        bool drawTangents = (gizmos & GizmoMode.Tangents) != 0;

        Transform t = transform;
        for (int i = 0; i < myMesh.vertices.Length; i++)
        {
            Vector3 position = t.TransformPoint(myMesh.vertices[i]);
            if (drawVertices)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(position, 0.02f);
            }
            if (drawNormals)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(position, t.TransformDirection(myMesh.normals[i]) * 0.2f);
            }
            if (drawTangents)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(position, t.TransformDirection(myMesh.normals[i]) * 0.2f);
            }
        }
    }

    void ModifyRing(int ringLevel, float value)
    {
        Vector3[] myVertices = myMesh.vertices;
        float ringRadiusPer = ringRadiusPercentages[ringLevel];
        ringRadiusPercentages[ringLevel] = Mathf.Clamp(ringRadiusPer + value, minRadiusPercentage, maxRadiusPercentage);
        for (int vi = ringLevel * (resolutionU + 1); vi < (ringLevel + 1) * (resolutionU + 1); vi++)
        {
            myVertices[vi].x = ringRadiusPer * verticesInitialPos[vi].x;
            myVertices[vi].z = ringRadiusPer * verticesInitialPos[vi].z;
        }

        myMesh.vertices = myVertices;
        myMesh.RecalculateBounds();
    }

    void Start()
    {
        Renderer objectRenderer = GetComponent<Renderer>();
        Vector3 objectSize = objectRenderer.bounds.size;
        for (int i = 0; i <= resolutionV; i++)
        {
            ringRadiusPercentages[i] = 1.0f;
        }
    }

    void OnMouseDown()
    {
        chosenZCoordinate = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
        GetAndSetRingLevel();
    }

    int GetAndSetRingLevel()
    {
        // get the mouse offset relative to the object
        mouseOffset = gameObject.transform.position - GetMouseWorldPosition();
        // get the dimensions of the object
        Renderer objectRenderer = GetComponent<Renderer>();
        Vector3 objectSize = objectRenderer.bounds.size;
        // find out which ring is hovered over
        float relativeYFromBottom = Mathf.Clamp((objectSize.y / 2 - mouseOffset.y), 0.0f, (1.0f / resolutionV) + objectSize.y);
        float percentageY = relativeYFromBottom / objectSize.y;
        ringLevel = min((int)Mathf.Round(resolutionV * percentageY), resolutionV - 1);
        return ringLevel;
    }

    void OnMouseUp()
    {
        ringLevel = -1;
        // for(int i = 0; i < resolutionV; i++)
        // {
        //     Debug.Log(i.ToString() + ' ' + ringRadiusPercentages[i].ToString("F4"));
        // }
    }

    void OnMouseDrag()
    {
        Debug.Log(ringLevel);
        if (ringLevel > -1)
        {
            mousePosX = -mouseOffset.x;
            mouseOffset = gameObject.transform.position - GetMouseWorldPosition();
            ModifyNRings();
        }
    }

    void ModifyNRings()
    {
        int firstRing = max(0, ringLevel - numberOfAffectedNeighbourPairs);
        int lastRing = min(resolutionV, ringLevel + numberOfAffectedNeighbourPairs);
        float modificationRate;
        for(int ring = firstRing; ring <= lastRing; ring++)
        {
            Debug.Log(Input.GetAxis("Mouse X").ToString("F4"));
            modificationRate = (Input.GetAxis("Mouse X") / 2.0f) * GetModificationRate(ring);
            // Debug.Log(modificationRate.ToString("F4"));
            ModifyRing(ring, modificationRate * Time.deltaTime);
        }
    }

    float GetModificationRate(int currentRing)
    {
        // get the distance to the principal ring (the one that is hovered over)
        int ringDistance = (int)Mathf.Abs(currentRing - ringLevel);
        // calculate the altered modification rate
        // Debug.Log(currentRing.ToString() + ' ' + ringDistance.ToString() + ' ' + pow(((float)ringDistance / (numberOfAffectedNeighbourPairs + 1)), 2).ToString());
        // Debug.Log(radiusModifyRate - pow((ringDistance / (numberOfAffectedNeighbourPairs + 1)), 2));
        return radiusModifyRate * (1.0f - pow(((float)ringDistance / (numberOfAffectedNeighbourPairs + 1)), 2));
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 hoveredPixel = Input.mousePosition; // the mouse position on the screen (the position of the pixel hovered over)
        hoveredPixel.z = chosenZCoordinate;

        return Camera.main.ScreenToWorldPoint(hoveredPixel);
    }

    void Update()
    {

    }
}