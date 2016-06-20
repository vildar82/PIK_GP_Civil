using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Civil.InfraWorks
{
    public static class ExportService
    {
        private const string blNameDOO = PIK_GP_Acad.Commands.BlockNameDOO;
        private const string blNameSchool = PIK_GP_Acad.Commands.BlockNameSchool;
        private const string blNameKpParking = PIK_GP_Acad.Commands.BlockNameKpParking;        

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

                    List<Polyline> pls = null;
                    // Блок-Секции Концепции
                    if (PIK_GP_Acad.KP.KP_BlockSection.KP_BlockSectionService.IsBlockSection(blName))
                    {
                        pls = getPlsInBs(blRef.BlockTableRecord, PIK_GP_Acad.KP.KP_BlockSection.KP_BlockSectionService.blKpParkingLayerContour);
                    }
                    // ДОО, СОШ, Паркинг
                    else if (blName.Equals(blNameDOO) ||
                        blName.Equals(blNameSchool) ||
                        blName.Equals(blNameKpParking))                        
                    {
                        // Определение полилиний
                        pls = getPlsInBs(blRef.BlockTableRecord);                                                
                    }
                    // Скопировать все полтилинии
                    if (pls != null)
                    {
                        foreach (var pl in pls)
                        {
                            CopyPl(cs, blRef, pl);
                        }
                    }
                }
                t.Commit();
            }
            Application.ShowAlertDialog("Готово");
        }

        private static void CopyPl (BlockTableRecord btr, BlockReference blRef, Polyline pl)
        {
            if (pl != null)
            {
                var idPlCopy = pl.Id.CopyEnt(btr.Id);
                var plCopy = idPlCopy.GetObject(OpenMode.ForWrite, false, true) as Polyline;
                plCopy.TransformBy(blRef.BlockTransform);
            }
        }

        /// <summary>
        /// Получение всех полилиний в блоке - СОШ, ДОШ
        /// </summary>        
        private static List<Polyline> getPlsInBs (ObjectId idBtr, string layer = null)
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
