using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Civil.InfraWorks.Blocks
{
    public class SchoolInfro : PIK_GP_Acad.KP.Social.SchoolBlock, IInfrastructure
    {
        public const string LayerBuilding = "_ГП_здания СОШ";
        public const string LayerCoverage = "_ГП_проект проездов";

        public int Height;

        public SchoolInfro (string blName, BlockReference blRef) : base(blRef, blName)
        {
            Height = Floors * 4;
        }

        public void Export (BlockTableRecord model)
        {            
            var blRef = IdBlRef.GetObject( OpenMode.ForRead, false, true) as BlockReference;
            // Определение полилинии контура и копирование в модель
            var pl = ExportService.GetPls(blRef.BlockTableRecord, LayerBuilding).First();
            var idPlbuilding = ExportService.CopyPl(model, blRef, pl);
            ODs.ODBuilding od = new ODs.ODBuilding (ODs.BuildingType.Social, Height);
            od.AddRecord(idPlbuilding);

            // определение полилиний покрытия
            var pls = ExportService.GetPls(blRef.BlockTableRecord, LayerCoverage);
            foreach (var item in pls)
            {
                var idPlCoverage = ExportService.CopyPl(model, blRef, item);
                ODs.ODCoverage odCov = new ODs.ODCoverage (ODs.CoverageType.SideWalk);
                odCov.AddRecord(idPlCoverage);
            }            
        }
    }
}
