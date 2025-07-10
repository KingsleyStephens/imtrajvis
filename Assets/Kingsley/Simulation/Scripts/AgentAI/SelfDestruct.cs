using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    public float destructTime = 3.0f;
    private float spawnTime;

    // Start is called before the first frame update
    void Start()
    {
        spawnTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - spawnTime > destructTime)
        {
            gameObject.SetActive(false);
            //Destroy(gameObject);
        }
    }
}
