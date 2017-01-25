using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Civil.DatabaseServices;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;

namespace PIK_GP_Civil.Surface.ChangeLabelStyles
{
    public class TextOverrideComponent : ISafeComponent
    {
        private LabelStyleReferenceTextComponent component;
        private Label label;
        private string name;
        private string overrideText;

        public TextOverrideComponent(Label label, LabelStyleComponent component)
        {
            this.label = label;
            overrideText = label.GetTextComponentOverride(component.Id);
        }

        public TextOverrideComponent(Label label, string name, string overrideText)
        {
            this.label = label;
            this.name = name;
            this.overrideText = overrideText;
        }

        public void Restore()
        {
            foreach (ObjectId componentId in label.GetTextComponentIds())
            {
                var component = componentId.GetObject(OpenMode.ForRead) as LabelStyleComponent;
                if (component != null && component.Name.Equals(this.name, StringComparison.OrdinalIgnoreCase))
                {
                    label.SetTextComponentOverride(componentId, overrideText);
                    return;
                }
            }
            Inspector.AddError($"Не найден компонент '{name}' в стиле '{label.StyleName}'", label, System.Drawing.SystemIcons.Error);
        }

        public void Restore(ObjectId componentId)
        {
            label.SetTextComponentOverride(componentId, overrideText);
        }
    }
}
