using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.PaletteCommands;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.ApplicationServices;
using PIK_GP_Civil.Properties;

[assembly: CommandClass(typeof(PIK_GP_Civil.Commands))]
[assembly: ExtensionApplication(typeof(PIK_GP_Civil.Commands))]

namespace PIK_GP_Civil
{
    public class Commands : IExtensionApplication
    {
        /// <summary>
        /// Ответственные пользователи. Изменение настроек, тестирование, и т.п.
        /// </summary>
        public static readonly List<string> ResponsibleUsers = new List<string>() { "PrudnikovVS", AutoCAD_PIK_Manager.Env.CadManLogin };
        public const string Group = AutoCAD_PIK_Manager.Commands.Group;
        public const string GroupKP = PIK_GP_Acad.Commands.GroupKP;
        public const string GroupCivil = "Civil";        
        public static List<IPaletteCommand> CommandsPalette { get; set; }
        // Комманды        

        public static void InitCommands()
        {            
            CommandsPalette = new List<IPaletteCommand>()
            {
                new PaletteCommand("Кадастр", Resources.GP_Civil_OKSXML,
                        nameof(GP_Civil_OKSXML), "Конвертер выписок РосРеестра", GroupKP),
                new PaletteCommand("Создание поворотных точек", Resources.GP_Civil_TutningPoints,
                        nameof(GP_Civil_TurningPointCreate), "", GroupKP),
                new PaletteCommand("Создание таблицы поворотных точек", Resources.GP_Civil_TutningPointsTable,
                        nameof(GP_Civil_TurningPointTable), "", GroupKP),
            };
        }

        [CommandMethod(Group, nameof(GP_Civil_OKSXML), CommandFlags.Modal)]
        public static void GP_Civil_OKSXML()
        {
            CommandStart.Start(doc =>
            {
                Kadastr.KadastrService.OKSXML(doc);
            });
        }

        [CommandMethod(Group,nameof(GP_Civil_TurningPointCreate), CommandFlags.Modal)]
        public static void GP_Civil_TurningPointCreate()
        {            
            CommandStart.Start(doc =>
            {
                TurningPoint.TurningPointService tps = new TurningPoint.TurningPointService();
                tps.StartCreatePoints();
            });
        }

        [CommandMethod(Group, nameof(GP_Civil_TurningPointTable), CommandFlags.Modal)]
        public static void GP_Civil_TurningPointTable()
        {
            CommandStart.Start(doc =>
            {
                TurningPoint.TurningPointService tps = new TurningPoint.TurningPointService();
                tps.StartCreateTable();
            });
        }

        public void Initialize()
        {
            try
            {
                CivilDocument civil = CivilApplication.ActiveDocument;
            }
            catch
            {
                return;
            }
            if (PaletteSetCommands.CommandsAddin != null && PaletteSetCommands.CommandsAddin.Count > 0)
            {
                InitCommands();
                PaletteSetCommands.CommandsAddin.AddRange(CommandsPalette);
            }
        }

        public void Terminate()
        {            
        }
    }
}
