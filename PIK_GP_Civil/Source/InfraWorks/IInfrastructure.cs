using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib.Errors;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Civil.InfraWorks
{
    /// <summary>
    /// Блок инфраструктуры для экспорта в InfraWorks
    /// </summary>
    public interface IInfrastructure
    {
        Error Error { get; set; }

        /// <summary>
        /// Преобразование блока для экспорта
        /// Копирование контуров полилиний, добавление OD
        /// </summary>
        void Export (BlockTableRecord model);
    }
}
