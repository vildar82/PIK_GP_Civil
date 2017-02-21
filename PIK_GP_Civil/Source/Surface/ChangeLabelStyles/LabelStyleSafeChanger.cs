using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using AcadLib;

namespace PIK_GP_Civil.Surface.ChangeLabelStyles
{
    /// <summary>
    /// Меняет стиль у меток поверхностей, с сохранением привязанных значений
    /// </summary>
    public class LabelStyleSafeChanger
    {
        private Label label;
        private List<ISafeComponent> safeComponents;
        private double scale;

        public LabelStyleSafeChanger(Label label, double scale)
        {
            this.label = label;
            this.scale = scale;
        }

        public void Change(ObjectId newStyleId)
        {
            // Сохранение привязанных значений
            ReadReferences();

            label.StyleId = newStyleId;

            WriteReferences();
        }

        private void WriteReferences()
        {
            foreach (var safeComp in safeComponents)
            {
                safeComp.Restore();
            }
        }

        private void ReadReferences()
        {
            safeComponents = new List<ISafeComponent>();
            foreach (ObjectId textCompId in label.GetTextComponentIds())
            {
                var textComp = textCompId.GetObject(OpenMode.ForRead) as LabelStyleComponent;
                if (textComp == null) continue;
                if (textComp is LabelStyleReferenceTextComponent)
                {                    
                    var refTextTargetId = label.GetReferenceTextTarget(textCompId);
                    if (refTextTargetId.IsValidEx())
                    {
                        var targetSafeComp = new TargetComponent(label, textComp, refTextTargetId);
                        safeComponents.Add(targetSafeComp);
                    }
                }
                else if (label.IsTextComponentOverriden(textCompId))
                {
                    var overrideText = label.GetTextComponentOverride(textCompId);
                    var textOverrideSafeComp = new TextOverrideComponent(label, textComp.Name, overrideText);
                    safeComponents.Add(textOverrideSafeComp);
                }
            }
            // Сохранение положение полки
            safeComponents.Add(new LeaderLocationSafe(label, scale));
        }
    }
}
