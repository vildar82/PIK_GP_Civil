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

                    // Блок-Секции Концепции
                    if (PIK_GP_Acad.BlockSection.SectionService.IsBlockNameSection(blName))
                    {
                        PIK_GP_Acad.BlockSection.BlockSectionContours.CreateContour(doc);
                        var pl = PIK_GP_Acad.BlockSection.BlockSectionContours.FindContourPolyline(blRef);
                        CopyPl(cs, blRef, pl);
                    }
                    // ДОО, СОШ, Паркинг
                    else if (blName.Equals(blNameDOO) ||
                        blName.Equals(blNameSchool) ||
                        blName.Equals(blNameKpParking))                        
                    {
                        // Определение полилиний
                        List<Polyline> plsDoo = getPlsInBs(blRef.BlockTableRecord);                        
                        // Скопировать все полтилинии
                        foreach (var pl in plsDoo)
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
        private static List<Polyline> getPlsInBs (ObjectId idBtr)
        {
            List<Polyline> pls = new List<Polyline> ();                        
            var btr = idBtr.GetObject( OpenMode.ForRead) as BlockTableRecord;
            foreach (var item in btr)
            {
                var pl = item.GetObject( OpenMode.ForRead, false, true) as Polyline;
                if (pl == null || !pl.Visible) continue;
                pls.Add(pl);
            }            
            return pls;
        }
    }
}
