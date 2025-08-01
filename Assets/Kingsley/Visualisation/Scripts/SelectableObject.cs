using System.Runtime.CompilerServices;
using UnityEngine;

namespace KVisualisation
{

    public class SelectableObject : MonoBehaviour
    {
        public enum StateType
        {
            None,
            Hover,
            Selected
        }

        private StateType state = StateType.None;

        public StateType State { get { return state; } set { state = value; UpdateChildren(state); } }


        public void UpdateChildren(StateType state)
        {
            SelectableChildObject[] so = GetComponentsInChildren<SelectableChildObject>();
            foreach (SelectableChildObject soObj in so)
            {
                soObj.SetState(state);
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            State = StateType.None;
        }
    }

}