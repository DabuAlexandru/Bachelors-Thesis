using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Puzzle : MonoBehaviour
{
    const int resolutionU = Constants.resolutionU;
    const int resolutionV = Constants.resolutionV;
    [System.Flags]
    public enum GizmoMode { Nothing = 0, Vertices = 1, Normals = 0b10, Tangents = 0b100 }

    [SerializeField]
    int columnID;

    public int getColumnID()
    {
        return columnID;
    }

    [SerializeField]
    private GizmoMode gizmos;
    private SingleStreamCylindricalProceduralMesh puzzleColumn;
    private Mesh columnMesh;

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

    [SerializeField]
    bool isEditable = false;

    private Vector3[] verticesInitialPos;

    void OnEnable()
    {
        puzzleColumn = new SingleStreamCylindricalProceduralMesh(GetComponent<MeshFilter>());
    }

    void OnDrawGizmos()
    {
        if (gizmos == GizmoMode.Nothing || columnMesh == null)
        {
            return;
        }

        bool drawVertices = (gizmos & GizmoMode.Vertices) != 0;
        bool drawNormals = (gizmos & GizmoMode.Normals) != 0;
        bool drawTangents = (gizmos & GizmoMode.Tangents) != 0;

        Transform t = transform;
        for (int i = 0; i < columnMesh.vertices.Length; i++)
        {
            Vector3 position = t.TransformPoint(columnMesh.vertices[i]);
            if (drawVertices)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(position, 0.02f);
            }
            if (drawNormals)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(position, t.TransformDirection(columnMesh.normals[i]) * 0.2f);
            }
            if (drawTangents)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(position, t.TransformDirection(columnMesh.normals[i]) * 0.2f);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        columnID = PlayerPrefs.GetInt("PuzzleID");
        columnMesh = puzzleColumn.GetMeshFilter().mesh;
        verticesInitialPos = columnMesh.vertices;
        for (int i = 0; i <= resolutionV; i++)
        {
            ringRadiusPercentages[i] = 1.0f;
        }
    }

    void ModifyRing(int ringLevel, float value)
    {
        if (!isEditable)
        {
            return;
        }
        Vector3[] myVertices = columnMesh.vertices;
        Debug.Log(myVertices.ToString());
        float ringRadiusPer = ringRadiusPercentages[ringLevel];
        ringRadiusPercentages[ringLevel] = Mathf.Clamp(ringRadiusPer + value, minRadiusPercentage, maxRadiusPercentage);
        for (int vi = ringLevel * (resolutionU + 1); vi < (ringLevel + 1) * (resolutionU + 1); vi++)
        {
            myVertices[vi].x = ringRadiusPer * verticesInitialPos[vi].x;
            myVertices[vi].z = ringRadiusPer * verticesInitialPos[vi].z;
        }

        columnMesh.vertices = myVertices;
        columnMesh.RecalculateBounds();
    }

    void ModifyNRings()
    {
        if (!isEditable)
        {
            return;
        }
        int firstRing = Mathf.Max(0, ringLevel - numberOfAffectedNeighbourPairs);
        int lastRing = Mathf.Min(resolutionV, ringLevel + numberOfAffectedNeighbourPairs);
        Debug.Log(firstRing.ToString() + " " + lastRing.ToString());
        float modificationRate;
        for (int ring = firstRing; ring <= lastRing; ring++)
        {
            modificationRate = (Input.GetAxis("Mouse X") / 2.0f) * GetModificationRate(ring);
            ModifyRing(ring, modificationRate * Time.deltaTime);
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
        ringLevel = Mathf.Min((int)Mathf.Round(resolutionV * percentageY), resolutionV - 1);
        return ringLevel;
    }

    void OnMouseUp()
    {
        ringLevel = -1;
    }

    void OnMouseDrag()
    {
        if (ringLevel > -1)
        {
            mousePosX = -mouseOffset.x;
            mouseOffset = gameObject.transform.position - GetMouseWorldPosition();
            ModifyNRings();
        }
    }

    float GetModificationRate(int currentRing)
    {
        // get the distance to the principal ring (the one that is hovered over)
        int ringDistance = (int)Mathf.Abs(currentRing - ringLevel);
        // calculate the altered modification rate
        return radiusModifyRate * (1.0f - Mathf.Pow(((float)ringDistance / (numberOfAffectedNeighbourPairs + 1)), 2));
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 hoveredPixel = Input.mousePosition; // the mouse position on the screen (the position of the pixel hovered over)
        hoveredPixel.z = chosenZCoordinate;

        return Camera.main.ScreenToWorldPoint(hoveredPixel);
    }
}
