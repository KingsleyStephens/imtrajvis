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
            Selected,
            GroupSelected,
            GroupDeselected
        }

        private StateType state = StateType.None;

        public StateType State { get { return state; } 
            set {
                if (state != value)
                {
                    if (value == StateType.GroupSelected)
                    {
                        UpdateGroup(StateType.Selected);
                    }
                    else if (value == StateType.GroupDeselected)
                    {
                        UpdateGroup(StateType.None);
                    }
                    else
                    {
                        state = value;
                        UpdateChildren(state);
                    }
                }
            } }


        public void UpdateChildren(StateType state)
        {
            SelectableChildObject[] so = GetComponentsInChildren<SelectableChildObject>();
            foreach (SelectableChildObject soObj in so)
            {
                soObj.SetState(state);
            }
        }

        public void UpdateGroup(StateType state)
        {
            VisAgent data = gameObject.GetComponent<VisAgent>();
            if (data != null)
            {
                if (data.parentId >= 0)
                {
                    //GameObject[] formation = VisClient3D.Instance.GetAllOjectsOfFormation(data.parentId);
                    //foreach (var f in formation)
                    //{
                    //    VisAgent fData = f.GetComponent<VisAgent>();
                    //    if (fData  != null)
                    //    {
                    //        if (fData.id != data.id)
                    //        {
                    //            SelectableObject formationObject = f.GetComponent<SelectableObject>();
                    //            if (formationObject != null)
                    //            {
                    //                formationObject.State = state;
                    //            }
                    //        }
                    //    }
                    //}
                }
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            State = StateType.None;
        }
    }

}