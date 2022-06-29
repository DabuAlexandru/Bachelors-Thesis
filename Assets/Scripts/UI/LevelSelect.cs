using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelect : MonoBehaviour
{
    public void PlatformerLevel() => SceneManager.LoadScene("DemoScene");
    public void IslandLevel() => SceneManager.LoadScene("VegetationScene");
    public void MainMenu() => SceneManager.LoadScene("MainMenu");
}
