﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using AcadLib.PaletteCommands;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
using Autodesk.Civil.ApplicationServices;
using PIK_GP_Civil.Properties;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.DatabaseServices;

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

        [CommandMethod("TestTin", CommandFlags.Modal)]
        public void TestTin()
        {
            var doc = Application.DocumentManager.MdiActiveDocument;
            var db = doc.Database;
            var ed = doc.Editor;

            var selOPt = new PromptEntityOptions("\nВыбор поверхности");
            selOPt.SetRejectMessage("\nТолько поверхность");
            selOPt.AddAllowedClass(typeof(TinSurface), true);
            var selTin = ed.GetEntity(selOPt);
            if (selTin.Status != PromptStatus.OK) return;

            using (var t = doc.TransactionManager.StartTransaction())
            {
                var surfId = selTin.ObjectId;
                var surf = surfId.GetObject(OpenMode.ForRead) as TinSurface;
                dynamic surfCom = surf.AcadObject;
                                
                for (int i = 0; i < surfCom.Breaklines.Count; i++)
                {
                    var brLineCom = surfCom.Breaklines.Item(i);
                    var brLineEnts = (object[])brLineCom.BreaklineEntities;                 
                    for (int b = 0; b < brLineEnts.Length; b++)
                    {
                        var brLineId = Autodesk.AutoCAD.DatabaseServices.DBObject.FromAcadObject(brLineEnts[b]);
                        if (brLineId.IsValidEx())
                        {
                            var brLine = brLineId.GetObject(OpenMode.ForRead) as FeatureLine;
                            if (brLine.PointsCount== 0)
                            {
                                surfCom.Breaklines.Remove(i);                                
                            }
                        }                        
                    }
                }
                surf.Rebuild();
                t.Commit();
            }
        }

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
                new PaletteCommand("Экспорт в InfraWorks", Resources.KP_ExportToInfraworks,
                        nameof(GP_Civil_ExportToInfraWorks), "Копирование контуров полилиний из блоков инфраструктуры (блок-секции, СОШ, ДОО) в модель, для последующего экспорта в InfraWorks.",
                        GroupKP),
                new PaletteCommand("Установка настроек чертежа", Resources.Settings,
                        nameof(GP_Civil_DrawingSettings), "Установка стандартнвх настроек чертежа (Еденицы измерения, Параметры среды)", PIK_GP_Acad.Commands.GroupCommon)
            };
        }
        //
        // Концепция
        //
        #region Концепция    

        [CommandMethod(Group, nameof(GP_Civil_OKSXML), CommandFlags.Modal)]
        public static void GP_Civil_OKSXML()
        {
            CommandStart.Start(doc =>
            {
                Kadastr.KadastrService.OKSXML(doc);
            });
        }

        [CommandMethod(Group, nameof(GP_Civil_TurningPointCreate), CommandFlags.Modal)]
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

        [CommandMethod(Group, nameof(GP_Civil_ExportToInfraWorks), CommandFlags.Modal)]
        public static void GP_Civil_ExportToInfraWorks()
        {
            CommandStart.Start(doc =>
            {
                InfraWorks.ExportService.Export(doc);
            });
        }
        #endregion Концепция  

        /// <summary>
        /// Установка стандартных настроек чертежа цивила - единицы и т.п.
        /// </summary>
        [CommandMethod(Group, nameof(GP_Civil_DrawingSettings), CommandFlags.Modal)]
        public static void GP_Civil_DrawingSettings()
        {
            CommandStart.Start(doc =>
            {
                Settings.DrawingSettings.Setds();
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

            // Контекстное меню для трассы Alignment
            Navigator.AligmentShowProfile.AttachContextMenu();
            // Контекстное меню для FeatureLine
            FeatureLines.DetachFLFromTINSurface.DetachFlFromSurface.AttachContextMenu();
        }

        public void Terminate()
        {
        }
    }
}
