using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.Jigs;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;

namespace PIK_GP_Civil.TEP
{
    public class TableService
    {
        Document doc;
        Database db;
        List<ITEPRow> data;
        public TableService (Document doc)
        {
            this.doc = doc;
            db = doc.Database;
        }

        public void Create (List<ITEPRow> data)
        {
            this.data = data;
            Table table = getTable();
            InsertTable(table);
        }

        private Table getTable ()
        {
            Table table = new Table();
            table.SetDatabaseDefaults();
            table.TableStyle = db.GetTableStylePIK();

            table.SetSize(data.Count()+2, 3);
            table.SetBorders(LineWeight.LineWeight050);
            table.SetRowHeight(8);

            var rowHeaders = table.Rows[1];
            rowHeaders.Height = 15;
            var lwBold = rowHeaders.Borders.Top.LineWeight;
            rowHeaders.Borders.Bottom.LineWeight = lwBold;

            var titleCells = table.Cells[0, 0];
            titleCells.TextString = "ТЭП";
            titleCells.Alignment = CellAlignment.MiddleCenter;

            var col = table.Columns[0];
            col[1, 0].TextString = "Наименование показателя";            
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 50;

            col = table.Columns[1];
            col[1, 1].TextString = "Ед.изм.";
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 20;

            col = table.Columns[2];
            col[1, 2].TextString = "Проект";
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 20;

            Cell cell;
            int row =2;
            foreach (var item in data)
            {
                cell = table.Cells[row, 0];
                cell.TextString = item.Name;
                cell.Alignment = CellAlignment.MiddleLeft;

                cell = table.Cells[row, 1];
                cell.TextString = item.Units;

                cell = table.Cells[row, 2];
                cell.TextString = item.Value.ToString();

                row++;
            }

            var lastRow = table.Rows.Last();
            lastRow.Borders.Bottom.LineWeight = lwBold;

            table.GenerateLayout();
            return table;
        }

        private void SetNorm (string title, string value, Table table, int colIndex)
        {
            var col = table.Columns[colIndex];
            col[1, colIndex].TextString = title;
            var mCells = CellRange.Create(table, 2, colIndex, 4, colIndex);
            table.MergeCells(mCells);
            col[2, colIndex].TextString = value;
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 20;
        }

        private void InsertTable (Table table)
        {
            TableJig jigTable = new TableJig(table, 1 / db.Cannoscale.Scale, "Вставка таблицы блок-секций");
            if (doc.Editor.Drag(jigTable).Status == PromptStatus.OK)
            {
                using (var t = db.TransactionManager.StartTransaction())
                {
                    //table.ScaleFactors = new Scale3d(100);
                    var cs = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                    cs.AppendEntity(table);
                    t.AddNewlyCreatedDBObject(table, true);
                    t.Commit();
                }
            }
        }
    }
}
