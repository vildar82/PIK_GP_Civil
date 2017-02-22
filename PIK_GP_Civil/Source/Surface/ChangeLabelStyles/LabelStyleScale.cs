using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Civil.DatabaseServices.Styles;

namespace PIK_GP_Civil.Surface.ChangeLabelStyles
{
    public class LabelStyleScale
    {
        private StyleBase toStyle;
        private StyleBase toStyleMirror;

        public LabelStyleScale(StyleBase toStyle, StyleBase toStyleMirror)
        {
            this.toStyle = toStyle;
            this.toStyleMirror = toStyleMirror;
        }

        public StyleBase GetStyle(bool mirror)
        {
            if (mirror)
            {
                if (toStyleMirror == null)
                {

                }
            }
            return mirror ? (toStyleMirror ?? toStyle) : toStyle;
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
    }
}
