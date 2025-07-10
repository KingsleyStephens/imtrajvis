using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentStatus : MonoBehaviour
{
    public enum StatusType
    {
        Moving,
        Attacking
    }

    public StatusType Status {  get; set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

}
