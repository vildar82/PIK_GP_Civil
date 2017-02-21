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
        public const string MirrorStylePrefix = "я_";
        Document doc;
        CivilDocument civil;
        string fromScale;
        string toScale;        
        Dictionary<string, LabelStyleScale> dictLabelsStylesElevFromToScale;
        Dictionary<string, LabelStyleScale> dictLabelsStylesSlopeFromToScale;        
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

        private Dictionary<string, LabelStyleScale> GetStylesFromToScale(IEnumerable<StyleBase> styles)
        {
            var dictFromToStyles = new Dictionary<string, LabelStyleScale>(StringComparer.OrdinalIgnoreCase);
            var dictStyles = styles.ToDictionary(k=>k.Name, v =>v, StringComparer.OrdinalIgnoreCase);
            
            foreach (var item in styles)
            {
                string styleNameWoMirr;
                if (IsStyleFromScale(item.Name, out styleNameWoMirr) && !dictFromToStyles.ContainsKey(styleNameWoMirr))
                {
                    var toStyleName = Regex.Replace(styleNameWoMirr, fromScale, toScale, RegexOptions.IgnoreCase).Trim();
                    StyleBase toStyle;
                    if (dictStyles.TryGetValue(toStyleName, out toStyle))
                    {
                        // Если есть зеркальный стиль
                        StyleBase toStyleMirror;
                        dictStyles.TryGetValue(MirrorStylePrefix + toStyle.Name, out toStyleMirror);
                        dictFromToStyles.Add(styleNameWoMirr, new LabelStyleScale (toStyle, toStyleMirror));
                    }
                }
            }
            return dictFromToStyles;
        }

        private bool IsStyleToScale(string name, out string nameWithoutMirror)
        {
            nameWithoutMirror = LabelStyleScale.GetStyleNameWithoutMirror(name);            
            return (fromScale== string.Empty || nameWithoutMirror.IndexOf(fromScale, StringComparison.OrdinalIgnoreCase) == -1) &&
                   (toScale == string.Empty || nameWithoutMirror.IndexOf(toScale, StringComparison.OrdinalIgnoreCase) != -1);
        }
        private bool IsStyleFromScale(string name, out string nameWithoutMirror)
        {
            nameWithoutMirror = LabelStyleScale.GetStyleNameWithoutMirror(name);
            return (toScale == string.Empty || nameWithoutMirror.IndexOf(toScale, StringComparison.OrdinalIgnoreCase) == -1) &&
                   (fromScale == string.Empty || nameWithoutMirror.IndexOf(fromScale, StringComparison.OrdinalIgnoreCase) != -1);
        }

        private void ChangeLabelStyle(Autodesk.AutoCAD.DatabaseServices.DBObject dbo)
        {
            LabelStyleScale newStyleId = null;            
            if (dbo is SurfaceElevationLabel)
            {
                var label = dbo as SurfaceElevationLabel;  
                newStyleId = GetNewLabelScaleStyle(label, ref dictLabelsStylesElevFromToScale);
                if (SetLabelStyle(label, newStyleId))
                    countLabelChangedStyleElevation++;
                countLabelElevation++;               
            }
            else if (dbo is SurfaceSlopeLabel)
            {
                var label = dbo as SurfaceSlopeLabel;
                newStyleId = GetNewLabelScaleStyle(label, ref dictLabelsStylesSlopeFromToScale);
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

        private bool SetLabelStyle(Label label, LabelStyleScale newStyle)
        {
            if (newStyle!= null)
            {
                label.UpgradeOpen();
                var changer = new LabelStyleSafeChanger(label, scale);
                changer.Change(newStyle);                
                return true;
            }
            return false;
        }

        private LabelStyleScale GetNewLabelScaleStyle(Label label,            
            ref Dictionary<string, LabelStyleScale> dictLabelsStylesFromToScale)
        {            
            LabelStyleScale newStyle;
            var styleNameWoMirr = LabelStyleScale.GetStyleNameWithoutMirror(label.StyleName);
            dictLabelsStylesFromToScale.TryGetValue(styleNameWoMirr, out newStyle);                        
            return newStyle;
        }

        private List<ObjectId> Select()
        {
            return doc.Editor.Select("\nВыбор объектов:");
        }
    }
}
