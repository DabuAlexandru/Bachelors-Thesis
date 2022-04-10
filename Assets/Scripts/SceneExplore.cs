using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneExplore : MonoBehaviour
{
    Camera mainCamera;
    GameObject cameraParent;
    Quaternion initialRigRot;

    Vector2 rotation = new Vector2(0.0f, 0.0f);

    float zoom = 1.0f;
    const float BASE_FOW = 60.0f, ZOOM_SPEED = 5.0f;
    const float MIN_ZOOM = 0.75f, MAX_ZOOM = 1.25f;

    const float ROTATION_SPEED = 50.0f;
    const float MAX_ROT_X = 15.0f;


    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        cameraParent = mainCamera.transform.parent.gameObject;
        mainCamera.fieldOfView = BASE_FOW;
    }

    // Update is called once per frame
    void Update()
    {
        bool rotated = false;
        // if we have movement on the scroll
        if (Input.mouseScrollDelta.y != 0)
        {
            zoom -= Input.mouseScrollDelta.y * ZOOM_SPEED * Time.deltaTime;
            zoom = Mathf.Clamp(zoom, MIN_ZOOM, MAX_ZOOM);
            Camera.main.fieldOfView = BASE_FOW * zoom;
        }

        if(Input.GetKey("right"))
        {
            rotation.y += ROTATION_SPEED * Time.deltaTime;
            rotated = true;
        }

        if(Input.GetKey("left"))
        {
            rotation.y -= ROTATION_SPEED * Time.deltaTime;
            rotated = true;
        }

        if(Input.GetKey("up"))
        {
            rotation.x += ROTATION_SPEED * Time.deltaTime;
            rotated = true;
        }

        if(Input.GetKey("down"))
        {
            rotation.x -= ROTATION_SPEED * Time.deltaTime;
            rotated = true;
        }

        // Dampen towards the target rotation
        if (rotated)
        {
            rotation.x = Mathf.Clamp(rotation.x, -MAX_ROT_X, MAX_ROT_X);
            rotation.y %= 360;
            Quaternion target = Quaternion.Euler(rotation.x, rotation.y, 0.0f);
            cameraParent.transform.rotation = target;
        }

    }
}
