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
        Dictionary<string, string> dictLabelsStylesSpotFromToScale;
        Dictionary<string, string> dictLabelsStylesSlopeFromToScale;
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
                dictLabelsStylesSpotFromToScale = GetStylesFromToScale(civil.Styles.LabelStyles.SurfaceLabelStyles.SpotElevationLabelStyles.EnumerateStyles());
                dictLabelsStylesSlopeFromToScale = GetStylesFromToScale(civil.Styles.LabelStyles.SurfaceLabelStyles.SlopeLabelStyles.EnumerateStyles());


                if (!dictLabelsStylesSpotFromToScale.Any() && !dictLabelsStylesSlopeFromToScale.Any())
                {
                    doc.Editor.WriteMessage($"\nНе найдено подходящих стилей меток заданного масштаба преобразования из '{fromScale}' -> в '{toScale}'.");
                    return;
                }

                // Выбор меток
                var sel = Select();
                var labels = GetLabels(sel, dictLabelsStylesSpotFromToScale, dictLabelsStylesSlopeFromToScale);

                // Проверка стилей тилей - если нет, то загрузка из шаблона                                
                CheckNeededStylesAndLoad(labels);

                foreach (var labelScale in labels)
                {
                    labelScale.ChangeStyle(scale);
                }
                t.Commit();

                doc.Editor.WriteMessage($"\nВсего выбрано меток: Откосов = {countLabelSlope}, Отметок в точке = {countLabelElevation}.");
                doc.Editor.WriteMessage($"\nИзменено стилей меток на масштаб '{toScale}':\n Откосов = {countLabelChangedStyleSlope},\n Отметка в точке = {countLabelChangedStyleElevation}.");
            }
        }

        private void CheckNeededStylesAndLoad(List<LabelStyleScale> labels)
        {            
            var stylesNeeded = labels.GroupBy(g => new { g.LabelStyleType, g.ToStyleName }).
                Select(s => new { styleName = s.Key.ToStyleName, styleType = s.Key.LabelStyleType });

            var stylesToLoad = new List<Tuple<string, LabelStyleType>>();
            // стили отсутствующие в текущем документе
            foreach (var styleNeed in stylesNeeded)
            {
                var findStyleId = FindLabelStyleInRoot(civil.Styles, styleNeed.styleName, styleNeed.styleType);
                if (findStyleId.IsNull)
                {
                    stylesToLoad.Add(new Tuple<string, LabelStyleType>(styleNeed.styleName, styleNeed.styleType));
                }
                // Зеркальный стиль
                var styleNameMirr = LabelStyleScale.GetMirrorStyleName(styleNeed.styleName);
                var findStyleMirrId = FindLabelStyleInRoot(civil.Styles, styleNeed.styleName, styleNeed.styleType);
                if (findStyleMirrId.IsNull)
                {
                    stylesToLoad.Add(new Tuple<string, LabelStyleType>(styleNameMirr, styleNeed.styleType));
                }
            }

            StyleManager.LoadStyles(doc.Database, (r) =>{
                var idsStyles = new List<ObjectId>();
                foreach (var styleNeed in stylesToLoad)
                {
                    var styleId = FindLabelStyleInRoot(r, styleNeed.Item1, styleNeed.Item2);
                    idsStyles.Add(styleId);
                }
                return idsStyles;
            });

            // заполнение стилей
            var dictStyles = new Dictionary<string, StyleBase>();
            foreach (var styleNeed in stylesNeeded)
            {
                var styleId = FindLabelStyleInRoot(civil.Styles, styleNeed.styleName, styleNeed.styleType);
                var style = styleId.GetObject( OpenMode.ForRead) as StyleBase;
                dictStyles.Add(styleNeed.styleName + "##" + (int)styleNeed.styleType, style);
            }

            foreach (var label in labels)
            {
                StyleBase styleBase;
                dictStyles.TryGetValue(label.ToStyleName + "##" + (int)label.LabelStyleType, out styleBase);
                label.ToStyle = styleBase;
                var styleMirr = LabelStyleScale.GetMirrorStyleName(label.ToStyleName);
                dictStyles.TryGetValue(styleMirr + "##" + (int)label.LabelStyleType, out styleBase);
                label.ToStyleMirror = styleBase;
            }
        }

        private ObjectId FindLabelStyleInRoot(StylesRoot stylesRoot, string styleName, LabelStyleType labelStyleType)
        {
            ObjectId resId = ObjectId.Null;
            LabelStyleCollection styleCol = null;
            if (labelStyleType == LabelStyleType.SurfaceSpotElevation)
            {
                styleCol = stylesRoot.LabelStyles.SurfaceLabelStyles.SpotElevationLabelStyles;                
            }
            else if (labelStyleType == LabelStyleType.SurfaceSlope)
            {
                styleCol = stylesRoot.LabelStyles.SurfaceLabelStyles.SlopeLabelStyles;
            }
            if (styleCol != null && styleCol.Contains(styleName))
            {
                resId = styleCol[styleName];
            }
            return ObjectId.Null;
        }

        /// <summary>
        /// Получение словаря стилей смены масштаба (без зеркалирований).
        /// Ключ - имя стили из котрого масштабируется, без зеркалирования
        /// Значение - имя стиля в который масштабируется ключевой стиль
        /// </summary>
        /// <param name="styles">Стили</param>        
        private Dictionary<string, string> GetStylesFromToScale(IEnumerable<StyleBase> styles)
        {
            var dictFromToStyles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var dictStyles = styles.ToDictionary(k => k.Name, v => v, StringComparer.OrdinalIgnoreCase);

            foreach (var item in styles)
            {
                if (IsStyleFromScale(item.Name, out string styleNameWoMirr) && !dictFromToStyles.ContainsKey(styleNameWoMirr))
                {
                    var toStyleName = Regex.Replace(styleNameWoMirr, fromScale, toScale, RegexOptions.IgnoreCase).Trim();
                    StyleBase toStyle;
                    if (dictStyles.TryGetValue(toStyleName, out toStyle))
                    {
                        dictFromToStyles.Add(styleNameWoMirr, toStyle.Name);
                    }
                }
            }
            return dictFromToStyles;
        }
                
        private bool IsStyleFromScale(string name, out string nameWithoutMirror)
        {
            nameWithoutMirror = LabelStyleScale.GetStyleNameWithoutMirror(name);
            return (toScale == string.Empty || nameWithoutMirror.IndexOf(toScale, StringComparison.OrdinalIgnoreCase) == -1) &&
                   (fromScale == string.Empty || nameWithoutMirror.IndexOf(fromScale, StringComparison.OrdinalIgnoreCase) != -1);
        }                
        
        private List<ObjectId> Select()
        {
            return doc.Editor.Select("\nВыбор объектов:");
        }

        private List<LabelStyleScale> GetLabels(List<ObjectId> sel,
            Dictionary<string, string> dictLabelsStylesSpotElevFromToScale,
            Dictionary<string, string> dictLabelsStylesSlopeFromToScale)
        {
            var labels = new List<LabelStyleScale>();
            foreach (var entId in sel)
            {
                if (entId.IsValidEx())
                {
                    if (entId.GetObject(OpenMode.ForRead) is Label label)
                    {
                        var toStyleName = LabelStyleScale.GetStyleNameWithoutMirror(label.StyleName);
                        LabelStyleScale lss = null;
                        if (label is SurfaceElevationLabel)
                        {
                            lss = new LabelStyleScale(label, LabelStyleType.SurfaceSpotElevation, toStyleName);
                        }
                        else if (label is SurfaceSlopeLabel)
                        {
                            lss = new LabelStyleScale(label, LabelStyleType.SurfaceSlope, toStyleName);
                        }
                        if (lss != null)
                        {
                            labels.Add(lss);
                        }
                    }
                }
            }
            return labels;
        }
    }
}
