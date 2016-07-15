using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AcadLib.Errors;
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
                { BlockSectionKPInfro.BlockNameKPMatch, typeof(BlockSectionKPInfro) },
                { BlockSectionGPInfro.BlockNameGPMatch, typeof(BlockSectionGPInfro) },
                { KindergartenInfro.BlockName, typeof(KindergartenInfro) },
                { SchoolInfro.BlockName, typeof(SchoolInfro) },
                { ParkingInfro.BlockName, typeof(ParkingInfro) }
            };

        public static IInfrastructure Create (Entity ent)
        {
            IInfrastructure res = null;            
            if (ent is BlockReference)
            {
                var blRef = (BlockReference)ent;
                string blName = blRef.GetEffectiveName();
                var blockType = GetType(blName);              
                if (blockType != null)
                {
                    res = (IInfrastructure)Activator.CreateInstance(blockType, blName, blRef);                    
                    if (res?.Error != null)
                    {
                        Inspector.AddError(res.Error);
                        res = null;
                    }
                }                
            }
            return res;
        }

        private static Type GetType (string blName)
        {
            var res = BlockTypes.FirstOrDefault(t => Regex.IsMatch(blName, t.Key, RegexOptions.IgnoreCase));            
            return res.Value;
        }
    }
}
