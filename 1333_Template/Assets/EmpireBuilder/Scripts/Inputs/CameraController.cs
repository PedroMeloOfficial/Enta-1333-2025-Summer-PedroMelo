using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera cam;

    [Header("General Settings:")]
    public float cameraSpeed = 20.0f;
    public float scrollSpeed = 20.0f;

    [Header("Scroll Clamp Settings:")]
    public float minScroll = 5.0f;
    public float maxScroll = 100.0f;

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = cam.transform.position;

        // Camera movements inputs
        if(Input.GetKey(KeyCode.W))
        {
            pos.z += cameraSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S))
        {
            pos.z -= cameraSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D))
        {
            pos.x += cameraSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A))
        {
            pos.x -= cameraSpeed * Time.deltaTime;
        }

        // Camera zoom input
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        pos.y -= scroll * scrollSpeed * 50f * Time.deltaTime;

        // Clamps
        pos.y = Mathf.Clamp(pos.y, minScroll, maxScroll);

        // Set position of this game object
        cam.transform.position = pos;
    }

    public void Initialize(Camera _cam)
    {
        cam = _cam;
    }

}