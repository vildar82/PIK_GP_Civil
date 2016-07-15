using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Civil.FCS;

namespace PIK_GP_Civil.Elements.Buildings
{
    /// <summary>
    /// Создание зданий
    /// </summary>
    public static class BuildingFactory
    {
        public static IBuilding CreateBuilding(Entity ent)
        {
            IBuilding res = null;
            var pl = ent as Polyline;
            if (pl != null)
            {
                var props = FCService.GetProperties(pl.Id);
                res = new Surround(pl, props);
            }
            return res;
        }

        
    }
}
