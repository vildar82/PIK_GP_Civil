using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Gis.Map;
using Autodesk.Gis.Map.ObjectData;

namespace PIK_GP_Civil.Lib.OD
{
    /// <summary>
    /// Сервис управления Object Data (OD)
    /// </summary>
    public static class ODService
    {
        public static MapApplication MapApp { get { return HostMapApplicationServices.Application; } }

        public static void AddRecord (DBObject dbo, ODTable odTable)
        {
            CheckTable(odTable);            
            var table= MapApp.ActiveProject.ODTables[odTable.Name];
            using (var recs = table.GetObjectTableRecords(Convert.ToUInt32(0), dbo, Autodesk.Gis.Map.Constants.OpenMode.OpenForRead, false))
            {
                var rec = recs[0];
                using (rec = Record.Create())
                { 
                    table.InitRecord(rec);
                    odTable.SetValues(rec);                    
                    table.AddRecord(rec, dbo);
                }
            }
        }

        private static void CheckTable(ODTable odTable)
        {
            var tables = MapApp.ActiveProject.ODTables.GetTableNames();
            if (tables.Contains(odTable.Name))
            {
                // Проверить параметры таблицы !!!
                var table = MapApp.ActiveProject.ODTables[odTable.Name];
                int index =0;
                foreach (var item in odTable.Parameters)
                {
                    var field = table.FieldDefinitions[index];
                    if (!field.Name.Equals(item.Name) ||
                        field.Type != item.Type)                        
                    {
                        // Таблица не соответствует требуемой
                        string err = $"Таблица OD '{odTable.Name}' в чертеже не соответствует требуемой: {odTable.GetInfo()}";
                        Inspector.AddError(err);
                        throw new Exception(err);
                    }
                    else
                    {
                        item.Index = index;
                    }
                    index++;
                }
            }
            else
            {
                // Создание таблицы
                CreateTable(odTable);
            }
        }
       
        private static void CreateTable(ODTable odTable)
        {
            int index =0;
            FieldDefinitions fields = MapApp.ActiveProject.MapUtility.NewODFieldDefinitions();
            foreach (var item in odTable.Parameters)
            {                
                var f= fields.Add(item.Name, item.Description, item.Type, index);
                f.DefaultMapValue.Assign(item.DefaultValue);
                item.Index = index;
                index++;
            }
            MapApp.ActiveProject.ODTables.Add(odTable.Name, fields, odTable.Description, true);
        }
    }
}