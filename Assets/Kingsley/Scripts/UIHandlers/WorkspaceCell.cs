using KingsleyPhD;
using KingsleyStephens.VRUI;
using UnityEditor;
using UnityEngine;

namespace KingsleyStephens
{
    namespace ImTrajVis
    {
        /**
         * A UI handler for a workspace cell
         */
        public class WorkspaceCell : MonoBehaviour
        {
            // FIELDS

            [Header("UI")]
            [SerializeField]
            private Button menuButton;
            [SerializeField]
            private Button fpvButton;

            [Header("Design space")]
            [SerializeField]
            private DesignSpace designSpace;

            public delegate void ActivateMenu(int index);
            public event ActivateMenu OnMenu = delegate { };

            // PRIVATE

            private CellView mCellView;

            private void Start()
            {
                mCellView = GetComponent<CellView>();

                if (menuButton != null)
                {
                    menuButton.OnActivateButton += () => OnMenu(mCellView != null ? mCellView.CellIndex : -1);
                }

                //if (fpvButton != null)
                //{
                //    fpvButton.OnActivateButton += () => FPVActive = !FPVActive;
                //}
            }
        }
    }
}