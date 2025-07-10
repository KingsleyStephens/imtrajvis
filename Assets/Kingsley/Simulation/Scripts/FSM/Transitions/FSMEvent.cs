using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMEvent<S, E> : FSMTransition where S : Enum where E : Enum
{
    private E eventType;
    private FSM<S, E> fsm;

    public FSMEvent(FSM<S, E> fsm, E eventType)
    {
        this.fsm = fsm;
        this.eventType = eventType;
    }

    public override bool Test()
    {
        return fsm.HasEvent(eventType);
    }

    public override void OnTrigger() 
    {
    }
}
