using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.PaletteCommands;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.ApplicationServices;

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
        private const string Command_Civil_TurningPointCreate = "GP_Civil_TurningPointCreate";

        public static void InitCommands()
        {            
            CommandsPalette = new List<IPaletteCommand>()
            {
                new PaletteCommand(ResponsibleUsers, "Создание поворотных точек", Properties.Resources.GP_Civil_ImportSettingsKR,
                        Command_Civil_TurningPointCreate, "", GroupCivil),                
            };
        }

        [CommandMethod(Group, Command_Civil_TurningPointCreate, CommandFlags.Modal)]
        public static void TurningPointCreate()
        {            
            CommandStart.Start(doc =>
            {
                TurningPoint.TurningPointService tps = new TurningPoint.TurningPointService();
                tps.Start();
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
