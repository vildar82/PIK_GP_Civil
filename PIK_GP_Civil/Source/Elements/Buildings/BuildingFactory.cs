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
                int floors = getFloors(props, ent);
                if (floors !=0)
                {
                    res = new Surround(pl, floors, props);
                }                
            }
            return res;
        }

        private static int getFloors (List<FCProperty> props, Entity ent)
        {
            int floor = 0;
            var floorProps = props.Find(p => p.Name.Equals("Этажность", StringComparison.OrdinalIgnoreCase));
            if (floorProps != null)
            {
                try
                {
                    floor = Convert.ToInt32(floorProps.Value);
                }
                catch
                {
                    Inspector.AddError($"Не определена этажность объекта.", ent, System.Drawing.SystemIcons.Error);
                }                
            }
            return floor;
        }
    }
}
