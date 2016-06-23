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
    /// Параметры Object Data для зданий
    /// </summary>
    public class ODBuilding
    {
        public const string BuildingLive = "Жилье";
        public const string BuildingSocial = "Социалка";
        public const string BuildingGarage = "Гаражи";

        public const string ParamTable = "Здания";
        public const string ParamTableDesc = "Здания";
        public const string ParamBuildingType = "Тип";
        public const string ParamBuildingTypeDesc = "Тип здания: Жилье, Социалка";
        public const string ParamHeight = "Высота";
        public const string ParamHeightDesc = "Высота здания, м";

        public static ODTable ODTableBuildings { get; set; } = new ODTable(ParamTable, ParamTableDesc) 
        {
            Parameters = new List<ODParameter>() 
            {
                new ODParameter(ParamBuildingType, Autodesk.Gis.Map.Constants.DataType.Character,ParamBuildingTypeDesc, ""),
                new ODParameter(ParamHeight,  Autodesk.Gis.Map.Constants.DataType.Integer,ParamHeightDesc, 0)
            }
        };

        public string Building { get; set; }
        public int Height { get; set; }

        /// <summary>
        /// Запись OD Здания
        /// </summary>
        /// <param name="type">Тип здания - Жилье, Социалька</param>
        /// <param name="height">Высота, м</param>
        public ODBuilding (BuildingType type, int height)
        {
            Building = type.Name;
            Height = height;
        }

        public void AddRecord (ObjectId idDbo)
        {
            ODTableBuildings[ParamBuildingType].Value = Building;
            ODTableBuildings[ParamHeight].Value = Height;
            using (var dbo = idDbo.GetObject(OpenMode.ForWrite, false))
            {
                ODService.AddRecord(dbo, ODTableBuildings);
            }
        }
    }

    public class BuildingType
    {
        public static readonly BuildingType Live = new BuildingType(ODBuilding.BuildingLive);
        public static readonly BuildingType Social = new BuildingType(ODBuilding.BuildingSocial);
        public static readonly BuildingType Garage = new BuildingType(ODBuilding.BuildingGarage);

        public readonly string Name;                
        private BuildingType (string name)
        {
            Name = name;            
        }
    }
}
