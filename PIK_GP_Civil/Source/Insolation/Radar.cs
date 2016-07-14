using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using AcadLib;
using AcadLib.Geometry;
using AcadLib.Errors;
using PIK_GP_Civil.Insolation.SunlightRule;
using PIK_GP_Civil.Insolation.Constructions;

namespace PIK_GP_Civil.Insolation
{
    /// <summary>
    /// Радар - расчета инсоляции в точке
    /// </summary>
    public class Radar
    {
        Options options;
        ISunlightRule rule;
        Database db;

        public Radar (Database db, Options options)
        {
            this.db = db;
            this.options = options;
            rule = options.SunlightRule;
        }

        /// <summary>
        /// Сканирование области из точки
        /// </summary>        
        public List<IlluminationArea> Scan (Point3d ptScan, Scope scope, BlockTableRecord ms)
        {
            // найти точки пересечения с объектами по направлению                                    
            var illuminations = new List<IlluminationArea>();
            var buildings = scope.Buildings;
            //var plBuildings = scope.Buildings.Select(b => b.GetContour()).ToList();                        

            // Сканирование с крупным шагом, до обнаружения препятствия
            int countLargeStep = Convert.ToInt32((rule.EndAngle - rule.StartAngle) / options.ScaningStepLarge);

            IlluminationArea illumArea = null;
            bool scanToStartIllumArea = true;
            Point3d ptIntersectNearest = Point3d.Origin;
            Point3d ptEndScanPoint = Point3d.Origin;

            Vector2d vecRay = Vector2d.XAxis;
            vecRay.RotateBy(rule.StartAngle.ToRadians());

            for (double i = 0; i < countLargeStep; i += options.ScaningStepLarge)
            {
                using (Line ray = new Line(ptScan, rule.GetPointByHeightInVector(ptScan, vecRay, options.MaxHeight)))
                {
                    ptEndScanPoint = ray.EndPoint;
                    Point3d ptFound;
                    if (FindIntersectNearestPiont(buildings, ray, out ptFound))
                    {
                        ptIntersectNearest = ptFound;
                        // Найдено пересечение луча со зданием
                        if (!scanToStartIllumArea)
                        {
                            // Конец освещенного участка
                            illumArea.EndPoint = CorrectScan(ray, buildings, true);
                            illuminations.Add(illumArea);
                            illumArea = null;
                            scanToStartIllumArea = true;
                        }
                    }
                    else
                    {
                        // Не найдено пересечений

                        if (scanToStartIllumArea)
                        {
                            // Начало освещенного участка
                            illumArea = new IlluminationArea(options, rule, db, ray.StartPoint);
                            if (ptIntersectNearest == Point3d.Origin)
                            {
                                illumArea.StartPoint = ray.EndPoint;
                            }
                            else
                            {
                                // найти точку пересечения
                                illumArea.StartPoint = CorrectScan(ray, buildings, false);
                            }
                            scanToStartIllumArea = false;
                        }
                    }
                }
                vecRay.RotateBy(options.ScaningStepLarge.ToRadians());
                //RotateRay(ray, options.ScaningStepLarge);
            }

            // Если есть начатая освещенная зона - добавить ее
            if (illumArea != null)
            {
                illumArea.EndPoint = ptEndScanPoint;
                illuminations.Add(illumArea);
            }

            return illuminations;
        }        

        /// <summary>
        /// Поворот луча на заданный угол в градусах
        /// </summary>        
        private void RotateRay (Line ray, double angle)
        {
            ray.TransformBy(Matrix3d.Rotation(-angle.ToRadians(), Vector3d.ZAxis, ray.StartPoint));
        }

        private static bool FindIntersectNearestPiont (IEnumerable<IBuilding> buildings, Line ray, 
            out Point3d ptIntersectNearest)
        {
            bool findRes = false;
            ptIntersectNearest = Point3d.Origin;
            foreach (var build in buildings)
            {
                var ptIntersects = new Point3dCollection();
                ray.IntersectWith(build.Contour, Intersect.OnBothOperands, new Plane(), ptIntersects, IntPtr.Zero, IntPtr.Zero);
                if (ptIntersects.Count > 0)
                {
                    Point3d ptNearest;
                    if (NearestPoint(ptIntersects, ray.StartPoint, out ptNearest))
                    {
                        if (findRes)
                        {
                            if ((ptIntersectNearest - ray.StartPoint).Length > (ptNearest - ray.StartPoint).Length)
                            {
                                ptIntersectNearest = ptNearest;
                            }

                        }
                        else
                        {
                            ptIntersectNearest = ptNearest;
                        }
                        findRes = true;
                    }                    
                }
            }
            return findRes;
        }

        private Point3d CorrectScan (Line ray, IEnumerable<IBuilding> buildings, bool wasIntersect)
        {
            Point3d ptRes = Point3d.Origin;            
            int clockwiseFactor = -1;
            var rayCorrect = (Line)ray.Clone();
            bool repeat = false;
            bool defineRes = false;        
            do
            {
                Point3d ptIntersect;     
                if (FindIntersectNearestPiont(buildings, rayCorrect, out ptIntersect))
                {
                    // Найдено пересечение
                    if (wasIntersect)
                    {
                        // Если изначально было пересечение, то надо найти пустую область без пересечений, тогда точка последнего пересечения будет результатом
                        repeat = true;
                        ptRes = ptIntersect;
                    }
                    else
                    {
                        // Если изначально не было пересечения, то результат определен
                        repeat = false;
                        ptRes = ptIntersect;
                        defineRes = true;
                    }
                }
                else
                {
                    // Пересечение не найдено
                    if (wasIntersect)
                    {                        
                        repeat = false;
                        defineRes = true;
                    }
                    else
                    {
                        // Если изначально не было пересечения, то продолжаем поиск пересечения
                        repeat = true;
                    }
                }
                // поворот луча
                RotateRay(rayCorrect, options.ScaningStepSmall * clockwiseFactor);
                
            } while (repeat);
            
            if (!defineRes)
            {
                throw new InvalidOperationException($"Недопустимая ситуация - не определено точное пересечение.");
            }

            return ptRes;
        }

        private static bool NearestPoint (Point3dCollection pts, Point3d ptDest, out Point3d ptNearest)
        {
            bool res = false;
            ptNearest = Point3d.Origin;
            var ptsArray = new Point3d[pts.Count];
            pts.CopyTo(ptsArray, 0);
            var find = ptsArray.Select(p=>new { point = p, length = (p - ptDest).Length }).OrderBy(p => p.length).First(p=>p.length>5);
            if (find != null)
            {
                ptNearest = find.point;
                res = true;
            }
            return res;
        }        
    }
}
