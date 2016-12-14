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
            Menu.Click += (o,e)=>ShowAligmentProfile();
            //Menu.Icon = Properties.Resources.AlignmentProfile;            
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

                var mVisible = true;
                var mEnabled = true;
                if (selImpl.Status == PromptStatus.OK)
                {
                    // Если выбрано одна трасса - то проверяем есть ли у нее виды профиля и тогда активируем меню
                    if (selImpl.Value.Count == 1)
                    {
                        mVisible = true;
                        using (var t = doc.TransactionManager.StartTransaction())
                        {
                            var align = GetSelectedAlignment(ed);
                            if (align == null || align.GetProfileViewIds().Count ==0)
                            {
                                // У трассы нет видов профилей - деактивируем меню
                                mEnabled = false;
                            }
                            t.Commit();
                        }                    
                    }
                    else
                    {
                        // Выбрано несколько трасс - скрываем меню
                        mVisible = false;
                    }
                }
                menu.Enabled = mEnabled;
                menu.Visible = mVisible;
            }
        }

        public static void ShowAligmentProfile ()
        {
            CommandStart.Start(doc =>
            {                   
                Editor ed = doc.Editor;
                using (var t = doc.TransactionManager.StartTransaction())
                {
                    var align = GetSelectedAlignment(ed);
                    if (align != null)
                    {
                        Show(align, doc);
                        ed.SetImpliedSelection(new ObjectId[0]);
                    }
                    t.Commit();
                }
            });
        }

        private static Alignment GetSelectedAlignment (Editor ed)
        {
            Alignment align = null;
            PromptSelectionResult selImplRes = ed.SelectImplied();
            if (selImplRes.Status == PromptStatus.OK)
            {
                foreach (SelectedObject selEnt in selImplRes.Value)
                {
                    var selId = selEnt.ObjectId;
                    if (selId.IsValidEx() && selId.ObjectClass == RxClassAlignment)
                    {
                        align = selEnt.ObjectId.GetObject(OpenMode.ForRead) as Alignment;
                        break;
                    }
                }
            }
            return align;
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
