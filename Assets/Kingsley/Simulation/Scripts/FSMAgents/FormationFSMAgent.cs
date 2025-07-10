using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum FomationFSMStates
{
    Explore,
    Engage,
    Evade
}

public enum FomationFSMEvents
{

}

public class FomationFSMAgent : FSMAgent<FomationFSMEvents>
{ 


    public float sightRange = 100.0f;
    public float disengageRange = 110.0f;
    public float evadeRange = 150.0f;

    protected override void OnAgentStart() {

        FSM<FomationFSMStates, FomationFSMEvents> stateMachine = new FSM<FomationFSMStates, FomationFSMEvents>(FomationFSMStates.Explore);
        stateMachine.AddState(FomationFSMStates.Explore, new ExploreState<FomationFSMEvents>(1.0f, 50.0f, 50.0f, 1.0f));
        stateMachine.AddState(FomationFSMStates.Engage, new EngageState<FomationFSMEvents>(1.0f));
        stateMachine.AddState(FomationFSMStates.Evade, new EvadeState<FomationFSMEvents>(1.0f));

        stateMachine.AddTransition(FomationFSMStates.Explore, FomationFSMStates.Engage, new TargetSighted(this, sightRange));
        stateMachine.AddTransition(FomationFSMStates.Engage, FomationFSMStates.Explore, new NoTargets(this, disengageRange));
        stateMachine.AddTransition(FomationFSMStates.Explore, FomationFSMStates.Evade, new UnderAttack(this));
        stateMachine.AddTransition(FomationFSMStates.Evade, FomationFSMStates.Explore, new NoTargets(this, evadeRange));

        FSMAgentInit(stateMachine);

        base.OnAgentStart();

        EventManager.Instance?.AddEvent(new EventManager.Event(
            Id,
            EventManager.EventType.OperationLifeCycle, 
            EventManager.EventSubType.Begin, 
            "Begin operations: " + transform.position.ToString(), 
            transform.position, transform.forward));
    }
}
