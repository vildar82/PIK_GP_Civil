using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.DatabaseServices;
using AcadLib;

namespace PIK_GP_Civil.Navigator
{
    class AligmentShowProfile
    {        
        static RXClass RxClassAlignment = RXObject.GetClass(typeof(Alignment));        

        public static void AttachContextMenu ()
        {
            var cme = new ContextMenuExtension();            
            MenuItem miShow = new MenuItem("Показать профиль трассы");
            miShow.Click += ShowImplied;
            cme.MenuItems.Add(miShow);
            cme.MenuItems.Add(new MenuItem(""));
            //cme.Popup += cme_Popup;                        
            Application.AddObjectContextMenuExtension(RxClassAlignment, cme);
        }       

        public static void ShowImplied (object sender, EventArgs e)
        {            
            var doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptSelectionResult selImplRes = ed.SelectImplied();
            if (selImplRes.Status== PromptStatus.OK)
            {
                using (var t = doc.TransactionManager.StartTransaction())
                {
                    foreach (var idEnt in selImplRes.Value.GetObjectIds())
                    {
                        if (idEnt.ObjectClass == RxClassAlignment)
                        {
                            var align = idEnt.GetObject(OpenMode.ForRead) as Alignment;
                            Show(align, doc);
                            break;
                        }
                    }
                    t.Commit();
                }
            }
        }

        private static void Show (Alignment align, Document doc)
        {
            var viewsId = align.GetProfileViewIds();
            if (viewsId.Count > 0)
            {
                var viewId = viewsId[0];
                var view = viewId.GetObject(OpenMode.ForRead) as ProfileView;
                if (view.Bounds.HasValue)
                {
                    viewId.ShowEnt(view.Bounds.Value, doc);

                    if (viewsId.Count>1)
                    {
                        Application.ShowAlertDialog($"Несколько видов профиля у трассы - {align.Name}. Показан первый вид профиля - {view.Name}");
                    }
                }
                else
                {
                    Application.ShowAlertDialog($"Не определен вид профиля трассы - {align.Name}. Рекомендуется выполнить проверку ошибок в чертеже - audit.");
                }
            }
            else
            {
                Application.ShowAlertDialog($"Нет видов профиля у трассы - {align.Name}");
            }
        }        
    }
}
