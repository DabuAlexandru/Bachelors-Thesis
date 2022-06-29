using UnityEngine;

public class ChangeMaterial : MonoBehaviour
{
    public Material inactive;
    public Material active;
    void Start()
    {
        PuzzlesHandler puzzlesHandler = GetComponent<PuzzlesHandler>();
        Renderer renderer = GetComponent<Renderer>();
        renderer.sharedMaterial = puzzlesHandler.HasBeenValidated() ? active : inactive;
        // Collider collider = GetComponent<Collider>();
        // collider.isTrigger = puzzlesHandler.HasBeenValidated() ? true : false;
    }
}
