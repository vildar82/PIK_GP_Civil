using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Civil.Utils
{
    public static class TinSurfaceUtils
    {
        /// <summary>
        /// Очистка 0 структурных линий из поверхности
        /// </summary>
        public static void ClearZeroBreaklines()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            if (doc == null) return;
            var db = doc.Database;
            var ed = doc.Editor;

            var selOPt = new PromptEntityOptions("\nВыбор поверхности");
            selOPt.SetRejectMessage("\nТолько поверхность");
            selOPt.AddAllowedClass(typeof(TinSurface), true);
            var selTin = ed.GetEntity(selOPt);
            if (selTin.Status != PromptStatus.OK) return;

            int countRemovedBreaklines = 0;

            using (var t = doc.TransactionManager.StartTransaction())
            {
                var surfId = selTin.ObjectId;
                var surf = surfId.GetObject(OpenMode.ForRead) as TinSurface;
                dynamic surfCom = surf.AcadObject;

                for (int i = 0; i < surfCom.Breaklines.Count; i++)
                {
                    var brLineCom = surfCom.Breaklines.Item(i);
                    var brLineEnts = (object[])brLineCom.BreaklineEntities;
                    if (brLineEnts.Length == 0)
                    {
                        surfCom.Breaklines.Remove(i--);
                        countRemovedBreaklines++;
                    }
                }
                if (countRemovedBreaklines > 0)
                {
                    surf.Rebuild();
                }
                t.Commit();
            }
            doc.Editor.WriteMessage($"\nУдалено {countRemovedBreaklines} пустых структурных линий.");
        }
    }
}
