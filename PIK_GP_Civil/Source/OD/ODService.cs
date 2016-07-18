using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Gis.Map;
using Autodesk.Gis.Map.ObjectData;
using PIK_GP_Acad.OD;

namespace PIK_GP_Civil.OD
{
    /// <summary>
    /// Сервис управления Object Data (OD)
    /// </summary>
    public static class ODService
    {
        public static MapApplication MapApp { get { return HostMapApplicationServices.Application; } }

        public static void AddRecord (IODRecord odRec)
        {
            CheckTable(odRec);
            var table= MapApp.ActiveProject.ODTables[odRec.TableName];
            var dbo = odRec.IdEnt.GetObject(OpenMode.ForWrite, false, true);
            using (var recs = table.GetObjectTableRecords(Convert.ToUInt32(0), dbo, Autodesk.Gis.Map.Constants.OpenMode.OpenForRead, false))
            {
                var rec = recs[0];
                using (rec = Record.Create())
                { 
                    table.InitRecord(rec);
                    // Установка параметров
                    //odTable.SetValues(rec);
                    foreach (var item in odRec.Parameters)
                    {
                        rec[item.Index].Assign(item.Value);
                    }                    
                    table.AddRecord(rec, dbo);
                }
            }
        }

        private static void CheckTable(IODRecord odRec)
        {
            var tables = MapApp.ActiveProject.ODTables.GetTableNames();
            if (tables.Contains(odRec.TableName))
            {
                // Проверить параметры таблицы !!!
                var table = MapApp.ActiveProject.ODTables[odRec.TableName];
                int index =0;
                foreach (var item in odRec.Parameters)
                {
                    var field = table.FieldDefinitions[index];
                    if (!field.Name.Equals(item.Name) ||
                        (int)field.Type != (int)item.Type)                        
                    {
                        // Таблица не соответствует требуемой
                        string err = $"Таблица OD '{odRec.TableName}' в чертеже не соответствует требуемой: {odRec.GetInfo()}";
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
                CreateTable(odRec);
            }
        }
       
        private static void CreateTable(IODRecord odTable)
        {
            int index =0;
            FieldDefinitions fields = MapApp.ActiveProject.MapUtility.NewODFieldDefinitions();
            foreach (var item in odTable.Parameters)
            {                
                var f= fields.Add(item.Name, item.Description, (Autodesk.Gis.Map.Constants.DataType)item.Type, index);
                f.DefaultMapValue.Assign(item.DefaultValue);
                item.Index = index;
                index++;
            }
            MapApp.ActiveProject.ODTables.Add(odTable.TableName, fields, "", true);
        }
    }
}