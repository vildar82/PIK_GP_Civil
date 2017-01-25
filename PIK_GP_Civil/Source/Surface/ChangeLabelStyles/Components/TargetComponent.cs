using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using AcadLib.Errors;

namespace PIK_GP_Civil.Surface.ChangeLabelStyles
{
    public class TargetComponent : ISafeComponent
    {
        private Label label;
        private ObjectId targetId;
        private string name;
        private TextOverrideComponent textOverrideComp;

        public TargetComponent(Label label, LabelStyleComponent component, ObjectId targetId)
        {
            this.label = label;
            this.name = component.Name;
            this.targetId = targetId;
            if (label.IsTextComponentOverriden(component.Id))
            {
                textOverrideComp = new TextOverrideComponent(label, component);
            }
        }

        public void Restore()
        {
            foreach (ObjectId componentId in label.GetTextComponentIds())
            {
                var component = componentId.GetObject( OpenMode.ForRead) as LabelStyleReferenceTextComponent;
                if (component != null && component.Name.Equals (this.name, StringComparison.OrdinalIgnoreCase))
                {
                    label.SetReferenceTextTarget(componentId, targetId);
                    if (textOverrideComp != null)
                    {
                        textOverrideComp.Restore(componentId);
                    }
                    return;
                }
            }
            Inspector.AddError($"Не найден компонент '{name}' в стиле '{label.StyleName}'", label, System.Drawing.SystemIcons.Error);
        }
    }
}
