using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Civil.TEP
{
    public interface ITEPRow
    {
        string Name { get; set; }
        string Units { get; set; }
        double Value { get; set; }
    }
}
