using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.Errors;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Civil.InfraWorks.Blocks;
using PIK_GP_Civil.Lib.OD;

namespace PIK_GP_Civil.InfraWorks
{
    public static class ExportService
    {   
        /// <summary>
        /// Преобразование блоков ГП для экспорта в infraWorks
        /// Добавление OD - таблица "Здания", поля: Тип, Высота
        /// </summary>
        /// <param name="doc"></param>
        public static void Export (Document doc)
        {
            // Выбор всех блоков инфраструктуры и извлечение контуров
            Database db = doc.Database;
            using (var t = db.TransactionManager.StartTransaction())
            {
                var cs = db.CurrentSpaceId.GetObject( OpenMode.ForWrite) as BlockTableRecord;

                foreach (var item in cs)
                {
                    var blRef = item.GetObject( OpenMode.ForRead, false, true) as BlockReference;
                    if (blRef == null) continue;
                    var blName = blRef.GetEffectiveName();

                    // Определение объекта инфраструктуры
                    try
                    {
                        var infroBlock = InfrastructureFactory.Create(blName, blRef);
                        if (infroBlock != null)
                        {
                            infroBlock.Export(cs);
                        }
                    }
                    catch (Exception ex)
                    {
                        Inspector.AddError($"Ошибка определения блока '{blName}' - {ex.Message}");
                    }
                }
                t.Commit();
            }
            if (!Inspector.HasErrors)
            {
                Application.ShowAlertDialog("Готово");
            }
        }             

        //public static void Export (Document doc)
        //{
        //    // Выбор всех блоков инфраструктуры и извлечение контуров
        //    Database db = doc.Database;            
        //    using (var t = db.TransactionManager.StartTransaction())
        //    {
        //        var cs = db.CurrentSpaceId.GetObject( OpenMode.ForWrite) as BlockTableRecord;                

        //        foreach (var item in cs)
        //        {
        //            var blRef = item.GetObject( OpenMode.ForRead, false, true) as BlockReference;
        //            if (blRef == null) continue;
        //            var blName = blRef.GetEffectiveName();

        //            List<Polyline> pls = null;
        //            // Блок-Секции Концепции
        //            if (PIK_GP_Acad.KP.KP_BlockSection.KP_BlockSectionService.IsBlockSection(blName))
        //            {
        //                pls = GetPls(blRef.BlockTableRecord, PIK_GP_Acad.KP.KP_BlockSection.KP_BlockSectionService.blKpParkingLayerContour);
        //            }
        //            // ДОО, СОШ, Паркинг
        //            else if (blName.Equals(blNameDOO) ||
        //                blName.Equals(blNameSchool) ||
        //                blName.Equals(blNameKpParking))                        
        //            {
        //                // Определение полилиний
        //                pls = GetPls(blRef.BlockTableRecord);                                                
        //            }
        //            // Скопировать все полтилинии
        //            if (pls != null)
        //            {
        //                foreach (var pl in pls)
        //                {
        //                    CopyPl(cs, blRef, pl);
        //                }
        //            }
        //        }
        //        t.Commit();
        //    }
        //    Application.ShowAlertDialog("Готово");
        //}

        public static ObjectId CopyPl (BlockTableRecord btr, BlockReference blRef, Polyline pl)
        {
            if (pl != null)
            {
                var idPlCopy = pl.Id.CopyEnt(btr.Id);
                using (var plCopy = idPlCopy.GetObject(OpenMode.ForWrite, false, true) as Polyline)
                {
                    plCopy.TransformBy(blRef.BlockTransform);
                    return plCopy.Id;
                }
            }
            return ObjectId.Null;
        }

        /// <summary>
        /// Получение всех полилиний в блоке
        /// Или только на заданном слое
        /// </summary>        
        public static List<Polyline> GetPls (ObjectId idBtr, string layer = null)
        {
            List<Polyline> pls = new List<Polyline> ();                        
            var btr = idBtr.GetObject( OpenMode.ForRead) as BlockTableRecord;
            foreach (var item in btr)
            {
                var pl = item.GetObject( OpenMode.ForRead, false, true) as Polyline;
                if (pl == null || !pl.Visible) continue;
                if (string.IsNullOrEmpty(layer) || pl.Layer.Equals(layer))
                {
                    pls.Add(pl);
                }
            }            
            return pls;
        }        
    }
}
