using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.Extensions;
using Autodesk.Aec.DatabaseServices;

namespace PIK_GP_Civil.Surface.ChangeLabelStyles
{
    public class LeaderLocationSafe
    {
        double scale;            
        Label label;
        private LabelStyleScale newScaleStyle;

        public LeaderLocationSafe(Label label, LabelStyleScale newScaleStyle, double scale)
        {
            this.label = label;
            this.scale = scale;
            this.newScaleStyle = newScaleStyle;            
        }        
        
        /// <summary>
        /// Смена стиля метки. Проверка стиля (зеркальный/обычный)
        /// </summary>
        public void ChangeLabelStyle()
        {
            // Определение типа зеркальности текущего стиля метки
            var mirrStyle = LabelStyleScale.IsMirrorStyle(label.StyleName);
            // Определение типа стиля по положению метки - если вектор выноски в -x то это зеркальный тип
            var mirrLabel = GetTypeMirrorLabel(label);
            // Стиль по типу зеркальности
            var newStyle = newScaleStyle.GetStyle(mirrLabel);

            // Сохранение положения метки
            var labelLocationOld = label.LabelLocation;

            label.StyleId = newStyle.Id;    
            
            // Если зеркальность стиля не соответствует метке, то процедура исправления
            if (mirrStyle != mirrLabel)
            {
                CorrectMirror(labelLocationOld, mirrLabel);
            }
        }

        /// <summary>
        /// Коррктировка положения выноски в соответствии с зеркальностью метки и стиля
        /// </summary>
        /// <param name="labelLocationOld">Старое положение метки</param>
        private void CorrectMirror(Point3d labelLocationOld, bool mirrLabel)
        {
            label.ResetLocation();
            var lengthLabel = scale == 2 ? 5 : 10;
            var dir = mirrLabel ? 1 : -1;
            label.LabelLocation = new Point3d(labelLocationOld.X + lengthLabel * dir, labelLocationOld.Y, labelLocationOld.Z);
        }        

        /// <summary>
        /// Определение типа зеркальности метки
        /// </summary>
        /// <param name="label">Метка</param>
        /// <returns>Зеркальная или нет</returns>
        private bool GetTypeMirrorLabel(Label label)
        {
            // Если смещена выноска в противоположном X направлении
            return label.Dragged && label.DraggedOffset.X < 0;
        }
    }
}
