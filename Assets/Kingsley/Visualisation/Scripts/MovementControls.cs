using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class MovementControls : MonoBehaviour
{
    public float rotationSpeed = 1.0f;
    public float movementSpeed = 10.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return; // No keyboard connected.

        if (keyboard.leftShiftKey.isPressed && keyboard.rightArrowKey.isPressed)
        {
            transform.Translate(movementSpeed, 0.0f, 0.0f);
        }
        if (keyboard.leftShiftKey.isPressed && keyboard.leftArrowKey.isPressed)
        {
            transform.Translate(-movementSpeed, 0.0f, 0.0f);
        }
        if (keyboard.leftShiftKey.isPressed && keyboard.upArrowKey.isPressed)
        {
            transform.Translate(0.0f, 0.0f, movementSpeed);
        }
        if (keyboard.leftShiftKey.isPressed && keyboard.downArrowKey.isPressed)
        {
            transform.Translate(0.0f, 0.0f, -movementSpeed);
        }


        if (keyboard.leftCtrlKey.isPressed && keyboard.rightArrowKey.isPressed)
        {
            transform.Rotate(rotationSpeed, 0.0f, 0.0f);
        }
        if (keyboard.leftCtrlKey.isPressed && keyboard.leftArrowKey.isPressed)
        {
            transform.Rotate(-rotationSpeed, 0.0f, 0.0f);
        }
        if (keyboard.leftCtrlKey.isPressed && keyboard.upArrowKey.isPressed)
        {
            transform.Rotate(0.0f, 0.0f, rotationSpeed);
        }
        if (keyboard.leftCtrlKey.isPressed && keyboard.downArrowKey.isPressed)
        {
            transform.Rotate(0.0f, 0.0f, -rotationSpeed);
        }
    }
}
