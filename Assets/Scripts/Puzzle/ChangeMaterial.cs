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
    }
}
