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
    public class BlockSectionKPInfro : PIK_GP_Acad.KP.KP_BlockSection.BlockSection, IInfrastructure
    {
        public const string BlockNameKPMatch = "ГП_К_Секция";        
        public const string  BlockNameOrdinary ="ГП_К_Секция_Рядовая";
        public const string  BlockNameAngle ="ГП_К_Секция_Угловая";
        public const string BlockNameTower = "ГП_К_Секция_Башня";
        public int Height;

        public BlockSectionKPInfro (string blName, BlockReference blRef) : base(blRef, blName)
        {
            Height = Floors * 3 + 3;
        }        

        public void Export (BlockTableRecord model)
        {            
            var blRef = IdBlRef.GetObject( OpenMode.ForRead, false, true) as BlockReference;
            // Определение полилинии контура и копирование в модель
            var pl = IdPlContour.GetObject(OpenMode.ForRead) as Polyline;
            //var pl = ExportService.GetPls(blRef.BlockTableRecord, PIK_GP_Acad.KP.KP_BlockSection.KP_BlockSectionService.blKpParkingLayerAxisContour).First();
            var idPlbuilding = ExportService.CopyPl(model, blRef, pl);
            ODBuilding od = new ODBuilding (BuildingType.Live, Height);
            od.AddRecord(idPlbuilding);
        }
    }
}
