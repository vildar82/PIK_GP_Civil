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
        public const string GroupCivil = "Civil";        
        public static List<IPaletteCommand> CommandsPalette { get; set; }
        // Комманды        

        public static void InitCommands()
        {            
            CommandsPalette = new List<IPaletteCommand>()
            {
                new PaletteCommand(ResponsibleUsers, "Кадастр", Resources.GP_Civil_KPTXML,
                        nameof(GP_Civil_KPTXML), "Конвертер выписок РосРеестра"),
                new PaletteCommand("Создание поворотных точек", Resources.GP_Civil_TutningPoints,
                        nameof(GP_Civil_TurningPointCreate), "", GroupCivil),
                new PaletteCommand("Создание таблицы поворотных точек", Resources.GP_Civil_TutningPointsTable,
                        nameof(GP_Civil_TurningPointTable), "", GroupCivil),
            };
        }

        [CommandMethod(Group, nameof(GP_Civil_KPTXML), CommandFlags.Modal)]
        public static void GP_Civil_KPTXML()
        {
            CommandStart.Start(doc =>
            {
                Kadastr.KadastrService.KPTXML(doc);
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
