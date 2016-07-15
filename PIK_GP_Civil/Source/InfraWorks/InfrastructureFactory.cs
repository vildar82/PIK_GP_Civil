using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using PIK_GP_Civil.InfraWorks.Blocks;

namespace PIK_GP_Civil.InfraWorks
{
    /// <summary>
    /// Фаьрика определения инфраструктурных объектов
    /// </summary>
    public static class InfrastructureFactory
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

        public static IInfrastructure Create (Entity ent)
        {
            IInfrastructure res = null;
            Type typeBlock;
            if (ent is BlockReference)
            {
                var blRef = (BlockReference)ent;
                string blName = blRef.GetEffectiveName();
                if (BlockTypes.TryGetValue(blName, out typeBlock))
                {
                    res = (IInfrastructure)Activator.CreateInstance(typeBlock, blName, blRef);
                }                
            }
            return res;
        }
    }
}
