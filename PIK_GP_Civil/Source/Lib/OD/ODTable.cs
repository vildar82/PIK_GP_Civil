using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Gis.Map.ObjectData;

namespace PIK_GP_Civil.Lib.OD
{
    /// <summary>
    /// Описание таблицы данных Object Data
    /// </summary>
    public class ODTable
    {
        /// <summary>
        /// Имя таблицы
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Параметры в таблице
        /// </summary>
        public List<ODParameter> Parameters { get; set; }
        public string Description { get; set; }

        public ODTable(string name, string desc)
        {
            Name = name;
            Description = desc;
        }

        public ODParameter this[string name] {
            get { return Parameters.Find(p => p.Name == name); }
        }

        public void SetValues (Record rec)
        {
            foreach (var item in Parameters)
            {
                rec[item.Index].Assign(item.Value);
            }                               
        }

        public object GetInfo ()
        {
            var paramsInfo = string.Join(";", Parameters.Select(p=>p.GetInfo()));
            return $"Название = {Name}, Параметры: {paramsInfo}";
        }
    }
}
