using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AcadLib;
using Autodesk.AutoCAD.ApplicationServices;

namespace PIK_GP_Civil.TurningPoint
{
    public enum TurningPointType
    {        
        None,
        Coordinates
    }
    
    public class TurningUserOptions
    {
        private const string dict = TurningPointService.innerDictName;
        private const string recLabelType = "LabelType";
        public static string[] labelStyles;        

        //public string StyleLabelPoint { get; private set; }

        [Category("Поворотные точки")]
        [DisplayName("Стиль меток")]
        [Description("Стиль меток поворотоных точек")]  
        [TypeConverter(typeof(PointLabelStyleConverter))]      
        public string PaintLabelStyle { get; set; }

        public static TurningUserOptions Load(string[] labelStyles)
        {
            TurningUserOptions.labelStyles = labelStyles;
            TurningUserOptions res = new TurningUserOptions();
            DictNOD nod = new DictNOD(dict, true);
            res.PaintLabelStyle= nod.Load(recLabelType, labelStyles.First());
            return res;
        }

        public void Save()
        {
            DictNOD nod = new DictNOD(dict, true);
            nod.Save(PaintLabelStyle, recLabelType);
        }

        public static TurningUserOptions PromptOptions(TurningUserOptions options)
        {            
            TurningUserOptions resVal = options;            
            //Запрос начальных значений
            AcadLib.UI.FormProperties formProp = new AcadLib.UI.FormProperties();
            TurningUserOptions thisCopy = (TurningUserOptions)resVal.MemberwiseClone();
            formProp.propertyGrid1.SelectedObject = thisCopy;
            if (Application.ShowModalDialog(formProp) != System.Windows.Forms.DialogResult.OK)
            {
                throw new Exception(General.CanceledByUser);
            }            
            resVal = thisCopy;
            resVal.Save();                              
            return resVal;
        }

        //public void Define(TurningPointOptions options)
        //{
        //    switch (LabelType)
        //    {
        //        case TurningPointType.None:
        //            StyleLabelPoint = options.StylesLabelPoint.First(s => !s.Contains("без координат", StringComparison.OrdinalIgnoreCase));
        //            break;
        //        case TurningPointType.Coordinates:
        //            StyleLabelPoint = options.StylesLabelPoint.First(s => s.Contains("без координат", StringComparison.OrdinalIgnoreCase));
        //            break;
        //        default:
        //            break;
        //    }
        //}
    }

    class PointLabelStyleConverter : TypeConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) { return true; }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) { return true; }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(TurningUserOptions.labelStyles);
        }
    }

    //public class EnumTurningPointTypeConvertor : EnumConverter
    //{
    //    public EnumTurningPointTypeConvertor() : base(typeof(TurningPointType)) { }

    //    public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
    //    {
    //        return sourceType == typeof(string);
    //    }
    //    public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
    //    {
    //        switch (value.ToString())
    //        {
    //            case "Без координат":
    //                return Enum.Parse(typeof(TurningPointType), "None");
    //            case "С координатами":
    //                return Enum.Parse(typeof(TurningPointType), "Coordinates");
    //        }
    //        return Enum.Parse(typeof(TurningPointType), value.ToString());
    //    }
    //    public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
    //    {
    //        return destinationType == typeof(string);
    //    }
    //    public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
    //    {
    //        switch (value.ToString())
    //        {
    //            case "None":
    //                return "Без координат";
    //            case "Coordinates":
    //                return "С координатами";
    //        }
    //        return value.ToString();
    //    }
    //}    
}