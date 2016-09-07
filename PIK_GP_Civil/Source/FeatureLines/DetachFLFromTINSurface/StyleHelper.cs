using Autodesk.AutoCAD.DatabaseServices;
using System;
using System.Collections.Generic;
using AecDb = Autodesk.Aec.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using Autodesk.AutoCAD.Colors;

namespace PIK_GP_Civil.FeatureLines.DetachFLFromTINSurface
{
    public static class StyleHelper
    {
        public static void Change(List<ObjectId> idsCivilSomeEnt, string styleName)
        {
            // Проверка аргументов
            if (idsCivilSomeEnt == null || idsCivilSomeEnt.Count==0 || string.IsNullOrEmpty(styleName))
            {
                return;
            }

            using (var t = idsCivilSomeEnt[0].Database.TransactionManager.StartTransaction())
            {
                ObjectId styleId = GetStyleId(idsCivilSomeEnt[0], styleName);
                if (!styleId.IsNull)
                {
                    foreach (var idEnt in idsCivilSomeEnt)
                    {
                        var ent = idEnt.GetObject(OpenMode.ForRead) as AecDb.Entity;
                        if (ent == null)
                            continue;
                        ent.UpgradeOpen();
                        ent.StyleId = styleId;
                    }
                }
                t.Commit();
            }
        }

        private static ObjectId GetStyleId(ObjectId idEnt, string styleName)
        {
            ObjectId resStyleId = ObjectId.Null;
            var ent = idEnt.GetObject(OpenMode.ForRead) as AecDb.Entity;
            if (ent == null)
            {
                return resStyleId;
            }
            
            var civil = Autodesk.Civil.ApplicationServices.CivilApplication.ActiveDocument;
            StyleCollectionBase styles = null;

            if (ent is FeatureLine)
            {
                styles = civil.Styles.FeatureLineStyles;                
            }

            resStyleId = GetStyle(styleName, styles);
            return resStyleId;
        }

        private static ObjectId GetStyle(string styleName, StyleCollectionBase styles)
        {
            ObjectId resStyleId = ObjectId.Null;
            if (styles == null)
            {
                return resStyleId;
            }
            if (styles.Contains(styleName))
            {
                resStyleId = styles[styleName];
            }
            else
            {
                resStyleId = styles.Add(styleName);
                var fls = resStyleId.GetObject(OpenMode.ForWrite) as FeatureLineStyle;
                var displayStylePlan = fls.GetFeatureLineDisplayStylePlan();
                displayStylePlan.Color = Color.FromRgb(255, 0, 255);
                displayStylePlan.Lineweight = LineWeight.LineWeight040;
            }
            return resStyleId;
        }
    }
}