﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Civil.FCS
{
    public static class ClassFactory
    {
        public static IClassificator Create (ObjectId idEnt, IEnumerable<string> tags, IClassTypeService classService)
        {
            Classificator res = null;
            var classType = classService.GetClassType(tags);
            if (classType == null) return null;
            double value = GetValue(idEnt, classType.UnitFactor, classType.ClassName);
            if (value != 0)
            {
                res = new Classificator(idEnt, classType, value);
            }                        
            return res;
        }

        private static double GetValue (ObjectId idEnt, double unitFactor, string tag)
        {
            double res = 0;
            var ent = idEnt.GetObject(OpenMode.ForRead, false, true);

            if (ent is Curve)
            {
                var curve = ent as Curve;
                res = curve.Area * unitFactor;
            }
            else if (ent is Hatch)
            {
                var h = ent as Hatch;
                res = h.Area * unitFactor;
            }
            else
            {
                Inspector.AddError($"Неподдерживаемый тип объекта - {idEnt.ObjectClass.Name}. Классификатор - {tag}",
                        idEnt, System.Drawing.SystemIcons.Error);
            }
            if (res == 0)
            {
                Inspector.AddError($"Не определена площадь объекта", idEnt, System.Drawing.SystemIcons.Error);
            }

            return res;
        }
    }
}
