using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] float xDisplacementFactor;
    [SerializeField] float yDisplacementFactor;
    [SerializeField] float fovDisplacement;
    [SerializeField] Rigidbody playerRb;

    Vector3 initialPosition;
    float initialFov;

    private void Start()
    {
        initialPosition = transform.localPosition;
        initialFov = GetComponent<Camera>().fieldOfView;
    }

    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        Rect screenSize = Screen.safeArea;

        float xDisplacement = (mousePos.x - screenSize.width / 2) * xDisplacementFactor / screenSize.width;
        float yDisplacement = (mousePos.y - screenSize.height / 2) * yDisplacementFactor / screenSize.height;

        transform.localPosition = initialPosition + new Vector3(xDisplacement, yDisplacement, 0);

        Vector3 flatVelocity = playerRb.velocity - Vector3.up * playerRb.velocity.y;
        GetComponent<Camera>().fieldOfView = initialFov + flatVelocity.magnitude * fovDisplacement;
    }
}
