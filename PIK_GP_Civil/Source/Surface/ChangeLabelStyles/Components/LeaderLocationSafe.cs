using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.DatabaseServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.Extensions;

namespace PIK_GP_Civil.Surface.ChangeLabelStyles
{
    public class LeaderLocationSafe : ISafeComponent
    {
        double scale;
        Point3d location;
        Point3d labelLocation;
        Vector3d dragOffset;
        bool dragged;
        double labelLenghtNeed;
        Label label;

        public LeaderLocationSafe(Label label, double scale)
        {
            this.label = label;
            this.scale = scale;
            location = label.AnchorInfo.Location;
            labelLocation = label.LabelLocation;
            dragged = label.Dragged;
            dragOffset = label.DraggedOffset;
            labelLenghtNeed = scale == 2 ? 10 : 5; // Для масштаба 1000 - длина полки 10мм, для 500 - 5мм.
        }
        public void Restore()
        {            
            if (!dragged || dragOffset.X >= 0) return;

            // Определение длины полки
            Point3d ptLabelLineEnd;
            var curLabelLineLength = GetLabelLineLength(out ptLabelLineEnd);
            // Если длина полки отличается от заданной в стиле (для 500 - 5мм, для 1000 - 10мм)
            if (!curLabelLineLength.IsEqual(labelLenghtNeed, 0.1))
            {
                // Смещение точки полки и изменение вектора смещения выноски
                var newLabelLocation = new Point3d(ptLabelLineEnd.X- 10, ptLabelLineEnd.Y, ptLabelLineEnd.Z);
                label.LabelLocation = newLabelLocation;
                label.LeaderAttachment = Autodesk.Civil.LeaderAttachmentBehaviorType.ToMarkerExtents;
                //label.DraggedOffset = dragOffset;                                  
            }
        }

        private double GetLabelLineLength(out Point3d ptLabelLineEnd)
        {
            ptLabelLineEnd = location + dragOffset;
            var length = ptLabelLineEnd - labelLocation;
            return length.Length;
        }
    }
}
