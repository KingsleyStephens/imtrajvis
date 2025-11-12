using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KingsleyStephens
{
    namespace VRUI
    {
        /**
         * The view class for the timeline MVC
         */
        public class TimelineView
        {
            // PUBLIC INTERFACES

            /**
             * Display cell
             */
            public class CellData
            {
                /**
                 * The type of visual cell this is
                 */
                public enum CellType
                {
                    Data,
                    NewCellButton,
                    Label,
                    Toggle,
                    Radio,
                    IconLabel
                }

                public string id;  // Unique ID
                public CellType type;   // It's type
                public string name;
                public bool selected;
                public string image;
                public Sprite icon;

                /**
                 * Constructor
                 */
                public CellData(string id, CellType type, string name = "", bool selected = false, string image = "", Sprite icon = null)
                {
                    this.id = id;
                    this.type = type;
                    this.name = name;
                    this.selected = selected;
                    this.image = image;
                    this.icon = icon;
                }
            }

            /**
             * Visual property template
             */
            [Serializable]
            public class CellConfig
            {
                public Vector3 anchorPosition;
                public Quaternion anchorRotation;
                public Vector2 cellSize;
            }


            /**
             * The type of view layout
             */
            public enum LayoutType
            {
                HorizontalLine,
                VerticalLine,
                Grid
            }

            public delegate void ActivateCell(int index, CellData cellData, bool selected);
            public event ActivateCell OnActivateCell;

            public delegate void CloseCell(CellData cellData);
            public event CloseCell OnCloseCell;

            // PUBLIC DATA

            // PRIVATE DATA

            // Current layout
            private LayoutType mLayout = LayoutType.HorizontalLine;
            private int mNumCells = 0;

            private GameObject mCellPrototype;
            private GameObject mLineBatcherPrototype;
            private Transform mParent;
            private CellConfig mDisplayConfig;

            // PUBLIC

            // Accessor
            public LayoutType Layout { get { return mLayout; } }

            public int NumCells { get { return mNumCells; } }

            /**
             * Constructor
             */
            public TimelineView(GameObject cellPrototype, GameObject lineBatcherPrototype, Transform parent, CellConfig cellConfig)
            {
                mCellPrototype = cellPrototype;
                mLineBatcherPrototype = lineBatcherPrototype;
                mParent = parent;
                mDisplayConfig = cellConfig;
            }

            /**
             * Specify the layout type to display
             */
            public void SetLayout(LayoutType layout, int numCells)
            {
                mLayout = layout;
                mNumCells = numCells;

                //int width, height;
                //GetDimensions(out width, out height);
                //Vector3 shift = Vector3.zero;
                //if (width > 1)
                //{
                //    shift = new Vector3(mParent.transform.position.x - width * mDisplayConfig.cellSize.x * 0.5f, 0.0f, 0.0f);
                //}
                //if (mLayout == LayoutType.Grid)
                //{
                //    shift.y += mParent.transform.position.y + height * mDisplayConfig.cellSize.y * 0.5f;
                //}

                //mDisplayConfig.anchorPosition += shift;
            }

            public void Display(CellData[] cellsData)
            {
                DeleteChildTransforms(mParent);

                int index = 0;
                foreach (var cellData in cellsData)
                {
                    GameObject cell = GameObject.Instantiate(mCellPrototype, mParent, true);
                    cell.transform.localPosition = Vector3.zero;
                    cell.transform.localRotation = Quaternion.identity;

                    Canvas[] canvases = cell.GetComponentsInChildren<Canvas>();
                    foreach (var c in canvases)
                    {
                        RectTransform rt = c.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            if (rt.name == "Toggle" || rt.name == "Radio" || rt.name == "Image")
                            {
                                rt.localPosition = rt.localPosition + new Vector3(50.0f, 0.0f, 0.0f);

                                RectTransform[] rts = c.GetComponentsInChildren<RectTransform>();
                                foreach (var r in rts)
                                {
                                    r.sizeDelta = r.sizeDelta * new Vector2(mDisplayConfig.cellSize.y, mDisplayConfig.cellSize.y);
                                }

                                if (cellData.type == CellData.CellType.Radio)
                                {
                                    ToggleGroup tg = mParent.GetComponent<ToggleGroup>();
                                    if (tg != null)
                                    {
                                        UnityEngine.UI.Toggle[] toggles = rt.GetComponentsInChildren<UnityEngine.UI.Toggle>();
                                        foreach (var t in toggles)
                                        {
                                            if (t.gameObject.activeInHierarchy)
                                            {
                                                t.group = tg;
                                            }
                                        }
                                    }
                                }

                            }
                            else
                            {
                                rt.sizeDelta = rt.sizeDelta * mDisplayConfig.cellSize;
                            }
                        }

                        BoxCollider col = c.GetComponent<BoxCollider>();
                        if (col != null)
                        {
                            col.size = new Vector3(col.size.x * mDisplayConfig.cellSize.x, col.size.y * mDisplayConfig.cellSize.y, col.size.z);
                            col.center = new Vector3(col.center.x * mDisplayConfig.cellSize.x, col.center.y * mDisplayConfig.cellSize.y, col.center.z);
                        }
                    }

                    CellView cellView = cell.GetComponent<CellView>();
                    if (cellView != null)
                    {
                        cellView.CellIndex = index;
                        cellView.CellData = cellData;
                        cellView.OnActivateCell += CellActivation;
                        cellView.OnCloseCell += CellClose;
                    }

                    index++;
                }

                DoLayout(mDisplayConfig, mParent);

                GameObject lineBatcher = GameObject.Instantiate(mLineBatcherPrototype, mParent, true);
                lineBatcher.transform.localPosition = Vector3.zero;
                lineBatcher.transform.localRotation = Quaternion.identity;
                DrawGrid(mDisplayConfig, lineBatcher);
            }

            private void CellActivation(int index, CellData cellData, bool selected)
            {
                OnActivateCell?.Invoke(index, cellData, selected);
            }

            private void CellClose(CellData cellData)
            {
                OnCloseCell?.Invoke(cellData);
            }

            private void DrawGrid(CellConfig displayConfig, GameObject lineBatcher)
            {
                int width, height;
                GetDimensions(out width, out height);

                List<Vector3> vertices = new List<Vector3>();
                List<int> indices = new List<int>();

                Vector3 topLeft = new Vector3(displayConfig.anchorPosition.x - displayConfig.cellSize.x * 0.5f,
                                   displayConfig.anchorPosition.y + displayConfig.cellSize.y * 0.5f,
                                   0.0f);

                // Horizontal lines
                Vector3 startPos = topLeft;
                Vector3 endPos = topLeft + new Vector3(width * displayConfig.cellSize.x, 0.0f, 0.0f);

                for (int i = 0; i < height + 1; i++)
                {
                    vertices.Add(startPos);
                    indices.Add(vertices.Count - 1);

                    vertices.Add(endPos);
                    indices.Add(vertices.Count - 1);

                    // Vertical lines
                    if (i < height)
                    {
                        Vector3 startVPos = startPos;
                        Vector3 endVPos = startVPos;
                        endVPos.y -= displayConfig.cellSize.y;

                        for (int j = 0; j < width + 1; j++)
                        {
                            vertices.Add(startVPos);
                            indices.Add(vertices.Count - 1);

                            vertices.Add(endVPos);
                            indices.Add(vertices.Count - 1);

                            startVPos.x += displayConfig.cellSize.x;
                            endVPos.x = startVPos.x;
                        }
                    }

                    startPos.y -= displayConfig.cellSize.y;
                    endPos.y = startPos.y;
                }

                // Create mesh
                LineBatcher batcher = lineBatcher.GetComponent<LineBatcher>();
                if (batcher != null)
                {
                    batcher.Create(vertices.ToArray(), indices.ToArray());
                }
            }

            private void DoLayout(CellConfig displayConfig, Transform cellsParent)
            {
                int width, height;
                GetDimensions(out width, out height);


                Vector3 topLeft = new Vector3(displayConfig.anchorPosition.x, displayConfig.anchorPosition.y, 0.0f);

                Vector3 currentPos = topLeft;

                int hIndex = 0;
                int vIndex = 0;
                foreach (Transform cell in cellsParent)
                {
                    cell.localPosition = currentPos;

                    hIndex++;
                    if (hIndex >= width)
                    {
                        hIndex = 0;
                        vIndex++;
                    }

                    currentPos = topLeft + new Vector3(displayConfig.cellSize.x * hIndex, -displayConfig.cellSize.y * vIndex, 0.0f);
                }
            }

            private void GetDimensions(out int width, out int height)
            {
                switch (mLayout)
                {
                    case LayoutType.Grid:
                        {
                            width = height = (int)Mathf.Sqrt(mNumCells);
                            break;
                        }
                    case LayoutType.HorizontalLine:
                        {
                            width = mNumCells;
                            height = 1;
                            break;
                        }
                    case LayoutType.VerticalLine:
                        {
                            width = 1;
                            height = mNumCells;
                            break;
                        }
                    default:
                        {
                            width = height = 0;
                            break;
                        }
                }
            }

            private void DeleteChildTransforms(Transform parent)
            {
                List<Transform> children = new List<Transform>();
                foreach (Transform child in parent)
                {
                    children.Add(child);
                }
                parent.DetachChildren();
                foreach (Transform child in children)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }
        }

    }
}
