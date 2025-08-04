using System.Collections.Generic;
using UnityEngine;
using static KVisualisation.SelectableObject;

namespace KVisualisation
{

    public class SelectableChildObject : MonoBehaviour
    {
        [SerializeField]
        private Material hoverMaterial;

        [SerializeField]
        private Material selectMaterial;

        private Material baseMaterial;

        private MeshRenderer meshRenderer;
        private LineRenderer lineRenderer;

        private SelectableObject parentSelectable;

        private float doubleTapTime = 1.0f;


        private class History
        {
            public History(StateType state, float time)
            {
                this.state = state;
                this.time = time;
            }

            public StateType state = StateType.None;
            public float time = 0.0f;
        }

        private List<History> history = new List<History>();

        public void SetState(StateType state)
        {
            switch (state)
            {
                case StateType.None:
                    {
                        SetMaterial(baseMaterial);
                        break;
                    }
                case StateType.Hover:
                    {
                        SetMaterial(hoverMaterial);
                        break;
                    }
                case StateType.Selected:
                    {
                        SetMaterial(selectMaterial);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        private void SetMaterial(Material material)
        {
            if (meshRenderer != null)
            {
                meshRenderer.material = material;
            }

            if (lineRenderer != null)
            {
                lineRenderer.material = material;
            }
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                baseMaterial = meshRenderer.material;
            }

            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                baseMaterial = lineRenderer.material;
            }


            parentSelectable = GetComponentInParent<SelectableObject>();
        }


        public void OnHover()
        {
            if (parentSelectable.State != StateType.Selected)
            {
                parentSelectable.State = StateType.Hover;
            }
        }

        public void OnUnHover()
        {
            if (parentSelectable.State != StateType.Selected)
            {
                parentSelectable.State = StateType.None;
            }
        }

        public void OnSelect()
        {
            if (parentSelectable.State != StateType.Selected)
            {
                if (history.Count == 1 && Time.time - history[0].time < doubleTapTime)
                { 
                    parentSelectable.State = StateType.GroupDeselected;
                    history.Clear();
                }
                else
                {
                    history.Clear();
                    parentSelectable.State = StateType.Selected;
                    history.Add(new History(StateType.Selected, Time.time));
                }
            }
            else
            {
                if (history.Count == 1 && Time.time - history[0].time < doubleTapTime)
                {
                    parentSelectable.State = StateType.GroupSelected;
                    history.Clear();
                }
                else
                {
                    history.Clear();
                    parentSelectable.State = StateType.Hover;
                    history.Add(new History(StateType.Hover, Time.time));
                }
            }
        }

    }

}