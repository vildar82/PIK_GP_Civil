using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Civil.Lib.OD;

namespace PIK_GP_Civil.InfraWorks.ODs
{  
    /// <summary>
    /// Параметры Object Data для Покрытий
    /// </summary>
    public class ODCoverage
    {
        public const string CoverageSideWalk = "Тротуары с проездом";        

        public const string ParamTable = "Покрытия";
        public const string ParamTableDesc = "Покрытия";
        public const string ParamCoverageType = "Тип";
        public const string ParamCoverageTypeDesc = "Тип покрытия: Тротуары с проездом";

        public static ODTable ODTableCoverage { get; set; } = new ODTable(ParamTable, ParamTableDesc) {
            Parameters = new List<ODParameter>()
            {
                new ODParameter(ParamCoverageType, Autodesk.Gis.Map.Constants.DataType.Character,ParamCoverageTypeDesc, "")
            }
        };

        public string Coverage { get; set; }        

        /// <summary>
        /// Запись OD Покрытия
        /// </summary>
        /// <param name="type">Тип покрытия: Тротуары с проездом</param>        
        public ODCoverage (CoverageType type)
        {
            Coverage = type.Name;            
        }

        public void AddRecord (ObjectId idDbo)
        {
            ODTableCoverage[ParamCoverageType].Value = Coverage;
            using (var dbo = idDbo.GetObject(OpenMode.ForWrite, false))
            {
                ODService.AddRecord(dbo, ODTableCoverage);
            }
        }
    }

    public class CoverageType
    {
        public static readonly CoverageType SideWalk = new CoverageType(ODCoverage.CoverageSideWalk);        

        public readonly string Name;                
        private CoverageType (string name)
        {
            Name = name;            
        }
    }
}
