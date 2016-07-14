﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Civil.Insolation.SunlightRule;

namespace PIK_GP_Civil.Insolation
{
    public abstract class Options
    {
        /// <summary>
        /// Максимальная высота расчетная
        /// </summary>
        public int MaxHeight { get; internal set; }
        public ISunlightRule SunlightRule { get; set; }
        public double ScaningStepLarge { get; set; } = 1;
        public double ScaningStepSmall { get; set; } = 0.1;

        public Options (ISunlightRule rule, int maxHeight)
        {
            SunlightRule = rule;
            MaxHeight = maxHeight;
        }
    }
}
