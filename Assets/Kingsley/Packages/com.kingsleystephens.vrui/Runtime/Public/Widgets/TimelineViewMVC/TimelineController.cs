using System.Collections.Generic;
using UnityEngine;

namespace KingsleyStephens
{
    namespace VRUI
    {
        /**
         * The controller class for the timeline MVC
         */
        public class TimelineController
        {
            // PUBLIC INTERFACES

            public delegate void NewCell(string id);
            public event NewCell OnNewCell;

            public delegate void ActivateCell(int index, TimelineView.CellData cellData, bool selected);
            public event ActivateCell OnActivateCell;

            public delegate void CloseCell(TimelineView.CellData cellData);
            public event CloseCell OnCloseCell;

            // PRIVATE DATA
            private TimelineModel mModel;
            private TimelineView mView;

            private bool mDirty = true;

            // PUBLIC

            public bool Dirty { set { mDirty = value; } }

            /*
             * Constructor
             */
            public TimelineController(TimelineModel model, TimelineView view)
            {
                mModel = model;
                mView = view;

                mView.OnActivateCell += CellActivation;
                mView.OnCloseCell += CellClose;
            }


            public void Update()
            {
                List<TimelineView.CellData> cellData = new List<TimelineView.CellData>();
                foreach (var cell in mModel.Cells())
                {
                    TimelineView.CellData.CellType cellType = TimelineView.CellData.CellType.Data;
                    switch (cell.type)
                    {
                        case TimelineModel.Cell.CellType.Button:
                            {
                                cellType = TimelineView.CellData.CellType.Data;
                                break;
                            }
                        case TimelineModel.Cell.CellType.Static:
                            {
                                cellType = TimelineView.CellData.CellType.Label;
                                break;
                            }
                        case TimelineModel.Cell.CellType.Toggle:
                            {
                                cellType = TimelineView.CellData.CellType.Toggle;
                                break;
                            }
                        case TimelineModel.Cell.CellType.Radio:
                            {
                                cellType = TimelineView.CellData.CellType.Radio;
                                break;
                            }
                        case TimelineModel.Cell.CellType.IconStatic:
                            {
                                cellType = TimelineView.CellData.CellType.IconLabel;
                                break;
                            }
                    }

                    cellData.Add(new TimelineView.CellData(cell.id, cellType, cell.label, cell.selected, cell.image, cell.icon));
                }

                if (cellData.Count < mView.NumCells)
                {
                    cellData.Add(new TimelineView.CellData("", TimelineView.CellData.CellType.NewCellButton));
                }

                if (mView != null)
                {
                    if (mDirty)
                    {
                        mView.Display(cellData.ToArray());
                        mDirty = false;
                    }
                }
            }

            private void CellActivation(int index, TimelineView.CellData cellData, bool selected)
            {
                switch (cellData.type)
                {
                    case TimelineView.CellData.CellType.NewCellButton:
                        {
                            string newId = mModel.AddCell(TimelineModel.Cell.CellType.Button);
                            OnNewCell?.Invoke(newId);
                            mDirty = true;
                            break;
                        }
                    case TimelineView.CellData.CellType.Data:
                    case TimelineView.CellData.CellType.Toggle:
                    case TimelineView.CellData.CellType.Radio:
                        {
                            OnActivateCell?.Invoke(index, cellData, selected);
                            break;
                        }
                }
            }

            private void CellClose(TimelineView.CellData cellData)
            {
                switch (cellData.type)
                {
                    case TimelineView.CellData.CellType.Data:
                        {
                            mModel.RemoveCell(cellData.id);
                            mDirty = true;
                            OnCloseCell?.Invoke(cellData);
                            break;
                        }
                }
            }
        }

    }
}
