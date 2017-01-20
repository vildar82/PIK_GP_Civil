using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Civil.DatabaseServices.Styles;
using AcadLib;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Civil.Styles
{
    public static class StyleHelper
    {
        /// <summary>
        /// Перебор стилей в коллекции.
        /// Требуется транзакция
        /// </summary>        
        public static IEnumerable<StyleBase> EnumerateStyles(this StyleCollectionBase styleCollection)
        {
            foreach (var styleId in styleCollection)
            {               
                if (!styleId.IsValidEx()) continue;
                yield return styleId.GetObject(OpenMode.ForRead) as StyleBase;
            }
        }        
    }
}
