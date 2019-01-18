using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float speed = 10;
    public float jumpSpeed = 5;
    public float rotationSpeed = 100;

    public float lookUpSpeed = 100;

    public float viewPitchRange = 90;

    public float gravity = 20f;

    float pitch = 1;
    Vector3 moveDirection = Vector3.zero;

    Camera fppCamera;

    CharacterController controller;

    private void Start()
    {
        fppCamera = GetComponentInChildren<Camera>();
        controller = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    private void Update()
    {
        if(Mathf.Abs(Input.GetAxis("Mouse X")) > 0)
        {
            float yaw = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
            transform.Rotate(new Vector3(0, yaw, 0));
        }

        if (Mathf.Abs(Input.GetAxis("Mouse Y")) > 0)
        {
            pitch += Input.GetAxis("Mouse Y") * lookUpSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, -viewPitchRange, viewPitchRange);

            fppCamera.transform.localRotation = Quaternion.Euler(pitch, 0, 0);
        }

        if(controller.isGrounded)
        {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection *= speed;

            if(Input.GetButton("Jump"))
            {
                moveDirection.y = jumpSpeed;
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;
        controller.Move(moveDirection * Time.deltaTime);
    }
}
