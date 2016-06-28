using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Civil.TEP
{
    public class Classificator : IClassificator
    {
        public int Index { get; set; }

        public string Name { get; set; }

        public string Units { get; set; }

        public double Value { get; set; }

        public Classificator(string name, int index, string units, double value)
        {
            Name = name;
            Index = index;
            Units = units;
            Value = Math.Round(value,2);            
        }            
    }
}
