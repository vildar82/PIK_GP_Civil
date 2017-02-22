using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Civil.DatabaseServices.Styles;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using PIK_GP_Civil.Styles;

namespace PIK_GP_Civil.Surface.ChangeLabelStyles
{
    public class LabelStyleScale
    {                
        Database db;        
        Label label;

        public LabelStyleScale(Label label, LabelStyleType labelStyleType, string toStyleName)
        {
            db = label.Database;
            this.label = label;
            ToStyleName = toStyleName;
            LabelStyleType = labelStyleType;
        }

        public string ToStyleName { get; set; }
        public LabelStyleType LabelStyleType { get; set; }
        public StyleBase ToStyle { get; set; }
        public StyleBase ToStyleMirror { get; set; }

        public StyleBase GetStyle(bool mirror)
        {   
            return mirror ? (ToStyleMirror ?? ToStyle) : ToStyle;
        }
        
        public static bool IsMirrorStyle(string styleName)
        {
            return styleName.StartsWith(SurfaceChangeLabelStyles.MirrorStylePrefix, StringComparison.OrdinalIgnoreCase);
        }       

        /// <summary>
        /// Имя стиля, без зеркальности
        /// </summary>
        /// <param name="name">Имя стиля</param>        
        public static string GetStyleNameWithoutMirror(string name)
        {
            if (IsMirrorStyle(name))
            {
                return name.Substring(SurfaceChangeLabelStyles.MirrorStylePrefix.Length);
            }
            return name;
        }

        public static string GetMirrorStyleName(string styleName)
        {
            return SurfaceChangeLabelStyles.MirrorStylePrefix + styleName;
        }

        public void ChangeStyle(double scale)
        {
            label.UpgradeOpen();
            var changer = new LabelStyleSafeChanger(label, scale, db);
            changer.Change(this);            
        }        
    }
}
