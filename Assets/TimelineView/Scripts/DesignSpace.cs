using System.Collections.Generic;
using UnityEngine;
using static KingsleyPhD.TimelineView;
using UnityEngine.InputSystem.XR;
using System.IO;
using UnityEngine.UI;

namespace KingsleyPhD
{
    /**
     * User facing class for adding a timeline to a scene
     */
    public class DesignSpace : MonoBehaviour
    {
        // PUBLIC FIELDS
        [SerializeField]
        private GameObject cellPrototype;
        [SerializeField]
        private GameObject lineBatcherPrototype;
        [SerializeField]
        private TimelineView.CellConfig cellConfig;
        [SerializeField]
        float categoriesBuffer = 5.0f;

        [SerializeField]
        private string jsonFile;


        // PRIVATE DATA

        private class CellLine
        {
            public TimelineModel model;
            public TimelineView view;
            public TimelineController controller;
        }

        private class CellLookup
        {
            public string category;
            public string dimension;
            public string option;
        }

        private List<CellLine> mCategoryRows = new List<CellLine>();
        private List<CellLine> mDimensionRows = new List<CellLine>();
        private List<CellLine> mOptionColumns = new List<CellLine>();

        private Dictionary<string, CellLookup> mCellLookup= new Dictionary<string, CellLookup>();

        // PUBLIC
        public DesignInstanceJSON DesignInstance { get; set; }

        public delegate void Change();
        public event Change OnChange;

        // PRIVATE


        /**
         * Start
         */
        void Start()
        {
            DesignSpaceJSON designSpaceJSON = DesignSpaceJSON.Load(Path.Combine(Application.streamingAssetsPath,jsonFile));
            if (designSpaceJSON != null)
            {
                float width = 0.0f;
                foreach (var category in designSpaceJSON.categories)
                {
                    width += cellConfig.cellSize.x * category.dimensions.Length + categoriesBuffer;
                }
                float start = -width * 0.5f;

                float spacing = 0.0f;
                int index = 0;
                float pos = 0.0f;
                foreach (var category in designSpaceJSON.categories)
                {
                    spacing = cellConfig.cellSize.x * category.dimensions.Length;
                    GameObject catParent = new GameObject("Category " + category.name);
                    catParent.transform.SetParent(transform);
                    catParent.transform.localPosition = new Vector3(start + pos, 0.0f, 0.0f);

                    CellLine catRow = new CellLine();

                    catRow.model = new TimelineModel();
                    catRow.model.AddCell(TimelineModel.Cell.CellType.Static, category.name);

                    catRow.view = new TimelineView(cellPrototype, lineBatcherPrototype, catParent.transform, cellConfig);
                    catRow.view.SetLayout(LayoutType.HorizontalLine, 1);
                    catRow.controller = new TimelineController(catRow.model, catRow.view);
                    //catRow.controller.OnActivateCell += CellActivation;

                    mCategoryRows.Add(catRow);

                    index++;

                    GameObject dimParent = new GameObject("Dimensions " + category.name);
                    dimParent.transform.SetParent(transform);
                    dimParent.transform.position = catParent.transform.position + new Vector3(0.0f, -2.5f, 0.0f);
                    CellLine dimRow = new CellLine();
                    dimRow.model = new TimelineModel();

                    int subIndex = 0;
                    foreach (var dimension in category.dimensions)
                    {
                        dimRow.model.AddCell(TimelineModel.Cell.CellType.Static,dimension.name);

                        GameObject optParent = new GameObject("Options " + category.name);
                        optParent.AddComponent<ToggleGroup>();
                        optParent.transform.SetParent(transform);
                        optParent.transform.position = dimParent.transform.position + new Vector3(subIndex * cellConfig.cellSize.x, -2.5f, 0.0f);
                        CellLine optCol = new CellLine();
                        optCol.model = new TimelineModel();

                        foreach (var option in dimension.options)
                        {
                            bool selected = DesignInstance.Query(category.name, dimension.name, option);

                            string id = optCol.model.AddCell(dimension.singular ? TimelineModel.Cell.CellType.Toggle : TimelineModel.Cell.CellType.Radio, option, selected);
                            mCellLookup.Add(id, new CellLookup() { category = category.name, dimension = dimension.name, option = option });
                        }

                        optCol.view = new TimelineView(cellPrototype, lineBatcherPrototype, optParent.transform, cellConfig);
                        optCol.view.SetLayout(LayoutType.VerticalLine, dimension.options.Length);
                        optCol.controller = new TimelineController(optCol.model, optCol.view);
                        optCol.controller.OnActivateCell += CellActivation;

                        mOptionColumns.Add(optCol);

                        subIndex++;
                    }

                    dimRow.view = new TimelineView(cellPrototype, lineBatcherPrototype, dimParent.transform, cellConfig);
                    dimRow.view.SetLayout(LayoutType.HorizontalLine, category.dimensions.Length);
                    dimRow.controller = new TimelineController(dimRow.model, dimRow.view);
                    //dimRow.controller.OnActivateCell += CellActivation;

                    mDimensionRows.Add(dimRow);

                    pos += spacing + categoriesBuffer;
                }
            }

            OnChange?.Invoke();
        }

        /**
         * Update
         */
        void Update()
        {
            foreach (var category in mCategoryRows)
            {
                category.controller.Update();
            }

            foreach (var dim in mDimensionRows)
            {
                dim.controller.Update();
            }

            foreach (var opt in mOptionColumns)
            {
                opt.controller.Update();
            }
        }

        private void CellActivation(int index,TimelineView.CellData cellData, bool selected)
        {
            CellLookup cell;
            if (mCellLookup.TryGetValue(cellData.id, out cell))
            {
                DesignInstance.Set(cell.category, cell.dimension, cell.option, selected);

                OnChange?.Invoke();
            }
        }
    }
 }
