﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using PIK_GP_Civil.FCS;

namespace PIK_GP_Civil.Elements.Buildings
{
    /// <summary>
    ///  Окружающая застройка
    /// </summary>
    public class Surround : Building
    {
        public Surround (Polyline pl, List<FCProperty> props) : base(pl, props)
        {
        }
    }
}
