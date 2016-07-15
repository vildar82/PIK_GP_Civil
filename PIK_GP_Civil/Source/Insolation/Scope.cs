using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Civil.Elements;
using PIK_GP_Civil.Elements.Buildings;

namespace PIK_GP_Civil.Insolation
{
    /// <summary>
    /// Расчетная область
    /// </summary>
    public class Scope
    {
        Extents3d ext;
        public List<IBuilding> Buildings { get; set; }        

        public Scope (Extents3d ext, List<IBuilding> items)
        {            
            this.ext = ext;
            Buildings = items;
        }
    }
}
