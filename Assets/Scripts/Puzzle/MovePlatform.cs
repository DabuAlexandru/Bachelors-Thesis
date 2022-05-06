using UnityEngine;

public class MovePlatform : MonoBehaviour
{
    [SerializeField]
    private Vector3 translate = new Vector3(0.0f, 0.0f, 0.0f);
    private bool doneTranslating = false;
    private bool beginTranslation = false;

    [SerializeField, Range(0.1f, 10.0f)]
    private float unitsPerSecond = 1.0f;

    private Vector3 origin;
    private Vector3 destination;
    private float totalDistance;

    private float startTime;

    void Start()
    {
        origin = transform.position;
        destination = transform.position + translate;
        totalDistance = Vector3.Distance(Vector3.zero, translate);
        startTime = Time.time;

        PuzzlesHandler puzzlesHandler = GetComponent<PuzzlesHandler>();
        beginTranslation = puzzlesHandler.HasBeenValidated();
    }

    void Update()
    {
        if (!doneTranslating && beginTranslation)
        {
            float distCovered = (Time.time - startTime) * unitsPerSecond;
            if (distCovered == totalDistance)
                doneTranslating = true;
            distCovered = Mathf.Min(distCovered, totalDistance);
            transform.position = Vector3.Lerp(origin, destination, distCovered / totalDistance);
        }
    }
}
