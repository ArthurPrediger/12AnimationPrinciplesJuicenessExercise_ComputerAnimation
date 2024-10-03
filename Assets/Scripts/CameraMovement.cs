using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CameraMovement : MonoBehaviour
{
    public float speed = 8.0f;
    public float mouseSensitivityHorizontal = 1024.0f;
    public float mouseSensitivityVertical = 768.0f;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetMouseButton((int)MouseButton.LeftMouse) && Time.deltaTime < 0.1f)
        {
            // Mouse input for looking around
            rotationX += Input.GetAxis("Mouse X") * mouseSensitivityHorizontal * Time.deltaTime;
            rotationY -= Input.GetAxis("Mouse Y") * mouseSensitivityVertical * Time.deltaTime;
            rotationY = Mathf.Clamp(rotationY, -90.0f, 90.0f);

            transform.localRotation = Quaternion.Euler(rotationY, rotationX, 0.0f);
        }

        // WASD input for movement
        float moveForwardBackward = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        float moveLeftRight = Input.GetAxis("Horizontal") * speed * Time.deltaTime;

        transform.Translate(moveLeftRight, 0.0f, moveForwardBackward);
    }
}
