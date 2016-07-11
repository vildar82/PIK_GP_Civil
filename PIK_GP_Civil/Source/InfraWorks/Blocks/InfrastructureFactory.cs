using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;

namespace PIK_GP_Civil.InfraWorks.Blocks
{
    /// <summary>
    /// Фаьрика определения инфраструктурных объектов
    /// </summary>
    public static class InfrastructureFactory
    {
        private static Dictionary<string, Type> typesBlock = new Dictionary<string, Type> ()
            {
                { BlockSectionInfro.BlockNameOrdinary, typeof(BlockSectionInfro) },
                { BlockSectionInfro.BlockNameAngle, typeof(BlockSectionInfro) },
                { BlockSectionInfro.BlockNameTower, typeof(BlockSectionInfro) },
                { KindergartenInfro.BlockName, typeof(KindergartenInfro) },
                { SchoolInfro.BlockName, typeof(SchoolInfro) },
                { ParkingInfro.BlockName, typeof(ParkingInfro) }
            };

        public static IInfrastructure Create (string blName, BlockReference blRef)
        {
            Type typeBlock;
            if (typesBlock.TryGetValue(blName, out typeBlock))
            {
                return (IInfrastructure)Activator.CreateInstance(typeBlock, blName, blRef);
            }
            else
            {
                return null;
            }
        }
    }
}
