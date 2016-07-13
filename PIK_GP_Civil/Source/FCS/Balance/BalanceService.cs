using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using AcadLib.Tables;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Civil.FCS.Balance
{
    class BalanceService : CreateTable, ITableService
    {
        private const string HomeArea = "Участок дома";
        static List<ClassType> classTypes = new List<ClassType>() {
             new ClassType(HomeArea, "Площадь участка дома", null, 0),
             new ClassType("Размещаемые БРП_ТП", "Площадь размещаемой БРП, ТП", null, 1),
             new ClassType("Жилые здания", "Площадь застройки дома", null, 2),
             new ClassType("Общественные здания", "Площадь застройки дома", null, 2),
             new ClassType("Асфальтобетон", "Площадь проектируемых проездов и автостоянок с покрытием асфальтобетоном", ClassGroup.HardCoating, 3),
             new ClassType("Тротуарная плитка_Тротуары", "Прощадь проектируемых троутаров с покрытием тротуарной плиткой", ClassGroup.HardCoating, 4),
             new ClassType("Тротуарная плитка_Тротуары с проездом", "Площадь проектируемых усиленных тротуаров с покрытием тротуарной плиткой с возможностью проезда пожарного автотранспорта", ClassGroup.HardCoating, 5),
             new ClassType("Газон", "Площадь газонов", ClassGroup.Landscaping, 6),
             new ClassType("Газон_отмостка", "Площадь отмостки с газонным покрытием", ClassGroup.Landscaping, 7),
             new ClassType("Гравийные высевки_ДП", "Детские игровые площадки с покрытием гравийными высевками", ClassGroup.Landscaping, 8),
             new ClassType("Гравийные высевки_ПО", "Площадки отдыха взрослого населения с покрытием гравийными высевками", ClassGroup.Landscaping, 9),
             new ClassType("Гравийные высевки_ПАО", "Площадка активного отдыха гравийными высевками", ClassGroup.Landscaping, 10),
             new ClassType("Газонная решетка_Проезд пож.транспорта", "Площадь проездов пожарного автотранспорта с покрытием газонной решеткой", ClassGroup.Landscaping, 11)
        };

        List<BalanceRow> data;
        List<IGrouping<ClassGroup, BalanceRow>> groupsData;
        private Color colorHomeRow = Color.FromRgb(199,200,202);
        private Color colorTotalRow = Color.FromRgb(199, 200, 202);

        public BalanceService (Database db) : base(db)
        {
        }

        public ClassType GetClassType (StringCollection tags)
        {
            foreach (var tag in tags)
            {
                var classType = classTypes.Find(c => c.ClassName.Equals(tag, StringComparison.OrdinalIgnoreCase));
                if (classType != null)
                    return classType;
            }
            return null;
        }          

        public void CalcRows (List<IGrouping<string, IClassificator>> groups)
        {
            data = new List<BalanceRow>();
            foreach (var item in groups)
            {
                BalanceRow row = new BalanceRow(item.ToList());
                data.Add(row);
            }

            // Строка Площади участка дома
            var homeRow = data.Find(r => r.ClassType.ClassName == HomeArea);
            if (homeRow != null)
            {
                // Процент остальных площадей
                foreach (var item in data)
                {
                    item.PercentTerritory = Math.Round(item.Value / homeRow.Value * 100,2);
                }
            }
            else
            {
                Inspector.AddError($"Не определен объект класса '{HomeArea}'");
            }

            CalcRows();
        }

        public override void CalcRows ()
        {
            Title = "Баланс территории жилого дома";
            NumColumns = 4;
            NumRows = data.Count+2;
            groupsData = data.GroupBy(g => g.ClassType.Group).ToList();
            var countGroups = groupsData.Where(g => g.Key != null).Count();
            NumRows += countGroups * 2; // Подзаголовок группы и итого
        }

        protected override void SetColumnsAndCap (ColumnsCollection columns)
        {
            int colIndex = 0;
            var col = columns[colIndex];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 10;
            var cell = col[1, colIndex];
            cell.TextString = "№ п/п";

            colIndex++;

            col = columns[colIndex];
            col.Alignment = CellAlignment.MiddleLeft;
            col.Width = 125;
            col.Borders.Horizontal.Margin = 2;
            cell = col[1, colIndex];
            cell.TextString = "Наименование";
            cell.Alignment = CellAlignment.MiddleCenter;

            colIndex++;

            col = columns[colIndex];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 25;
            cell = col[1, colIndex];
            cell.TextString = "Площадь, м" + AcadLib.General.Symbols.Square;            

            colIndex++;

            col = columns[colIndex];
            col.Alignment = CellAlignment.MiddleCenter;
            col.Width = 25;
            cell = col[1, colIndex];
            cell.TextString = "Процент территории, %";            
        }

        protected override void FillCells (Table table)
        {
            table.Cells.TextStyleId = db.GetTextStylePIK(PIK_GP_Acad.GPHelper.TextStylePikKP);

            int row = 2;
            Cell cell;
            CellRange mCells;
            int count =1;

            foreach (var itemGroup in groupsData)
            {
                if (itemGroup.Key != null)
                {
                    mCells = CellRange.Create(table, row, 0, row, table.Columns.Count - 1);
                    table.MergeCells(mCells);

                    cell = table.Cells[row, 0];
                    cell.TextString = itemGroup.Key.Name;
                    cell.Alignment = CellAlignment.MiddleCenter;
                    row++;
                }

                foreach (var itemRow in itemGroup)
                {
                    cell = table.Cells[row, 0];
                    cell.TextString = count++.ToString();

                    cell = table.Cells[row, 1];
                    cell.TextString = itemRow.Name;

                    cell = table.Cells[row, 2];
                    cell.TextString = itemRow.Value.ToString();

                    cell = table.Cells[row, 3];
                    cell.TextString = itemRow.PercentTerritory.ToString();                    

                    if (itemRow.ClassType.ClassName == HomeArea)
                    {
                        table.Rows[row].BackgroundColor = colorHomeRow;
                    }

                    row++; 
                }

                if (itemGroup.Key != null)
                {
                    mCells = CellRange.Create(table, row, 0, row, 1);
                    table.MergeCells(mCells);

                    cell = table.Cells[row, 0];
                    cell.TextString = itemGroup.Key.TotalName;
                    cell.Alignment = CellAlignment.MiddleRight;
                    cell.Borders.Horizontal.Margin = 2;

                    cell = table.Cells[row, 2];
                    cell.TextString = itemGroup.Sum(s => s.Value).ToString();

                    cell = table.Cells[row, 3];
                    cell.TextString = itemGroup.Sum(s => s.PercentTerritory).ToString();

                    table.Rows[row].BackgroundColor = colorTotalRow;

                    row++;
                }
            }
        }        
    }
}
