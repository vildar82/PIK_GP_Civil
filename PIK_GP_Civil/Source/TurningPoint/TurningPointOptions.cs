using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AcadLib;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.Civil.Settings;

namespace PIK_GP_Civil.TurningPoint
{
    [Serializable]
    public class TurningPointOptions
    {        
        static string FileXml = Path.Combine(AutoCAD_PIK_Manager.Settings.PikSettings.ServerShareSettingsFolder,
                                @"ГП\\Civil\\TurningPoint.xml");
        const string DictNod = "PIK";
        //public const string RecAbsoluteZero = "AbsoluteZero";

        [Category("Файл шаблона")]
        [DisplayName("Путь к файлу шаблона")]
        [Description(@"Например: z:\Civil3D_server\KR\PIK_KR.dwt.")]
        [DefaultValue(@"z:\Civil3D_server\KR\PIK_KR.dwt")]
        public string TemplateFilePath { get; set; } = @"z:\Civil3D_server\KR\PIK_KR.dwt";

        [Category("Стили")]
        [DisplayName("Стиль точки")]
        [Description("Копируемый стиль точки из файла шаблона.")]
        [DefaultValue("PIK_Поворотные точки")]
        public string StylePoint { get; set; } = "PIK_Поворотные точки";

        [Category("Стили")]
        [DisplayName("Стиль метки точек")]
        [Description("Копируемый стиль метки точек из файла шаблона.")]
        [DefaultValue("PIK_Номер Точки")]
        public string StylesLabelPoint { get; set; } = "PIK_Номер Точки";
                
        [Category("Команда - CreatePoints")]
        [DisplayName("Next Point Number")]
        [Description("Значение для: PointIdentity->Next Point Number.")]
        [DefaultValue(1)]
        public uint CmdCreatePointsNextPointNumber { get; set; } = 1;

        [Category("Команда - CreatePoints")]
        [DisplayName("Prompt For Description")]
        [Description("Значение для: Points Creation->Prompt For Description.")]
        [DefaultValue(AutomaticManual.Automatic)]
        public AutomaticManual CmdCreatePointsPromptForDescription { get; set; } = AutomaticManual.Automatic;

        [Category("Команда - CreatePoints")]
        [DisplayName("Default Description")]
        [Description("Значение для: Points Creation->Default Description.")]
        [DefaultValue("Поворотные")]
        public string CmdCreatePointsDefaultDescription { get; set; } = "Поворотные";

        [Category("Навигатор - Группа точек")]
        [DisplayName("Имя группы")]
        [Description("Имя для группы точек.")]
        [DefaultValue("Поворотные точки_Эскиз2")]
        public string PointGroupName { get; set; } = "Поворотные точки_Эскиз2";        

        [Category("Дата изменений")]
        [DisplayName("Дата изменений")]
        [Description("Дата последних изменений в файле шаблона связанных с командой создания поворотных точек.")]        
        public DateTime DateChanges { get; set; }

        public TurningPointOptions PromptOptions()
        {
            TurningPointOptions resVal = this;
            //Запрос начальных значений
            AcadLib.UI.FormProperties formProp = new AcadLib.UI.FormProperties();
            TurningPointOptions thisCopy = (TurningPointOptions)resVal.MemberwiseClone();
            formProp.propertyGrid1.SelectedObject = thisCopy;
            if (Application.ShowModalDialog(formProp) != System.Windows.Forms.DialogResult.OK)
            {
                throw new Exception(General.CanceledByUser);
            }
            try
            {
                resVal = thisCopy;
                Save();
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex, "Не удалось сохранить стартовые параметры.");
            }
            return resVal;
        }

        public static TurningPointOptions Load()
        {
            TurningPointOptions options = null;
            if (File.Exists(FileXml))
            {
                try
                {
                    // Загрузка настроек таблицы из файла XML
                    options = TurningPointOptions.LoadFromXml();
                    // Загрузка настроек чертежа
                    //options.LoadFromNOD();
                }
                catch (Exception ex)
                {
                    Logger.Log.Error(ex, $"Ошибка при попытке загрузки настроек таблицы из XML файла {FileXml}");
                }
            }
            if (options == null)
            {
                // Создать дефолтные
                options = new TurningPointOptions();
                options.SetDefault();
                // Сохранение дефолтных настроек 
                try
                {
                    options.Save();
                }
                catch (Exception exSave)
                {
                    Logger.Log.Error(exSave, $"Попытка сохранение настроек в файл {FileXml}");
                }
            }
            return options;
        }

        private void SetDefault()
        {            
        }

        private static TurningPointOptions LoadFromXml()
        {
            AcadLib.Files.SerializerXml ser = new AcadLib.Files.SerializerXml(FileXml);
            return ser.DeserializeXmlFile<TurningPointOptions>();
        }

        public void Save()
        {
            //SaveToNOD();
            AcadLib.Files.SerializerXml ser = new AcadLib.Files.SerializerXml(FileXml);
            ser.SerializeList(this);
        }

        private void SaveToNOD()
        {
            //var nod = new AcadLib.DictNOD(DictNod);
            //nod.Save(AbsoluteZero, RecAbsoluteZero);
        }

        private void LoadFromNOD()
        {
            //var nod = new AcadLib.DictNOD(DictNod);
            //AbsoluteZero = nod.Load(RecAbsoluteZero, 150.00);
        }
    }
}