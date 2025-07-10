using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SquadFSMStates
{
    Formation,
    Attack,
    Evade
}

public enum SquadFSMEvents
{
    Attack,
    TargetDestroyed,
    TargetLost,
    Evade,
    Formation
}

public class SquadFSMAgent : FSMAgent<SquadFSMEvents>
{

    protected override void OnAgentStart() {

        FSM<SquadFSMStates, SquadFSMEvents> stateMachine = new FSM<SquadFSMStates, SquadFSMEvents>(SquadFSMStates.Formation);
        stateMachine.AddState(SquadFSMStates.Formation, new FormationState<SquadFSMEvents>(1.0f));
        stateMachine.AddState(SquadFSMStates.Attack, new AttackState<SquadFSMEvents>(1.0f));
        stateMachine.AddState(SquadFSMStates.Evade, new EvadeState<SquadFSMEvents>(1.0f));

        stateMachine.AddTransition(SquadFSMStates.Formation, SquadFSMStates.Attack, new FSMEvent<SquadFSMStates, SquadFSMEvents>(stateMachine, SquadFSMEvents.Attack));
        stateMachine.AddTransition(SquadFSMStates.Attack, SquadFSMStates.Formation, new FSMEvent<SquadFSMStates, SquadFSMEvents>(stateMachine, SquadFSMEvents.TargetDestroyed));
        stateMachine.AddTransition(SquadFSMStates.Attack, SquadFSMStates.Formation, new FSMEvent<SquadFSMStates, SquadFSMEvents>(stateMachine, SquadFSMEvents.TargetLost));
        stateMachine.AddTransition(SquadFSMStates.Formation, SquadFSMStates.Evade, new FSMEvent<SquadFSMStates, SquadFSMEvents>(stateMachine, SquadFSMEvents.Evade));
        stateMachine.AddTransition(SquadFSMStates.Evade, SquadFSMStates.Formation, new FSMEvent<SquadFSMStates, SquadFSMEvents>(stateMachine, SquadFSMEvents.Formation));


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
