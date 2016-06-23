using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Civil.InfraWorks.Blocks
{
    public class BlockSectionInfro : PIK_GP_Acad.KP.KP_BlockSection.BlockSection, IInfrastructure
    {
        public const string  BlockNameOrdinary ="ГП_К_Секция_Рядовая";
        public const string  BlockNameAngle ="ГП_К_Секция_Угловая";
        public int Height;

        public BlockSectionInfro (string blName, BlockReference blRef) : base(blRef, blName)
        {
            Height = Floors * 3 + 3;
        }        

        public void Export (BlockTableRecord model)
        {            
            var blRef = IdBlRef.GetObject( OpenMode.ForRead, false, true) as BlockReference;
            // Определение полилинии контура и копирование в модель
            var pl = ExportService.GetPls(blRef.BlockTableRecord, PIK_GP_Acad.KP.KP_BlockSection.KP_BlockSectionService.blKpParkingLayerContour).First();
            var idPlbuilding = ExportService.CopyPl(model, blRef, pl);
            ODs.ODBuilding od = new ODs.ODBuilding (ODs.BuildingType.Live, Height);
            od.AddRecord(idPlbuilding);
        }
    }
}
