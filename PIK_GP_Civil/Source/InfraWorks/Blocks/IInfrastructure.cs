using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Civil.InfraWorks.Blocks
{
    /// <summary>
    /// Блок инфраструктуры для экспорта в InfraWorks
    /// </summary>
    public interface IInfrastructure
    {
        /// <summary>
        /// Преобразование блока для экспорта
        /// Копирование контуров полилиний, добавление OD
        /// </summary>
        void Export (BlockTableRecord model);
    }
}
