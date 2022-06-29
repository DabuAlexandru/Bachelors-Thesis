using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    [SerializeField] private GameObject loadingUI;

    private void Start() {
        loadingUI.SetActive(false);
    }
    public void PlatformerLevel() {
        SceneManager.LoadScene("DemoScene");
        loadingUI.SetActive(true);
    }

    public void IslandLevel() {
        SceneManager.LoadScene("VegetationScene");
        loadingUI.SetActive(true);
    }
    public void MainMenu() => SceneManager.LoadScene("MainMenu");
}
