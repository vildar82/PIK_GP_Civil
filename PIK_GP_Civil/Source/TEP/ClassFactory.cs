using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Civil.TEP
{
    public static class ClassFactory
    {
        const string LandBoundary = "Граница участка";
        const string EcoSystem = "Природный комплекс";
        const string BusinesZone = "Административно-деловая";

        public static IClassificator Create (ObjectId idEnt, StringCollection tags, StringCollection schemas)
        {
            Classificator res = null;            
            if (tags.Contains(LandBoundary))
            {                
                double value = GetValue(idEnt, 0.0001, LandBoundary);
                if (value != 0)
                {
                    res = new Classificator("Площадь участка", 0, "га", value);
                }
            }
            else if (tags.Contains(EcoSystem))
            {
                double value = GetValue(idEnt, 0.0001, EcoSystem);
                if (value != 0)
                {
                    res = new Classificator("Участок ПК", 1, "га", value);
                }
            }
            else if (tags.Contains(BusinesZone))
            {
                double value = GetValue(idEnt, 0.0001, BusinesZone);
                if (value != 0)
                {
                    res = new Classificator("Участок адм-деловой зоны", 2, "га", value);
                }
            }
            Inspector.AddError($"Класс: {res.Name}", idEnt, System.Drawing.SystemIcons.Information);
            return res;
        }

        private static double GetValue (ObjectId idEnt, double unitFactor, string tag)
        {
            double res = 0;
            var pl = idEnt.GetObject( OpenMode.ForRead, false, true) as Polyline;
            if (pl == null)
            {
                Inspector.AddError($"Неподдерживаемый тип объекта - {idEnt.ObjectClass.Name}. Классификатор - {tag}",
                    idEnt, System.Drawing.SystemIcons.Error);
            }
            else
            {
                res = pl.Area * unitFactor;
            }
            return res;
        }
    }
}
