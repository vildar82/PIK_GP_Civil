using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Civil.FCS;

namespace PIK_GP_Civil.Insolation.Constructions
{
    /// <summary>
    /// Здание - по контуру полилинии
    /// </summary>
    public abstract class Building : IBuilding
    {
        protected ObjectId IdEnt { get; set; }
        public int Floors { get; set; }
        public Extents3d ExtentsInModel { get; set; }
        public Polyline Contour { get; set; }
        public List<FCProperty> FCProperties { get; set; }

        //public virtual Polyline GetContour ()
        //{
        //    var curve = IdEnt.GetObject(OpenMode.ForRead, false, true) as Polyline;
        //    return curve;
        //}

        public Building (Polyline pl, int floors, List<FCProperty> props)
        {
            IdEnt = pl.Id;
            ExtentsInModel = pl.GeometricExtents;
            Contour = pl;
            Floors = floors;
            FCProperties = props;
        }
    }
}
