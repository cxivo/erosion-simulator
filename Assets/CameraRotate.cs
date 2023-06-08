using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraRotate : MonoBehaviour
{
    public MeshGenerator meshGenerator;
    public Transform centre;
    public Transform cameraTransform;

    // different from my Vector3D - I just really didn't want to use Unity's structures
    Vector3 previousMousePosition = new Vector3();

    void Update()
    {
        // so we don't move the scene while clicking buttons
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            if (Input.GetMouseButton(0))
            {
                // rotating the camera around a centre
                float horizontal = 5f * (Input.mousePosition.x - previousMousePosition.x);
                cameraTransform.RotateAround(centre.position, Vector3.up, horizontal * Time.deltaTime);
                float vertical = -5f * (Input.mousePosition.y - previousMousePosition.y);
                cameraTransform.RotateAround(centre.position, cameraTransform.right, vertical * Time.deltaTime);
            }

            // zoom
            cameraTransform.position += cameraTransform.forward * Input.mouseScrollDelta.y * 250f * Time.deltaTime;
        }
        previousMousePosition = Input.mousePosition;
    }
}
