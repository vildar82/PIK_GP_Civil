using AcadLib;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Civil.Utils
{
    public static class GradingMaskUtil
    {
        private const string MenuName = "Маскировка";
        private static RXClass RxClassAlignment = RXObject.GetClass(typeof(Grading));        

        public static void AttachContextMenu()
        {
            try
            {
                var cme = new ContextMenuExtension();
                var menu = new MenuItem(MenuName);
                menu.Click += (o, e) => GP_Civil_GradingMask();
                menu.Icon = Properties.Resources.mask;
                cme.MenuItems.Add(menu);
                cme.MenuItems.Add(new MenuItem(""));
                Application.AddObjectContextMenuExtension(RxClassAlignment, cme);
            }
            catch { }
        }

        private static void GP_Civil_GradingMask()
        {
            CommandStart.Start(doc =>
            {   
                var db = doc.Database;
                var ed = doc.Editor;

                var selRes = ed.SelectImplied();
                if (selRes.Status == PromptStatus.OK)
                {
                    using (var t = doc.TransactionManager.StartTransaction())
                    {
                        var gradingId = selRes.Value[0].ObjectId;
                        var grading = gradingId.GetObject(OpenMode.ForRead) as Grading;
                        var pts = new Point2dCollection();
                        for (int i = 0; i < grading.EndParam; i++)
                        {
                            var vertex = grading.GetPointAtParameter(i);
                            pts.Add(vertex.Convert2d());
                        }
                        if (!pts[0].IsEqualTo(pts[pts.Count - 1]))
                        {
                            pts.Add(pts[0]);
                        }
#if DEBUG
                        //EntityHelper.AddEntityToCurrentSpace( pts.Cast<Point2d>().ToList().CreatePolyline());
#endif
                        var mask = new Wipeout();
                        mask.SetFrom(pts, Vector3d.ZAxis);
                        var cs = db.CurrentSpaceId.GetObject(OpenMode.ForWrite) as BlockTableRecord;
                        cs.AppendEntity(mask);
                        t.AddNewlyCreatedDBObject(mask, true);

                        // Порядок сортировки - маскировка должна быть выше поверхности, но ниже откоса
                        var drawOrder = cs.DrawOrderTableId.GetObject(OpenMode.ForWrite) as DrawOrderTable;
                        var idsAbove = new ObjectIdCollection();
                        idsAbove.Add(grading.Id);
                        drawOrder.MoveAbove(idsAbove, mask.Id);

                        t.Commit();
                    }
                }
            });      
        }
    }
}
