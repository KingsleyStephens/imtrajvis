using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderAttack : FSMTransition
{
    private Agent thisAgent;
    private Formation formation;
    private int currentSize = 0;

    public UnderAttack(Agent thisAgent)
    {
        this.thisAgent = thisAgent;

        formation = thisAgent.GetComponent<Formation>();
        if (formation != null)
        {
            currentSize = formation.SquadSize;
        }
    }

    public override bool Test()
    {
        return formation.SquadSize < currentSize;
    }

    public override void OnTrigger() 
    {
   
    }
}
