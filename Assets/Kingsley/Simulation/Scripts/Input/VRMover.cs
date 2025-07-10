using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using Unity.XR.OpenVR;

public class VRMover : MonoBehaviour
{

    /*
    Writen by Windexglow 11-13-10.  Use it, edit it, steal it I don't care.  
    Converted to C# 27-02-13 - no credit wanted.
    Simple flycam I made, since I couldn't find any others made public.  
    Made simple to use (drag and drop, done) for regular keyboard layout  
    wasd : basic movement
    shift : Makes camera accelerate
    space : Moves camera on X and Z axis only.  So camera doesn't gain any height*/

    public GameObject mainCamera;

//    private InputControls controls;

    float mainSpeed = 100.0f; //regular speed
    float shiftAdd = 250.0f; //multiplied by how long shift is held.  Basically running
    float maxShift = 1000.0f; //Maximum speed when holdin gshift
    float camSens = 25.0f; //How sensitive it with mouse
    private Vector3 lastMouse = new Vector3(255, 255, 255); //kind of in the middle of the screen, rather than at the top (play)
    private float totalRun = 1.0f;

    Vector3 p_Velocity = new Vector3();

    private void Awake()
    {
  //      controls = new InputControls();
    }

    private void OnEnable()
    {
    //    controls.Player.Enable();
    }

    private void OnDisable()
    {
      //  controls.Player.Disable();
    }

    private void Start()
    {
    }

    void FixedUpdate()
    {
        //Keyboard commands
        float f = 0.0f;
        Vector3 p = p_Velocity;
        if (p.sqrMagnitude > 0)
        { // only move while a direction key is pressed
            p = p * mainSpeed;
            p = p * Time.deltaTime;
            transform.Translate(p);

            p_Velocity = Vector3.zero;
        }
    }

    public void Pan(Vector2 axis)
    {
        Debug.Log("Pan");
        // p_Velocity += mainCamera.transform.TransformDirection(Vector3.right) * controls.Player.Pan.ReadValue<float>();

        p_Velocity += mainCamera.transform.TransformDirection(Vector3.right) * axis.x;
    }

    public void Move()
    {
 //       p_Velocity += mainCamera.transform.TransformDirection(Vector3.right) * controls.Player.Move.ReadValue<float>();
    }

    public void Vertical()
    {
  //      p_Velocity += mainCamera.transform.TransformDirection(Vector3.right) * controls.Player.Vertical.ReadValue<float>();
    }

}