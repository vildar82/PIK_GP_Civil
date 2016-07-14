using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PIK_GP_Civil.Elements.Blocks;

namespace PIK_GP_Civil.Elements
{
    public static class ElementsTypes
    {
        public static Dictionary<string, Type> BlockTypes = new Dictionary<string, Type>()
            {
                { BlockSectionInfro.BlockNameOrdinary, typeof(BlockSectionInfro) },
                { BlockSectionInfro.BlockNameAngle, typeof(BlockSectionInfro) },
                { BlockSectionInfro.BlockNameTower, typeof(BlockSectionInfro) },
                { KindergartenInfro.BlockName, typeof(KindergartenInfro) },
                { SchoolInfro.BlockName, typeof(SchoolInfro) },
                { ParkingInfro.BlockName, typeof(ParkingInfro) }
            };
    }
}
