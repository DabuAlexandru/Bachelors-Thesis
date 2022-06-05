using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameHandler : MonoBehaviour
{
    Transform Player;

    [SerializeReference] private Material skybox;

    void Start()
    {
        Player = GameObject.FindWithTag("Player").transform;

        Scene scene = SceneManager.GetActiveScene();

        if(scene.name == "DemoScene")
            RenderSettings.skybox = skybox;
            
        InitializePlayerPosition();
    }

    void InitializePlayerPosition()
    {
        Vector3 position;
        position.x = PlayerPrefs.GetFloat("PlayerPositionX", 0.0f);
        PlayerPrefs.DeleteKey("PlayerPositionX");

        position.y = PlayerPrefs.GetFloat("PlayerPositionY", 0.0f);
        PlayerPrefs.DeleteKey("PlayerPositionY");

        position.z = PlayerPrefs.GetFloat("PlayerPositionZ", -3.0f);
        PlayerPrefs.DeleteKey("PlayerPositionZ");
        Player.transform.position = position;

        float rotationY;
        rotationY = PlayerPrefs.GetFloat("PlayerRotationY", 0.0f);
        PlayerPrefs.DeleteKey("PlayerRotationY");

        Player.transform.Rotate(new Vector3(0.0f, rotationY, 0.0f));
    }

    private void OnApplicationQuit() => SavePuzzleData.instance.SavePuzzleDataToFile();
}
