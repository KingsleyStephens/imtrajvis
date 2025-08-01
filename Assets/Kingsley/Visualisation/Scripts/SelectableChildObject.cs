using System.Runtime.CompilerServices;
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
                parentSelectable.State = StateType.Selected;
            }
            else if (parentSelectable.State == StateType.Selected)
            {
                parentSelectable.State = StateType.Hover;
            }
        }

    }

}