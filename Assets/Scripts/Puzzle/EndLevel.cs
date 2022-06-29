using UnityEngine.SceneManagement;
using UnityEngine;

public class EndLevel : MonoBehaviour
{
    PuzzlesHandler puzzlesHandler;
    private void Start() {
        puzzlesHandler = GetComponent<PuzzlesHandler>();
    }

    void OnControllerColliderHit(ControllerColliderHit other) {
        if(puzzlesHandler.HasBeenValidated() && other.gameObject.CompareTag("Player"))
            SceneManager.LoadScene("LevelSelect");
    }
}
