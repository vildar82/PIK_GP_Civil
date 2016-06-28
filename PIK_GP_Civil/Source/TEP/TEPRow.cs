using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Civil.TEP
{
    public class TEPRow: ITEPRow
    {
        private List<IClassificator> items;        
        
        public string Name { get; set; }
        public string Units { get; set; }
        public double Value { get; set; }

        public TEPRow (string key, List<IClassificator> items)
        {
            Name = key;
            this.items = items;
            var first = items.First();
            Units = first.Units;
            Value = items.Sum(i => i.Value);
        }
    }
}
