using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace KingsleyStephens
{
    namespace VRUI
    {
        /**
         * Model class for a timeline
         */
        public class TimelineModel
        {
            // PUBLIC CLASSES

            /**
             * Class for an individual element on a timeline
             */
            public class Cell
            {
                public enum CellType
                {
                    Static,
                    Button,
                    Toggle,
                    Radio,
                    IconStatic
                }

                public CellType type = CellType.Static;

                public string id;  // Unique program relevant ID
                public string label;
                public bool selected;
                public string image;
                public Sprite icon;

                /**
                 * Constructor
                 */
                public Cell(CellType type, string id, string label = "", bool selected = false, string image = "", Sprite icon = null)
                {
                    this.type = type;
                    this.id = id;
                    this.label = label;
                    this.selected = selected;
                    this.image = image;
                    this.icon = icon;
                }
            }

            // PRIVATE DATA

            private List<Cell> mCells = new List<Cell>();


            // PUBLIC INTERFACE

            /**
             * Number of cells in the model
             */
            public int CellCount { get { return mCells.Count; } }

            /**
             * Enumerator to access cells
             */
            public IEnumerable<Cell> Cells()
            {
                foreach (var cell in mCells)
                {
                    yield return cell;
                }
            }

            /**
             * Add a new cell
             */
            public string AddCell(Cell.CellType type, string label = "", bool selected = false, string image = "", Sprite icon = null)
            {
                string id = Guid.NewGuid().ToString();
                mCells.Add(new Cell(type, id, label, selected, image, icon));
                return id;
            }

            public void AddCell(string id, Cell.CellType type, string label = "", bool selected = false, string image = "", Sprite icon = null)
            {
                mCells.Add(new Cell(type, id, label, selected, image, icon));
            }

            /**
             * Remove cell
             */
            public void RemoveCell(string id)
            {
                int index = mCells.FindIndex(cell => cell.id == id);
                if (index >= 0)
                {
                    mCells.RemoveAt(index);
                }
            }

            public void Reset()
            {
                mCells.Clear();
            }
        }

    }
}
