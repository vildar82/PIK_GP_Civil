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
        private const string MenuName = "Показать профиль трассы";
        private static RXClass RxClassAlignment = RXObject.GetClass(typeof(Alignment));
        private static MenuItem Menu;

        public static void AttachContextMenu ()
        {
            var cme = new ContextMenuExtension();            
            Menu = new MenuItem(MenuName);
            Menu.Click += ShowImplied;
            Menu.Icon = Properties.Resources.AlignmentProfile;            
            cme.MenuItems.Add(Menu);
            cme.MenuItems.Add(new MenuItem(""));
            cme.Popup += Cme_Popup;            
            Application.AddObjectContextMenuExtension(RxClassAlignment, cme);
        }

        private static void Cme_Popup (object sender, EventArgs e)
        {
            var contextMenu = sender as ContextMenuExtension;
            if (contextMenu != null)
            {
                var doc = Application.DocumentManager.MdiActiveDocument;
                if (doc == null) return;
                var ed = doc.Editor;

                var menu = contextMenu.MenuItems[0];
                var selImpl = ed.SelectImplied();
                var mVisible = false;
                if (selImpl.Status == PromptStatus.OK)
                {
                    mVisible = selImpl.Value.Count <= 1;
                }
                menu.Enabled = mVisible;
            }
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
                    foreach (SelectedObject selEnt in selImplRes.Value)
                    {   
                        if (selEnt.ObjectId.ObjectClass == RxClassAlignment)
                        {
                            var align = selEnt.ObjectId.GetObject(OpenMode.ForRead) as Alignment;
                            Show(align, doc);                            
                            break;
                        }
                    }
                    ed.SetImpliedSelection(new ObjectId[0]);
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
