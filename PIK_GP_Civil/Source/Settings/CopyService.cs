using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices.Styles;

namespace PIK_GP_Civil.Settings
{
    /// <summary>
    /// Копирование настроек из одного файла в другой
    /// </summary>
    public class CopyService
    {        
        Database dbDest;

        public CopyService(Database dbDest)
        {            
            this.dbDest = dbDest;
        }

        public void CopyStyle(string[] copyStyles, StyleCollectionBase styleSourceCollection)
        {
            foreach (var item in copyStyles)
            {
                CopyStyle(item, styleSourceCollection);
            }
        }

        /// <summary>
        /// Копирование стиля из коллекции стилей
        /// </summary>        
        public bool CopyStyle(string copyStyle, StyleCollectionBase styleSourceCollection)
        {
            if (styleSourceCollection.Contains(copyStyle))
            {
                var idStyle = styleSourceCollection[copyStyle];
                ExportStyle(dbDest, idStyle);
                return true;              
            }
            else
            {
                Inspector.AddError($"В шаблоне не найден стиль точки '{copyStyle}'", System.Drawing.SystemIcons.Warning);
                return false;
            }
        }

        private static void ExportStyle(Database db, ObjectId idStyle)
        {
            var style = idStyle.GetObject(OpenMode.ForRead) as StyleBase;
            style.ExportTo(db, StyleConflictResolverType.Override);
        }
    }
}
