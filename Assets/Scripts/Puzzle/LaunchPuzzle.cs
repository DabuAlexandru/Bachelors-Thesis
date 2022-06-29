using UnityEngine;
using UnityEngine.SceneManagement;

public class LaunchPuzzle : MonoBehaviour
{
    [SerializeField]
    GameObject InteractPromt;
    Transform Player;
    bool isActive;
    int puzzleID;
    float minValidScore;
    // Start is called before the first frame update
    void Start()
    {
        isActive = false;
        InteractPromt.SetActive(false);
        Player = GameObject.FindWithTag("Player").transform;
        GameObject parent = transform.parent.gameObject;
        puzzleID = parent.GetComponent<Puzzle>().getColumnID();
        minValidScore = parent.GetComponent<Puzzle>().getMinValidScore();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isActive = true;
            InteractPromt.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isActive = false;
            InteractPromt.SetActive(false);
        }
    }
    
    // Update is called once per frame
    void Update()
    {
        if (isActive && Input.GetKeyUp(KeyCode.E) && !PauseMenu.gameIsPaused)
        {
            PlayerPrefs.SetFloat("PlayerPositionX", Player.position.x);
            PlayerPrefs.SetFloat("PlayerPositionY", Player.position.y);
            PlayerPrefs.SetFloat("PlayerPositionZ", Player.position.z);
            PlayerPrefs.SetFloat("PlayerRotationY", Player.rotation.y);
            PlayerPrefs.SetInt("PuzzleID", puzzleID);
            PlayerPrefs.SetFloat("PuzzleMinScore", minValidScore);
            SceneManager.LoadScene("SculptingPuzzle");
        }
    }
}
