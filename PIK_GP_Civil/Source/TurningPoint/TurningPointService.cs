using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using Autodesk.Civil.DatabaseServices.Styles;
using AcadLib.Files;
using AcadLib.Errors;
using PIK_GP_Civil.Lib.Settings;
using Autodesk.Civil.Settings;
using Autodesk.AutoCAD.EditorInput;

namespace PIK_GP_Civil.TurningPoint
{
    /// <summary>
    /// Процедура создания поворотных точек.
    /// Копирование настроек из шаблона.
    /// Установка параметров.
    /// Запуск команды CREATEPTPLYLNCTRVERTAUTO (простановка точек в вершинах полилинии автоматически)
    /// </summary>
    public class TurningPointService
    {
        const string innerDictName = "CIVIL_TurningPoint";
        const string recDateStyles = "StylesDate";
        TurningPointOptions options;
        Document doc;
        Database db;
        Editor ed;
        CivilDocument civil;
        ObjectId idPointStyle;
        ObjectId idLabelPointStyle;
        ObjectId idPointGroup;

        public async void Start()
        {
            doc = Application.DocumentManager.MdiActiveDocument;
            db = doc.Database;
            ed = doc.Editor;
            civil = CivilApplication.ActiveDocument;

            options = TurningPointOptions.Load();
            // Изменение настроек, только для ответственных пользователей
            if (Commands.ResponsibleUsers.Contains(Environment.UserName, StringComparer.OrdinalIgnoreCase))
            {
                try
                {
                    options = TurningPointOptions.PromptOptions(TurningPointOptions.Load());
                }
                catch
                {
                    return;                        
                }
            }

            // Копирование стилей из шаблона при необходимости
            CopyStyles();

            // Настроки
            SetSettings();

            // Запуск команды простановки точек в вершинах полилинии              
            //dynamic acadApp = Application.AcadApplication;
            //acadApp.ActiveDocument.SendCommand("CREATEPTPLYLNCTRVERTAUTO ");         
            try
            {
                await ed.CommandAsync("CREATEPTPLYLNCTRVERTAUTO", Editor.PauseToken);
            }
            catch
            {
                return;
            }                
            while (((string)Application.GetSystemVariable("CMDNAMES")).Contains("CREATEPTPLYLNCTRVERTAUTO"))
            {
                try { await ed.CommandAsync(Editor.PauseToken); }
                catch { break; }
            }
            //doc.SendStringToExecute("CREATEPTPLYLNCTRVERTAUTO ", true, false, true);

            // Обновление точек
            if (!idPointGroup.IsNull)
            {
                using (var pg = idPointGroup.Open(OpenMode.ForRead) as PointGroup)
                {
                    pg.Update();
                }
            }
        }

        /// <summary>
        /// Установка настроек
        /// </summary>
        private void SetSettings()
        {
            // Настройка команды CreatePoints
            SettingsCmdCreatePoints pointsCmdSettings = civil.Settings.GetSettings<SettingsCmdCreatePoints>();
            var npn = pointsCmdSettings.PointIdentity.NextPointNumber.Value;
            try
            {
                pointsCmdSettings.PointIdentity.NextPointNumber.Value = options.CmdCreatePointsNextPointNumber;                
            }
            catch { }
            pointsCmdSettings.PointsCreation.PromptForDescriptions.Value = options.CmdCreatePointsPromptForDescription;
            pointsCmdSettings.PointsCreation.DefaultDescription.Value = options.CmdCreatePointsDefaultDescription;

            // Настройка группы точек - Поворотные точки_Эскиз2            
            if (!civil.PointGroups.Contains(options.PointGroupName))
            {
                idPointGroup = civil.PointGroups.Add(options.PointGroupName);
                using (var pointGroup = idPointGroup.Open(OpenMode.ForRead) as PointGroup)
                {
                    pointGroup.PointStyleId = idPointStyle;
                    pointGroup.PointLabelStyleId = idLabelPointStyle;
                    StandardPointGroupQuery query = new StandardPointGroupQuery();
                    query.IncludeRawDescriptions = options.CmdCreatePointsDefaultDescription;
                    pointGroup.SetQuery(query);                    
                }
            }
            else
            {
                idPointGroup = civil.PointGroups[options.PointGroupName];
            }
        }

        private void CopyStyles()
        {   
            // Проверка необходимости копирования настроек по дате изменений
            if (!UpdateRequired())            
                // Проверка наличия необходимих стилей
                if (IsExistStyles())                
                    return;                                            

            string fileTemplateKR = options.TemplateFilePath.GetCadSettingsRealPath();

            using (var dbTemplate = new Database(false, true))
            {
                dbTemplate.ReadDwgFile(fileTemplateKR, FileShare.ReadWrite, false, "");
                dbTemplate.CloseInput(true);
                var civilTemplate = CivilDocument.GetCivilDocument(dbTemplate);

                CopyService copyService = new CopyService(db);

                using (var t = dbTemplate.TransactionManager.StartTransaction())
                {
                    // Копирование стиля точек
                    copyService.CopyStyle(options.StylePoint, civilTemplate.Styles.PointStyles);
                    idPointStyle = civil.Styles.PointStyles[options.StylePoint];
                    // Стиль меток
                    copyService.CopyStyle(options.StylesLabelPoint,
                        civilTemplate.Styles.LabelStyles.PointLabelStyles.LabelStyles);
                    idLabelPointStyle = civil.Styles.LabelStyles.PointLabelStyles.LabelStyles[options.StylesLabelPoint];

                    SaveStylesDate();

                    t.Commit();
                }
            }
        }

        private bool IsExistStyles()
        {
            idLabelPointStyle = checkStyle(options.StylesLabelPoint, civil.Styles.LabelStyles.PointLabelStyles.LabelStyles);
            if (idLabelPointStyle.IsNull) return false;
            idPointStyle = checkStyle(options.StylePoint, civil.Styles.PointStyles);
            if (idPointStyle.IsNull) return false;
            return true;
        }

        private ObjectId checkStyle(string style, StyleCollectionBase styleBase)
        {
            if (styleBase.Contains(style))
            {
                return styleBase[style];
            }
            return ObjectId.Null;
        }

        /// <summary>
        /// Проверка актуальности текущих стилей в чертеже
        /// </summary>        
        private bool UpdateRequired()
        {
            // Дата обновления настроек в чертеже
            DateTime dateCurStyles = GetCurrentStylesDate();           
            return options.DateChanges > dateCurStyles;            
        }

        /// <summary>
        /// Определение текущей даты стилей в чертеже
        /// </summary>        
        private DateTime GetCurrentStylesDate()
        {
            // Чтение даты из словаря
            AcadLib.DictNOD nod = new AcadLib.DictNOD(innerDictName, true);
            var value = nod.Load(recDateStyles, DateTime.MinValue.ToString());
            DateTime res;
            DateTime.TryParse(value, out res);
            return res;                
        }

        /// <summary>
        /// Сохранение даты стилей в чертеже
        /// </summary>        
        private void SaveStylesDate()
        {
            // Чтение даты из словаря
            AcadLib.DictNOD nod = new AcadLib.DictNOD(innerDictName, true);
            nod.Save(DateTime.Now.ToString(), recDateStyles);            
        }
    }
}
