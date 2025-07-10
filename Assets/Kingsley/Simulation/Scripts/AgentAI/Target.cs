using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public Vector3 Position;
    public Vector3 Orientation;
    public GameObject TargetGameObject;

    [SerializeField]
    private float debugScale = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        if (TargetGameObject != null)
        {
            Position = TargetGameObject.transform.position;
            Orientation = TargetGameObject.transform.rotation.eulerAngles;
        }

        Debug.DrawLine(Position, Position + Vector3.right * debugScale, Color.red);
        Debug.DrawLine(Position, Position - Vector3.right * debugScale, Color.red);
        Debug.DrawLine(Position, Position + Vector3.up * debugScale, Color.red);
        Debug.DrawLine(Position, Position - Vector3.up * debugScale, Color.red);
        Debug.DrawLine(Position, Position + Vector3.forward * debugScale, Color.red);
        Debug.DrawLine(Position, Position - Vector3.forward * debugScale, Color.red);
    }
}
