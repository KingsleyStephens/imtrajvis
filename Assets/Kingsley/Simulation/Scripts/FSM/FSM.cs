using System;
using System.Collections;
using System.Collections.Generic;

public class FSMState<E> where E : Enum
{
    protected Agent mAgent;

    public virtual void OnEnter(Agent agent) { mAgent = agent; }
    public virtual void OnExit() { }
    public virtual void Update(float dt) { }

    public virtual void QueueEvent(E e) { }

}

public class FSMTransition
{
    public virtual bool Test() { return false; }
    public virtual void OnTrigger() { }
}

public class FSM<T, E> : FSMState<E> where T : Enum where E : Enum
{
    private Agent mAgent;
    private T mDefaultState;
    private T mCurrentState;

    private List<E> mEventList = new List<E>();

    private Dictionary<T, FSMState<E>> mStates = new Dictionary<T, FSMState<E>>();

    private struct Transition
    {
        public T state;
        public FSMTransition transition;

        public Transition(T state, FSMTransition transition)
        {
            this.state = state;
            this.transition = transition;
        }
    }
    private Dictionary<T, List<Transition>> mTransitions = new Dictionary<T, List<Transition>>();

    public FSM(T defaultState)
    {
        mDefaultState = defaultState;
    }

    public override void QueueEvent(E e)
    {
        mEventList.Add(e);
    }

    public bool HasEvent(E e)
    {
        return mEventList.Contains(e);
    }

    public void AddState(T state, FSMState<E> stateBehaviour)
    {
        mStates.Add(state, stateBehaviour);
    }

    public void AddTransition(T fromState, T toState, FSMTransition transitionBehaviour)
    {
        if (!mTransitions.ContainsKey(fromState))
        {
            mTransitions.Add(fromState, new List<Transition>());
        }

        List<Transition> transitions;
        mTransitions.TryGetValue(fromState, out transitions);

        transitions.Add(new Transition(toState, transitionBehaviour));
        mTransitions[fromState] = transitions;
    }

    public override void OnEnter(Agent agent) {
        mAgent = agent;
        mCurrentState = mDefaultState;

        RunStateOnEnter();
    }
    public override void OnExit() {
        RunStateOnExit();
    }

    public override void Update(float dt) {
        List<Transition> transitions;
        mTransitions.TryGetValue(mCurrentState, out transitions);
        if (transitions != null)
        {
            foreach (var t in transitions)
            {
                if (t.transition.Test())
                {
                    RunStateOnExit();
                    t.transition.OnTrigger();
                    mCurrentState = t.state;
                    RunStateOnEnter();
                }
            }
        }

        mEventList.Clear();

        RunStateOnUpdate(dt);
    }

    private void RunStateOnEnter()
    {
        FSMState<E> state;
        mStates.TryGetValue(mCurrentState, out state);
        if (state != null)
        {
            state.OnEnter(mAgent);
        }
    }

    private void RunStateOnUpdate(float dt)
    {
        FSMState<E> state;
        mStates.TryGetValue(mCurrentState, out state);
        if (state != null)
        {
            state.Update(dt);
        }
    }

    private void RunStateOnExit()
    {
        FSMState<E> state;
        mStates.TryGetValue(mCurrentState, out state);
        if (state != null)
        {
            state.OnExit();
        }
    }
}

