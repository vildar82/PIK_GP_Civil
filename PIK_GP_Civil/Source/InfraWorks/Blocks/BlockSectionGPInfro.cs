using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Civil.InfraWorks;
using PIK_GP_Civil.InfraWorks.ODs;

namespace PIK_GP_Civil.InfraWorks.Blocks
{
    public class BlockSectionGPInfro : PIK_GP_Acad.BlockSection.Section, IInfrastructure
    {
        public const string BlockNameGPMatch = "ГП_Блок-секция";
        public int Height;

        public BlockSectionGPInfro (string blName, BlockReference blRef) : base(blRef, blName)
        {
            Height = NumberFloor * 3 + 3;
        }        

        public void Export (BlockTableRecord model)
        {            
            var blRef = IdBlRef.GetObject( OpenMode.ForRead, false, true) as BlockReference;
            // Определение полилинии контура и копирование в модель
            var pl = IdPlContour.GetObject(OpenMode.ForRead) as Polyline;
            //var pl = ExportService.GetPls(blRef.BlockTableRecord).First();
            var idPlbuilding = ExportService.CopyPl(model, blRef, pl);
            ODBuilding od = new ODBuilding (BuildingType.Live, Height);
            od.AddRecord(idPlbuilding);
        }
    }
}
