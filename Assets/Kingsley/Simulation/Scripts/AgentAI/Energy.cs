using UnityEngine;

public class Energy : MonoBehaviour
{
    [SerializeField]
    private float energy = 0.0f;

    [SerializeField]
    private float mass = 0.0f;

    [SerializeField]
    private float power = 0.0f;


    public float EnergyReserve { get => energy; }

    private MoveableObject moveableObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        moveableObject = GetComponent<MoveableObject>();

        energy = 100.0f;
        mass = Random.Range(1.0f, 3.0f);
        power = Random.Range(0.1f, 0.2f);
    }

    // Update is called once per frame
    void Update()
    {
        float s = moveableObject.Velocity.magnitude;

        energy -= s * power * mass * Time.deltaTime;

        if (energy < 0.0f)
        {
            energy = 0.0f;
        }
    }
}
