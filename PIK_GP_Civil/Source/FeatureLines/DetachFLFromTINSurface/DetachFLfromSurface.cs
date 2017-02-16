using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using AcadLib;

namespace PIK_GP_Civil.FeatureLines.DetachFLFromTINSurface
{
    static class DetachFeatureLineFromSurface
    {
        private const string MenuName = "Извлечь ХЛ из поверхности";
        private static RXClass RxClassFeatureLine = RXObject.GetClass(typeof(FeatureLine));        

        public static void AttachContextMenu ()
        {
            try
            {
                var cme = new ContextMenuExtension();
                var menu = new MenuItem(MenuName);
                menu.Click += (o, e) => DetachFeatureLine();
                menu.Icon = Properties.Resources.DetachFeatureLine;
                cme.MenuItems.Add(menu);
                cme.MenuItems.Add(new MenuItem(""));
                // пока не имеет смысла, нужно найчится проверять принадлежность хар.линии поверхности, без перебора всех поверхностей, только по самой линии
                //cme.Popup += Cme_Popup;
                Application.AddObjectContextMenuExtension(RxClassFeatureLine, cme);
            }
            catch { }
        }

        //private static void Cme_Popup (object sender, EventArgs e)
        //{
        //    var contextMenu = sender as ContextMenuExtension;
        //    if (contextMenu != null)
        //    {
        //        var doc = Application.DocumentManager.MdiActiveDocument;
        //        if (doc == null) return;
        //        var ed = doc.Editor;

        //        var menu = contextMenu.MenuItems[0];                

        //        // TODO: проверить принадлежит ли хар.линия какой-либо поверхности. (пока непонятно как это сделать)

        //        var mVisible = true;
        //        var mEnabled = true;                
        //        menu.Enabled = mEnabled;
        //        menu.Visible = mVisible;
        //    }
        //}

        public static void DetachFeatureLine ()
        {
            CommandStart.Start(doc =>
            {
                var ed = doc.Editor;
                var civil = CivilApplication.ActiveDocument;
                var surfIds = civil.GetSurfaceIds();

                var selRes = ed.SelectImplied();
                if (selRes.Status != PromptStatus.OK)
                {
                    return;
                }
                List<ObjectId> idsFlToDetach = selRes.Value.GetObjectIds().ToList();
                List<ObjectId> idsFlDetached = new List<ObjectId>();
                List<ObjectId> idsEditedSurf = new List<ObjectId>();

                using (var t = doc.TransactionManager.StartTransaction())
                {
                    bool isEditedSurf = false;
                    foreach (ObjectId surfId in surfIds)
                    {
                        var surf = surfId.GetObject(OpenMode.ForRead) as TinSurface;
                        dynamic surfCom = surf.AcadObject;

                        for (int i = 0; i < surfCom.Breaklines.Count; i++)
                        {
                            var brLine = surfCom.Breaklines.Item(i);
                            var brLineEnts = (object[])brLine.BreaklineEntities;
                            List<ObjectId> idBreaklinesToAdd = new List<ObjectId>();
                            bool isFind = false;
                            bool isRemovedBreaklines = false;
                            for (int b = 0; b < brLineEnts.Length; b++)
                            {
                                var brLineId = Autodesk.AutoCAD.DatabaseServices.DBObject.FromAcadObject(brLineEnts[b]);
                                if (brLineId.IsNull)
                                    continue;
                                idBreaklinesToAdd.Add(brLineId);
                                if (idsFlToDetach.Contains(brLineId))
                                {
                                    //surf.BreaklinesDefinition.RemoveAt(i); // не всегда срабатывает!?
                                    if (!isRemovedBreaklines)
                                    {
                                        surfCom.Breaklines.Remove(i);
                                        isRemovedBreaklines = true;
                                    }
                                    idBreaklinesToAdd.Remove(brLineId);
                                    isFind = true;
                                    idsFlDetached.Add(brLineId);
                                }
                            }
                            if (isFind)
                            {
                                isEditedSurf = true;
                                if (idBreaklinesToAdd.Any())
                                {
                                    AddBreaklinesToSurface(surf, idBreaklinesToAdd);
                                }
                            }
                        }
                        if (isEditedSurf)
                        {
                            isEditedSurf = false;
                            idsEditedSurf.Add(surfId);
                        }
                    }

                    // Изменение стиля характерной линии
                    StyleHelper.Change(idsFlDetached, "Удаленные из поверхности");

                    t.Commit();
                }

                // Перестройка поверхностей
                using (var t = doc.TransactionManager.StartTransaction())
                {
                    foreach (var idSurf in idsEditedSurf)
                    {
                        TinSurface surface = idSurf.GetObject(OpenMode.ForWrite) as TinSurface;
                        surface.Rebuild();
                    }
                    t.Commit();
                }
            });
        }

        private static void AddBreaklinesToSurface (TinSurface surf, List<ObjectId> idEntsToAdd)
        {
            ObjectIdCollection ids = new ObjectIdCollection(idEntsToAdd.ToArray());
            surf.BreaklinesDefinition.AddStandardBreaklines(ids, 0.1, 0, 0, 0);
        }
    }
}
