using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices.Styles;
using PIK_GP_Civil.Styles;
using System.Text.RegularExpressions;
using Autodesk.AutoCAD.Geometry;
using System.Collections.Specialized;

namespace PIK_GP_Civil.Surface.ChangeLabelStyles
{
    /// <summary>
    /// Смена стилей у меток в поверхности
    /// </summary>
    public class SurfaceChangeLabelStyles
    {
        Document doc;
        CivilDocument civil;
        string fromScale;
        string toScale;
        Dictionary<ObjectId, ObjectId> dictChangeStylesElevLabel;
        Dictionary<ObjectId, ObjectId> dictChangeStylesSlopeLabel;
        Dictionary<string, StyleBase> dictLabelsStylesElevFromToScale;
        Dictionary<string, StyleBase> dictLabelsStylesSlopeFromToScale;
        int countLabelChangedStyleElevation;
        int countLabelChangedStyleSlope;
        int countLabelElevation;
        int countLabelSlope;
        double scale;

        public SurfaceChangeLabelStyles(Document doc)
        {
            this.doc = doc;            
            civil = CivilApplication.ActiveDocument;            
        }

        public void ChangeStyles(string fromScale, string toScale, double scale)
        {
            if (fromScale == null || toScale == null) return;

            this.scale = scale;
            this.fromScale = fromScale;
            this.toScale = toScale;
            countLabelChangedStyleElevation = 0;
            countLabelChangedStyleSlope = 0;

            dictChangeStylesElevLabel = new Dictionary<ObjectId, ObjectId>();
            dictChangeStylesSlopeLabel = new Dictionary<ObjectId, ObjectId>();

            using (var t = doc.TransactionManager.StartTransaction())
            {                
                dictLabelsStylesElevFromToScale = GetStylesFromToScale(civil.Styles.LabelStyles.SurfaceLabelStyles.SpotElevationLabelStyles.EnumerateStyles());                
                dictLabelsStylesSlopeFromToScale = GetStylesFromToScale(civil.Styles.LabelStyles.SurfaceLabelStyles.SlopeLabelStyles.EnumerateStyles());

                if (!dictLabelsStylesElevFromToScale.Any() && !dictLabelsStylesSlopeFromToScale.Any())
                {
                    doc.Editor.WriteMessage($"\nНе найдено подходящих стилей меток заданного масштаба преобразования из '{fromScale}' -> в '{toScale}'.");
                    return;
                }

                // Выбор меток
                var sel = Select();
                
                foreach (var idEnt in sel)
                {
                    if (!idEnt.IsValidEx()) continue;
                    var dbo = idEnt.GetObject(OpenMode.ForRead);
                    ChangeLabelStyle(dbo);                                                            
                }
                t.Commit();

                doc.Editor.WriteMessage($"\nВсего выбрано меток: Откосов = {countLabelSlope}, Отметок в точке = {countLabelElevation}.");
                doc.Editor.WriteMessage($"\nИзменено стилей меток на масштаб '{toScale}':\n Откосов = {countLabelChangedStyleSlope},\n Отметка в точке = {countLabelChangedStyleElevation}.");
            }
        }

        private Dictionary<string, StyleBase> GetStylesFromToScale(IEnumerable<StyleBase> styles)
        {
            var dictToStyles = new Dictionary<string, StyleBase>();
            var dictStyles = styles.ToDictionary(k=>k.Name, v =>v);
            foreach (var item in styles)
            {
                if (IsStyleFromScale(item.Name))
                {
                    var toStyleName = Regex.Replace(item.Name, fromScale, toScale, RegexOptions.IgnoreCase).Trim();
                    StyleBase toStyle;
                    if (dictStyles.TryGetValue(toStyleName, out toStyle))
                    {
                        dictToStyles.Add(item.Name, toStyle);
                    }
                }
            }
            return dictToStyles;
        }

        private bool IsStyleToScale(string name)
        {
            return (fromScale== string.Empty || name.IndexOf(fromScale, StringComparison.OrdinalIgnoreCase) == -1) &&
                   (toScale == string.Empty || name.IndexOf(toScale, StringComparison.OrdinalIgnoreCase) != -1);
        }
        private bool IsStyleFromScale(string name)
        {
            return (toScale == string.Empty || name.IndexOf(toScale, StringComparison.OrdinalIgnoreCase) == -1) &&
                   (fromScale == string.Empty || name.IndexOf(fromScale, StringComparison.OrdinalIgnoreCase) != -1);
        }

        private void ChangeLabelStyle(Autodesk.AutoCAD.DatabaseServices.DBObject dbo)
        {
            ObjectId newStyleId = ObjectId.Null;            
            if (dbo is SurfaceElevationLabel)
            {
                var label = dbo as SurfaceElevationLabel;  
                newStyleId = GetNewLabelScaleStyle(label, ref dictChangeStylesElevLabel, ref dictLabelsStylesElevFromToScale);
                if (SetLabelStyle(label, newStyleId))
                    countLabelChangedStyleElevation++;
                countLabelElevation++;               
            }
            else if (dbo is SurfaceSlopeLabel)
            {
                var label = dbo as SurfaceSlopeLabel;
                newStyleId = GetNewLabelScaleStyle(label, ref dictChangeStylesSlopeLabel, ref dictLabelsStylesSlopeFromToScale);
                if (SetLabelStyle(label, newStyleId))
                    countLabelChangedStyleSlope++;
                countLabelSlope++;
            }                       
        }

        private void TestExploreLabel(SurfaceElevationLabel label)
        {
            var AllowsAnchorMarker = label.AllowsAnchorMarker;
            var AllowsDimensionAnchors = label.AllowsDimensionAnchors;
            var AnchorInfo = label.AnchorInfo;
            var TextComponentIds = label.GetTextComponentIds();
            foreach (ObjectId textCompId in TextComponentIds)
            {
                var textComp = textCompId.GetObject(OpenMode.ForRead) as LabelStyleReferenceTextComponent;                                
                if (textComp != null)
                {
                    var refTextTargetId = label.GetReferenceTextTarget(textCompId);                    
                    var refTextTarget = refTextTargetId.GetObject( OpenMode.ForRead);
                }
            }

        }

        private bool SetLabelStyle(Label label, ObjectId newStyleId)
        {
            if (!newStyleId.IsNull)
            {
                label.UpgradeOpen();
                var changer = new LabelStyleSafeChanger(label, scale);
                changer.Change(newStyleId);                
                return true;
            }
            return false;
        }        

        private ObjectId GetNewLabelScaleStyle(Label label,
            ref Dictionary<ObjectId, ObjectId> dictChangeStyles,
            ref Dictionary<string, StyleBase> dictLabelsStylesFromToScale)
        {
            ObjectId newStyleId;
            if (!dictChangeStyles.TryGetValue(label.StyleId, out newStyleId))
            {                
                StyleBase newStyle;
                if (dictLabelsStylesFromToScale.TryGetValue(label.StyleName, out newStyle))
                {
                    newStyleId = newStyle.Id;                    
                }
                dictChangeStyles.Add(label.StyleId, newStyleId);
            }
            return newStyleId;
        }

        private List<ObjectId> Select()
        {
            return doc.Editor.Select("\nВыбор объектов:");
        }
    }
}
