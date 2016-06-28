using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIK_GP_Civil.TEP
{
    public interface IClassificator
    {
        /// <summary>
        /// Порядок строки в таблице
        /// </summary>
        int Index { get; set; }

        /// <summary>
        /// Наименован ие показателя
        /// </summary>
        string Name { get; set; }
        string Units { get; set; }
        double Value { get; set; }
    }
}
