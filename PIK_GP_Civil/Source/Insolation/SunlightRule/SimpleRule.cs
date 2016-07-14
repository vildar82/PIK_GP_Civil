using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

namespace PIK_GP_Civil.Insolation.SunlightRule
{
    /// <summary>
    /// Простая инсоляционная линейка
    /// </summary>
    public class SimpleRule : ISunlightRule
    {
        private double ratioLength = 1.42814;

        public double EndAngle { get; set; } = 168;

        public double StartAngle { get; set; } = 12;        

        /// <summary>
        /// Определение длины по высоте
        /// </summary>        
        public int GetLength (int height)
        {
            return Convert.ToInt32(height * ratioLength);
        }

        /// <summary>
        /// Определение высоты точки
        /// </summary>
        /// <param name="point">Точка определения высоты</param>
        /// <param name="origin">Начало</param>
        /// <returns>Высота, м</returns>
        public double GetHeightAtPoint (Point3d point, Point3d origin)
        {
            // Определение длины проекции на ось Y
            var vec = point - origin;
            var vecHeight = vec.OrthoProjectTo(Vector3d.YAxis);
            var height = vecHeight.Length / ratioLength;
            return height;
        }

        public Point3d GetPointByHeightInVector (Point3d ptOrigin, Vector2d vec, int height)
        {
            Point3d ptRes = Point3d.Origin;

            var len = GetLength(height);
            var vecHeight = new Vector2d(0, -len);
            var vecHeightProjection = vecHeight.GetPerpendicularVector();            

            return ptRes;
        }
    }
}
