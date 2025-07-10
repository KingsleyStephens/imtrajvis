using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSMAgent<E> : Agent where E : Enum
{
    public FSMState<E> mState;

    protected void FSMAgentInit(FSMState<E> state)
    {
        mState = state;
    }

    protected override void OnAgentStart() { mState?.OnEnter(this);  }
    protected override void OnAgentUpdate(float dt) { mState?.Update(dt); }
    protected override void OnAgentStop() { mState?.OnExit(); }
}
